// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Sine of pi times x, evaluated by T.SinPi without forming a rounded
    /// pi*x. Real domain: finite x; IEEE NaN and infinities produce NaN. Signed-zero
    /// and argument-reduction rules follow T. BCL fast-profile accuracy, without a
    /// universal bound or cross-platform bit guarantee. No facade conversion/allocation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SinPi<T>(T x) where T : ITrigonometricFunctions<T> => T.SinPi(x);

    /// <summary>Cosine of pi times x, evaluated by T.CosPi without forming a rounded
    /// pi*x. Real domain: finite x; IEEE NaN and infinities produce NaN. Argument
    /// reduction follows T. BCL fast-profile accuracy, without a universal bound or
    /// cross-platform bit guarantee. No facade conversion/allocation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CosPi<T>(T x) where T : ITrigonometricFunctions<T> => T.CosPi(x);

    /// <summary>Tangent of pi times x, evaluated by T.TanPi without forming a rounded
    /// pi*x. Mathematically undefined at half-integers; poles, signed-zero and argument
    /// reduction follow T. IEEE NaN and infinities produce NaN. BCL fast-profile
    /// accuracy, without a universal bound or cross-platform bit guarantee.
    /// No facade conversion/allocation; custom T owns special values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T TanPi<T>(T x) where T : ITrigonometricFunctions<T> => T.TanPi(x);

    /// <summary>Principal inverse sine divided by pi, evaluated by T.AsinPi, not a
    /// division by rounded pi. Real domain [-1, 1], range [-1/2, 1/2]. IEEE NaN,
    /// infinities and outside-domain inputs produce NaN; zero rules follow T.
    /// BCL fast-profile accuracy with no universal bound/cross-platform bit guarantee.
    /// No facade conversion/allocation; custom T owns special values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AsinPi<T>(T x) where T : ITrigonometricFunctions<T> => T.AsinPi(x);

    /// <summary>Principal inverse cosine divided by pi, evaluated by T.AcosPi, not
    /// a division by rounded pi. Real domain [-1, 1], range [0, 1]. IEEE NaN,
    /// infinities and outside-domain inputs produce NaN. BCL fast-profile accuracy
    /// with no universal bound/cross-platform bit guarantee. No facade conversion
    /// or allocation; custom T owns special values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AcosPi<T>(T x) where T : ITrigonometricFunctions<T> => T.AcosPi(x);

    /// <summary>Principal inverse tangent divided by pi, evaluated by T.AtanPi, not
    /// a division by rounded pi. Real domain: finite x, range [-1/2, 1/2] including
    /// infinite-input limits. IEEE NaN propagates; signed zeros and limits follow T.
    /// BCL fast-profile accuracy with no universal bound/cross-platform bit guarantee.
    /// No facade conversion/allocation; custom T owns special values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AtanPi<T>(T x) where T : ITrigonometricFunctions<T> => T.AtanPi(x);
}
