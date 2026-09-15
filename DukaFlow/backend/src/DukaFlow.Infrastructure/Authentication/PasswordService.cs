using DukaFlow.Application.Auth.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DukaFlow.Infrastructure.Authentication;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(new object(), password);

    public bool Verify(string password, string hash)
    {
        var result = _hasher.VerifyHashedPassword(new object(), hash, password);
        return result == PasswordVerificationResult.Success
            || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
