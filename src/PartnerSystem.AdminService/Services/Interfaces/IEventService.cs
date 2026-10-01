using PartnerSystem.Contracts.Dtos.Events;
using PartnerSystem.Shared.Contracts.Dtos.Events;

namespace PartnerSystem.AdminService.Services.Interfaces;

public interface IEventService
{
    Task<IReadOnlyList<EventDto>> GetUserEventsAsync(long userId, CancellationToken cancellationToken);
    Task<EventDetailDto?> GetEventDetailAsync(long eventId, CancellationToken cancellationToken);
}