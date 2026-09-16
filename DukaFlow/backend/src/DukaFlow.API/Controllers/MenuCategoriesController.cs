using System.Security.Claims;
using DukaFlow.Application.Menu.DTOs;
using DukaFlow.Application.Restaurants.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DukaFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/menu/categories")]
public class MenuCategoriesController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly IRestaurantService _restaurantService;

    public MenuCategoriesController(IMenuService menuService, IRestaurantService restaurantService)
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
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var categories = await _menuService.GetCategoriesAsync(restaurantId, ct);
        return Ok(new { success = true, data = categories });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMenuCategoryRequest request, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var category = await _menuService.CreateCategoryAsync(restaurantId, request, ct);
        return Ok(new { success = true, data = category });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMenuCategoryRequest request, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var category = await _menuService.UpdateCategoryAsync(restaurantId, id, request, ct);
        return Ok(new { success = true, data = category });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        await _menuService.DeleteCategoryAsync(restaurantId, id, ct);
        return NoContent();
    }
}