using DukaFlow.Application.Common;
using DukaFlow.Application.Menu.DTOs;
using DukaFlow.Domain.Entities;
using DukaFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DukaFlow.Application.Menu.Services;

public class MenuService : IMenuService
{
    private readonly DukaFlowDbContext _db;

    public MenuService(DukaFlowDbContext db) => _db = db;

    public async Task<List<MenuCategoryDto>> GetCategoriesAsync(Guid restaurantId, CancellationToken ct)
    {
        return await _db.MenuCategories
            .AsNoTracking()
            .Where(c => c.RestaurantId == restaurantId)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new MenuCategoryDto(c.Id, c.Name, c.Description, c.DisplayOrder, c.IsActive))
            .ToListAsync(ct);
    }

    public async Task<MenuCategoryDto> CreateCategoryAsync(Guid restaurantId, CreateMenuCategoryRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Category name is required.");

        var category = new MenuCategory
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            Name = request.Name.Trim(),
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.MenuCategories.Add(category);
        await _db.SaveChangesAsync(ct);

        return new MenuCategoryDto(category.Id, category.Name, category.Description, category.DisplayOrder, category.IsActive);
    }

    public async Task<MenuCategoryDto> UpdateCategoryAsync(Guid restaurantId, Guid categoryId, UpdateMenuCategoryRequest request, CancellationToken ct)
    {
        var category = await GetOwnedCategoryAsync(restaurantId, categoryId, ct);

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Category name is required.");

        category.Name = request.Name.Trim();
        category.Description = request.Description;
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new MenuCategoryDto(category.Id, category.Name, category.Description, category.DisplayOrder, category.IsActive);
    }

    public async Task DeleteCategoryAsync(Guid restaurantId, Guid categoryId, CancellationToken ct)
    {
        var category = await GetOwnedCategoryAsync(restaurantId, categoryId, ct);
        // Soft delete — see note below
        category.IsActive = false;
        category.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<MenuItemDto>> GetItemsAsync(Guid restaurantId, Guid? categoryId, CancellationToken ct)
    {
        var query = _db.MenuItems.AsNoTracking().Where(i => i.RestaurantId == restaurantId);

        if (categoryId is not null)
            query = query.Where(i => i.MenuCategoryId == categoryId);

        return await query
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new MenuItemDto(i.Id, i.MenuCategoryId, i.Name, i.Description, i.Price, i.ImageUrl, i.IsAvailable, i.IsActive, i.DisplayOrder))
            .ToListAsync(ct);
    }

    public async Task<MenuItemDto> GetItemAsync(Guid restaurantId, Guid itemId, CancellationToken ct)
    {
        var item = await GetOwnedItemAsync(restaurantId, itemId, ct);
        return ToDto(item);
    }

    public async Task<MenuItemDto> CreateItemAsync(Guid restaurantId, CreateMenuItemRequest request, CancellationToken ct)
    {
        ValidateItemRequest(request.Name, request.Price);

        // Tenant check: category must belong to the same restaurant
        var categoryExists = await _db.MenuCategories
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.MenuCategoryId && c.RestaurantId == restaurantId, ct);

        if (!categoryExists)
            throw new ValidationException("Invalid category for this restaurant.");

        var item = new MenuItem
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            MenuCategoryId = request.MenuCategoryId,
            Name = request.Name.Trim(),
            Description = request.Description,
            Price = request.Price,
            ImageUrl = request.ImageUrl,
            IsAvailable = true,
            IsActive = true,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync(ct);

        return ToDto(item);
    }

    public async Task<MenuItemDto> UpdateItemAsync(Guid restaurantId, Guid itemId, UpdateMenuItemRequest request, CancellationToken ct)
    {
        var item = await GetOwnedItemAsync(restaurantId, itemId, ct);
        ValidateItemRequest(request.Name, request.Price);

        var categoryExists = await _db.MenuCategories
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.MenuCategoryId && c.RestaurantId == restaurantId, ct);

        if (!categoryExists)
            throw new ValidationException("Invalid category for this restaurant.");

        item.MenuCategoryId = request.MenuCategoryId;
        item.Name = request.Name.Trim();
        item.Description = request.Description;
        item.Price = request.Price;
        item.ImageUrl = request.ImageUrl;
        item.IsAvailable = request.IsAvailable;
        item.IsActive = request.IsActive;
        item.DisplayOrder = request.DisplayOrder;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToDto(item);
    }

    public async Task<MenuItemDto> SetItemAvailabilityAsync(Guid restaurantId, Guid itemId, bool isAvailable, CancellationToken ct)
    {
        var item = await GetOwnedItemAsync(restaurantId, itemId, ct);
        item.IsAvailable = isAvailable;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return ToDto(item);
    }

    public async Task DeleteItemAsync(Guid restaurantId, Guid itemId, CancellationToken ct)
    {
        var item = await GetOwnedItemAsync(restaurantId, itemId, ct);
        item.IsActive = false;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    // --- Helpers: these enforce tenant isolation in one place ---

    private async Task<MenuCategory> GetOwnedCategoryAsync(Guid restaurantId, Guid categoryId, CancellationToken ct)
    {
        var category = await _db.MenuCategories
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.RestaurantId == restaurantId, ct);

        return category ?? throw new NotFoundException("Menu category not found.");
    }

    private async Task<MenuItem> GetOwnedItemAsync(Guid restaurantId, Guid itemId, CancellationToken ct)
    {
        var item = await _db.MenuItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.RestaurantId == restaurantId, ct);

        return item ?? throw new NotFoundException("Menu item not found.");
    }

    private static void ValidateItemRequest(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Item name is required.");
        if (price < 0)
            throw new ValidationException("Price cannot be negative.");
    }

    private static MenuItemDto ToDto(MenuItem i) =>
        new(i.Id, i.MenuCategoryId, i.Name, i.Description, i.Price, i.ImageUrl, i.IsAvailable, i.IsActive, i.DisplayOrder);
}