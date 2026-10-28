using Ecosystem.Validation;

namespace Ecosystem.Validation.Tests;

public class UiContrastTests
{
    static readonly UiRgb White = new(255, 255, 255);
    static readonly UiRgb Black = new(0, 0, 0);

    [Fact]
    public void BlackAndWhiteHaveMaximumContrast() => Assert.Equal(21, UiContrast.Ratio(Black, White), 10);

    [Fact]
    public void IdenticalColorsHaveUnitContrast() => Assert.Equal(1, UiContrast.Ratio(White, White));

    [Fact]
    public void RatioIsSymmetric()
    {
        var a = new UiRgb(36, 103, 186); var b = new UiRgb(252, 252, 251);
        Assert.Equal(UiContrast.Ratio(a, b), UiContrast.Ratio(b, a));
    }

    [Fact]
    public void NearThresholdGrayIsNotRoundedIntoAPass()
    {
        var gray = new UiRgb(119, 119, 119);
        Assert.InRange(UiContrast.Ratio(gray, White), 4.47, 4.49);
        Assert.False(UiContrast.MeetsNormalText(gray, White));
    }

    [Fact]
    public void RejectsInvalidThresholds()
    {
        Assert.False(UiContrast.MeetsNormalText(Black, White, double.NaN));
        Assert.False(UiContrast.MeetsNormalText(Black, White, 0));
        Assert.False(UiContrast.MeetsNormalText(Black, White, 22));
    }

    [Theory]
    [InlineData(36, 103, 186)]
    [InlineData(107, 106, 101)]
    public void CandidateThemeColorsMeetNormalTextCriterion(byte red, byte green, byte blue)
        => Assert.True(UiContrast.MeetsNormalText(new(red, green, blue), new(246, 245, 241)));
}
