using System.Security.Claims;
using DukaFlow.Application.Restaurants.DTOs;
using DukaFlow.Application.Restaurants.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DukaFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/restaurants")]
public class RestaurantsController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;
    public RestaurantsController(IRestaurantService restaurantService) => _restaurantService = restaurantService;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("me")]
    public async Task<ActionResult<RestaurantResponse>> GetMine(CancellationToken ct)
    {
        var result = await _restaurantService.GetMyRestaurantAsync(CurrentUserId, ct);
        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<ActionResult<RestaurantResponse>> UpdateMine(UpdateRestaurantRequest request, CancellationToken ct)
    {
        var result = await _restaurantService.UpdateMyRestaurantAsync(CurrentUserId, request, ct);
        return Ok(result);
    }
}
