// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Globalization;
using System.Numerics;

namespace MathemaSharp;

/// <summary>Nature of a scientific constant definition, independent of display precision.</summary>
public enum ConstantStatus
{
    /// <summary>A mathematical definition; a stored irrational value is a finite approximation.</summary>
    Mathematical,
    /// <summary>An exact SI defining value.</summary>
    ExactSi,
    /// <summary>An exact formula derived from exact definitions.</summary>
    ExactDerived,
    /// <summary>An experimentally measured value with uncertainty.</summary>
    Measured
}

/// <summary>Immutable metadata for a generated mathematical constant. This first catalog
/// contains only dimensionless mathematical definitions, not CODATA measurements.
/// Stored values have finite precision; zero measurement uncertainty does not mean
/// a finite decimal expansion represents an irrational definition exactly.</summary>
public sealed class ConstantDefinition
{
    private static readonly int[] Dimensionless = [0, 0, 0, 0, 0, 0, 0];

    internal ConstantDefinition(string id, string symbol, string value, int significantDigits, string source)
    {
        Id = id; Symbol = symbol; DecimalValue = value;
        SignificantDigits = significantDigits; Source = source;
    }

    /// <summary>Stable ordinal, case-sensitive catalog identifier.</summary>
    public string Id { get; }
    /// <summary>Conventional mathematical symbol or expression.</summary>
    public string Symbol { get; }
    /// <summary>Mathematical for every entry currently supplied by this catalog.</summary>
    public ConstantStatus Status => ConstantStatus.Mathematical;
    /// <summary>Generated invariant decimal approximation, not a typed floating value.</summary>
    public string DecimalValue { get; }
    /// <summary>Significant digits retained in DecimalValue; presently 100.</summary>
    public int SignificantDigits { get; }
    /// <summary>Unit text; all current mathematical definitions are dimensionless.</summary>
    public string Unit => "1";
    /// <summary>SI exponents in order m, kg, s, A, K, mol, cd; all seven are zero.</summary>
    public ReadOnlySpan<int> UnitExponents => Dimensionless;
    /// <summary>Standard measurement uncertainty, zero for mathematical definitions.
    /// This is not the approximation error of the stored decimal string.</summary>
    public string StandardUncertainty => "0";
    /// <summary>Relative standard measurement uncertainty, zero for mathematical definitions.</summary>
    public string RelativeStandardUncertainty => "0";
    /// <summary>Generator source and pinned version, not a physical table edition.</summary>
    public string Source { get; }
    /// <summary>No table date applies to timeless mathematical definitions.</summary>
    public DateOnly? SourceDate => null;

    /// <summary>Parses DecimalValue into T with invariant culture. Six IEEE formats are
    /// checked against independent generated rounding references; custom T uses its
    /// own parsing semantics. Can allocate; formats above stored precision are not promised
    /// mathematically exact. No conversion through double.</summary>
    public T Value<T>() where T : IFloatingPoint<T> => T.Parse(DecimalValue, CultureInfo.InvariantCulture);
}
