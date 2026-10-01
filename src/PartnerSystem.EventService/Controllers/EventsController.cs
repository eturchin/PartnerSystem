using Microsoft.AspNetCore.Mvc;
using PartnerSystem.EventService.Services.Interfaces;
using PartnerSystem.Shared.Contracts.Dtos.Events;

namespace PartnerSystem.EventService.Controllers;

/// <summary>
/// HTTP endpoints for accepting and querying profit/loss events.
/// </summary>
[ApiController]
[Route("api/events")]
public sealed class EventsController(IEventsService service) : ControllerBase
{
    /// <summary>
    /// Accepts a new profit/loss event. Idempotent by EventId.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] EventDto dto, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(dto, cancellationToken);
        
        return created
            ? Accepted()
            : Ok(new { status = "already_exists" });
    }

    /// <summary>
    /// Returns all events of the given user.
    /// </summary>
    [HttpGet("user/{externalId:long}")]
    [ProducesResponseType(typeof(IReadOnlyList<EventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUser(long externalId, CancellationToken cancellationToken)
    {
        var events = await service.GetByUserAsync(externalId, cancellationToken);
        
        return Ok(events);
    }

    /// <summary>
    /// Returns a single event by its EventId.
    /// </summary>
    [HttpGet("{eventId:long}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long eventId, CancellationToken cancellationToken)
    {
        var evt = await service.GetAsync(eventId, cancellationToken);
        
        return evt is null ? NotFound() : Ok(evt);
    }
}