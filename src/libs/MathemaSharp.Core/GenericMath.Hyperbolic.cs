// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Hyperbolic sine. Domain: all real values; finite results can overflow.
    /// IEEE Sinh(±0)=±0, Sinh(±infinity)=±infinity, Sinh(NaN)=NaN.
    /// Direct T.Sinh fast-profile forward: accuracy follows T, with no cross-platform
    /// bit-identity guarantee. No conversion or allocation is introduced by the facade.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sinh<T>(T x) where T : IHyperbolicFunctions<T> => T.Sinh(x);

    /// <summary>Hyperbolic cosine. Domain: all real values; finite results can overflow.
    /// IEEE Cosh(±0)=1, Cosh(±infinity)=positive infinity, Cosh(NaN)=NaN.
    /// Direct T.Cosh fast-profile forward: accuracy follows T, with no cross-platform
    /// bit-identity guarantee. Custom T determines its own special values and allocations.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Cosh<T>(T x) where T : IHyperbolicFunctions<T> => T.Cosh(x);

    /// <summary>Hyperbolic tangent. Domain: all real values.
    /// IEEE Tanh(±0)=±0, Tanh(±infinity)=±1, Tanh(NaN)=NaN.
    /// Direct T.Tanh fast-profile forward: accuracy follows T, with no cross-platform
    /// bit-identity guarantee. No conversion or allocation is introduced by the facade.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Tanh<T>(T x) where T : IHyperbolicFunctions<T> => T.Tanh(x);

    /// <summary>Inverse hyperbolic sine. Real domain: all real values.
    /// IEEE Asinh(±0)=±0, Asinh(±infinity)=±infinity, Asinh(NaN)=NaN.
    /// Direct T.Asinh fast-profile forward: accuracy follows T, without a cross-platform
    /// bit-identity guarantee. The facade introduces no conversion or allocation;
    /// custom T defines its own special-value and allocation behaviour.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Asinh<T>(T x) where T : IHyperbolicFunctions<T> => T.Asinh(x);

    /// <summary>Inverse hyperbolic cosine. Real domain: x ≥ 1.
    /// IEEE Acosh(1)=positive zero, Acosh(+infinity)=+infinity;
    /// values below one (including -infinity) and NaN produce NaN.
    /// Direct T.Acosh fast-profile forward: accuracy follows T, without a cross-platform
    /// bit-identity guarantee. The facade introduces no conversion or allocation;
    /// custom T defines its own special-value and allocation behaviour.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Acosh<T>(T x) where T : IHyperbolicFunctions<T> => T.Acosh(x);

    /// <summary>Inverse hyperbolic tangent. Real finite domain: -1 &lt; x &lt; 1.
    /// IEEE Atanh(±0)=±0, Atanh(±1)=±infinity; |x| &gt; 1 and NaN produce NaN.
    /// Direct T.Atanh fast-profile forward: accuracy follows T, without a cross-platform
    /// bit-identity guarantee. The facade introduces no conversion or allocation;
    /// custom T defines its own special-value and allocation behaviour.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Atanh<T>(T x) where T : IHyperbolicFunctions<T> => T.Atanh(x);
}
