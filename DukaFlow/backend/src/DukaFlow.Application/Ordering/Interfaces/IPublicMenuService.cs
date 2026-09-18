using DukaFlow.Application.Ordering.DTOs;

namespace DukaFlow.Application.Ordering.Interfaces;

public interface IPublicMenuService
{
    Task<PublicMenuResponse> GetMenuAsync(Guid restaurantId, CancellationToken ct);
}
