// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericLogarithmicTests
{
    [Fact] public void HalfFormat() => Check<Half>();
    [Fact] public void SingleFormat() => Check<float>();
    [Fact] public void DoubleFormat() => Check<double>();
    [Fact] public void Decimal32Format() => Check<Decimal32>();
    [Fact] public void Decimal64Format() => Check<Decimal64>();
    [Fact] public void Decimal128Format() => Check<Decimal128>();

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        foreach (var zero in new[] { T.Zero, T.NegativeZero })
        {
            Assert.Equal(T.NegativeInfinity, GenericMath.Log2(zero));
            Assert.Equal(T.NegativeInfinity, GenericMath.Log10(zero));
            Assert.True(T.IsZero(GenericMath.LogP1(zero)));
            Assert.Equal(T.IsNegative(zero), T.IsNegative(GenericMath.LogP1(zero)));
        }
        Assert.True(T.IsZero(GenericMath.Log2(T.One)));
        Assert.False(T.IsNegative(GenericMath.Log2(T.One)));
        Assert.True(T.IsZero(GenericMath.Log10(T.One)));
        Assert.False(T.IsNegative(GenericMath.Log10(T.One)));
        Assert.Equal(T.One, GenericMath.Log2(T.CreateChecked(2)));
        Assert.Equal(T.One, GenericMath.Log10(T.CreateChecked(10)));
        Assert.Equal(T.PositiveInfinity, GenericMath.Log2(T.PositiveInfinity));
        Assert.Equal(T.PositiveInfinity, GenericMath.Log10(T.PositiveInfinity));
        Assert.Equal(T.PositiveInfinity, GenericMath.LogP1(T.PositiveInfinity));
        Assert.Equal(T.NegativeInfinity, GenericMath.LogP1(-T.One));
        foreach (var value in new[] { -T.One, -T.Epsilon, T.NegativeInfinity, T.NaN })
        {
            Assert.True(T.IsNaN(GenericMath.Log2(value)));
            Assert.True(T.IsNaN(GenericMath.Log10(value)));
        }
        foreach (var value in new[] { T.BitDecrement(-T.One), T.CreateChecked(-2), T.NegativeInfinity, T.NaN })
            Assert.True(T.IsNaN(GenericMath.LogP1(value)));
        Assert.True(T.IsFinite(GenericMath.LogP1(T.BitIncrement(-T.One))));
        // Same-runtime forwarding parity, not independent accuracy.
        var nearZero = T.BitIncrement(T.One) - T.One;
        foreach (var value in new[] { T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon,
            nearZero, -nearZero, T.CreateChecked(0.001m), T.CreateChecked(-0.001m),
            T.One, -T.One, T.BitIncrement(-T.One), T.BitDecrement(-T.One),
            T.CreateChecked(2), T.CreateChecked(10), T.CreateChecked(10000),
            T.NaN, T.PositiveInfinity, T.NegativeInfinity })
        {
            SameBits(T.Log2(value), GenericMath.Log2(value));
            SameBits(T.Log10(value), GenericMath.Log10(value));
            // The facade preserves zero signs even where the binary BCL does not.
            SameBits(T.IsZero(value) ? value : T.LogP1(value), GenericMath.LogP1(value));
        }
    }

    private static void SameBits<T>(T expected, T actual) where T : unmanaged
    {
        T[] left = [expected], right = [actual];
        Assert.Equal(MemoryMarshal.AsBytes(left.AsSpan()).ToArray(), MemoryMarshal.AsBytes(right.AsSpan()).ToArray());
    }
}
