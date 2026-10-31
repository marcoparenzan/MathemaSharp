// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MathemaSharp;

/// <summary>Explicit midpoint and directed rounding policies; no language-specific default.</summary>
public enum RoundingMode
{
    /// <summary>Nearest value; midpoint ties go to the even integer.</summary>
    HalfToEven,
    /// <summary>Nearest value; midpoint ties go away from zero.</summary>
    HalfAwayFromZero,
    /// <summary>Discard the fractional part, toward zero.</summary>
    TowardZero,
    /// <summary>Round toward negative infinity.</summary>
    TowardNegative,
    /// <summary>Round toward positive infinity.</summary>
    TowardPositive
}

/// <summary>Generic scalar mathematics with explicit semantic policies.
/// Fast profile: BCL implementations, not a cross-platform reproducible profile.</summary>
public static partial class GenericMath
{
    /// <summary>Rounds x to an integral value using the explicit mode. Delegates directly to
    /// T.Round; accuracy and special-value behaviour follow T's IFloatingPoint implementation.
    /// IEEE formats preserve signed zero and pass through NaN/infinity. Invalid modes throw
    /// ArgumentOutOfRangeException; no conversion or allocation is introduced by the facade.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Round<T>(T x, RoundingMode mode) where T : IFloatingPoint<T>
        => T.Round(x, ToBclMode(mode));

    /// <summary>Rounds x to digits decimal fractional places using the explicit mode.
    /// Delegates directly to T.Round, including its supported digit range and scaling accuracy.
    /// Negative or excessive digits may throw ArgumentOutOfRangeException according to T;
    /// negative-digit rounding is not implemented by this overload. IEEE NaN/infinity and signed
    /// zero behaviour follow T. Invalid policy values throw ArgumentOutOfRangeException.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Round<T>(T x, int digits, RoundingMode mode) where T : IFloatingPoint<T>
        => T.Round(x, digits, ToBclMode(mode));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static MidpointRounding ToBclMode(RoundingMode mode) => mode switch
    {
        RoundingMode.HalfToEven => MidpointRounding.ToEven,
        RoundingMode.HalfAwayFromZero => MidpointRounding.AwayFromZero,
        RoundingMode.TowardZero => MidpointRounding.ToZero,
        RoundingMode.TowardNegative => MidpointRounding.ToNegativeInfinity,
        RoundingMode.TowardPositive => MidpointRounding.ToPositiveInfinity,
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };
}
