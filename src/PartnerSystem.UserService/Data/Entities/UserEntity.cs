using PartnerSystem.Shared.Entities;

namespace PartnerSystem.UserService.Data.Entities;

public class UserEntity : BaseEntity
{
    public long ExternalId { get; set; }
    public string Name { get; set; } = "";
    public long? PartnerExternalId { get; set; }
}