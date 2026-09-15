using DukaFlow.Application.Restaurants.DTOs;

namespace DukaFlow.Application.Restaurants.Interfaces;

public interface IRestaurantService
{
    Task<RestaurantResponse> GetMyRestaurantAsync(Guid ownerId, CancellationToken ct);
    Task<RestaurantResponse> UpdateMyRestaurantAsync(Guid ownerId, UpdateRestaurantRequest request, CancellationToken ct);
}
