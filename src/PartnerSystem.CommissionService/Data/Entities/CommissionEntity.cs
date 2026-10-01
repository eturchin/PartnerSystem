using PartnerSystem.Shared.Contracts.Enums;
using PartnerSystem.Shared.Entities;

namespace PartnerSystem.CommissionService.Data.Entities;

public class CommissionEntity : BaseEntity
{
    public long EventId { get; set; }
    public long BeneficiaryUserId { get; set; }
    public long SourceUserId { get; set; }
    public decimal Amount { get; set; }
    public int Level { get; set; }
    public CommissionSchema SchemaType { get; set; }
    public CommissionStatus Status { get; set; }
    public DateTime AccruedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}