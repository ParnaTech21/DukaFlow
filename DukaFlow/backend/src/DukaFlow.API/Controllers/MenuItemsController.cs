using System.Security.Claims;
using DukaFlow.Application.Menu.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DukaFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/menu/items")]
public class MenuItemsController : ControllerBase
{
    private readonly IMenuService _menuService;
    public MenuItemsController(IMenuService menuService) => _menuService = menuService;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId, CancellationToken ct)
    {
        var items = await _menuService.GetItemsAsync(CurrentUserId, categoryId, ct);
        return Ok(new { success = true, data = items });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var item = await _menuService.GetItemAsync(CurrentUserId, id, ct);
        return Ok(new { success = true, data = item });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMenuItemRequest request, CancellationToken ct)
    {
        var item = await _menuService.CreateItemAsync(CurrentUserId, request, ct);
        return Ok(new { success = true, data = item });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMenuItemRequest request, CancellationToken ct)
    {
        var item = await _menuService.UpdateItemAsync(CurrentUserId, id, request, ct);
        return Ok(new { success = true, data = item });
    }

    [HttpPatch("{id:guid}/availability")]
    public async Task<IActionResult> SetAvailability(Guid id, [FromBody] bool isAvailable, CancellationToken ct)
    {
        var item = await _menuService.SetItemAvailabilityAsync(CurrentUserId, id, isAvailable, ct);
        return Ok(new { success = true, data = item });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _menuService.DeleteItemAsync(CurrentUserId, id, ct);
        return NoContent();
    }
}