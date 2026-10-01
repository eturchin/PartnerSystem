using Microsoft.AspNetCore.Mvc;
using PartnerSystem.AdminService.Services.Interfaces;
using PartnerSystem.Contracts.Dtos.Commissions;
using PartnerSystem.Contracts.Dtos.Events;
using PartnerSystem.Shared.Contracts.Dtos.Events;

namespace PartnerSystem.AdminService.Controllers;

/// <summary>
/// Administrative endpoints: schema switching and read-only access
/// to events and commissions across services.
/// </summary>
[ApiController]
[Route("api/admin")]
[Produces("application/json")]
public sealed class AdminController(ISchemaService schemaService, IEventService eventService) : ControllerBase
{
    /// <summary>Switches the active commission schema.</summary>
    /// <remarks>Affects only new calculations; existing commissions are not recalculated.</remarks>
    [HttpPut("schema")]
    [ProducesResponseType(typeof(SchemaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SwitchSchema(
        [FromBody] SchemaSwitchDto dto,
        CancellationToken cancellationToken)
    {
        var applied = await schemaService.SwitchSchemaAsync(dto.Schema, cancellationToken);
        
        return Ok(applied);
    }

    /// <summary>Returns the currently active commission schema.</summary>
    [HttpGet("schema")]
    [ProducesResponseType(typeof(SchemaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSchema(CancellationToken cancellationToken)
    {
        var schema = await schemaService.GetSchemaAsync(cancellationToken);
        
        return Ok(schema);
    }

    /// <summary>Returns all events of the given user without commission details.</summary>
    [HttpGet("users/{userId:long}/events")]
    [ProducesResponseType(typeof(IReadOnlyList<EventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserEvents(long userId, CancellationToken cancellationToken)
    {
        var events = await eventService.GetUserEventsAsync(userId, cancellationToken);
        
        return Ok(events);
    }

    /// <summary>Returns a single event with all commissions accrued for it.</summary>
    [HttpGet("events/{eventId:long}")]
    [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEventDetail(long eventId, CancellationToken cancellationToken)
    {
        var detail = await eventService.GetEventDetailAsync(eventId, cancellationToken);
        
        return detail is null ? NotFound() : Ok(detail);
    }
}