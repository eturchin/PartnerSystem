using System.ComponentModel.DataAnnotations;
using PartnerSystem.Shared.Contracts.Enums;

namespace PartnerSystem.Contracts.Dtos.Commissions;

public sealed record CommissionDetailDto(
    long CommissionId,
    long BeneficiaryUserId,
    decimal Amount,
    int Level,
    [EnumDataType(typeof(CommissionSchema))] CommissionSchema SchemaType,
    [EnumDataType(typeof(CommissionStatus))] CommissionStatus Status,
    DateTime AccruedAt);
