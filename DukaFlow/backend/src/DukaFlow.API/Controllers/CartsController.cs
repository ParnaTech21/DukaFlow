using System.Security.Claims;
using DukaFlow.Application.Ordering.DTOs;
using DukaFlow.Application.Ordering.Interfaces;
using DukaFlow.Application.Restaurants.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DukaFlow.API.Controllers;

// Authenticated for now - Phase 3 validates the engine through
// Swagger/Postman before WhatsApp exists (Phase 4).
[ApiController]
[Authorize]
[Route("api/carts")]
public class CartsController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly IRestaurantService _restaurantService;

    public CartsController(ICartService cartService, IRestaurantService restaurantService)
    {
        _cartService = cartService;
        _restaurantService = restaurantService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<Guid> GetCurrentRestaurantIdAsync(CancellationToken ct)
    {
        var restaurant = await _restaurantService.GetMyRestaurantAsync(CurrentUserId, ct);
        return restaurant.Id;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCartRequest request, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var cart = await _cartService.CreateCartAsync(restaurantId, request, ct);
        return Ok(new { success = true, data = cart });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var cart = await _cartService.GetCartAsync(restaurantId, id, ct);
        return Ok(new { success = true, data = cart });
    }

    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItem(Guid id, AddCartItemRequest request, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var cart = await _cartService.AddItemAsync(restaurantId, id, request, ct);
        return Ok(new { success = true, data = cart });
    }

    [HttpPut("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, Guid itemId, UpdateCartItemRequest request, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var cart = await _cartService.UpdateItemQuantityAsync(restaurantId, id, itemId, request, ct);
        return Ok(new { success = true, data = cart });
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var cart = await _cartService.RemoveItemAsync(restaurantId, id, itemId, ct);
        return Ok(new { success = true, data = cart });
    }

    [HttpPost("{id:guid}/checkout")]
    public async Task<IActionResult> Checkout(Guid id, CheckoutRequest request, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var order = await _cartService.CheckoutAsync(restaurantId, id, request, ct);
        return Ok(new { success = true, data = order });
    }
}
