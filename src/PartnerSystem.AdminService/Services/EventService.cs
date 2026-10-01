using PartnerSystem.AdminService.Services.Interfaces;
using PartnerSystem.Contracts.Dtos.Events;
using PartnerSystem.Contracts.Http;
using PartnerSystem.Shared.Contracts.Dtos.Events;

namespace PartnerSystem.AdminService.Services;

internal sealed class EventService(
    ICommissionsApi commissionApi,
    IEventsApi eventApi
) : IEventService
{
    public Task<IReadOnlyList<EventDto>> GetUserEventsAsync(long userId, CancellationToken cancellationToken) =>
        eventApi.GetByUserAsync(userId, cancellationToken);

    public async Task<EventDetailDto?> GetEventDetailAsync(long eventId, CancellationToken cancellationToken)
    {
        var eventDto = await eventApi.GetAsync(eventId, cancellationToken);

        if (eventDto is null)
        {
            return null;
        }

        var commissions = await commissionApi.GetByEventAsync(eventId, cancellationToken);

        return new EventDetailDto(
            eventDto.EventId,
            eventDto.UserExternalId,
            eventDto.Profit,
            eventDto.OccurredAt,
            commissions.ToList());
    }
}