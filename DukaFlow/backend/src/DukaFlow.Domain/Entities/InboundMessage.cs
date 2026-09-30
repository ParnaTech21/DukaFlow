using DukaFlow.Domain.Common;

namespace DukaFlow.Domain.Entities;

// Meta may redeliver the same webhook event. Recording ExternalMessageId
// here (unique-indexed) before doing any processing turns a duplicate
// delivery into a no-op instead of a duplicate cart/order/outbound
// message. See Phase 4 CLAUDE.md #7.
public class InboundMessage : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public string ExternalMessageId { get; set; } = string.Empty;
    public string CustomerPhoneNumber { get; set; } = string.Empty;
    public string MessageType { get; set; } = "Text";
    public string ProcessingStatus { get; set; } = "Received";
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }
}
