using DukaFlow.Application.Common;
using DukaFlow.Application.Restaurants.DTOs;
using DukaFlow.Application.Restaurants.Interfaces;
using DukaFlow.Domain.Entities;
using DukaFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DukaFlow.Infrastructure.Restaurants;

public class RestaurantService : IRestaurantService
{
    private readonly DukaFlowDbContext _db;
    public RestaurantService(DukaFlowDbContext db) => _db = db;

    public async Task<RestaurantResponse> GetMyRestaurantAsync(Guid ownerId, CancellationToken ct)
    {
        var restaurant = await _db.Restaurants.AsNoTracking()
            .SingleOrDefaultAsync(r => r.OwnerId == ownerId, ct)
            ?? throw new NotFoundException("Restaurant not found.");

        return Map(restaurant);
    }

    public async Task<RestaurantResponse> UpdateMyRestaurantAsync(Guid ownerId, UpdateRestaurantRequest request, CancellationToken ct)
    {
        var restaurant = await _db.Restaurants
            .SingleOrDefaultAsync(r => r.OwnerId == ownerId, ct)
            ?? throw new NotFoundException("Restaurant not found.");

        restaurant.Name = request.Name.Trim();
        restaurant.Description = request.Description;
        restaurant.PhoneNumber = request.PhoneNumber;
        restaurant.WhatsAppNumber = request.WhatsAppNumber;
        restaurant.Address = request.Address;
        restaurant.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Map(restaurant);
    }

    private static RestaurantResponse Map(Restaurant r) =>
        new(r.Id, r.Name, r.Description, r.PhoneNumber, r.WhatsAppNumber, r.Address, r.IsActive);
}
