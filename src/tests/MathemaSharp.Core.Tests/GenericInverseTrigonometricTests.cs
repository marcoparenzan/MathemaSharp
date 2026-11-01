// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericInverseTrigonometricTests
{
    [Fact] public void HalfFormat() => Check<Half>();
    [Fact] public void SingleFormat() => Check<float>();
    [Fact] public void DoubleFormat() => Check<double>();
    [Fact] public void Decimal32Format() => Check<Decimal32>();
    [Fact] public void Decimal64Format() => Check<Decimal64>();
    [Fact] public void Decimal128Format() => Check<Decimal128>();

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        // Exact analytic anchors are separate from same-runtime forwarding checks.
        foreach (var zero in new[] { T.Zero, T.NegativeZero })
        {
            var asin = GenericMath.Asin(zero);
            var atan = GenericMath.Atan(zero);
            Assert.True(T.IsZero(asin));
            Assert.True(T.IsZero(atan));
            Assert.Equal(T.IsNegative(zero), T.IsNegative(asin));
            Assert.Equal(T.IsNegative(zero), T.IsNegative(atan));
        }
        var acosOne = GenericMath.Acos(T.One);
        Assert.True(T.IsZero(acosOne));
        Assert.False(T.IsNegative(acosOne));
        foreach (var value in new[] { T.NaN, T.PositiveInfinity, T.NegativeInfinity,
            T.BitIncrement(T.One), T.BitDecrement(-T.One), T.CreateChecked(2), T.CreateChecked(-2) })
        {
            Assert.True(T.IsNaN(GenericMath.Asin(value)));
            Assert.True(T.IsNaN(GenericMath.Acos(value)));
        }
        Assert.True(T.IsNaN(GenericMath.Atan(T.NaN)));
        var positiveLimit = GenericMath.Atan(T.PositiveInfinity);
        var negativeLimit = GenericMath.Atan(T.NegativeInfinity);
        Assert.True(T.IsFinite(positiveLimit) && positiveLimit > T.One);
        Assert.True(T.IsFinite(negativeLimit) && negativeLimit < -T.One);
        SameBits(-positiveLimit, negativeLimit);
        foreach (var value in new[] { T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon,
            T.CreateChecked(0.25m), T.CreateChecked(-0.25m), T.One, -T.One,
            T.BitDecrement(T.One), T.BitIncrement(T.One), T.BitIncrement(-T.One),
            T.BitDecrement(-T.One), T.CreateChecked(1000), T.CreateChecked(-1000),
            T.PositiveInfinity, T.NegativeInfinity, T.NaN })
        {
            SameBits(T.Asin(value), GenericMath.Asin(value));
            SameBits(T.Acos(value), GenericMath.Acos(value));
            SameBits(T.Atan(value), GenericMath.Atan(value));
        }
    }

    private static void SameBits<T>(T expected, T actual) where T : unmanaged
    {
        T[] left = [expected], right = [actual];
        Assert.Equal(MemoryMarshal.AsBytes(left.AsSpan()).ToArray(), MemoryMarshal.AsBytes(right.AsSpan()).ToArray());
    }
}
