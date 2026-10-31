// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Text.RegularExpressions;

namespace MathemaSharp;

/// <summary>A CODATA source row, preserving printed numeric precision and exact markers.
/// Normalized values with ellipses removed are truncated approximations, not exact values.</summary>
/// <param name="Name">NIST quantity name, retained as an ordinal key.</param>
/// <param name="ValueText">Printed value, including spacing and ellipsis.</param>
/// <param name="UncertaintyText">Printed uncertainty or the exact marker.</param>
/// <param name="Unit">Printed unit text; no SI dimension inference.</param>
/// <param name="DecimalValue">Numeric text without grouping spaces/ellipsis; may be truncated.</param>
/// <param name="DecimalUncertainty">Normalized uncertainty, zero for exact markers.</param>
/// <param name="IsExact">Whether source says exact; not an SI-defining classification.</param>
/// <param name="ValueIsTruncated">Whether the printed value contains an ellipsis.</param>
public sealed record CodataRecord(
    string Name, string ValueText, string UncertaintyText, string Unit,
    string DecimalValue, string DecimalUncertainty, bool IsExact, bool ValueIsTruncated);

/// <summary>Strict fixed-column importer for NIST allascii tables. No numerical conversion,
/// SI dimension inference, or classification as SI-defining/derived is performed.</summary>
public static partial class CodataParser
{
    /// <summary>Reads the named edition's allascii table (60/25/25 character columns).
    /// Preserves source order/text and returns a read-only collection. Rejects missing edition,
    /// malformed numeric fields, duplicate names and empty tables with FormatException.
    /// Exact marker is source provenance, not a guarantee that a printed truncated value is exact.</summary>
    public static IReadOnlyList<CodataRecord> Parse(string text, int edition)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (!text.Contains($"{edition} CODATA adjustment", StringComparison.Ordinal))
            throw new FormatException("CODATA edition does not match.");
        using var reader = new StringReader(text);
        bool rowsStarted = false;
        var records = new List<CodataRecord>();
        var names = new HashSet<string>(StringComparer.Ordinal);
        while (reader.ReadLine() is { } line)
        {
            if (!rowsStarted)
            {
                if (line.Length >= 100 && line.All(c => c == '-')) rowsStarted = true;
                continue;
            }
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.Length < 110) throw new FormatException("Truncated CODATA row.");
            string name = line[..60].Trim(), value = line.Substring(60, 25).Trim();
            string uncertainty = line.Substring(85, 25).Trim(), unit = line[110..].Trim();
            if (name.Length == 0 || !names.Add(name)) throw new FormatException("Missing or duplicate quantity name.");
            bool exact = uncertainty == "(exact)";
            bool truncated = value.Contains("...", StringComparison.Ordinal);
            if (truncated && !PrintedValuePattern().IsMatch(value.Replace(" ", "", StringComparison.Ordinal)))
                throw new FormatException("Malformed value ellipsis.");
            if (!exact && uncertainty.Contains("...", StringComparison.Ordinal))
                throw new FormatException("Truncated uncertainty is not supported.");
            string normalized = Normalize(value);
            string normalizedUncertainty = exact ? "0" : Normalize(uncertainty);
            if (normalizedUncertainty.StartsWith('-')) throw new FormatException("Negative uncertainty.");
            records.Add(new(name, value, uncertainty, unit, normalized, normalizedUncertainty, exact, truncated));
        }
        if (records.Count == 0) throw new FormatException("No CODATA records found.");
        return records.AsReadOnly();
    }

    private static string Normalize(string text)
    {
        var result = text.Replace(" ", "", StringComparison.Ordinal).Replace("...", "", StringComparison.Ordinal);
        if (!DecimalPattern().IsMatch(result)) throw new FormatException("Invalid CODATA numeric field: " + text);
        return result;
    }

    [GeneratedRegex(@"^[+-]?[0-9]+(?:\.[0-9]*)?(?:[eE][+-]?[0-9]+)?$", RegexOptions.CultureInvariant)]
    private static partial Regex DecimalPattern();

    [GeneratedRegex(@"^[+-]?[0-9]+(?:\.[0-9]*)?(?:\.\.\.)?(?:[eE][+-]?[0-9]+)?$", RegexOptions.CultureInvariant)]
    private static partial Regex PrintedValuePattern();
}
