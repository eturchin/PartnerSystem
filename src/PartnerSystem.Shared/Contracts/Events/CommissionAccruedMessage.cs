using PartnerSystem.Shared.Contracts.Enums;

namespace PartnerSystem.Shared.Contracts.Events;

public sealed record CommissionAccruedMessage(
    Guid CommissionId,
    Guid EventId,
    Guid BeneficiaryUserId,
    Guid SourceUserId,
    decimal Amount,
    int Level,
    CommissionSchema SchemaType,
    DateTime AccruedAt);