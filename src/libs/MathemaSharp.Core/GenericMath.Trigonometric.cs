// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Sine of x in radians. Domain: all real finite values. IEEE Sin(±0)=±0;
    /// NaN and infinities produce NaN. Direct T.Sin forward; accuracy and argument reduction
    /// follow T's BCL implementation, not a correctly rounded cross-platform profile.
    /// No conversion or allocation is introduced by the facade; custom T defines its own special values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sin<T>(T x) where T : ITrigonometricFunctions<T> => T.Sin(x);

    /// <summary>Cosine of x in radians. Domain: all real finite values. IEEE Cos(±0)=1;
    /// NaN and infinities produce NaN. Direct T.Cos forward; accuracy and argument reduction
    /// follow T's BCL implementation, not a correctly rounded cross-platform profile.
    /// No conversion or allocation is introduced by the facade; custom T defines its own special values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Cos<T>(T x) where T : ITrigonometricFunctions<T> => T.Cos(x);

    /// <summary>Tangent of x in radians. Mathematically undefined at odd multiples of pi/2;
    /// finite representable arguments follow T.Tan's argument reduction and overflow behaviour.
    /// IEEE Tan(±0)=±0; NaN and infinities produce NaN. Direct BCL fast-profile forward;
    /// no universal accuracy bound or cross-platform bit identity is added.
    /// No conversion or allocation is introduced by the facade; custom T defines its own special values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Tan<T>(T x) where T : ITrigonometricFunctions<T> => T.Tan(x);

    /// <summary>Inverse sine in radians, with principal range [-pi/2, pi/2].
    /// Real domain: [-1, 1]. IEEE Asin(±0)=±0; NaN, infinities and values outside
    /// the domain produce NaN. Direct T.Asin fast-profile forward: accuracy follows
    /// T's BCL implementation, with no universal bound or cross-platform bit guarantee.
    /// No conversion or allocation is introduced; custom T defines its own special values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Asin<T>(T x) where T : ITrigonometricFunctions<T> => T.Asin(x);

    /// <summary>Inverse cosine in radians, with principal range [0, pi].
    /// Real domain: [-1, 1]. IEEE Acos(1)=+0; NaN, infinities and values outside
    /// the domain produce NaN. Direct T.Acos fast-profile forward: accuracy follows
    /// T's BCL implementation, with no universal bound or cross-platform bit guarantee.
    /// No conversion or allocation is introduced; custom T defines its own special values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Acos<T>(T x) where T : ITrigonometricFunctions<T> => T.Acos(x);

    /// <summary>Inverse tangent in radians, with principal range [-pi/2, pi/2].
    /// Real domain: all finite values. IEEE Atan(±0)=±0, Atan(±infinity)=±pi/2,
    /// and NaN propagates. Direct T.Atan fast-profile forward: accuracy follows
    /// T's BCL implementation, with no universal bound or cross-platform bit guarantee.
    /// No conversion or allocation is introduced; custom T defines its own special values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Atan<T>(T x) where T : ITrigonometricFunctions<T> => T.Atan(x);
}
