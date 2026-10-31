// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericElementaryTests
{
    [Fact] public void HalfFormat() => Check<Half>();
    [Fact] public void SingleFormat() => Check<float>();
    [Fact] public void DoubleFormat() => Check<double>();
    [Fact] public void Decimal32Format() => Check<Decimal32>();
    [Fact] public void Decimal64Format() => Check<Decimal64>();
    [Fact] public void Decimal128Format() => Check<Decimal128>();

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        Assert.Equal(T.CreateChecked(2), GenericMath.Sqrt(T.CreateChecked(4)));
        Assert.Equal(T.One, GenericMath.Exp(T.Zero));
        Assert.Equal(T.One, GenericMath.Exp(T.NegativeZero));
        Assert.True(T.IsZero(GenericMath.Log(T.One)));
        Assert.False(T.IsNegative(GenericMath.Log(T.One)));
        Assert.Equal(T.PositiveInfinity, GenericMath.Sqrt(T.PositiveInfinity));
        Assert.Equal(T.PositiveInfinity, GenericMath.Exp(T.PositiveInfinity));
        Assert.Equal(T.PositiveInfinity, GenericMath.Log(T.PositiveInfinity));
        Assert.True(T.IsZero(GenericMath.Exp(T.NegativeInfinity)));
        Assert.False(T.IsNegative(GenericMath.Exp(T.NegativeInfinity)));
        foreach (var zero in new[] { T.Zero, T.NegativeZero })
        {
            Assert.True(T.IsZero(GenericMath.Sqrt(zero)));
            Assert.Equal(T.IsNegative(zero), T.IsNegative(GenericMath.Sqrt(zero)));
            Assert.Equal(T.NegativeInfinity, GenericMath.Log(zero));
        }
        foreach (var negative in new[] { -T.One, -T.Epsilon, T.NegativeInfinity })
        {
            Assert.True(T.IsNaN(GenericMath.Sqrt(negative)));
            Assert.True(T.IsNaN(GenericMath.Log(negative)));
        }
        Assert.True(T.IsNaN(GenericMath.Sqrt(T.NaN)));
        Assert.True(T.IsNaN(GenericMath.Exp(T.NaN)));
        Assert.True(T.IsNaN(GenericMath.Log(T.NaN)));
        // Same-runtime forwarding parity, not an external mathematical accuracy oracle.
        foreach (var value in new[] { T.Zero, T.NegativeZero, T.Epsilon,
            T.CreateChecked(0.25m), T.One, T.CreateChecked(2), T.CreateChecked(1000) })
        {
            SameBits(T.Sqrt(value), GenericMath.Sqrt(value));
            SameBits(T.Exp(value), GenericMath.Exp(value));
            SameBits(T.Log(value), GenericMath.Log(value));
        }
        foreach (var value in new[] { -T.Epsilon, -T.One, T.CreateChecked(-1000) })
            SameBits(T.Exp(value), GenericMath.Exp(value));
    }

    private static void SameBits<T>(T expected, T actual) where T : unmanaged
    {
        T[] left = [expected], right = [actual];
        Assert.Equal(MemoryMarshal.AsBytes(left.AsSpan()).ToArray(), MemoryMarshal.AsBytes(right.AsSpan()).ToArray());
    }
}
