using DukaFlow.Domain.Common;

namespace DukaFlow.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }

    // Snapshotted at checkout time so historical orders stay readable even
    // if the restaurant later renames, re-prices, or archives the item.
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public Order Order { get; set; } = null!;
}
