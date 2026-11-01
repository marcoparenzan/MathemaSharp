// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Real cube root. Domain: all real finite values. IEEE signed zeros and
    /// infinities retain their sign; NaN propagates. Direct T.Cbrt fast-profile forward;
    /// accuracy follows T's BCL, without a universal bound or cross-platform bit guarantee.
    /// No conversion or allocation is introduced; custom T owns its special values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Cbrt<T>(T x) where T : IRootFunctions<T> => T.Cbrt(x);

    /// <summary>Euclidean length of (x, y), computed by T.Hypot rather than squaring
    /// inputs in the facade. Finite domain: all real pairs. IEEE result is nonnegative,
    /// including +0 for zero inputs; infinity and NaN handling follows T.Hypot.
    /// Overflow, underflow and accuracy follow the BCL fast profile, with no universal
    /// bound or cross-platform bit guarantee. No conversion or allocation is introduced;
    /// custom T owns its special values and allocation behaviour.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Hypot<T>(T x, T y) where T : IRootFunctions<T> => T.Hypot(x, y);

    /// <summary>Real nth root, directly evaluated by T.RootN(x, n). For positive n,
    /// odd roots permit negative x; even roots require nonnegative x. Negative n
    /// denotes the reciprocal root, subject to T's zero, overflow and underflow rules.
    /// Degree zero, signed zeros, infinities and NaN follow T.RootN's BCL semantics;
    /// this facade adds no degree restriction or language-specific coercion.
    /// Accuracy follows the fast profile without a universal bound or cross-platform
    /// bit guarantee. No conversion or allocation is introduced; custom T owns behaviour.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T RootN<T>(T x, int n) where T : IRootFunctions<T> => T.RootN(x, n);
}
