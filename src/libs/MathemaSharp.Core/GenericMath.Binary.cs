// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Raises x to y by directly calling T.Pow. Positive finite x accepts
    /// finite y; negative x has real results for appropriate integer exponents.
    /// Zero, negative-base/noninteger exponent, NaN, infinities, overflow and underflow
    /// follow T's BCL rules, including its indeterminate-form conventions. No implicit
    /// complex promotion. Accuracy follows the fast profile, not a universal bound or
    /// cross-platform bit guarantee. No facade conversion/allocation; custom T owns behaviour.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Pow<T>(T x, T y) where T : IPowerFunctions<T> => T.Pow(x, y);

    /// <summary>Logarithm of x to base newBase, evaluated by T.Log(x, newBase), not
    /// a facade rewrite using two logarithms. Ordinary real domain: x &gt; 0 and
    /// newBase &gt; 0, newBase != 1. Zeros, negative inputs, NaN and infinite bases/inputs
    /// follow T's BCL conventions, without implicit complex promotion. Accuracy follows
    /// the fast profile, not a universal bound or cross-platform bit guarantee.
    /// No facade conversion/allocation; custom T owns behaviour.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Log<T>(T x, T newBase) where T : ILogarithmicFunctions<T> => T.Log(x, newBase);
}
