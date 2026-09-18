using DukaFlow.Application.Ordering.DTOs;

namespace DukaFlow.Application.Ordering.Interfaces;

public interface IOrderService
{
    Task<PagedResult<OrderSummaryResponse>> GetOrdersAsync(Guid restaurantId, OrderFilterQuery query, CancellationToken ct);

    Task<OrderResponse> GetOrderAsync(Guid restaurantId, Guid orderId, CancellationToken ct);

    // Validates the transition through OrderStatusTransitions before
    // applying it - never assigns the client's requested status directly.
    Task<OrderResponse> UpdateStatusAsync(Guid restaurantId, Guid orderId, UpdateOrderStatusRequest request, CancellationToken ct);
}
