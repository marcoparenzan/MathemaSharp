// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp;

/// <summary>Immutable source metadata for one of the seven SI defining constants.
/// ExactSi refers to the definition, not exact representability in every floating format.</summary>
public sealed class PhysicalConstantDefinition
{
    internal PhysicalConstantDefinition(string id, string symbol, string value, string unit, string sourceHash)
    {
        Id = id; Symbol = symbol; DecimalValue = value; Unit = unit; SourceSha256 = sourceHash;
        Dimensions = SiUnitDimensions.Parse(unit);
    }
    /// <summary>Exact ordinal NIST quantity name.</summary>
    public string Id { get; }
    /// <summary>Conventional SI defining symbol.</summary>
    public string Symbol { get; }
    /// <summary>Exact defining decimal value from the verified NIST row, without ellipses.</summary>
    public string DecimalValue { get; }
    /// <summary>Printed source unit, without scale conversion.</summary>
    public string Unit { get; }
    /// <summary>Coherent SI base dimension vector.</summary>
    public SiDimensions Dimensions { get; }
    /// <summary>ExactSi for all seven entries in this initial catalog.</summary>
    public ConstantStatus Status => ConstantStatus.ExactSi;
    /// <summary>Zero standard measurement uncertainty for exact defining values; not rounding error.</summary>
    public string StandardUncertainty => "0";
    /// <summary>Source table edition, CODATA2022.</summary>
    public int SourceEdition => 2022;
    /// <summary>NIST source URL used by the generator.</summary>
    public string Source => "https://physics.nist.gov/cuu/Constants/Table/allascii.txt";
    /// <summary>SHA256 of the raw source table.</summary>
    public string SourceSha256 { get; }
}

/// <summary>A rounded typed physical value with immutable exact-source metadata.
/// Values returned by PhysicalConstants.Get are finite and nonzero; no scale conversion.</summary>
/// <param name="Value">Value rounded by T.Parse, not universally exact despite ExactSi status.</param>
/// <param name="StandardUncertainty">Zero source measurement uncertainty, not type rounding error.</param>
/// <param name="Definition">Immutable source definition.</param>
public readonly record struct PhysicalConstant<T>(T Value, T StandardUncertainty, PhysicalConstantDefinition Definition) where T : IFloatingPoint<T>;
