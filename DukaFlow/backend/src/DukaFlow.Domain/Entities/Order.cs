using DukaFlow.Domain.Common;
using DukaFlow.Domain.Enums;

namespace DukaFlow.Domain.Entities;

public class Order : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public Guid CustomerId { get; set; }

    // Human-friendly identifier (e.g. "DF-A3F2-00001"). Never used for
    // authorization - the Guid Id remains the trusted internal key.
    public string OrderNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public FulfillmentType FulfillmentType { get; set; } = FulfillmentType.Pickup;

    // All monetary fields are server-calculated at checkout time. Never
    // trust equivalent values submitted by a client.
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    // Snapshotted contact/delivery details at order time - independent of
    // whatever the Customer record looks like later.
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhoneNumber { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? CustomerNotes { get; set; }

    public DateTimeOffset? PlacedAt { get; set; }

    public Customer Customer { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
