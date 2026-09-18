using DukaFlow.Domain.Enums;

namespace DukaFlow.Domain.Ordering;

// Single source of truth for which order-status transitions are legal.
// Neither the frontend nor a controller should ever assign OrderStatus
// directly - everything routes through CanTransition. See Phase 3
// CLAUDE.md #13.
public static class OrderStatusTransitions
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> Allowed = new()
    {
        [OrderStatus.Pending] = new[] { OrderStatus.Confirmed, OrderStatus.Rejected, OrderStatus.Cancelled },
        [OrderStatus.Confirmed] = new[] { OrderStatus.Preparing, OrderStatus.Cancelled },
        [OrderStatus.Preparing] = new[] { OrderStatus.Ready, OrderStatus.Cancelled },
        [OrderStatus.Ready] = new[] { OrderStatus.Completed },
        [OrderStatus.Completed] = Array.Empty<OrderStatus>(),
        [OrderStatus.Cancelled] = Array.Empty<OrderStatus>(),
        [OrderStatus.Rejected] = Array.Empty<OrderStatus>()
    };

    public static bool CanTransition(OrderStatus from, OrderStatus to)
    {
        return Allowed.TryGetValue(from, out var next) && next.Contains(to);
    }

    public static IReadOnlyCollection<OrderStatus> GetAllowedNextStates(OrderStatus from)
    {
        return Allowed.TryGetValue(from, out var next) ? next : Array.Empty<OrderStatus>();
    }
}
