using System.ComponentModel.DataAnnotations;
using DukaFlow.Application.Ordering.DTOs;
using DukaFlow.Application.Ordering.Interfaces;
using DukaFlow.Application.WhatsApp.Interfaces;
using DukaFlow.Domain.Entities;
using DukaFlow.Domain.Enums;
using DukaFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DukaFlow.Infrastructure.WhatsApp;

// Owns the numbered-menu conversation flow. Deliberately thin on
// "intelligence" - structured numbered choices, not NLP - per Phase 4
// CLAUDE.md #11. All actual ordering logic (pricing, checkout,
// transactions) is delegated to the Phase 3 ICartService/IPublicMenuService;
// this class never touches Cart/Order rows directly.
public class ConversationService : IConversationService
{
    private readonly DukaFlowDbContext _db;
    private readonly ICartService _cartService;
    private readonly IPublicMenuService _publicMenuService;
    private readonly IWhatsAppMessageSender _sender;

    public ConversationService(
        DukaFlowDbContext db,
        ICartService cartService,
        IPublicMenuService publicMenuService,
        IWhatsAppMessageSender sender)
    {
        _db = db;
        _cartService = cartService;
        _publicMenuService = publicMenuService;
        _sender = sender;
    }

    public async Task HandleInboundTextMessageAsync(
        Guid restaurantId, string customerPhoneNumber, string externalMessageId, string messageText, CancellationToken ct)
    {
        var normalizedPhone = PhoneNumberNormalizer.Normalize(customerPhoneNumber);
        var trimmedText = messageText.Trim();

        var customer = await FindOrCreateCustomerAsync(restaurantId, normalizedPhone, ct);
        var conversation = await FindOrCreateConversationAsync(restaurantId, customer.Id, ct);

        RecordMessage(conversation.Id, MessageDirection.Inbound, trimmedText, externalMessageId);

        // Global shortcuts, honored from any state.
        var lower = trimmedText.ToLowerInvariant();
        if (lower is "menu" or "hi" or "hello" or "start")
        {
            await ResetToMenuAsync(restaurantId, customer.PhoneNumber!, conversation, ct);
            return;
        }

        if (lower == "cancel")
        {
            conversation.CurrentState = ConversationState.Idle;
            conversation.ActiveCartId = null;
            conversation.PendingCategoryId = null;
            conversation.PendingMenuItemId = null;
            await _db.SaveChangesAsync(ct);
            await ReplyAsync(restaurantId, normalizedPhone, conversation,
                "No problem - cancelled. Reply \"menu\" any time to start again.");
            return;
        }

        try
        {
            switch (conversation.CurrentState)
            {
                case ConversationState.Idle:
                    await ResetToMenuAsync(restaurantId, customer.PhoneNumber!, conversation, ct);
                    break;

                case ConversationState.BrowsingMenu:
                    await HandleCategorySelectionAsync(restaurantId, customer, conversation, trimmedText, ct);
                    break;

                case ConversationState.SelectingItem:
                    await HandleItemSelectionAsync(restaurantId, customer, conversation, trimmedText, ct);
                    break;

                case ConversationState.SelectingQuantity:
                    await HandleQuantityAsync(restaurantId, customer, conversation, trimmedText, ct);
                    break;

                case ConversationState.ReviewingCart:
                    await HandleCartMenuAsync(restaurantId, customer, conversation, trimmedText, ct);
                    break;

                case ConversationState.CollectingFulfillment:
                    await HandleFulfillmentChoiceAsync(restaurantId, customer, conversation, trimmedText, ct);
                    break;

                case ConversationState.CollectingAddress:
                    await HandleAddressAsync(restaurantId, customer, conversation, trimmedText, ct);
                    break;

                case ConversationState.ConfirmingOrder:
                    await HandleConfirmationAsync(restaurantId, customer, conversation, trimmedText, ct);
                    break;

                default:
                    await ResetToMenuAsync(restaurantId, customer.PhoneNumber!, conversation, ct);
                    break;
            }
        }
        catch (ValidationException ex)
        {
            // Menu item went unavailable, quantity malformed, etc. - tell
            // the customer plainly and leave them in a state they can
            // recover from, rather than surfacing a stack trace or
            // silently dropping the message.
            await ReplyAsync(restaurantId, normalizedPhone, conversation, ex.Message);
        }
    }

    // ---- State handlers ----

    private async Task ResetToMenuAsync(Guid restaurantId, string customerPhoneNumber, Conversation conversation, CancellationToken ct)
    {
        var menu = await _publicMenuService.GetMenuAsync(restaurantId, ct);

        if (menu.Categories.Count == 0)
        {
            conversation.CurrentState = ConversationState.Idle;
            await _db.SaveChangesAsync(ct);
            await ReplyAsync(restaurantId, customerPhoneNumber, conversation,
                $"Welcome to {menu.RestaurantName}! Our menu isn't set up yet - please check back soon.");
            return;
        }

        var lines = menu.Categories
            .Select((c, i) => $"{i + 1}. {c.Name}")
            .ToList();

        conversation.CurrentState = ConversationState.BrowsingMenu;
        conversation.PendingCategoryId = null;
        conversation.PendingMenuItemId = null;
        await _db.SaveChangesAsync(ct);

        await ReplyAsync(restaurantId, customerPhoneNumber, conversation,
            $"Welcome to {menu.RestaurantName}! What would you like?\n" + string.Join("\n", lines) +
            "\n\nReply with a number.");
    }

    private async Task HandleCategorySelectionAsync(Guid restaurantId, Customer customer, Conversation conversation, string text, CancellationToken ct)
    {
        var menu = await _publicMenuService.GetMenuAsync(restaurantId, ct);

        if (!TryParseIndex(text, menu.Categories.Count, out var index))
        {
            await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation,
                $"Please reply with a number from 1 to {menu.Categories.Count}, or \"menu\" to see the categories again.");
            return;
        }

        var category = menu.Categories[index];

        if (category.Items.Count == 0)
        {
            await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation,
                $"{category.Name} has no items available right now. Reply \"menu\" to pick another category.");
            return;
        }

        conversation.PendingCategoryId = category.Id;
        conversation.CurrentState = ConversationState.SelectingItem;
        await _db.SaveChangesAsync(ct);

        var lines = category.Items
            .Select((item, i) => $"{i + 1}. {item.Name} - UGX {item.Price:N0}")
            .ToList();

        await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation,
            $"{category.Name}:\n" + string.Join("\n", lines) + "\n\nReply with a number to choose an item.");
    }

    private async Task HandleItemSelectionAsync(Guid restaurantId, Customer customer, Conversation conversation, string text, CancellationToken ct)
    {
        var menu = await _publicMenuService.GetMenuAsync(restaurantId, ct);
        var category = menu.Categories.FirstOrDefault(c => c.Id == conversation.PendingCategoryId);

        if (category is null || !TryParseIndex(text, category.Items.Count, out var index))
        {
            await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation,
                "Sorry, I didn't catch that. Reply \"menu\" to start over.");
            return;
        }

        var item = category.Items[index];
        conversation.PendingMenuItemId = item.Id;
        conversation.CurrentState = ConversationState.SelectingQuantity;
        await _db.SaveChangesAsync(ct);

        await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation,
            $"How many \"{item.Name}\" would you like? Reply with a number.");
    }

    private async Task HandleQuantityAsync(Guid restaurantId, Customer customer, Conversation conversation, string text, CancellationToken ct)
    {
        if (!int.TryParse(text, out var quantity) || quantity < 1)
        {
            await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation,
                "Please reply with a quantity of 1 or more.");
            return;
        }

        var cart = await GetOrCreateActiveCartAsync(restaurantId, customer, conversation, ct);

        await _cartService.AddItemAsync(
            restaurantId, cart.Id, new AddCartItemRequest(conversation.PendingMenuItemId!.Value, quantity), ct);

        conversation.CurrentState = ConversationState.ReviewingCart;
        conversation.PendingMenuItemId = null;
        await _db.SaveChangesAsync(ct);

        await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation,
            "Added to your cart!\n1. Add another item\n2. View cart\n3. Checkout\n\nReply with a number.");
    }

    private async Task HandleCartMenuAsync(Guid restaurantId, Customer customer, Conversation conversation, string text, CancellationToken ct)
    {
        switch (text)
        {
            case "1":
                await ResetToMenuAsync(restaurantId, customer.PhoneNumber!, conversation, ct);
                return;

            case "2":
                await ShowCartAsync(restaurantId, customer, conversation, ct);
                return;

            case "3":
                conversation.CurrentState = ConversationState.CollectingFulfillment;
                await _db.SaveChangesAsync(ct);
                await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation,
                    "How would you like your order?\n1. Pickup\n2. Delivery\n\nReply with a number.");
                return;

            default:
                await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation,
                    "Reply 1 to add another item, 2 to view your cart, or 3 to checkout.");
                return;
        }
    }

    private async Task ShowCartAsync(Guid restaurantId, Customer customer, Conversation conversation, CancellationToken ct)
    {
        if (conversation.ActiveCartId is null)
        {
            await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation, "Your cart is empty. Reply \"menu\" to start browsing.");
            return;
        }

        var cart = await _cartService.GetCartAsync(restaurantId, conversation.ActiveCartId.Value, ct);
        if (cart.Items.Count == 0)
        {
            await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation, "Your cart is empty. Reply \"menu\" to start browsing.");
            return;
        }

        var lines = cart.Items.Select(i => $"{i.Quantity} x {i.MenuItemName} - UGX {i.LineTotal:N0}");
        await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation,
            "Your cart:\n" + string.Join("\n", lines) + $"\n\nSubtotal: UGX {cart.Subtotal:N0}" +
            "\n\n1. Add another item\n2. View cart\n3. Checkout");
    }

    private async Task HandleFulfillmentChoiceAsync(Guid restaurantId, Customer customer, Conversation conversation, string text, CancellationToken ct)
    {
        if (text == "1")
        {
            await MoveToConfirmationAsync(restaurantId, customer, conversation, FulfillmentType.Pickup, deliveryAddress: null, ct);
        }
        else if (text == "2")
        {
            conversation.CurrentState = ConversationState.CollectingAddress;
            await _db.SaveChangesAsync(ct);
            await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation, "What's the delivery address?");
        }
        else
        {
            await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation, "Reply 1 for Pickup or 2 for Delivery.");
        }
    }

    private async Task HandleAddressAsync(Guid restaurantId, Customer customer, Conversation conversation, string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation, "Please share a delivery address.");
            return;
        }

        await MoveToConfirmationAsync(restaurantId, customer, conversation, FulfillmentType.Delivery, text, ct);
    }

    private async Task MoveToConfirmationAsync(
        Guid restaurantId, Customer customer, Conversation conversation, FulfillmentType fulfillmentType, string? deliveryAddress, CancellationToken ct)
    {
        var cart = await _cartService.GetCartAsync(restaurantId, conversation.ActiveCartId!.Value, ct);

        conversation.CurrentState = ConversationState.ConfirmingOrder;
        conversation.PendingFulfillmentType = fulfillmentType;
        conversation.PendingDeliveryAddress = deliveryAddress;
        await _db.SaveChangesAsync(ct);

        var lines = cart.Items.Select(i => $"{i.Quantity} x {i.MenuItemName} - UGX {i.LineTotal:N0}");
        var summary = "Please confirm your order:\n" + string.Join("\n", lines) +
                      $"\n\nFulfillment: {fulfillmentType}" +
                      (deliveryAddress is null ? "" : $"\nAddress: {deliveryAddress}") +
                      $"\nTotal: UGX {cart.Subtotal:N0}" +
                      "\n\nReply YES to confirm, or \"cancel\" to start over.";

        await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation, summary);
    }

    private async Task HandleConfirmationAsync(Guid restaurantId, Customer customer, Conversation conversation, string text, CancellationToken ct)
    {
        var lower = text.Trim().ToLowerInvariant();
        if (lower is not ("yes" or "y" or "confirm"))
        {
            await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation, "Reply YES to confirm your order, or \"cancel\" to start over.");
            return;
        }

        var fulfillmentType = conversation.PendingFulfillmentType ?? FulfillmentType.Pickup;
        var deliveryAddress = conversation.PendingDeliveryAddress;

        var order = await _cartService.CheckoutAsync(
            restaurantId,
            conversation.ActiveCartId!.Value,
            new CheckoutRequest(fulfillmentType, customer.Name, customer.PhoneNumber, deliveryAddress, null),
            ct);

        conversation.CurrentState = ConversationState.Idle;
        conversation.ActiveCartId = null;
        conversation.PendingCategoryId = null;
        conversation.PendingMenuItemId = null;
        conversation.PendingFulfillmentType = null;
        conversation.PendingDeliveryAddress = null;
        await _db.SaveChangesAsync(ct);

        await ReplyAsync(restaurantId, customer.PhoneNumber!, conversation,
            $"Order confirmed! Your order number is {order.OrderNumber}. " +
            "We'll message you here with updates. Reply \"menu\" any time to order again.");
    }

    // ---- Helpers ----

    private async Task<Customer> FindOrCreateCustomerAsync(Guid restaurantId, string normalizedPhone, CancellationToken ct)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(
            c => c.RestaurantId == restaurantId && c.PhoneNumber == normalizedPhone, ct);

        if (customer is not null)
        {
            return customer;
        }

        customer = new Customer
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            Name = normalizedPhone, // Meta doesn't reliably give a display name on every message type; refined once the customer places an order or a name is captured elsewhere.
            PhoneNumber = normalizedPhone,
            WhatsAppNumber = normalizedPhone,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);
        return customer;
    }

    private async Task<Conversation> FindOrCreateConversationAsync(Guid restaurantId, Guid customerId, CancellationToken ct)
    {
        var conversation = await _db.Conversations.FirstOrDefaultAsync(
            c => c.RestaurantId == restaurantId && c.CustomerId == customerId, ct);

        if (conversation is not null)
        {
            conversation.LastMessageAt = DateTimeOffset.UtcNow;
            return conversation;
        }

        conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            CustomerId = customerId,
            Channel = "WhatsApp",
            Status = ConversationStatus.Automated,
            CurrentState = ConversationState.Idle,
            LastMessageAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Conversations.Add(conversation);
        await _db.SaveChangesAsync(ct);
        return conversation;
    }

    private async Task<CartResponse> GetOrCreateActiveCartAsync(Guid restaurantId, Customer customer, Conversation conversation, CancellationToken ct)
    {
        if (conversation.ActiveCartId is not null)
        {
            return await _cartService.GetCartAsync(restaurantId, conversation.ActiveCartId.Value, ct);
        }

        var cart = await _cartService.CreateCartAsync(
            restaurantId, new CreateCartRequest(customer.Name, customer.PhoneNumber, customer.WhatsAppNumber), ct);

        conversation.ActiveCartId = cart.Id;
        await _db.SaveChangesAsync(ct);
        return cart;
    }

    private void RecordMessage(Guid conversationId, MessageDirection direction, string content, string? externalMessageId)
    {
        _db.Messages.Add(new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Direction = direction,
            Channel = "WhatsApp",
            ExternalMessageId = externalMessageId,
            Content = content.Length > 1000 ? content[..1000] : content,
            Status = direction == MessageDirection.Inbound ? "Received" : "Sent",
            ReceivedAt = direction == MessageDirection.Inbound ? DateTimeOffset.UtcNow : null,
            SentAt = direction == MessageDirection.Outbound ? DateTimeOffset.UtcNow : null,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    private async Task ReplyAsync(Guid restaurantId, string toPhoneNumber, Conversation conversation, string text)
    {
        RecordMessage(conversation.Id, MessageDirection.Outbound, text, null);
        await _db.SaveChangesAsync();
        await _sender.SendTextAsync(restaurantId, toPhoneNumber, text, CancellationToken.None);
    }

    // Parses a 1-based number reply into a valid 0-based list index.
    private static bool TryParseIndex(string text, int count, out int index)
    {
        index = -1;
        if (!int.TryParse(text.Trim(), out var number) || number < 1 || number > count)
        {
            return false;
        }

        index = number - 1;
        return true;
    }
}
