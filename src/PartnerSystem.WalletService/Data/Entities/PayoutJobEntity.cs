using PartnerSystem.Shared.Entities;

namespace PartnerSystem.WalletService.Data.Entities;

public class PayoutJobEntity : BaseEntity
{
    public long UserExternalId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
}