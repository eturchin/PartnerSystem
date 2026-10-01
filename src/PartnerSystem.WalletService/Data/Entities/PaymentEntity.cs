using PartnerSystem.Shared.Entities;

namespace PartnerSystem.WalletService.Data.Entities;

public class PaymentEntity : BaseEntity
{
    public long CommissionId { get; set; }
    public long UserExternalId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
}