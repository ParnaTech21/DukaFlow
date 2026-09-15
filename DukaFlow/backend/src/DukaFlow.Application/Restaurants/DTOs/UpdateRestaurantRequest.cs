namespace DukaFlow.Application.Restaurants.DTOs;

public record UpdateRestaurantRequest(
    string Name,
    string? Description,
    string? PhoneNumber,
    string? WhatsAppNumber,
    string? Address);
