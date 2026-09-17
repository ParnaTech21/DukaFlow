using System.Security.Claims;
using DukaFlow.Application.Auth.Interfaces;
using DukaFlow.Application.Menu.DTOs;
using DukaFlow.Application.Restaurants.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DukaFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/menu/items")]
public class MenuItemsController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly IRestaurantService _restaurantService;

    public MenuItemsController(IMenuService menuService, IRestaurantService restaurantService)
    {
        _menuService = menuService;
        _restaurantService = restaurantService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Menu data is scoped by RestaurantId, not UserId — resolve the owner's
    // restaurant first, the same way RestaurantsController does.
    private async Task<Guid> GetCurrentRestaurantIdAsync(CancellationToken ct)
    {
        var restaurant = await _restaurantService.GetMyRestaurantAsync(CurrentUserId, ct);
        return restaurant.Id;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var items = await _menuService.GetItemsAsync(restaurantId, categoryId, ct);
        return Ok(new { success = true, data = items });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var item = await _menuService.GetItemAsync(restaurantId, id, ct);
        return Ok(new { success = true, data = item });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMenuItemRequest request, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var item = await _menuService.CreateItemAsync(restaurantId, request, ct);
        return Ok(new { success = true, data = item });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMenuItemRequest request, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var item = await _menuService.UpdateItemAsync(restaurantId, id, request, ct);
        return Ok(new { success = true, data = item });
    }

    [HttpPatch("{id:guid}/availability")]
    public async Task<IActionResult> SetAvailability(Guid id, SetMenuItemAvailabilityRequest request, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var item = await _menuService.SetItemAvailabilityAsync(restaurantId, id, request.IsAvailable, ct);
        return Ok(new { success = true, data = item });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        await _menuService.DeleteItemAsync(restaurantId, id, ct);
        return NoContent();
    }
}