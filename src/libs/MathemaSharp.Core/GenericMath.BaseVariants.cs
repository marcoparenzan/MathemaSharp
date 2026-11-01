// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Base-two exponential minus one, evaluated by T.Exp2M1, not Exp2(x)-1.
    /// Zero inputs are returned unchanged to preserve sign. Real domain: finite x;
    /// IEEE negative infinity maps to -1, positive infinity to infinity, NaN propagates.
    /// Overflow, underflow and accuracy follow T's BCL fast profile, without a universal
    /// bound or cross-platform guarantee. No facade conversion/allocation; custom T owns behaviour.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Exp2M1<T>(T x) where T : IExponentialFunctions<T> => T.IsZero(x) ? x : T.Exp2M1(x);

    /// <summary>Base-ten exponential minus one, evaluated by T.Exp10M1, not Exp10(x)-1.
    /// Zero inputs are returned unchanged to preserve sign. Real domain: finite x;
    /// IEEE negative infinity maps to -1, positive infinity to infinity, NaN propagates.
    /// Overflow, underflow and accuracy follow T's BCL fast profile, without a universal
    /// bound or cross-platform guarantee. No facade conversion/allocation; custom T owns behaviour.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Exp10M1<T>(T x) where T : IExponentialFunctions<T> => T.IsZero(x) ? x : T.Exp10M1(x);

    /// <summary>Base-two logarithm of one plus x, evaluated by T.Log2P1 without forming
    /// 1+x. Real domain: x &gt; -1; IEEE -1 maps to negative infinity, values below -1
    /// and NaN produce NaN, positive infinity maps to infinity. Zero inputs retain sign.
    /// Accuracy follows T's BCL fast profile, without a universal bound or cross-platform
    /// guarantee. No facade conversion/allocation; custom T owns behaviour.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Log2P1<T>(T x) where T : ILogarithmicFunctions<T> => T.IsZero(x) ? x : T.Log2P1(x);

    /// <summary>Base-ten logarithm of one plus x, evaluated by T.Log10P1 without forming
    /// 1+x. Real domain: x &gt; -1; IEEE -1 maps to negative infinity, values below -1
    /// and NaN produce NaN, positive infinity maps to infinity. Zero inputs retain sign.
    /// Accuracy follows T's BCL fast profile, without a universal bound or cross-platform
    /// guarantee. No facade conversion/allocation; custom T owns behaviour.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Log10P1<T>(T x) where T : ILogarithmicFunctions<T> => T.IsZero(x) ? x : T.Log10P1(x);
}
