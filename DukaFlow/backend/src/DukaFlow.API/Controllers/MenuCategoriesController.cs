using System.Security.Claims;
using DukaFlow.Application.Menu.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DukaFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/menu/categories")]
public class MenuCategoriesController : ControllerBase
{
    private readonly IMenuService _menuService;
    public MenuCategoriesController(IMenuService menuService) => _menuService = menuService;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var categories = await _menuService.GetCategoriesAsync(CurrentUserId, ct);
        return Ok(new { success = true, data = categories });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMenuCategoryRequest request, CancellationToken ct)
    {
        var category = await _menuService.CreateCategoryAsync(CurrentUserId, request, ct);
        return Ok(new { success = true, data = category });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMenuCategoryRequest request, CancellationToken ct)
    {
        var category = await _menuService.UpdateCategoryAsync(CurrentUserId, id, request, ct);
        return Ok(new { success = true, data = category });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _menuService.DeleteCategoryAsync(CurrentUserId, id, ct);
        return NoContent();
    }
}