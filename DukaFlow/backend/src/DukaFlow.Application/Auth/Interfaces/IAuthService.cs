using DukaFlow.Application.Auth.DTOs;

namespace DukaFlow.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct);
}
