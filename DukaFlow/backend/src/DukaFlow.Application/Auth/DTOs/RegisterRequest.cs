namespace DukaFlow.Application.Auth.DTOs;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string RestaurantName,
    string? PhoneNumber);
