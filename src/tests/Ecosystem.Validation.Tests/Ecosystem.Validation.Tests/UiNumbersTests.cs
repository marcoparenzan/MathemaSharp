using Ecosystem.Validation;

namespace Ecosystem.Validation.Tests;

public class UiNumbersTests
{
    [Theory]
    [InlineData("963 N, come appoggiarci 98 kg", 963)]
    [InlineData("3.853 N, come appoggiarci 393 kg", 3853)]
    [InlineData("3,853 N", 3853)]
    [InlineData("1 234 N", 1234)]
    public void ParsesRoundedForce(string text, double expected) => Assert.Equal(expected, UiNumbers.ForceNewtons(text));

    [Theory]
    [InlineData("Perdi 0,42 bar", 0.42)]
    [InlineData("Perdi 0.84 bar", 0.84)]
    [InlineData("Perdi 12 bar", 12)]
    public void ParsesPressureInBothCultures(string text, double expected) => Assert.Equal(expected, UiNumbers.PressureBar(text));

    [Theory]
    [InlineData("unknown")]
    [InlineData("1.23 N")]
    [InlineData("12 kg")]
    public void RejectsInvalidForce(string text) => Assert.Throws<FormatException>(() => UiNumbers.ForceNewtons(text));

    [Theory]
    [InlineData("Perdi NaN bar")]
    [InlineData("Perdi 1,2,3 bar")]
    [InlineData("unknown")]
    public void RejectsInvalidPressure(string text) => Assert.Throws<FormatException>(() => UiNumbers.PressureBar(text));
}
