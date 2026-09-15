namespace DukaFlow.Application.Restaurants.DTOs;

public record RestaurantResponse(
    Guid Id,
    string Name,
    string? Description,
    string? PhoneNumber,
    string? WhatsAppNumber,
    string? Address,
    bool IsActive);
