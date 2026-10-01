using PartnerSystem.Shared.Contracts.Dtos.Events;

namespace PartnerSystem.EventService.Services.Interfaces;

public interface IEventsService
{
    Task<bool> CreateAsync(EventDto dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<EventDto>> GetByUserAsync(long userExternalId, CancellationToken cancellationToken);
    Task<EventDto?> GetAsync(long eventId, CancellationToken cancellationToken);
}
