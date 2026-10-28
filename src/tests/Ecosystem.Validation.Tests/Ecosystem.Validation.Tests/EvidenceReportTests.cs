using Ecosystem.Validation;

namespace Ecosystem.Validation.Tests;

public class EvidenceReportTests
{
    static readonly DateTime Start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    static readonly DateTime End = Start.AddMinutes(1);

    [Fact]
    public void AcceptsFreshReportWithoutImageSaving()
        => EvidenceReport.Validate("""{"GeneratedUtc":"2026-01-01T00:00:30Z","SaveImages":false} """, Start, End, false);

    [Theory]
    [InlineData("2025-12-31T23:59:59Z")]
    [InlineData("2026-01-01T00:01:01Z")]
    public void RejectsStaleOrFutureEvidence(string timestamp)
        => Assert.Throws<InvalidOperationException>(() => EvidenceReport.Validate($"{{\"GeneratedUtc\":\"{timestamp}\"}}", Start, End));

    [Theory]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("{\"GeneratedUtc\":\"bad\"}")]
    public void RequiresTimestamp(string json)
        => Assert.Throws<FormatException>(() => EvidenceReport.Validate(json, Start, End));

    [Fact]
    public void RejectsImagePolicyMismatch()
        => Assert.Throws<InvalidOperationException>(() => EvidenceReport.Validate("""{"GeneratedUtc":"2026-01-01T00:00:30Z","SaveImages":true} """, Start, End, false));

    [Theory]
    [InlineData("{\"GeneratedUtc\":\"2026-01-01T00:00:30Z\"}")]
    [InlineData("{\"GeneratedUtc\":\"2026-01-01T00:00:30Z\",\"SaveImages\":\"false\"}")]
    public void RequiresBooleanImagePolicyWhenRequested(string json)
        => Assert.Throws<FormatException>(() => EvidenceReport.Validate(json, Start, End, false));

    [Fact]
    public void RejectsInvalidExecutionWindow()
        => Assert.Throws<ArgumentException>(() => EvidenceReport.Validate("{}", End, Start));
}
