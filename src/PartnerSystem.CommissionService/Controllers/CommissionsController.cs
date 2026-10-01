using Microsoft.AspNetCore.Mvc;
using PartnerSystem.CommissionService.Services.Interfaces;
using PartnerSystem.Contracts.Dtos.Commissions;

namespace PartnerSystem.CommissionService.Controllers;

/// <summary>
/// Manages partner commission accrual and history.
/// </summary>
/// <remarks>
/// Commissions are accrued automatically when a profit event is consumed from Kafka.
/// This controller only exposes read access to the accrued data and the active schema.
/// </remarks>
[ApiController]
[Route("api/commissions")]
public class CommissionsController(
    ICommissionService commissionService,
    ICommissionSchemaService schema
) : ControllerBase
{
    /// <summary>
    /// Returns all commissions accrued for the given event.
    /// </summary>
    /// <param name="eventId">Identifier of the source profit event.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">List of commissions (possibly empty).</response>
    [HttpGet("by-event/{eventId:long}")]
    [ProducesResponseType(typeof(IReadOnlyList<CommissionDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEvent(long eventId, CancellationToken cancellationToken)
    {
        var list = await commissionService.GetByEventAsync(eventId, cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Returns the commission accrual history of the given user, newest first.
    /// </summary>
    /// <param name="userId">External identifier of the beneficiary user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">List of commissions (possibly empty).</response>
    [HttpGet("by-user/{userId:long}")]
    [ProducesResponseType(typeof(IReadOnlyList<CommissionDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUser(long userId, CancellationToken cancellationToken)
    {
        var list = await commissionService.GetByUserAsync(userId, cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Returns the currently active commission calculation schema.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The active schema (Linear or Fibonacci).</response>
    [HttpGet("schema")]
    [ProducesResponseType(typeof(SchemaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSchema(CancellationToken cancellationToken)
    {
        var schema1 = await schema.GetCurrentAsync(cancellationToken);
        return Ok(new SchemaResponse(schema1.ToString()));
    }

    
    /// <summary>
    /// Switches the active commission calculation schema.
    /// </summary>
    /// <param name="dto">Requested schema.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Affects only new calculations. Existing commissions keep the schema
    /// they were accrued with; nothing is recalculated.
    /// </remarks>
    /// <response code="200">The newly applied schema.</response>
    [HttpPut("schema")]
    [ProducesResponseType(typeof(SchemaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetSchema(
        [FromBody] SchemaSwitchDto dto, CancellationToken cancellationToken)
    {
        var applied = await schema.SetAsync(dto.Schema, cancellationToken);
        return Ok(new SchemaResponse(applied.ToString()));
    }

    /// <summary>
    /// Marks the given commissions as paid.
    /// </summary>
    /// <param name="commissionIds">Identifiers of commissions to mark as paid.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Called by WalletService during payouts. Idempotent: commissions that are
    /// already marked as paid are ignored. Returns the number of commissions
    /// actually transitioned to the <c>Paid</c> status.
    /// </remarks>
    /// <response code="200">Number of commissions updated.</response>
    [HttpPost("mark-paid")]
    [ProducesResponseType(typeof(MarkPaidResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkPaid(
        [FromBody] IReadOnlyCollection<long> commissionIds, CancellationToken cancellationToken)
    {
        var updated = await commissionService.MarkPaidAsync(commissionIds, cancellationToken);
        return Ok(new MarkPaidResponse(updated));
    }

    /// <summary>
    /// Returns unpaid (accrued) commissions of the given user.
    /// </summary>
    /// <param name="userId">External identifier of the beneficiary user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Used by WalletService to determine which commissions are due for payout.
    /// </remarks>
    /// <response code="200">List of unpaid commissions (possibly empty).</response>
    [HttpGet("unpaid/{userId:long}")]
    [ProducesResponseType(typeof(IReadOnlyList<UnpaidCommissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnpaid(long userId, CancellationToken cancellationToken)
    {
        var list = await commissionService.GetUnpaidAsync(userId, cancellationToken);
        return Ok(list);
    }
}