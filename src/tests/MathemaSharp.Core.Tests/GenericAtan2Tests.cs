// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericAtan2Tests
{
    [Fact] public void HalfFormat() => Check<Half>();
    [Fact] public void SingleFormat() => Check<float>();
    [Fact] public void DoubleFormat() => Check<double>();
    [Fact] public void Decimal32Format() => Check<Decimal32>();
    [Fact] public void Decimal64Format() => Check<Decimal64>();
    [Fact] public void Decimal128Format() => Check<Decimal128>();

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        foreach (var y in new[] { T.Zero, T.NegativeZero })
        {
            foreach (var x in new[] { T.Zero, T.One, T.PositiveInfinity })
            {
                var result = GenericMath.Atan2(y, x);
                Assert.True(T.IsZero(result));
                Assert.Equal(T.IsNegative(y), T.IsNegative(result));
            }
            foreach (var x in new[] { T.NegativeZero, -T.One, T.NegativeInfinity })
            {
                var result = GenericMath.Atan2(y, x);
                Assert.True(T.IsFinite(result));
                Assert.True(T.Abs(result) > T.CreateChecked(3));
                Assert.Equal(T.IsNegative(y), T.IsNegative(result));
            }
        }
        // Quadrant and argument-order anchors do not rely on an external oracle.
        var first = GenericMath.Atan2(T.One, T.CreateChecked(2));
        var swapped = GenericMath.Atan2(T.CreateChecked(2), T.One);
        Assert.True(first > T.Zero && first < T.One);
        Assert.True(swapped > T.One && swapped < T.CreateChecked(2));
        var second = GenericMath.Atan2(T.One, -T.One);
        var third = GenericMath.Atan2(-T.One, -T.One);
        var fourth = GenericMath.Atan2(-T.One, T.One);
        Assert.True(second > T.CreateChecked(2) && second < T.CreateChecked(3));
        Assert.True(third < T.CreateChecked(-2) && third > T.CreateChecked(-3));
        Assert.True(fourth < T.Zero && fourth > -T.One);
        SameBits(-second, third);
        T[] values = [T.Zero, T.NegativeZero, T.One, -T.One, T.Epsilon, -T.Epsilon,
            T.CreateChecked(0.25m), T.CreateChecked(-0.25m), T.CreateChecked(1000),
            T.CreateChecked(-1000), T.PositiveInfinity, T.NegativeInfinity, T.NaN];
        foreach (var y in values)
        foreach (var x in values)
        {
            var actual = GenericMath.Atan2(y, x);
            SameBits(T.Atan2(y, x), actual);
            if (T.IsNaN(y) || T.IsNaN(x)) Assert.True(T.IsNaN(actual));
        }
    }

    private static void SameBits<T>(T expected, T actual) where T : unmanaged
    {
        T[] left = [expected], right = [actual];
        Assert.Equal(MemoryMarshal.AsBytes(left.AsSpan()).ToArray(), MemoryMarshal.AsBytes(right.AsSpan()).ToArray());
    }
}
