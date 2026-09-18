using System.Security.Claims;
using DukaFlow.Application.Ordering.DTOs;
using DukaFlow.Application.Ordering.Interfaces;
using DukaFlow.Application.Restaurants.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DukaFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IRestaurantService _restaurantService;

    public OrdersController(IOrderService orderService, IRestaurantService restaurantService)
    {
        _orderService = orderService;
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
    public async Task<IActionResult> GetAll([FromQuery] OrderFilterQuery query, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var result = await _orderService.GetOrdersAsync(restaurantId, query, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var order = await _orderService.GetOrderAsync(restaurantId, id, ct);
        return Ok(new { success = true, data = order });
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var restaurantId = await GetCurrentRestaurantIdAsync(ct);
        var order = await _orderService.UpdateStatusAsync(restaurantId, id, request, ct);
        return Ok(new { success = true, data = order });
    }
}
