using PartnerSystem.Shared.Entities;

namespace PartnerSystem.EventService.Data.Entities;

public class EventEntity : BaseEntity
{
    public long ExternalId { get; set; }
    public long UserExternalId { get; set; }
    public decimal Profit { get; set; }
    public DateTime OccurredAt { get; set; }
}