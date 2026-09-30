using DukaFlow.Domain.Common;
using DukaFlow.Domain.Enums;

namespace DukaFlow.Domain.Entities;

public class Conversation : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public Guid CustomerId { get; set; }

    // "WhatsApp" today; kept as a plain string so future channels
    // (web chat, Telegram, etc.) don't require a schema change. See
    // Phase 4 CLAUDE.md #8.
    public string Channel { get; set; } = "WhatsApp";
    public string? ExternalConversationReference { get; set; }

    public ConversationStatus Status { get; set; } = ConversationStatus.Automated;
    public ConversationState CurrentState { get; set; } = ConversationState.Idle;

    // Minimal working memory for the numbered-menu flow - which cart,
    // category, and item the customer is currently mid-selection on.
    // Cleared once an order is placed or the flow resets to Idle.
    public Guid? ActiveCartId { get; set; }
    public Guid? PendingCategoryId { get; set; }
    public Guid? PendingMenuItemId { get; set; }

    // Set while CollectingFulfillment/CollectingAddress/ConfirmingOrder,
    // consumed at checkout, cleared once the order is placed or the flow
    // is cancelled. Kept as real columns (not stashed in a Message row)
    // so the Messages table stays a genuine, faithful conversation log -
    // nothing here should ever need explaining to someone reading it back
    // in a future "Conversations" dashboard view (Phase 6 CLAUDE.md #20).
    public FulfillmentType? PendingFulfillmentType { get; set; }
    public string? PendingDeliveryAddress { get; set; }

    public DateTimeOffset LastMessageAt { get; set; } = DateTimeOffset.UtcNow;

    public Customer Customer { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
