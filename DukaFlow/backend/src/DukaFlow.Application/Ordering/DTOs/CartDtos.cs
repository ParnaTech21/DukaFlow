using DukaFlow.Domain.Enums;

namespace DukaFlow.Application.Ordering.DTOs;

public record CreateCartRequest(
    string CustomerName,
    string? CustomerPhoneNumber,
    string? CustomerWhatsAppNumber);

public record AddCartItemRequest(
    Guid MenuItemId,
    int Quantity);

public record UpdateCartItemRequest(int Quantity);

public record CheckoutRequest(
    FulfillmentType FulfillmentType,
    string CustomerName,
    string? CustomerPhoneNumber,
    string? DeliveryAddress,
    string? CustomerNotes);

public record CartItemResponse(
    Guid Id,
    Guid MenuItemId,
    string MenuItemName,
    int Quantity,
    decimal UnitPrice)
{
    public decimal LineTotal => Quantity * UnitPrice;
}

public record CartResponse(
    Guid Id,
    Guid CustomerId,
    string Status,
    List<CartItemResponse> Items)
{
    public decimal Subtotal => Items.Sum(i => i.LineTotal);
}
