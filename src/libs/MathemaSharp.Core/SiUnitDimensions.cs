// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Collections.Frozen;
using System.Globalization;

namespace MathemaSharp;

/// <summary>Parses products of coherent SI symbols with integer powers. No unit-scale
/// conversion, prefixes, non-SI units, parentheses or slash syntax are inferred.</summary>
public static class SiUnitDimensions
{
    private static readonly FrozenDictionary<string, SiDimensions> symbols = new Dictionary<string, SiDimensions>(StringComparer.Ordinal)
    {
        ["m"] = new(1, 0, 0, 0, 0, 0, 0), ["kg"] = new(0, 1, 0, 0, 0, 0, 0),
        ["s"] = new(0, 0, 1, 0, 0, 0, 0), ["A"] = new(0, 0, 0, 1, 0, 0, 0),
        ["K"] = new(0, 0, 0, 0, 1, 0, 0), ["mol"] = new(0, 0, 0, 0, 0, 1, 0),
        ["cd"] = new(0, 0, 0, 0, 0, 0, 1),
        ["Hz"] = new(0, 0, -1, 0, 0, 0, 0), ["N"] = new(1, 1, -2, 0, 0, 0, 0),
        ["Pa"] = new(-1, 1, -2, 0, 0, 0, 0), ["J"] = new(2, 1, -2, 0, 0, 0, 0),
        ["W"] = new(2, 1, -3, 0, 0, 0, 0), ["C"] = new(0, 0, 1, 1, 0, 0, 0),
        ["V"] = new(2, 1, -3, -1, 0, 0, 0), ["F"] = new(-2, -1, 4, 2, 0, 0, 0),
        ["ohm"] = new(2, 1, -3, -2, 0, 0, 0), ["S"] = new(-2, -1, 3, 2, 0, 0, 0),
        ["Wb"] = new(2, 1, -2, -1, 0, 0, 0), ["T"] = new(0, 1, -2, -1, 0, 0, 0),
        ["H"] = new(2, 1, -2, -2, 0, 0, 0), ["lm"] = new(0, 0, 0, 0, 0, 0, 1),
        ["lx"] = new(-2, 0, 0, 0, 0, 0, 1), ["rad"] = default, ["sr"] = default,
        ["1"] = default
    }.ToFrozenDictionary(StringComparer.Ordinal);

    /// <summary>Returns dimensions for a whitespace-separated coherent-SI product.
    /// Empty/whitespace/1 is dimensionless. Symbols are ordinal/case-sensitive.
    /// Unknown or malformed units and exponent overflow throw FormatException;
    /// null throws ArgumentNullException. Values/scales are never converted.</summary>
    public static SiDimensions Parse(string unit)
    {
        ArgumentNullException.ThrowIfNull(unit);
        if (!TryParse(unit, out var result)) throw new FormatException("Unsupported SI unit expression: " + unit);
        return result;
    }

    /// <summary>Tries the same syntax as Parse. Null, unknown symbols, malformed powers
    /// or integer overflow return false and a dimensionless default; false never means
    /// the unknown unit is scientifically dimensionless. May allocate token storage.</summary>
    public static bool TryParse(string? unit, out SiDimensions dimensions)
    {
        dimensions = default;
        if (unit is null) return false;
        var result = SiDimensions.Dimensionless;
        foreach (var token in unit.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            int caret = token.IndexOf('^');
            string symbol = caret < 0 ? token : token[..caret];
            int exponent = 1;
            if (caret >= 0 && !int.TryParse(token[(caret + 1)..], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out exponent)) return false;
            if (!symbols.TryGetValue(symbol, out var atom)) return false;
            try { result += atom.Power(exponent); }
            catch (OverflowException) { return false; }
        }
        dimensions = result;
        return true;
    }
}
