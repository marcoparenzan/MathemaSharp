// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp;

/// <summary>Immutable exact-SI, measured or formula-derived metadata from pinned NIST inputs.
/// Status and measurement uncertainty are distinct from floating representation error.</summary>
public sealed class PhysicalConstantDefinition
{
    internal PhysicalConstantDefinition(string id, string symbol, string value, string unit, string sourceHash)
        : this(id, symbol, value, unit, sourceHash, ConstantStatus.ExactSi, "0") { }

    internal PhysicalConstantDefinition(string id, string symbol, string value, string unit, string sourceHash, ConstantStatus status, string uncertainty, string? formula = null, int? generatedPrecisionDigits = null)
    {
        Id = id; Symbol = symbol; DecimalValue = value; Unit = unit; SourceSha256 = sourceHash;
        Dimensions = SiUnitDimensions.Parse(unit); Status = status; StandardUncertainty = uncertainty;
        Formula = formula; GeneratedPrecisionDigits = generatedPrecisionDigits;
    }
    /// <summary>Exact ordinal NIST quantity name.</summary>
    public string Id { get; }
    /// <summary>Conventional SI defining symbol.</summary>
    public string Symbol { get; }
    /// <summary>Source exact/measured decimal or regenerated formula value. No printed ellipses;
    /// a nonterminating derived formula retains finite generated precision.</summary>
    public string DecimalValue { get; }
    /// <summary>Printed source unit, without scale conversion.</summary>
    public string Unit { get; }
    /// <summary>Coherent SI base dimension vector.</summary>
    public SiDimensions Dimensions { get; }
    /// <summary>ExactSi, Measured or ExactDerived, according to the explicitly selected family.</summary>
    public ConstantStatus Status { get; }
    /// <summary>Source standard measurement uncertainty; zero for exact definitions, not rounding error.</summary>
    public string StandardUncertainty { get; }
    /// <summary>Exact defining formula for generated derived entries; null for direct source rows.</summary>
    public string? Formula { get; }
    /// <summary>Retained decimal digits for nonterminating generated values; null for direct
    /// source values or terminating exact products. This is not measurement uncertainty.</summary>
    public int? GeneratedPrecisionDigits { get; }
    /// <summary>Source table edition, CODATA2022.</summary>
    public int SourceEdition => 2022;
    /// <summary>NIST source provenance; generated entries include their formula rather than
    /// pretending the finite printed NIST approximation is their full value.</summary>
    public string Source => Formula is null ? "https://physics.nist.gov/cuu/Constants/Table/allascii.txt"
        : $"Generated {Formula} from exact inputs in https://physics.nist.gov/cuu/Constants/Table/allascii.txt";
    /// <summary>SHA256 of the raw source table.</summary>
    public string SourceSha256 { get; }
}

/// <summary>A rounded typed physical value with immutable source metadata.
/// Get returns finite nonzero values and preserves nonzero measured uncertainty; no scale conversion.</summary>
/// <param name="Value">Value rounded by T.Parse, not universally exact despite ExactSi status.</param>
/// <param name="StandardUncertainty">Rounded source measurement uncertainty, not type rounding error.</param>
/// <param name="Definition">Immutable source definition.</param>
public readonly record struct PhysicalConstant<T>(T Value, T StandardUncertainty, PhysicalConstantDefinition Definition) where T : IFloatingPoint<T>;
