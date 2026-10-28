namespace Ecosystem.Validation;

/// <summary>Horizontal layout evidence only, with a one-pixel rounding tolerance. Does not certify readability or touch usability.</summary>
public readonly record struct UiLayout(int ViewportWidth, int DocumentWidth, int OutsideCount)
{
    public bool FitsViewport => ViewportWidth > 0 && DocumentWidth >= 0
        && DocumentWidth <= (long)ViewportWidth + 1 && OutsideCount == 0;

    /// <summary>A measured SVG text-size floor, not a general accessibility/readability certification.</summary>
    public static bool MeetsSvgTextFloor(double minimumPixels, int labelCount, double floorPixels = 10)
        => MeetsFloor(minimumPixels, labelCount, floorPixels);

    /// <summary>Rendered element height only; not proof of physical touch ergonomics.</summary>
    public static bool MeetsControlHeightFloor(double minimumPixels, int controlCount, double floorPixels = 44)
        => MeetsFloor(minimumPixels, controlCount, floorPixels);

    private static bool MeetsFloor(double minimumPixels, int count, double floorPixels)
        => count > 0 && double.IsFinite(minimumPixels) && double.IsFinite(floorPixels)
            && floorPixels > 0 && minimumPixels >= floorPixels;
}
