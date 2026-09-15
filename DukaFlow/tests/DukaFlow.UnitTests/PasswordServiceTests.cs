using DukaFlow.Infrastructure.Authentication;
using Xunit;

namespace DukaFlow.UnitTests;

public class PasswordServiceTests
{
    [Fact]
    public void Hash_Then_Verify_Succeeds_For_Correct_Password()
    {
        var service = new PasswordService();
        var hash = service.Hash("SecurePassword123!");

        Assert.True(service.Verify("SecurePassword123!", hash));
    }

    [Fact]
    public void Verify_Fails_For_Wrong_Password()
    {
        var service = new PasswordService();
        var hash = service.Hash("SecurePassword123!");

        Assert.False(service.Verify("WrongPassword", hash));
    }
}
