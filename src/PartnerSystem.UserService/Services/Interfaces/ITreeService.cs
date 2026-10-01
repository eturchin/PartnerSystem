using PartnerSystem.Contracts.Dtos.Users;

namespace PartnerSystem.UserService.Services.Interfaces;

public interface ITreeService
{
    Task<List<long>> GetPartnerChainUpAsync(long userExternalId, CancellationToken cancellationToken);
    Task<TreeNodeDto?> GetTreeDownAsync(long userExternalId, CancellationToken cancellationToken);
    Task<bool> WouldCreateCycleAsync(long userExternalId, long partnerExternalId, CancellationToken cancellationToken);
}
