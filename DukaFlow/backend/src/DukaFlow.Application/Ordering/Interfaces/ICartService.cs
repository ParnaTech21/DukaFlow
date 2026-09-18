using DukaFlow.Application.Ordering.DTOs;

namespace DukaFlow.Application.Ordering.Interfaces;

public interface ICartService
{
    Task<CartResponse> CreateCartAsync(Guid restaurantId, CreateCartRequest request, CancellationToken ct);

    Task<CartResponse> GetCartAsync(Guid restaurantId, Guid cartId, CancellationToken ct);

    Task<CartResponse> AddItemAsync(Guid restaurantId, Guid cartId, AddCartItemRequest request, CancellationToken ct);

    Task<CartResponse> UpdateItemQuantityAsync(Guid restaurantId, Guid cartId, Guid cartItemId, UpdateCartItemRequest request, CancellationToken ct);

    Task<CartResponse> RemoveItemAsync(Guid restaurantId, Guid cartId, Guid cartItemId, CancellationToken ct);

    // Converts the cart into an Order. Recalculates every price server-side,
    // snapshots them into OrderItem, and marks the cart Converted - all in
    // one transaction. See Phase 3 CLAUDE.md #18.
    Task<OrderResponse> CheckoutAsync(Guid restaurantId, Guid cartId, CheckoutRequest request, CancellationToken ct);
}
