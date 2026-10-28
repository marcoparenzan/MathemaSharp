using Ecosystem.Validation;

namespace Ecosystem.Validation.Tests;

public class EvidenceOptionsTests
{
    [Fact]
    public void DefaultPreservesExistingImageSaving() => Assert.True(EvidenceOptions.Parse([]).SaveImages);

    [Fact]
    public void ExplicitFlagDisablesSaving() => Assert.False(EvidenceOptions.Parse(["--no-images"]).SaveImages);

    [Fact]
    public void RepeatedFlagIsIdempotent() => Assert.False(EvidenceOptions.Parse(["--no-images", "--no-images"]).SaveImages);

    [Theory]
    [InlineData("--unknown")]
    [InlineData("--screenshot")]
    public void UnknownFlagsCannotSilentlyChangePolicy(string value)
        => Assert.Throws<ArgumentException>(() => EvidenceOptions.Parse([value]));

    [Fact]
    public void NullArgumentsAreRejected() => Assert.Throws<ArgumentNullException>(() => EvidenceOptions.Parse(null!));
}
