namespace PartnerSystem.Shared.Contracts.Dtos.Users;

public sealed record TreeNodeDto(Guid ExternalId, string Name, List<TreeNodeDto> Children);