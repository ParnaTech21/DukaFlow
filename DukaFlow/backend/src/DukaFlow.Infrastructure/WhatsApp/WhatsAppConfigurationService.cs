using System.ComponentModel.DataAnnotations;
using DukaFlow.Application.Common;
using DukaFlow.Application.WhatsApp.DTOs;
using DukaFlow.Application.WhatsApp.Interfaces;
using DukaFlow.Domain.Entities;
using DukaFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DukaFlow.Infrastructure.WhatsApp;

public class WhatsAppConfigurationService : IWhatsAppConfigurationService
{
    private readonly DukaFlowDbContext _db;

    public WhatsAppConfigurationService(DukaFlowDbContext db) => _db = db;

    public async Task<WhatsAppConfigurationResponse?> GetMineAsync(Guid restaurantId, CancellationToken ct)
    {
        var config = await _db.WhatsAppBusinessConfigurations.AsNoTracking()
            .FirstOrDefaultAsync(c => c.RestaurantId == restaurantId, ct);

        return config is null ? null : ToDto(config);
    }

    public async Task<WhatsAppConfigurationResponse> UpsertMineAsync(Guid restaurantId, UpdateWhatsAppConfigurationRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumberId))
            throw new ValidationException("Phone number ID is required - copy it from your Meta App Dashboard.");

        // A given Meta phone_number_id must map to exactly one restaurant -
        // that mapping is how the webhook resolves tenant. See Phase 4
        // CLAUDE.md #15.
        var ownedByAnother = await _db.WhatsAppBusinessConfigurations.AnyAsync(
            c => c.PhoneNumberId == request.PhoneNumberId && c.RestaurantId != restaurantId, ct);
        if (ownedByAnother)
            throw new ConflictException("That phone number ID is already registered to a different restaurant.");

        var config = await _db.WhatsAppBusinessConfigurations
            .FirstOrDefaultAsync(c => c.RestaurantId == restaurantId, ct);

        if (config is null)
        {
            config = new WhatsAppBusinessConfiguration
            {
                Id = Guid.NewGuid(),
                RestaurantId = restaurantId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.WhatsAppBusinessConfigurations.Add(config);
        }

        config.PhoneNumberId = request.PhoneNumberId.Trim();
        config.BusinessAccountId = request.BusinessAccountId;
        config.DisplayPhoneNumber = request.DisplayPhoneNumber;
        config.IsActive = request.IsActive;
        config.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        return ToDto(config);
    }

    private static WhatsAppConfigurationResponse ToDto(WhatsAppBusinessConfiguration c) => new(
        c.Id, c.PhoneNumberId, c.BusinessAccountId, c.DisplayPhoneNumber, c.IsActive);
}
