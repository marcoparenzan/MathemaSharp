// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericHyperbolicTests
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
            Assert.Equal(T.One, GenericMath.Cosh(zero));
            Assert.True(T.IsZero(GenericMath.Sinh(zero)));
            Assert.True(T.IsZero(GenericMath.Tanh(zero)));
            Assert.Equal(T.IsNegative(zero), T.IsNegative(GenericMath.Sinh(zero)));
            Assert.Equal(T.IsNegative(zero), T.IsNegative(GenericMath.Tanh(zero)));
        }
        Assert.Equal(T.PositiveInfinity, GenericMath.Sinh(T.PositiveInfinity));
        Assert.Equal(T.NegativeInfinity, GenericMath.Sinh(T.NegativeInfinity));
        Assert.Equal(T.PositiveInfinity, GenericMath.Cosh(T.PositiveInfinity));
        Assert.Equal(T.PositiveInfinity, GenericMath.Cosh(T.NegativeInfinity));
        Assert.Equal(T.One, GenericMath.Tanh(T.PositiveInfinity));
        Assert.Equal(-T.One, GenericMath.Tanh(T.NegativeInfinity));
        Assert.True(T.IsNaN(GenericMath.Sinh(T.NaN)));
        Assert.True(T.IsNaN(GenericMath.Cosh(T.NaN)));
        Assert.True(T.IsNaN(GenericMath.Tanh(T.NaN)));
        // BCL forwarding parity is not an independent accuracy oracle.
        foreach (var value in new[] { T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon,
            T.CreateChecked(0.25m), T.CreateChecked(-0.25m), T.One, -T.One,
            T.CreateChecked(10), T.CreateChecked(-10), T.CreateChecked(1000), T.CreateChecked(-1000),
            T.NaN, T.PositiveInfinity, T.NegativeInfinity })
        {
            SameBits(T.Sinh(value), GenericMath.Sinh(value));
            SameBits(T.Cosh(value), GenericMath.Cosh(value));
            SameBits(T.Tanh(value), GenericMath.Tanh(value));
        }
    }

    private static void SameBits<T>(T expected, T actual) where T : unmanaged
    {
        T[] left = [expected], right = [actual];
        Assert.Equal(MemoryMarshal.AsBytes(left.AsSpan()).ToArray(), MemoryMarshal.AsBytes(right.AsSpan()).ToArray());
    }
}
