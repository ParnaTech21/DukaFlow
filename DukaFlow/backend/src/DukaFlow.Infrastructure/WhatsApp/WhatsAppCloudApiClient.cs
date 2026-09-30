using System.Net.Http.Headers;
using System.Net.Http.Json;
using DukaFlow.Application.WhatsApp.Interfaces;
using DukaFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DukaFlow.Infrastructure.WhatsApp;

// Only class in the codebase that constructs a Graph API request. It
// does not decide what to say - ConversationService owns that. See
// Phase 4 CLAUDE.md #17.
//
// Registered as a typed client: services.AddHttpClient<IWhatsAppMessageSender, WhatsAppCloudApiClient>();
public class WhatsAppCloudApiClient : IWhatsAppMessageSender
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly DukaFlowDbContext _db;
    private readonly ILogger<WhatsAppCloudApiClient> _logger;

    public WhatsAppCloudApiClient(HttpClient http, IConfiguration config, DukaFlowDbContext db, ILogger<WhatsAppCloudApiClient> logger)
    {
        _http = http;
        _config = config;
        _db = db;
        _logger = logger;
    }

    public async Task SendTextAsync(Guid restaurantId, string toPhoneNumber, string text, CancellationToken ct)
    {
        var businessConfig = await _db.WhatsAppBusinessConfigurations.AsNoTracking()
            .FirstOrDefaultAsync(c => c.RestaurantId == restaurantId && c.IsActive, ct);

        if (businessConfig is null)
        {
            _logger.LogWarning("No active WhatsApp configuration for restaurant {RestaurantId} - message not sent.", restaurantId);
            return;
        }

        // Confirm this is still current in Meta's dashboard before going
        // live - Graph API versions are deprecated on a rolling ~2-year
        // schedule (latest at the time this was written is v26.0; v23.0
        // is used as the fallback default here for a bit more runway).
        // See Phase 4 CLAUDE.md #4.
        var apiVersion = _config["WhatsApp:ApiVersion"] ?? "v23.0";
        var accessToken = _config["WhatsApp:AccessToken"];

        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogWarning("WhatsApp:AccessToken is not configured - message not sent.");
            return;
        }

        var url = $"https://graph.facebook.com/{apiVersion}/{businessConfig.PhoneNumberId}/messages";

        var payload = new
        {
            messaging_product = "whatsapp",
            to = toPhoneNumber,
            type = "text",
            text = new { body = text }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                // Log the failure and Meta's error body (no secrets in it),
                // but never the access token or the outbound message content
                // wholesale - see Phase 4 CLAUDE.md #19.
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning(
                    "WhatsApp send failed for restaurant {RestaurantId}: {Status} {Body}",
                    restaurantId, response.StatusCode, errorBody);
            }
        }
        catch (Exception ex)
        {
            // Never let an outbound-message failure bubble up and break
            // whatever business flow triggered it (e.g. order confirmation).
            // Business state stays authoritative regardless of delivery -
            // see Phase 4 CLAUDE.md #23 / Phase 6 CLAUDE.md #23.
            _logger.LogError(ex, "WhatsApp send threw for restaurant {RestaurantId}.", restaurantId);
        }
    }
}
