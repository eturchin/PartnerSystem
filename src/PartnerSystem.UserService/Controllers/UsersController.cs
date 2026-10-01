using Microsoft.AspNetCore.Mvc;
using PartnerSystem.Shared.Contracts.Dtos.Users;
using PartnerSystem.UserService.Services.Interfaces;

namespace PartnerSystem.UserService.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(
    IUserService userService,
    ITreeService treeService
) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserDto dto, CancellationToken cancellationToken)
    {
        var created = await userService.CreateAsync(dto, cancellationToken);
        
        return CreatedAtAction(nameof(Get), new
        {
            externalId = created.ExternalId
        }, created);
    }

    [HttpGet("{externalId:long}")]
    public async Task<IActionResult> Get(long externalId, CancellationToken cancellationToken)
    {
        var user = await userService.GetAsync(externalId, cancellationToken);
        
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost("{externalId:long}/partner")]
    public async Task<IActionResult> SetPartner(
        long externalId, [FromBody] PartnerLinkDto dto, CancellationToken cancellationToken)
    {
        await userService.SetPartnerAsync(externalId, dto, cancellationToken);
        
        return Ok();
    }

    [HttpGet("{externalId:long}/chain-up")]
    public async Task<IActionResult> GetChainUp(long externalId, CancellationToken cancellationToken)
    {
        var chain = await treeService.GetPartnerChainUpAsync(externalId, cancellationToken);
        
        return Ok(chain);
    }

    [HttpGet("{externalId:long}/tree-down")]
    public async Task<IActionResult> GetTreeDown(long externalId, CancellationToken cancellationToken)
    {
        var descendants = await treeService.GetTreeDownAsync(externalId, cancellationToken);
        
        return Ok(descendants);
    }
}