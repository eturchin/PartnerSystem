namespace PartnerSystem.Contracts.Dtos.Users;

public sealed record UserDetailsResponse(
    long ExternalId,
    string Name,
    long? PartnerExternalId);
