using DukaFlow.Domain.Common;
using DukaFlow.Domain.Enums;

namespace DukaFlow.Domain.Entities;

public class Cart : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public Guid CustomerId { get; set; }
    public CartStatus Status { get; set; } = CartStatus.Active;
    public DateTimeOffset? ExpiresAt { get; set; }

    public Customer Customer { get; set; } = null!;
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
