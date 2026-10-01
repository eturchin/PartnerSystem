using System.Text.Json;

namespace PartnerSystem.Shared.Options;

public static class JsonOptions
{
    public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
}
