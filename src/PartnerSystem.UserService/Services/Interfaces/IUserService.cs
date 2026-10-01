using PartnerSystem.Contracts.Dtos.Users;
using PartnerSystem.Shared.Contracts.Dtos.Users;

namespace PartnerSystem.UserService.Services.Interfaces;

public interface IUserService
{
    Task<UserDetailsResponse> CreateAsync(UserDto dto, CancellationToken cancellationToken);
    Task<UserDetailsResponse?> GetAsync(long externalId, CancellationToken cancellationToken);
    Task SetPartnerAsync(long externalId, PartnerLinkDto dto, CancellationToken cancellationToken);
}
