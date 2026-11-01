// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericPiTrigTests
{
    [Fact] public void HalfFormat() => Check<Half>();
    [Fact] public void SingleFormat() => Check<float>();
    [Fact] public void DoubleFormat() => Check<double>();
    [Fact] public void Decimal32Format() => Check<Decimal32>();
    [Fact] public void Decimal64Format() => Check<Decimal64>();
    [Fact] public void Decimal128Format() => Check<Decimal128>();

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        var half = T.CreateChecked(0.5m);
        // Exactly representable analytic anchors, separate from BCL bit parity.
        Assert.Equal(T.One, GenericMath.CosPi(T.Zero));
        Assert.Equal(T.One, GenericMath.SinPi(half));
        Assert.Equal(-T.One, GenericMath.SinPi(-half));
        Assert.Equal(-T.One, GenericMath.CosPi(T.One));
        Assert.Equal(half, GenericMath.AsinPi(T.One));
        Assert.Equal(-half, GenericMath.AsinPi(-T.One));
        Assert.Equal(T.One, GenericMath.AcosPi(-T.One));
        Assert.Equal(T.Zero, GenericMath.AcosPi(T.One));
        foreach (var value in new[] { T.NaN, T.PositiveInfinity, T.NegativeInfinity })
        {
            Assert.True(T.IsNaN(GenericMath.SinPi(value)));
            Assert.True(T.IsNaN(GenericMath.CosPi(value)));
            Assert.True(T.IsNaN(GenericMath.TanPi(value)));
            Assert.True(T.IsNaN(GenericMath.AsinPi(value)));
            Assert.True(T.IsNaN(GenericMath.AcosPi(value)));
        }
        foreach (var value in new[] { T.BitIncrement(T.One), T.BitDecrement(-T.One) })
        {
            Assert.True(T.IsNaN(GenericMath.AsinPi(value)));
            Assert.True(T.IsNaN(GenericMath.AcosPi(value)));
        }
        T[] values = [T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon,
            T.CreateChecked(0.25m), T.CreateChecked(-0.25m), half, -half,
            T.BitIncrement(half), T.BitDecrement(half), T.One, -T.One,
            T.BitIncrement(T.One), T.BitDecrement(T.One), T.BitIncrement(-T.One),
            T.BitDecrement(-T.One), T.CreateChecked(2), T.CreateChecked(-2),
            T.CreateChecked(1000), T.PositiveInfinity, T.NegativeInfinity, T.NaN];
        foreach (var value in values)
        {
            SameBits(T.SinPi(value), GenericMath.SinPi(value));
            SameBits(T.CosPi(value), GenericMath.CosPi(value));
            SameBits(T.TanPi(value), GenericMath.TanPi(value));
            SameBits(T.AsinPi(value), GenericMath.AsinPi(value));
            SameBits(T.AcosPi(value), GenericMath.AcosPi(value));
            SameBits(T.AtanPi(value), GenericMath.AtanPi(value));
        }
    }

    private static void SameBits<T>(T expected, T actual) where T : unmanaged
    {
        T[] left = [expected], right = [actual];
        Assert.Equal(MemoryMarshal.AsBytes(left.AsSpan()).ToArray(), MemoryMarshal.AsBytes(right.AsSpan()).ToArray());
    }
}
