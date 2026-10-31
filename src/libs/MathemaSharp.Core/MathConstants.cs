// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Globalization;
using System.Numerics;

namespace MathemaSharp;

/// <summary>Mathematical constants parsed once per floating type from generated 100-digit
/// decimal values. Source: pinned mpmath 1.3.0 with agreement at 160/200 digits.
/// Six IEEE formats have independent rounding reference tests; other implementations
/// use their own T.Parse behaviour. This is not a physical-constant catalog.</summary>
public static class MathConstants<T> where T : IFloatingPoint<T>
{
    /// <summary>Pi, the circumference-to-diameter ratio of a Euclidean circle.
    /// Dimensionless, positive, irrational. Rounded from the generated 100-digit source
    /// by T.Parse; initialization can allocate, subsequent field reads do not.</summary>
    public static readonly T Pi = T.Parse(MathematicalConstantData.Pi, CultureInfo.InvariantCulture);

    /// <summary>Tau, defined as twice pi. Dimensionless, positive, irrational.
    /// Parsed independently from its generated 100-digit value, not computed from rounded Pi.
    /// Rounded by T.Parse; initialization can allocate, subsequent field reads do not.</summary>
    public static readonly T Tau = T.Parse(MathematicalConstantData.Tau, CultureInfo.InvariantCulture);

    /// <summary>Euler's number, the base of the natural exponential. Dimensionless,
    /// positive, irrational. Rounded from its generated 100-digit source by T.Parse;
    /// initialization can allocate, subsequent field reads do not.</summary>
    public static readonly T E = T.Parse(MathematicalConstantData.E, CultureInfo.InvariantCulture);

    /// <summary>Positive square root of two. Dimensionless, irrational. Rounded from
    /// generated 100-digit data by T.Parse; initialization may allocate, field reads do not.</summary>
    public static readonly T Sqrt2 = T.Parse(MathematicalConstantData.Sqrt2, CultureInfo.InvariantCulture);

    /// <summary>Natural logarithm of two. Dimensionless, positive, irrational. Rounded from
    /// generated 100-digit data by T.Parse; initialization may allocate, field reads do not.</summary>
    public static readonly T Ln2 = T.Parse(MathematicalConstantData.Ln2, CultureInfo.InvariantCulture);

    /// <summary>Natural logarithm of ten. Dimensionless, positive, irrational. Rounded from
    /// generated 100-digit data by T.Parse; initialization may allocate, field reads do not.</summary>
    public static readonly T Ln10 = T.Parse(MathematicalConstantData.Ln10, CultureInfo.InvariantCulture);
}
