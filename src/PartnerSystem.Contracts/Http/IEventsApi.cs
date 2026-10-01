using PartnerSystem.Shared.Contracts.Dtos.Events;
using Refit;

namespace PartnerSystem.Contracts.Http;

/// <summary>
/// Refit contract for PartnerSystem.EventService.
/// </summary>
public interface IEventsApi
{
    [Post("/api/events")]
    Task CreateAsync([Body] EventDto dto, CancellationToken cancellationToken);

    [Get("/api/events/user/{externalId}")]
    Task<IReadOnlyList<EventDto>> GetByUserAsync(long externalId, CancellationToken cancellationToken);

    [Get("/api/events/{eventId}")]
    Task<EventDto?> GetAsync(long eventId, CancellationToken cancellationToken);
}