namespace DukaFlow.Application.WhatsApp.DTOs;

// Deliberately excludes any access token - that's shared platform
// configuration (WhatsApp:AccessToken), not something a restaurant owner
// sets themselves. See WhatsAppBusinessConfiguration.cs.
public record WhatsAppConfigurationResponse(
    Guid Id,
    string PhoneNumberId,
    string? BusinessAccountId,
    string? DisplayPhoneNumber,
    bool IsActive);

public record UpdateWhatsAppConfigurationRequest(
    string PhoneNumberId,
    string? BusinessAccountId,
    string? DisplayPhoneNumber,
    bool IsActive);
