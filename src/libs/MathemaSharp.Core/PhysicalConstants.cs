// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Collections.Frozen;
using System.Globalization;
using System.Numerics;

namespace MathemaSharp;

/// <summary>Initial physical catalog: seven exact SI definitions and three measured entries
/// from pinned NIST data. No truncated, exact-derived or non-SI variants are supplied.</summary>
public static partial class PhysicalConstants
{
    private static readonly IReadOnlyList<PhysicalConstantDefinition> definitions = Array.AsReadOnly(CreateDefinitions());
    private static readonly FrozenDictionary<string, PhysicalConstantDefinition> byId = definitions.ToFrozenDictionary(x => x.Id, StringComparer.Ordinal);
    /// <summary>Read-only definitions, including exact SI and explicitly selected measured sources.</summary>
    public static IReadOnlyList<PhysicalConstantDefinition> Definitions => definitions;

    /// <summary>Returns a typed value by exact NIST name, using invariant T.Parse.
    /// Exact source values may round in T. Nonfinite rounding throws OverflowException;
    /// zero value or measured-uncertainty underflow throws ArithmeticException.
    /// Source uncertainty is parsed separately; null and unknown IDs throw
    /// ArgumentNullException/KeyNotFoundException. No fallback type or scale conversion.</summary>
    public static PhysicalConstant<T> Get<T>(string id) where T : IFloatingPoint<T>
    {
        ArgumentNullException.ThrowIfNull(id);
        var definition = byId[id];
        var value = T.Parse(definition.DecimalValue, CultureInfo.InvariantCulture);
        if (!T.IsFinite(value)) throw new OverflowException("Physical value is nonfinite in the requested format.");
        if (T.IsZero(value)) throw new ArithmeticException("Physical value underflowed to zero in the requested format.");
        var uncertainty = T.Parse(definition.StandardUncertainty, CultureInfo.InvariantCulture);
        if (!T.IsFinite(uncertainty)) throw new OverflowException("Physical uncertainty is nonfinite in the requested format.");
        if (definition.Status == ConstantStatus.Measured && T.IsZero(uncertainty))
            throw new ArithmeticException("Measured uncertainty underflowed to zero in the requested format.");
        return new(value, uncertainty, definition);
    }

    /// <summary>Rounded binary64 speed of light in vacuum, in m/s.</summary>
    public static readonly double SpeedOfLight = Get<double>("speed of light in vacuum").Value;
    /// <summary>Rounded binary64 Planck constant, in source unit J Hz^-1.</summary>
    public static readonly double Planck = Get<double>("Planck constant").Value;
    /// <summary>Rounded binary64 Boltzmann constant, in J/K.</summary>
    public static readonly double Boltzmann = Get<double>("Boltzmann constant").Value;
    /// <summary>Rounded binary64 elementary charge, in C.</summary>
    public static readonly double ElementaryCharge = Get<double>("elementary charge").Value;
    /// <summary>Rounded binary64 Avogadro constant, in mol^-1.</summary>
    public static readonly double Avogadro = Get<double>("Avogadro constant").Value;
    /// <summary>Rounded binary64 Cs-133 hyperfine transition frequency, in Hz.</summary>
    public static readonly double CesiumFrequency = Get<double>("hyperfine transition frequency of Cs-133").Value;
    /// <summary>Rounded binary64 SI luminous efficacy defining value, in lm/W.</summary>
    public static readonly double LuminousEfficacy = Get<double>("luminous efficacy").Value;
    /// <summary>Recommended binary64 Newtonian constant of gravitation; uncertainty via Get.</summary>
    public static readonly double GravitationalConstant = Get<double>("Newtonian constant of gravitation").Value;
    /// <summary>Recommended binary64 fine-structure constant; uncertainty via Get.</summary>
    public static readonly double FineStructure = Get<double>("fine-structure constant").Value;
    /// <summary>Recommended binary64 vacuum permeability, measured rather than exact; uncertainty via Get.</summary>
    public static readonly double VacuumPermeability = Get<double>("vacuum mag. permeability").Value;
}
