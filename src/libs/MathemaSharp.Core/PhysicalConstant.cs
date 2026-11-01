// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp;

/// <summary>Immutable exact-SI or measured source metadata from pinned NIST data.
/// Status and measurement uncertainty are distinct from floating representation error.</summary>
public sealed class PhysicalConstantDefinition
{
    internal PhysicalConstantDefinition(string id, string symbol, string value, string unit, string sourceHash)
        : this(id, symbol, value, unit, sourceHash, ConstantStatus.ExactSi, "0") { }

    internal PhysicalConstantDefinition(string id, string symbol, string value, string unit, string sourceHash, ConstantStatus status, string uncertainty)
    {
        Id = id; Symbol = symbol; DecimalValue = value; Unit = unit; SourceSha256 = sourceHash;
        Dimensions = SiUnitDimensions.Parse(unit); Status = status; StandardUncertainty = uncertainty;
    }
    /// <summary>Exact ordinal NIST quantity name.</summary>
    public string Id { get; }
    /// <summary>Conventional SI defining symbol.</summary>
    public string Symbol { get; }
    /// <summary>Exact defining or recommended measured decimal value from NIST, without ellipses.</summary>
    public string DecimalValue { get; }
    /// <summary>Printed source unit, without scale conversion.</summary>
    public string Unit { get; }
    /// <summary>Coherent SI base dimension vector.</summary>
    public SiDimensions Dimensions { get; }
    /// <summary>ExactSi or Measured, determined from the explicitly selected source family.</summary>
    public ConstantStatus Status { get; }
    /// <summary>Source standard measurement uncertainty; zero for exact definitions, not rounding error.</summary>
    public string StandardUncertainty { get; }
    /// <summary>Source table edition, CODATA2022.</summary>
    public int SourceEdition => 2022;
    /// <summary>NIST source URL used by the generator.</summary>
    public string Source => "https://physics.nist.gov/cuu/Constants/Table/allascii.txt";
    /// <summary>SHA256 of the raw source table.</summary>
    public string SourceSha256 { get; }
}

/// <summary>A rounded typed physical value with immutable source metadata.
/// Get returns finite nonzero values and preserves nonzero measured uncertainty; no scale conversion.</summary>
/// <param name="Value">Value rounded by T.Parse, not universally exact despite ExactSi status.</param>
/// <param name="StandardUncertainty">Rounded source measurement uncertainty, not type rounding error.</param>
/// <param name="Definition">Immutable source definition.</param>
public readonly record struct PhysicalConstant<T>(T Value, T StandardUncertainty, PhysicalConstantDefinition Definition) where T : IFloatingPoint<T>;
