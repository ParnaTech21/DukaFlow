namespace DukaFlow.Domain.Enums;

// Whether a human needs to step in. See Phase 4 CLAUDE.md #21.
public enum ConversationStatus
{
    Automated = 0,
    NeedsHuman = 1,
    Resolved = 2
}

// Numbered-menu conversation flow - see Phase 4 CLAUDE.md #10/#11.
// Kept explicit and switch-driven in ConversationService rather than a
// generic transition table, since (unlike order status) the next state
// here depends on parsed user input, not just the current state.
public enum ConversationState
{
    Idle = 0,
    BrowsingMenu = 1,
    SelectingItem = 2,
    SelectingQuantity = 3,
    ReviewingCart = 4,
    CollectingFulfillment = 5,
    CollectingAddress = 6,
    ConfirmingOrder = 7,
    OrderPlaced = 8
}

public enum MessageDirection
{
    Inbound = 0,
    Outbound = 1
}
