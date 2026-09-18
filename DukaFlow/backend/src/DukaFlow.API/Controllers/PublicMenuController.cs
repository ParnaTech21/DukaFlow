using DukaFlow.Application.Ordering.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DukaFlow.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public/restaurants")]
public class PublicMenuController : ControllerBase
{
    private readonly IPublicMenuService _publicMenuService;

    public PublicMenuController(IPublicMenuService publicMenuService) => _publicMenuService = publicMenuService;

    [HttpGet("{restaurantId:guid}/menu")]
    public async Task<IActionResult> GetMenu(Guid restaurantId, CancellationToken ct)
    {
        var menu = await _publicMenuService.GetMenuAsync(restaurantId, ct);
        return Ok(new { success = true, data = menu });
    }
}
