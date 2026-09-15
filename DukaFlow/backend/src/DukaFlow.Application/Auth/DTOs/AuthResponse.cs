namespace DukaFlow.Application.Auth.DTOs;

public record UserDto(Guid Id, string FirstName, string LastName, string Email, string Role);
public record RestaurantSummaryDto(Guid Id, string Name);

public record AuthResponse(UserDto User, RestaurantSummaryDto Restaurant, string AccessToken);
