namespace DukaFlow.Application.WhatsApp.Interfaces;

// Deliberately narrow - only a text send for the Phase 4 MVP. Interactive
// buttons/lists and templates can be added here later without touching
// ConversationService's call sites. See Phase 4 CLAUDE.md #16.
public interface IWhatsAppMessageSender
{
    Task SendTextAsync(Guid restaurantId, string toPhoneNumber, string text, CancellationToken ct);
}
