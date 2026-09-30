using DukaFlow.Application.WhatsApp.DTOs;

namespace DukaFlow.Application.WhatsApp.Interfaces;

public interface IWhatsAppConfigurationService
{
    Task<WhatsAppConfigurationResponse?> GetMineAsync(Guid restaurantId, CancellationToken ct);

    Task<WhatsAppConfigurationResponse> UpsertMineAsync(Guid restaurantId, UpdateWhatsAppConfigurationRequest request, CancellationToken ct);
}
