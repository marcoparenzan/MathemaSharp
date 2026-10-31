// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Returns the greatest integral value not greater than x, in T.
    /// Domain: all values supported by T. Direct BCL forward; no conversion or allocation
    /// introduced by the facade. IEEE formats preserve signed zero and pass through NaN
    /// and infinities. Finite results follow T.Floor; no cross-platform bit guarantee is added.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Floor<T>(T x) where T : IFloatingPoint<T> => T.Floor(x);

    /// <summary>Returns the least integral value not less than x, in T.
    /// Domain: all values supported by T. Direct BCL forward; no conversion or allocation
    /// introduced by the facade. IEEE formats preserve signed zero and pass through NaN
    /// and infinities. Finite results follow T.Ceiling; no cross-platform bit guarantee is added.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Ceiling<T>(T x) where T : IFloatingPoint<T> => T.Ceiling(x);

    /// <summary>Returns the integral part of x by rounding toward zero, in T.
    /// Domain: all values supported by T. Direct BCL forward; no conversion or allocation
    /// introduced by the facade. IEEE formats preserve signed zero and pass through NaN
    /// and infinities. Finite results follow T.Truncate; no cross-platform bit guarantee is added.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Truncate<T>(T x) where T : IFloatingPoint<T> => T.Truncate(x);
}
