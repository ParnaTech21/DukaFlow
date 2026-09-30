using DukaFlow.Domain.Common;

namespace DukaFlow.Domain.Entities;

// Deliberately holds no access token - that stays in application
// configuration (WhatsApp:AccessToken), shared across the pilot's Meta
// app for now. See Phase 4 CLAUDE.md #15.
public class WhatsAppBusinessConfiguration : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public string PhoneNumberId { get; set; } = string.Empty;
    public string? BusinessAccountId { get; set; }
    public string? DisplayPhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;

    public Restaurant Restaurant { get; set; } = null!;
}
