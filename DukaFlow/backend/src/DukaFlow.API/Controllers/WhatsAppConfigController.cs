using System.Security.Claims;
using DukaFlow.Application.Restaurants.Interfaces;
using DukaFlow.Application.WhatsApp.DTOs;
using DukaFlow.Application.WhatsApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DukaFlow.API.Controllers;

// Deliberately minimal - just enough for a restaurant owner to tell
// DukaFlow which Meta phone_number_id is theirs, so the webhook can route
// to them. Everything else about setting up the Meta app (App Review,
// test number, access token) happens outside DukaFlow, in Meta's own
// dashboard, per Phase 4 CLAUDE.md #4/#5.
[ApiController]
[Authorize]
[Route("api/whatsapp/configuration")]
public class WhatsAppConfigController : ControllerBase
{
    private readonly IWhatsAppConfigurationService _configService;
    private readonly IRestaurantService _restaurantService;

    public WhatsAppConfigController(IWhatsAppConfigurationService configService, IRestaurantService restaurantService)
    {
        _configService = configService;
        _restaurantService = restaurantService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<Guid> GetCurrentRestaurantIdAsync(CancellationToken ct)
    {
        var restaurant = await _restaurantService.GetMyRestaurantAsync(CurrentUserId, ct);
        return restaurant.Id;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var config = await _configService.GetMineAsync(restaurantId, ct);
        return Ok(new { success = true, data = config });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMine(UpdateWhatsAppConfigurationRequest request, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var config = await _configService.UpsertMineAsync(restaurantId, request, ct);
        return Ok(new { success = true, data = config });
    }
}
