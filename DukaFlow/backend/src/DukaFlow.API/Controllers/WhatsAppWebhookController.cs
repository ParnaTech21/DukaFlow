using System.Text;
using System.Text.Json;
using DukaFlow.Application.WhatsApp.Interfaces;
using DukaFlow.Domain.Entities;
using DukaFlow.Infrastructure.Persistence;
using DukaFlow.Infrastructure.WhatsApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DukaFlow.API.Controllers;

// Public, unauthenticated, hostile-input surface by definition - see
// Phase 4 CLAUDE.md #23. Every request is either Meta's verification
// handshake or a signed webhook event; nothing here trusts anything in
// the body until the signature check passes.
[ApiController]
[AllowAnonymous]
[Route("api/webhooks/whatsapp")]
public class WhatsAppWebhookController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IConfiguration _config;
    private readonly DukaFlowDbContext _db;
    private readonly IConversationService _conversationService;
    private readonly ILogger<WhatsAppWebhookController> _logger;

    public WhatsAppWebhookController(
        IConfiguration config, DukaFlowDbContext db, IConversationService conversationService, ILogger<WhatsAppWebhookController> logger)
    {
        _config = config;
        _db = db;
        _conversationService = conversationService;
        _logger = logger;
    }

    // Meta calls this once, when you register/change the webhook URL in
    // the App Dashboard.
    [HttpGet]
    public IActionResult Verify(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken,
        [FromQuery(Name = "hub.challenge")] string? challenge)
    {
        var expectedToken = _config["WhatsApp:VerifyToken"];

        if (mode == "subscribe" && !string.IsNullOrEmpty(expectedToken) && verifyToken == expectedToken && challenge is not null)
        {
            return Content(challenge, "text/plain");
        }

        _logger.LogWarning("WhatsApp webhook verification failed (mode={Mode}).", mode);
        return Forbid();
    }

    [HttpPost]
    public async Task<IActionResult> Receive(CancellationToken ct)
    {
        // Signature verification needs the exact raw bytes Meta sent, so
        // read the body ourselves rather than letting model binding parse
        // it first.
        Request.EnableBuffering();
        string rawBody;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            rawBody = await reader.ReadToEndAsync(ct);
        }
        Request.Body.Position = 0;

        var signatureHeader = Request.Headers["X-Hub-Signature-256"].ToString();
        var appSecret = _config["WhatsApp:AppSecret"];

        if (!WhatsAppSignatureValidator.IsValid(rawBody, signatureHeader, appSecret))
        {
            _logger.LogWarning("WhatsApp webhook signature validation failed.");
            return Unauthorized();
        }

        WhatsAppWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<WhatsAppWebhookPayload>(rawBody, JsonOptions);
        }
        catch (JsonException ex)
        {
            // Ack anyway (200) so Meta doesn't retry a payload that will
            // never parse - but log it, since it means either a bug here
            // or a Meta payload shape DukaFlow doesn't model yet.
            _logger.LogWarning(ex, "Could not parse WhatsApp webhook payload.");
            return Ok();
        }

        foreach (var entry in payload?.Entry ?? [])
        {
            foreach (var change in entry.Changes ?? [])
            {
                await ProcessChangeAsync(change.Value, ct);
            }
        }

        // Always 200 once we've accepted the request - a non-2xx response
        // makes Meta retry the whole payload, including messages we
        // already processed successfully.
        return Ok();
    }

    private async Task ProcessChangeAsync(WhatsAppChangeValue? value, CancellationToken ct)
    {
        if (value?.Messages is null || value.Messages.Count == 0)
        {
            return; // Status callback (delivered/read), not an inbound message.
        }

        var phoneNumberId = value.Metadata?.PhoneNumberId;
        if (string.IsNullOrEmpty(phoneNumberId))
        {
            _logger.LogWarning("WhatsApp webhook message with no phone_number_id in metadata.");
            return;
        }

        var restaurantId = await _db.WhatsAppBusinessConfigurations.AsNoTracking()
            .Where(c => c.PhoneNumberId == phoneNumberId && c.IsActive)
            .Select(c => c.RestaurantId)
            .FirstOrDefaultAsync(ct);

        if (restaurantId == Guid.Empty)
        {
            // Never trust a restaurant identifier the customer supplies -
            // and there isn't one here anyway. Only a phone_number_id we
            // ourselves configured resolves to a restaurant. See Phase 4
            // CLAUDE.md #15.
            _logger.LogWarning("Unmapped WhatsApp phone_number_id {PhoneNumberId} - no matching restaurant.", phoneNumberId);
            return;
        }

        foreach (var message in value.Messages)
        {
            if (string.IsNullOrEmpty(message.Id) || string.IsNullOrEmpty(message.From))
            {
                continue;
            }

            if (await _db.InboundMessages.AnyAsync(m => m.ExternalMessageId == message.Id, ct))
            {
                continue; // Already processed - Meta redelivered it.
            }

            _db.InboundMessages.Add(new InboundMessage
            {
                Id = Guid.NewGuid(),
                RestaurantId = restaurantId,
                ExternalMessageId = message.Id,
                CustomerPhoneNumber = message.From,
                MessageType = message.Type ?? "unknown",
                ProcessingStatus = "Received",
                ReceivedAt = DateTimeOffset.UtcNow
            });
            await _db.SaveChangesAsync(ct);

            var text = message.Text?.Body
                ?? message.Interactive?.ButtonReply?.Title
                ?? message.Interactive?.ListReply?.Title
                ?? message.Button?.Text
                ?? string.Empty;

            try
            {
                await _conversationService.HandleInboundTextMessageAsync(restaurantId, message.From, message.Id, text, ct);
            }
            catch (Exception ex)
            {
                // A bug in the conversation flow must not take down
                // webhook processing for other messages/restaurants, and
                // must not make Meta think delivery failed (we already
                // returned/will return 200).
                _logger.LogError(ex, "Error handling WhatsApp message {MessageId} for restaurant {RestaurantId}.", message.Id, restaurantId);
            }
        }
    }
}
