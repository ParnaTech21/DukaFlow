using DukaFlow.Application.Common;
using DukaFlow.Application.Ordering.DTOs;
using DukaFlow.Application.Ordering.Interfaces;
using DukaFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DukaFlow.Infrastructure.Ordering;

public class PublicMenuService : IPublicMenuService
{
    private readonly DukaFlowDbContext _db;

    public PublicMenuService(DukaFlowDbContext db) => _db = db;

    public async Task<PublicMenuResponse> GetMenuAsync(Guid restaurantId, CancellationToken ct)
    {
        var restaurant = await _db.Restaurants.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == restaurantId && r.IsActive, ct)
            ?? throw new NotFoundException("Restaurant not found.");

        var items = await _db.MenuItems.AsNoTracking()
            .Include(i => i.MenuCategory)
            .Where(i => i.RestaurantId == restaurantId
                        && i.IsActive && i.IsAvailable
                        && i.MenuCategory.IsActive)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync(ct);

        var categories = items
            .GroupBy(i => new { i.MenuCategoryId, i.MenuCategory.Name, i.MenuCategory.DisplayOrder })
            .OrderBy(g => g.Key.DisplayOrder)
            .Select(g => new PublicMenuCategoryResponse(
                g.Key.MenuCategoryId,
                g.Key.Name,
                g.Select(i => new PublicMenuItemResponse(i.Id, i.Name, i.Description, i.Price, i.ImageUrl)).ToList()))
            .ToList();

        return new PublicMenuResponse(restaurant.Name, categories);
    }
}
