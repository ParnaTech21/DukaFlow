using System.Text.RegularExpressions;

namespace DukaFlow.Infrastructure.WhatsApp;

// Meta sends "from" as digits only, no "+" (e.g. "256700000001"). We
// standardize on a leading "+" everywhere DukaFlow stores/compares phone
// numbers, so a customer created from a Swagger-driven cart ("+256700...")
// still matches the same person messaging in from WhatsApp.
public static class PhoneNumberNormalizer
{
    public static string Normalize(string rawPhoneNumber)
    {
        var digitsOnly = Regex.Replace(rawPhoneNumber, @"[^\d]", "");
        return digitsOnly.Length == 0 ? rawPhoneNumber : $"+{digitsOnly}";
    }
}
