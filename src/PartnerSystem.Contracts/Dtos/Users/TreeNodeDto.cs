namespace PartnerSystem.Contracts.Dtos.Users;

public sealed record TreeNodeDto(
    long ExternalId,
    string Name,
    IReadOnlyList<TreeNodeDto> Children);
