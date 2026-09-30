namespace DukaFlow.Application.WhatsApp.Interfaces;

public interface IConversationService
{
    // customerPhoneNumber should already be normalized by the caller.
    // Never puts business logic in the webhook controller - see Phase 4
    // CLAUDE.md #2.
    Task HandleInboundTextMessageAsync(
        Guid restaurantId,
        string customerPhoneNumber,
        string externalMessageId,
        string messageText,
        CancellationToken ct);
}
