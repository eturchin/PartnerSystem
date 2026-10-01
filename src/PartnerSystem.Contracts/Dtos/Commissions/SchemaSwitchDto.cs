using System.ComponentModel.DataAnnotations;
using PartnerSystem.Shared.Contracts.Enums;

namespace PartnerSystem.Contracts.Dtos.Commissions;

public sealed record SchemaSwitchDto([EnumDataType(typeof(CommissionSchema))] CommissionSchema Schema);