using System.Text.Json;

namespace Ecosystem.Validation;

/// <summary>Reject stale/malformed reports or a mismatched image policy before archiving gate evidence.</summary>
public static class EvidenceReport
{
    public static void Validate(string json, DateTime startedUtc, DateTime finishedUtc, bool? saveImages = null)
    {
        if (startedUtc.Kind != DateTimeKind.Utc || finishedUtc.Kind != DateTimeKind.Utc || finishedUtc < startedUtc)
            throw new ArgumentException("Expected an ordered UTC execution window.");
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("GeneratedUtc", out var generated)
            || generated.ValueKind != JsonValueKind.String || !generated.TryGetDateTimeOffset(out var timestamp))
            throw new FormatException("Missing or malformed GeneratedUtc evidence.");
        var time = timestamp.UtcDateTime;
        if (time < startedUtc || time > finishedUtc)
            throw new InvalidOperationException("Evidence timestamp is outside this step's execution window.");
        if (saveImages is { } expected)
        {
            if (!root.TryGetProperty("SaveImages", out var policy) || policy.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                throw new FormatException("Missing or non-boolean SaveImages evidence.");
            if (policy.GetBoolean() != expected)
                throw new InvalidOperationException("Image policy mismatch in evidence.");
        }
    }
}
