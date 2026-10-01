using System.ComponentModel.DataAnnotations;
using PartnerSystem.Shared.Contracts.Enums;

namespace PartnerSystem.CommissionService.Options;

public class CommissionSchemaOptions
{
    /// <summary>
    /// Schema used when the database has no persisted value yet.
    /// </summary>
    [EnumDataType(typeof(CommissionSchema))]
    public CommissionSchema Default { get; set; } = CommissionSchema.Linear;
}
