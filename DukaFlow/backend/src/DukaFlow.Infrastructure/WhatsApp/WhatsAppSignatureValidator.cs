using System.Security.Cryptography;
using System.Text;

namespace DukaFlow.Infrastructure.WhatsApp;

// Verifies the X-Hub-Signature-256 header Meta sends on every webhook
// POST: "sha256=" + HMAC-SHA256(rawBody, appSecret) in hex. Must run
// against the exact raw request body, before any JSON parsing. See
// Phase 4 CLAUDE.md #23.
public static class WhatsAppSignatureValidator
{
    public static bool IsValid(string rawBody, string? signatureHeader, string? appSecret)
    {
        // No secret configured - this should only ever be true in local
        // sandbox testing. Treated as "skip verification", not "valid".
        if (string.IsNullOrEmpty(appSecret))
        {
            return true;
        }

        if (string.IsNullOrEmpty(signatureHeader) || !signatureHeader.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var providedHash = signatureHeader["sha256=".Length..];

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret));
        var computedHash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody))).ToLowerInvariant();

        var providedBytes = Encoding.UTF8.GetBytes(providedHash.ToLowerInvariant());
        var computedBytes = Encoding.UTF8.GetBytes(computedHash);

        return providedBytes.Length == computedBytes.Length
            && CryptographicOperations.FixedTimeEquals(providedBytes, computedBytes);
    }
}
