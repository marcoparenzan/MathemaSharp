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

    /// <summary>Positive square root of three. Dimensionless, irrational. Rounded from
    /// generated 100-digit data by T.Parse; initialization may allocate, field reads do not.</summary>
    public static readonly T Sqrt3 = T.Parse(MathematicalConstantData.Sqrt3, CultureInfo.InvariantCulture);

    /// <summary>Base-two logarithm of Euler's number, defined as 1/ln(2). Dimensionless,
    /// irrational. Parsed from independently generated 100-digit data, not rounded Ln2.
    /// Initialization may allocate; field reads do not.</summary>
    public static readonly T Log2E = T.Parse(MathematicalConstantData.Log2E, CultureInfo.InvariantCulture);

    /// <summary>Base-ten logarithm of Euler's number, defined as 1/ln(10). Dimensionless,
    /// irrational. Parsed from independently generated 100-digit data, not rounded Ln10.
    /// Initialization may allocate; field reads do not.</summary>
    public static readonly T Log10E = T.Parse(MathematicalConstantData.Log10E, CultureInfo.InvariantCulture);

    /// <summary>Euler–Mascheroni constant, the limit of the nth harmonic number minus ln(n).
    /// Dimensionless, positive. Rounded from generated 100-digit data by T.Parse;
    /// initialization may allocate, field reads do not. No irrationality claim is made.</summary>
    public static readonly T EulerGamma = T.Parse(MathematicalConstantData.EulerGamma, CultureInfo.InvariantCulture);

    /// <summary>Golden ratio, (1+sqrt(5))/2. Dimensionless, positive, irrational.
    /// Rounded from generated 100-digit data by T.Parse; initialization may allocate,
    /// field reads do not.</summary>
    public static readonly T GoldenRatio = T.Parse(MathematicalConstantData.GoldenRatio, CultureInfo.InvariantCulture);

    /// <summary>Catalan's constant, the sum from n=0 of (-1)^n/(2n+1)^2.
    /// Dimensionless, positive. Rounded from generated 100-digit data by T.Parse;
    /// initialization may allocate, field reads do not. No irrationality claim is made.</summary>
    public static readonly T Catalan = T.Parse(MathematicalConstantData.Catalan, CultureInfo.InvariantCulture);

    /// <summary>Apéry's constant, zeta(3), the sum from n=1 of 1/n^3.
    /// Dimensionless, positive, irrational. Rounded from generated 100-digit data by T.Parse;
    /// initialization may allocate, field reads do not.</summary>
    public static readonly T Apery = T.Parse(MathematicalConstantData.Apery, CultureInfo.InvariantCulture);
}
