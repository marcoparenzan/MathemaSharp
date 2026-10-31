// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Collections.Frozen;

namespace MathemaSharp;

/// <summary>Read-only catalog of the 13 generated mathematical definitions.
/// No physical/SI/CODATA entries or language aliases are provided by this catalog.</summary>
public static partial class MathematicalConstants
{
    private static readonly IReadOnlyList<ConstantDefinition> definitions = Array.AsReadOnly(CreateDefinitions());
    private static readonly FrozenDictionary<string, ConstantDefinition> byId = definitions.ToFrozenDictionary(value => value.Id, StringComparer.Ordinal);

    /// <summary>Stable generator order of immutable definitions; collection mutation is disallowed.</summary>
    public static IReadOnlyList<ConstantDefinition> Definitions => definitions;

    /// <summary>Returns an immutable definition by exact ordinal identifier. Null throws
    /// ArgumentNullException; unknown or differently cased IDs throw KeyNotFoundException.
    /// No parsing or numerical conversion occurs during lookup.</summary>
    public static ConstantDefinition Get(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return byId[id];
    }
}
