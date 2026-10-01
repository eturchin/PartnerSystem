namespace PartnerSystem.Shared.Seeding;

/// <summary>
/// Deterministic long Ids for seeded users.
/// Used by HasData in migrations and by integration tests.
/// </summary>
public static class SeedUsers
{
    // Id
    public const long AlicePk = 1;
    public const long BobPk = 2;
    public const long CharliePk = 3;
    public const long DavePk = 4;
    public const long EvePk = 5;
    public const long FrankPk = 6;

    // ExternalId
    public const long Alice = 1;
    public const long Bob = 2;
    public const long Charlie = 3;
    public const long Dave = 4;
    public const long Eve = 5;
    public const long Frank = 6;
}