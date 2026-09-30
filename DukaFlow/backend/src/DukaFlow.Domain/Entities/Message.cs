using DukaFlow.Domain.Common;
using DukaFlow.Domain.Enums;

namespace DukaFlow.Domain.Entities;

public class Message : BaseEntity
{
    public Guid ConversationId { get; set; }
    public MessageDirection Direction { get; set; }
    public string Channel { get; set; } = "WhatsApp";
    public string? ExternalMessageId { get; set; }

    // Kept simple ("Text") for the MVP - see Phase 4 CLAUDE.md #9.
    // Interactive/template message types can be added when actually used.
    public string MessageType { get; set; } = "Text";

    // Truncated defensively at the DB layer (Configuration) - don't store
    // unbounded raw content. See Phase 4 CLAUDE.md #19 on PII caution.
    public string Content { get; set; } = string.Empty;

    public string Status { get; set; } = "Received";
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
}
