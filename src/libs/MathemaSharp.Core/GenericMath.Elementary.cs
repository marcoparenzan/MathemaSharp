// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Principal nonnegative square root of x. Real domain: x >= 0.
    /// IEEE Sqrt(-0)=-0, Sqrt(+infinity)=+infinity; negative values and NaN produce NaN.
    /// Direct T.Sqrt forward; accuracy follows T's BCL implementation without a new
    /// cross-platform reproducibility guarantee. Custom T owns its domain behaviour.
    /// No conversion or allocation is introduced by the facade.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sqrt<T>(T x) where T : IRootFunctions<T> => T.Sqrt(x);

    /// <summary>Natural exponential of x. Real domain: all finite x.
    /// IEEE Exp(±0)=1, Exp(-infinity)=+0, Exp(+infinity)=+infinity; NaN produces NaN.
    /// Overflow and underflow follow T. Direct T.Exp forward with T's BCL accuracy,
    /// not a correctly rounded cross-platform profile. Custom T owns its special values.
    /// No conversion or allocation is introduced by the facade.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Exp<T>(T x) where T : IExponentialFunctions<T> => T.Exp(x);

    /// <summary>Base-two exponential, 2 raised to x. Real domain: all finite x.
    /// IEEE Exp2(±0)=1, Exp2(-infinity)=+0, Exp2(+infinity)=+infinity; NaN produces NaN.
    /// Overflow, underflow and accuracy follow T.Exp2's BCL fast profile, without a
    /// cross-platform bit-identity guarantee. No conversion or allocation is introduced
    /// by the facade; custom T owns its special values and allocation behaviour.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Exp2<T>(T x) where T : IExponentialFunctions<T> => T.Exp2(x);

    /// <summary>Base-ten exponential, 10 raised to x. Real domain: all finite x.
    /// IEEE Exp10(±0)=1, Exp10(-infinity)=+0, Exp10(+infinity)=+infinity; NaN produces NaN.
    /// Overflow, underflow and accuracy follow T.Exp10's BCL fast profile, without a
    /// cross-platform bit-identity guarantee. No conversion or allocation is introduced
    /// by the facade; custom T owns its special values and allocation behaviour.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Exp10<T>(T x) where T : IExponentialFunctions<T> => T.Exp10(x);

    /// <summary>Natural exponential minus one, evaluated by T.ExpM1 rather than Exp(x)-1.
    /// Zero inputs are returned unchanged to preserve their sign (the current binary
    /// BCL loses negative zero). Nonzero inputs forward directly. Real domain: all finite x.
    /// IEEE ExpM1(±0)=±0, ExpM1(-infinity)=-1,
    /// ExpM1(+infinity)=+infinity; NaN produces NaN. Overflow, underflow and accuracy
    /// follow T's BCL fast profile, without a cross-platform bit-identity guarantee.
    /// No conversion or allocation is introduced by the facade;
    /// custom T owns its special values and allocation behaviour.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ExpM1<T>(T x) where T : IExponentialFunctions<T> => T.IsZero(x) ? x : T.ExpM1(x);

    /// <summary>Natural logarithm of x. Real domain: x > 0.
    /// IEEE Log(±0)=-infinity, Log(1)=+0, Log(+infinity)=+infinity;
    /// negative inputs and NaN produce NaN. Direct T.Log forward with T's BCL accuracy,
    /// not a correctly rounded cross-platform profile. Custom T owns its domain behaviour.
    /// No conversion or allocation is introduced by the facade.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Log<T>(T x) where T : ILogarithmicFunctions<T> => T.Log(x);
}
