namespace DukaFlow.Application.Ordering.DTOs;

// Deliberately narrow - these are the only fields a customer/WhatsApp
// client should ever see. Never add OwnerId, staff data, or internal
// audit fields here. See Phase 3 CLAUDE.md #17.
public record PublicMenuItemResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl);

public record PublicMenuCategoryResponse(
    Guid Id,
    string Name,
    List<PublicMenuItemResponse> Items);

public record PublicMenuResponse(
    string RestaurantName,
    List<PublicMenuCategoryResponse> Categories);
