namespace DukaFlow.Application.Ordering.Interfaces;

public interface IOrderNumberGenerator
{
    // Produces a restaurant-scoped, human-friendly identifier such as
    // "DF-A3F2-00001". Must never be used for authorization - the order's
    // Guid Id remains the trusted key. See Phase 3 CLAUDE.md #11.
    Task<string> GenerateAsync(Guid restaurantId, CancellationToken ct);
}
