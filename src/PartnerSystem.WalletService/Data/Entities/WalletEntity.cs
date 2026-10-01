using PartnerSystem.Shared.Entities;

namespace PartnerSystem.WalletService.Data.Entities;

public class WalletEntity : BaseEntity
{
    public long UserExternalId { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}