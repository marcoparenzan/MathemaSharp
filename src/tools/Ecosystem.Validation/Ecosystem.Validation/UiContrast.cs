namespace Ecosystem.Validation;

public readonly record struct UiRgb(byte Red, byte Green, byte Blue);

/// <summary>sRGB contrast for already-composited foreground/background samples, not a whole-page accessibility audit.</summary>
public static class UiContrast
{
    public static double Ratio(UiRgb foreground, UiRgb background)
    {
        double a = Luminance(foreground), b = Luminance(background);
        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

    public static bool MeetsNormalText(UiRgb foreground, UiRgb background, double minimumRatio = 4.5)
        => double.IsFinite(minimumRatio) && minimumRatio >= 1 && minimumRatio <= 21
            && Ratio(foreground, background) >= minimumRatio;

    private static double Luminance(UiRgb color)
        => 0.2126 * Linear(color.Red) + 0.7152 * Linear(color.Green) + 0.0722 * Linear(color.Blue);

    private static double Linear(byte channel)
    {
        double value = channel / 255.0;
        return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
    }
}
