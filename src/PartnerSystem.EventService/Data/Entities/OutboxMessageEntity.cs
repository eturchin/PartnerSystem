using PartnerSystem.Shared.Entities;

namespace PartnerSystem.EventService.Data.Entities;

public class OutboxMessageEntity : BaseEntity
{
    public string Topic { get; set; } = "";
    public string Payload { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool Published { get; set; }
}