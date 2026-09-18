using System.ComponentModel.DataAnnotations;
using DukaFlow.Application.Common;
using DukaFlow.Application.Ordering.DTOs;
using DukaFlow.Application.Ordering.Interfaces;
using DukaFlow.Domain.Entities;
using DukaFlow.Domain.Enums;
using DukaFlow.Domain.Ordering;
using DukaFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DukaFlow.Infrastructure.Ordering;

public class OrderService : IOrderService
{
    private readonly DukaFlowDbContext _db;

    public OrderService(DukaFlowDbContext db) => _db = db;

    public async Task<PagedResult<OrderSummaryResponse>> GetOrdersAsync(Guid restaurantId, OrderFilterQuery query, CancellationToken ct)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var ordersQuery = _db.Orders.AsNoTracking().Where(o => o.RestaurantId == restaurantId);

        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<OrderStatus>(query.Status, true, out var status))
            ordersQuery = ordersQuery.Where(o => o.Status == status);

        if (query.FromDate.HasValue)
            ordersQuery = ordersQuery.Where(o => o.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            ordersQuery = ordersQuery.Where(o => o.CreatedAt <= query.ToDate.Value);

        var totalCount = await ordersQuery.CountAsync(ct);

        var items = await ordersQuery
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderSummaryResponse(
                o.Id, o.OrderNumber, o.Status.ToString(), o.TotalAmount, o.CustomerName, o.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<OrderSummaryResponse>(items, page, pageSize, totalCount);
    }

    public async Task<OrderResponse> GetOrderAsync(Guid restaurantId, Guid orderId, CancellationToken ct)
    {
        var order = await GetOwnedOrderAsync(restaurantId, orderId, ct);
        return MapOrder(order);
    }

    public async Task<OrderResponse> UpdateStatusAsync(Guid restaurantId, Guid orderId, UpdateOrderStatusRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var requestedStatus))
            throw new ValidationException($"'{request.Status}' is not a recognized order status.");

        var order = await GetOwnedOrderAsync(restaurantId, orderId, ct);

        if (!OrderStatusTransitions.CanTransition(order.Status, requestedStatus))
        {
            throw new ConflictException(
                $"Cannot move order from {order.Status} to {requestedStatus}. " +
                $"Allowed next states: {string.Join(", ", OrderStatusTransitions.GetAllowedNextStates(order.Status))}.");
        }

        order.Status = requestedStatus;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        return MapOrder(order);
    }

    private async Task<Order> GetOwnedOrderAsync(Guid restaurantId, Guid orderId, CancellationToken ct)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.RestaurantId == restaurantId, ct);

        // Restaurant A must never learn that an order ID belongs to
        // Restaurant B - a 404 either way. See Phase 3 CLAUDE.md #22
        // tenant isolation test.
        return order ?? throw new NotFoundException("Order not found.");
    }

    private static OrderResponse MapOrder(Order order) => new(
        order.Id,
        order.OrderNumber,
        order.Status.ToString(),
        order.FulfillmentType.ToString(),
        order.Subtotal,
        order.DeliveryFee,
        order.DiscountAmount,
        order.TotalAmount,
        order.PaymentStatus.ToString(),
        order.CustomerName,
        order.CustomerPhoneNumber,
        order.DeliveryAddress,
        order.CustomerNotes,
        order.CreatedAt,
        order.PlacedAt,
        order.Items.Select(i => new OrderItemResponse(
            i.Id, i.MenuItemId, i.ItemName, i.Quantity, i.UnitPrice, i.LineTotal)).ToList());
}
