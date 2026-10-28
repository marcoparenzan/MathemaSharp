using Ecosystem.Validation;

namespace Ecosystem.Validation.Tests;

public class UiLayoutTests
{
    [Theory]
    [InlineData(44, 1, true)]
    [InlineData(46, 3, true)]
    [InlineData(43.9, 1, false)]
    [InlineData(double.NaN, 1, false)]
    [InlineData(double.PositiveInfinity, 1, false)]
    [InlineData(44, 0, false)]
    [InlineData(44, -1, false)]
    public void ValidatesRenderedControlHeight(double minimum, int count, bool meets)
        => Assert.Equal(meets, UiLayout.MeetsControlHeightFloor(minimum, count));

    [Fact]
    public void RejectsInvalidControlHeightConfiguration()
    {
        Assert.False(UiLayout.MeetsControlHeightFloor(44, 1, 0));
        Assert.False(UiLayout.MeetsControlHeightFloor(44, 1, double.NaN));
        Assert.False(UiLayout.MeetsControlHeightFloor(44, 1, double.PositiveInfinity));
    }

    [Theory]
    [InlineData(11, 1, true)]
    [InlineData(10, 1, true)]
    [InlineData(9.9, 1, false)]
    [InlineData(double.NaN, 1, false)]
    [InlineData(double.PositiveInfinity, 1, false)]
    [InlineData(11, 0, false)]
    [InlineData(11, -1, false)]
    public void ValidatesMeasuredSvgTextFloor(double minimum, int count, bool meets)
        => Assert.Equal(meets, UiLayout.MeetsSvgTextFloor(minimum, count));

    [Fact]
    public void RejectsInvalidTextFloorConfiguration()
    {
        Assert.False(UiLayout.MeetsSvgTextFloor(11, 1, 0));
        Assert.False(UiLayout.MeetsSvgTextFloor(11, 1, double.NaN));
        Assert.False(UiLayout.MeetsSvgTextFloor(11, 1, double.PositiveInfinity));
    }

    [Theory]
    [InlineData(360, 360, 0, true)]
    [InlineData(360, 361, 0, true)]
    [InlineData(360, 362, 0, false)]
    [InlineData(360, 360, 1, false)]
    [InlineData(0, 0, 0, false)]
    [InlineData(360, -1, 0, false)]
    [InlineData(360, 360, -1, false)]
    [InlineData(int.MaxValue, int.MaxValue, 0, true)]
    public void ChecksHorizontalFitWithoutHidingInvalidEvidence(int viewport, int document, int outside, bool fits)
        => Assert.Equal(fits, new UiLayout(viewport, document, outside).FitsViewport);
}
