using DukaFlow.Domain.Entities;

namespace DukaFlow.Application.Auth.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
