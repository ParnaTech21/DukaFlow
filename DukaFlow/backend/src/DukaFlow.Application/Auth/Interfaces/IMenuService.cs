using DukaFlow.Application.Menu.DTOs;

public interface IMenuService
{
    Task<List<MenuCategoryDto>> GetCategoriesAsync(Guid restaurantId, CancellationToken ct);
    Task<MenuCategoryDto> CreateCategoryAsync(Guid restaurantId, CreateMenuCategoryRequest request, CancellationToken ct);
    Task<MenuCategoryDto> UpdateCategoryAsync(Guid restaurantId, Guid categoryId, UpdateMenuCategoryRequest request, CancellationToken ct);
    Task DeleteCategoryAsync(Guid restaurantId, Guid categoryId, CancellationToken ct);

    Task<List<MenuItemDto>> GetItemsAsync(Guid restaurantId, Guid? categoryId, CancellationToken ct);
    Task<MenuItemDto> GetItemAsync(Guid restaurantId, Guid itemId, CancellationToken ct);
    Task<MenuItemDto> CreateItemAsync(Guid restaurantId, CreateMenuItemRequest request, CancellationToken ct);
    Task<MenuItemDto> UpdateItemAsync(Guid restaurantId, Guid itemId, UpdateMenuItemRequest request, CancellationToken ct);
    Task<MenuItemDto> SetItemAvailabilityAsync(Guid restaurantId, Guid itemId, bool isAvailable, CancellationToken ct);
    Task DeleteItemAsync(Guid restaurantId, Guid itemId, CancellationToken ct);
}