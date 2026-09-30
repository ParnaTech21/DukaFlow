using System.Security.Cryptography;
using System.Text;
using DukaFlow.Infrastructure.WhatsApp;
using Xunit;

namespace DukaFlow.UnitTests.WhatsApp;

public class WhatsAppSignatureValidatorTests
{
    private const string AppSecret = "test-app-secret";

    private static string ComputeSignatureHeader(string body, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(body))).ToLowerInvariant();
        return $"sha256={hash}";
    }

    [Fact]
    public void IsValid_CorrectSignature_ReturnsTrue()
    {
        var body = "{\"entry\":[]}";
        var header = ComputeSignatureHeader(body, AppSecret);

        Assert.True(WhatsAppSignatureValidator.IsValid(body, header, AppSecret));
    }

    [Fact]
    public void IsValid_TamperedBody_ReturnsFalse()
    {
        var originalBody = "{\"entry\":[]}";
        var header = ComputeSignatureHeader(originalBody, AppSecret);
        var tamperedBody = "{\"entry\":[{\"injected\":true}]}";

        Assert.False(WhatsAppSignatureValidator.IsValid(tamperedBody, header, AppSecret));
    }

    [Fact]
    public void IsValid_WrongSecret_ReturnsFalse()
    {
        var body = "{\"entry\":[]}";
        var header = ComputeSignatureHeader(body, "a-different-secret");

        Assert.False(WhatsAppSignatureValidator.IsValid(body, header, AppSecret));
    }

    [Fact]
    public void IsValid_MissingHeader_ReturnsFalse()
    {
        Assert.False(WhatsAppSignatureValidator.IsValid("{}", null, AppSecret));
    }

    [Fact]
    public void IsValid_MalformedHeaderPrefix_ReturnsFalse()
    {
        Assert.False(WhatsAppSignatureValidator.IsValid("{}", "not-sha256=abc123", AppSecret));
    }

    [Fact]
    public void IsValid_NoAppSecretConfigured_SkipsVerificationAndReturnsTrue()
    {
        // Documents current sandbox-only behavior deliberately - see the
        // comment in WhatsAppSignatureValidator. If AppSecret is ever
        // required in production config, this test should change too.
        Assert.True(WhatsAppSignatureValidator.IsValid("{}", null, null));
    }
}
