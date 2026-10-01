namespace PartnerSystem.Shared.Seeding;

/// <summary>
/// Deterministic GUIDs and payloads for seeded events.
/// Time is fixed to a constant to keep HasData stable across migration regenerations.
/// </summary>
public static class SeedEvents
{
    public static readonly DateTime BaseTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public const long EventEve1000Pk = 1;
    public const long EventEve1000Id = 1;

    public const long EventFrank500Pk = 2;
    public const long EventFrank500Id = 2;
}