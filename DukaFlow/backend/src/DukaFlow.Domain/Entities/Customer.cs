using DukaFlow.Domain.Common;

namespace DukaFlow.Domain.Entities;

// A customer is scoped to a single restaurant for the Phase 3 MVP.
// No cross-restaurant customer identity yet - see Phase 3 CLAUDE.md #5.
public class Customer : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? WhatsAppNumber { get; set; }

    public Restaurant Restaurant { get; set; } = null!;
    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
