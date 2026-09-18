using DukaFlow.Application.Ordering.Interfaces;
using DukaFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DukaFlow.Infrastructure.Ordering;

// MVP-level implementation: restaurant-scoped sequential counter, e.g.
// "DF-A3F2-00001". Under concurrent checkouts for the same restaurant
// there's a small race window; the unique index on
// (RestaurantId, OrderNumber) turns that into a rare, retryable
// DbUpdateException rather than silent data corruption. Full distributed
// sequence generation is not justified yet - see Phase 3 CLAUDE.md #19
// ("do not over-engineer distributed locking").
public class OrderNumberGenerator : IOrderNumberGenerator
{
    private readonly DukaFlowDbContext _db;

    public OrderNumberGenerator(DukaFlowDbContext db) => _db = db;

    public async Task<string> GenerateAsync(Guid restaurantId, CancellationToken ct)
    {
        var existingCount = await _db.Orders
            .Where(o => o.RestaurantId == restaurantId)
            .CountAsync(ct);

        var shortCode = restaurantId.ToString("N")[..4].ToUpperInvariant();
        var sequence = (existingCount + 1).ToString("D5");

        return $"DF-{shortCode}-{sequence}";
    }
}
