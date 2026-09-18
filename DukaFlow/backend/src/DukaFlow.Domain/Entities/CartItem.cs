using DukaFlow.Domain.Common;

namespace DukaFlow.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }

    // Price snapshot taken when the item was added/last refreshed.
    // Re-validated against the live MenuItem price at checkout time -
    // see CartService.CheckoutAsync and Phase 3 CLAUDE.md #7.
    public decimal UnitPrice { get; set; }

    public Cart? Cart { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
}
