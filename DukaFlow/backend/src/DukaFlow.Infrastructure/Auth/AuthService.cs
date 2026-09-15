using DukaFlow.Application.Auth.DTOs;
using DukaFlow.Application.Auth.Interfaces;
using DukaFlow.Application.Common;
using DukaFlow.Domain.Entities;
using DukaFlow.Domain.Enums;
using DukaFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DukaFlow.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly DukaFlowDbContext _db;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(DukaFlowDbContext db, IPasswordService passwordService, IJwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(u => u.Email == normalizedEmail, ct);
        if (exists)
            throw new ConflictException("An account with this email already exists.");

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordService.Hash(request.Password),
            Role = UserRole.RestaurantOwner,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        var restaurant = new Restaurant
        {
            OwnerId = user.Id,
            Name = request.RestaurantName.Trim(),
            PhoneNumber = request.PhoneNumber,
            IsActive = true
        };

        _db.Restaurants.Add(restaurant);
        await _db.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponse(
            new UserDto(user.Id, user.FirstName, user.LastName, user.Email, user.Role.ToString()),
            new RestaurantSummaryDto(restaurant.Id, restaurant.Name),
            token);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .Include(u => u.Restaurant)
            .SingleOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        if (user is null || !_passwordService.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAppException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAppException("This account has been deactivated.");

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponse(
            new UserDto(user.Id, user.FirstName, user.LastName, user.Email, user.Role.ToString()),
            new RestaurantSummaryDto(user.Restaurant!.Id, user.Restaurant.Name),
            token);
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync([userId], ct)
            ?? throw new NotFoundException("User not found.");

        return new UserDto(user.Id, user.FirstName, user.LastName, user.Email, user.Role.ToString());
    }
}
