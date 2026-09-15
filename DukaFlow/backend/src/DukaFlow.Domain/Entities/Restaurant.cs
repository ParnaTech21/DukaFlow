using DukaFlow.Domain.Common;

namespace DukaFlow.Domain.Entities;

public class Restaurant : BaseEntity
{
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PhoneNumber { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}
