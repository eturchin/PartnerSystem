using PartnerSystem.Shared.Contracts.Enums;
using PartnerSystem.Shared.Entities;

namespace PartnerSystem.CommissionService.Data.Entities;

public class CommissionSchemaEntity : BaseEntity
{
    /// <summary>Well-known primary key. Always <c>1</c>.</summary>
    public const int SingletonId = 1;

    public new int Id { get; init; } = SingletonId;
    public CommissionSchema Schema { get; set; }
    public DateTime UpdatedAt { get; set; }
}
