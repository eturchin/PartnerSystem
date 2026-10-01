using PartnerSystem.Contracts.Dtos.Users;
using PartnerSystem.Shared.Contracts.Dtos.Users;
using Refit;
using TreeNodeDto = PartnerSystem.Contracts.Dtos.Users.TreeNodeDto;

namespace PartnerSystem.Contracts.Http;

/// <summary>
/// Refit contract for PartnerSystem.UserService.
/// </summary>
public interface IUsersApi
{
    [Post("/api/users")]
    Task<UserDetailsResponse> CreateAsync([Body] UserDto dto, CancellationToken cancellationToken);

    [Get("/api/users/{externalId}")]
    Task<UserDetailsResponse> GetAsync(long externalId, CancellationToken cancellationToken);

    [Post("/api/users/{externalId}/partner")]
    Task SetPartnerAsync(long externalId, [Body] PartnerLinkDto dto, CancellationToken cancellationToken);

    [Get("/api/users/{externalId}/chain-up")]
    Task<IReadOnlyList<long>> GetChainUpAsync(long externalId, CancellationToken cancellationToken);

    [Get("/api/users/{externalId}/tree-down")]
    Task<TreeNodeDto?> GetTreeDownAsync(long externalId, CancellationToken cancellationToken);
}