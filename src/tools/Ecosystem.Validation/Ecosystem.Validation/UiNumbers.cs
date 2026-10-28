using System.Globalization;
using System.Text.RegularExpressions;

namespace Ecosystem.Validation;

/// <summary>Parse the Italian NavierStokes UI's rounded displays; accepts decimal/group separators from either tested culture.</summary>
public static partial class UiNumbers
{
    [GeneratedRegex(@"^\s*(\d{1,3}(?:[.,\s]\d{3})+|\d+)\s+N\b", RegexOptions.CultureInvariant)]
    private static partial Regex ForcePattern();
    [GeneratedRegex(@"^\s*Perdi\s+(\d+(?:[,.]\d+)?)\s+bar\b", RegexOptions.CultureInvariant)]
    private static partial Regex PressurePattern();
    [GeneratedRegex(@"\D", RegexOptions.CultureInvariant)]
    private static partial Regex NonDigits();

    public static double ForceNewtons(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var match = ForcePattern().Match(text);
        if (!match.Success) throw new FormatException("Expected a rounded force display ending in N.");
        return Finite(NonDigits().Replace(match.Groups[1].Value, ""));
    }

    public static double PressureBar(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var match = PressurePattern().Match(text);
        if (!match.Success) throw new FormatException("Expected 'Perdi <value> bar'.");
        return Finite(match.Groups[1].Value.Replace(',', '.'));
    }

    private static double Finite(string value)
    {
        var number = double.Parse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
        return double.IsFinite(number) ? number : throw new FormatException("Non-finite UI value.");
    }
}
