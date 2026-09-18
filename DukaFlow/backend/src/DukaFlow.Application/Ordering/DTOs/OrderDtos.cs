namespace DukaFlow.Application.Ordering.DTOs;

// Sent as the enum's string name (e.g. "Confirmed") to keep the Swagger
// contract readable. Parsed and validated in OrderService.
public record UpdateOrderStatusRequest(string Status);

public record OrderItemResponse(
    Guid Id,
    Guid MenuItemId,
    string ItemName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record OrderResponse(
    Guid Id,
    string OrderNumber,
    string Status,
    string FulfillmentType,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal DiscountAmount,
    decimal TotalAmount,
    string PaymentStatus,
    string CustomerName,
    string? CustomerPhoneNumber,
    string? DeliveryAddress,
    string? CustomerNotes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PlacedAt,
    List<OrderItemResponse> Items);

// Lighter shape for the orders list page - avoids shipping every line
// item for every row.
public record OrderSummaryResponse(
    Guid Id,
    string OrderNumber,
    string Status,
    decimal TotalAmount,
    string CustomerName,
    DateTimeOffset CreatedAt);

public class OrderFilterQuery
{
    public string? Status { get; set; }
    public DateTimeOffset? FromDate { get; set; }
    public DateTimeOffset? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public record PagedResult<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
