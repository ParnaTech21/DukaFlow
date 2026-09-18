using System.ComponentModel.DataAnnotations;
using DukaFlow.Application.Common;
using DukaFlow.Application.Ordering.DTOs;
using DukaFlow.Application.Ordering.Interfaces;
using DukaFlow.Domain.Entities;
using DukaFlow.Domain.Enums;
using DukaFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DukaFlow.Infrastructure.Ordering;

public class CartService : ICartService
{
    private readonly DukaFlowDbContext _db;
    private readonly IOrderNumberGenerator _orderNumberGenerator;

    public CartService(DukaFlowDbContext db, IOrderNumberGenerator orderNumberGenerator)
    {
        _db = db;
        _orderNumberGenerator = orderNumberGenerator;
    }

    public async Task<CartResponse> CreateCartAsync(Guid restaurantId, CreateCartRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            throw new ValidationException("Customer name is required.");

        // Find-or-create the Customer for this restaurant. Matching by
        // phone number keeps repeat WhatsApp customers as one record -
        // see Phase 3 CLAUDE.md #14.
        Customer? customer = null;
        if (!string.IsNullOrWhiteSpace(request.CustomerPhoneNumber))
        {
            customer = await _db.Customers.FirstOrDefaultAsync(
                c => c.RestaurantId == restaurantId && c.PhoneNumber == request.CustomerPhoneNumber, ct);
        }

        if (customer is null)
        {
            customer = new Customer
            {
                Id = Guid.NewGuid(),
                RestaurantId = restaurantId,
                Name = request.CustomerName.Trim(),
                PhoneNumber = request.CustomerPhoneNumber,
                WhatsAppNumber = request.CustomerWhatsAppNumber,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.Customers.Add(customer);
        }

        // Only one active cart per customer/restaurant - see Phase 3
        // CLAUDE.md #6.
        var existingActiveCart = await _db.Carts
            .Include(c => c.Items).ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(
                c => c.RestaurantId == restaurantId && c.CustomerId == customer.Id && c.Status == CartStatus.Active, ct);

        if (existingActiveCart is not null)
            return ToDto(existingActiveCart);

        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            CustomerId = customer.Id,
            Status = CartStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            Items = new List<CartItem>()
        };
        _db.Carts.Add(cart);

        await _db.SaveChangesAsync(ct);

        return ToDto(cart);
    }

    public async Task<CartResponse> GetCartAsync(Guid restaurantId, Guid cartId, CancellationToken ct)
    {
        var cart = await GetOwnedCartAsync(restaurantId, cartId, ct);
        return ToDto(cart);
    }

    public async Task<CartResponse> AddItemAsync(Guid restaurantId, Guid cartId, AddCartItemRequest request, CancellationToken ct)
    {
        if (request.Quantity < 1)
            throw new ValidationException("Quantity must be at least 1.");

        var cart = await GetOwnedCartAsync(restaurantId, cartId, ct);
        EnsureCartIsActive(cart);

        var menuItem = await _db.MenuItems.FirstOrDefaultAsync(
            m => m.Id == request.MenuItemId && m.RestaurantId == restaurantId, ct)
            ?? throw new NotFoundException("Menu item not found.");

        if (!menuItem.IsActive || !menuItem.IsAvailable)
            throw new ValidationException("Menu item is not currently available.");

        var existingLine = cart.Items.FirstOrDefault(i => i.MenuItemId == menuItem.Id);
        if (existingLine is not null)
        {
            existingLine.Quantity += request.Quantity;
            existingLine.UnitPrice = menuItem.Price;
            existingLine.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            _db.CartItems.Add(new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                MenuItemId = menuItem.Id,
                Quantity = request.Quantity,
                UnitPrice = menuItem.Price,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await _db.SaveChangesAsync(ct);

        var refreshed = await GetOwnedCartAsync(restaurantId, cartId, ct);
        return ToDto(refreshed);
    }

    public async Task<CartResponse> UpdateItemQuantityAsync(Guid restaurantId, Guid cartId, Guid cartItemId, UpdateCartItemRequest request, CancellationToken ct)
    {
        if (request.Quantity < 1)
            throw new ValidationException("Quantity must be at least 1. Use the remove endpoint to delete a line.");

        var cart = await GetOwnedCartAsync(restaurantId, cartId, ct);
        EnsureCartIsActive(cart);

        var line = cart.Items.FirstOrDefault(i => i.Id == cartItemId)
            ?? throw new NotFoundException("Cart item not found.");

        // Refresh price in case it changed since the item was added - the
        // cart should reflect current pricing while still active.
        var menuItem = await _db.MenuItems.FirstOrDefaultAsync(
            m => m.Id == line.MenuItemId && m.RestaurantId == restaurantId, ct);

        if (menuItem is null || !menuItem.IsActive || !menuItem.IsAvailable)
            throw new ValidationException("Menu item is no longer available; remove it from the cart.");

        line.Quantity = request.Quantity;
        line.UnitPrice = menuItem.Price;
        line.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        var refreshed = await GetOwnedCartAsync(restaurantId, cartId, ct);
        return ToDto(refreshed);
    }

    public async Task<CartResponse> RemoveItemAsync(Guid restaurantId, Guid cartId, Guid cartItemId, CancellationToken ct)
    {
        var cart = await GetOwnedCartAsync(restaurantId, cartId, ct);
        EnsureCartIsActive(cart);

        var line = cart.Items.FirstOrDefault(i => i.Id == cartItemId)
            ?? throw new NotFoundException("Cart item not found.");

        _db.CartItems.Remove(line);
        await _db.SaveChangesAsync(ct);

        var refreshed = await GetOwnedCartAsync(restaurantId, cartId, ct);
        return ToDto(refreshed);
    }

    public async Task<OrderResponse> CheckoutAsync(Guid restaurantId, Guid cartId, CheckoutRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            throw new ValidationException("Customer name is required.");

        if (request.FulfillmentType == FulfillmentType.Delivery && string.IsNullOrWhiteSpace(request.DeliveryAddress))
            throw new ValidationException("Delivery address is required for delivery orders.");

        // InMemory (used in some test projects) isn't a relational provider
        // and doesn't support transactions - only wrap in one against a
        // real relational database (SQL Server in production/dev).
        IDbContextTransaction? transaction = null;
        if (_db.Database.IsRelational())
        {
            transaction = await _db.Database.BeginTransactionAsync(ct);
        }

        var cart = await GetOwnedCartAsync(restaurantId, cartId, ct);
        EnsureCartIsActive(cart);

        if (cart.Items.Count == 0)
            throw new ValidationException("Cannot check out an empty cart.");

        var orderItems = new List<OrderItem>();
        decimal subtotal = 0m;

        // Re-validate every line against the live menu at checkout time -
        // never trust the cart's cached price alone. See Phase 3
        // CLAUDE.md #7 and #10.
        foreach (var line in cart.Items)
        {
            var menuItem = await _db.MenuItems.FirstOrDefaultAsync(
                m => m.Id == line.MenuItemId && m.RestaurantId == restaurantId, ct);

            if (menuItem is null || !menuItem.IsActive || !menuItem.IsAvailable)
                throw new ValidationException($"'{line.MenuItem?.Name ?? "An item"}' in this cart is no longer available. Remove it and try again.");

            var lineTotal = menuItem.Price * line.Quantity;
            subtotal += lineTotal;

            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                MenuItemId = menuItem.Id,
                ItemName = menuItem.Name,
                Quantity = line.Quantity,
                UnitPrice = menuItem.Price,
                LineTotal = lineTotal,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        const decimal deliveryFee = 0m; // No delivery-fee rules yet - configurable in a later phase.
        const decimal discountAmount = 0m; // No promotions/coupons yet - out of scope per CLAUDE.md.
        var totalAmount = subtotal + deliveryFee - discountAmount;

        var orderNumber = await _orderNumberGenerator.GenerateAsync(restaurantId, ct);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            CustomerId = cart.CustomerId,
            OrderNumber = orderNumber,
            Status = OrderStatus.Pending,
            FulfillmentType = request.FulfillmentType,
            Subtotal = subtotal,
            DeliveryFee = deliveryFee,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            PaymentStatus = PaymentStatus.Unpaid,
            CustomerName = request.CustomerName.Trim(),
            CustomerPhoneNumber = request.CustomerPhoneNumber,
            DeliveryAddress = request.DeliveryAddress,
            CustomerNotes = request.CustomerNotes,
            PlacedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            Items = orderItems
        };

        _db.Orders.Add(order);
        cart.Status = CartStatus.Converted;
        cart.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        if (transaction is not null)
        {
            await transaction.CommitAsync(ct);
            await transaction.DisposeAsync();
        }

        return MapOrder(order);
    }

    private async Task<Cart> GetOwnedCartAsync(Guid restaurantId, Guid cartId, CancellationToken ct)
    {
        var cart = await _db.Carts
            .Include(c => c.Items).ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(c => c.Id == cartId && c.RestaurantId == restaurantId, ct);

        // Treat "exists but belongs to another restaurant" identically to
        // "does not exist" - see Phase 3 CLAUDE.md #7.
        return cart ?? throw new NotFoundException("Cart not found.");
    }

    private static void EnsureCartIsActive(Cart cart)
    {
        if (cart.Status != CartStatus.Active)
            throw new ConflictException($"Cart is {cart.Status} and can no longer be modified.");
    }

    private static CartResponse ToDto(Cart cart) => new(
        cart.Id,
        cart.CustomerId,
        cart.Status.ToString(),
        cart.Items.Select(i => new CartItemResponse(
            i.Id, i.MenuItemId, i.MenuItem?.Name ?? string.Empty, i.Quantity, i.UnitPrice)).ToList());

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
