// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericExponentialTests
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
            Assert.Equal(T.One, GenericMath.Exp2(zero));
            Assert.Equal(T.One, GenericMath.Exp10(zero));
            Assert.True(T.IsZero(GenericMath.ExpM1(zero)));
            Assert.Equal(T.IsNegative(zero), T.IsNegative(GenericMath.ExpM1(zero)));
        }
        Assert.Equal(T.CreateChecked(2), GenericMath.Exp2(T.One));
        Assert.Equal(T.CreateChecked(10), GenericMath.Exp10(T.One));
        Assert.Equal(T.PositiveInfinity, GenericMath.Exp2(T.PositiveInfinity));
        Assert.Equal(T.PositiveInfinity, GenericMath.Exp10(T.PositiveInfinity));
        Assert.Equal(T.PositiveInfinity, GenericMath.ExpM1(T.PositiveInfinity));
        Assert.True(T.IsZero(GenericMath.Exp2(T.NegativeInfinity)));
        Assert.False(T.IsNegative(GenericMath.Exp2(T.NegativeInfinity)));
        Assert.True(T.IsZero(GenericMath.Exp10(T.NegativeInfinity)));
        Assert.False(T.IsNegative(GenericMath.Exp10(T.NegativeInfinity)));
        Assert.Equal(-T.One, GenericMath.ExpM1(T.NegativeInfinity));
        Assert.True(T.IsNaN(GenericMath.Exp2(T.NaN)));
        Assert.True(T.IsNaN(GenericMath.Exp10(T.NaN)));
        Assert.True(T.IsNaN(GenericMath.ExpM1(T.NaN)));
        // Same-runtime forwarding parity, not an independent accuracy oracle.
        var nearZero = T.BitIncrement(T.One) - T.One;
        foreach (var value in new[] { T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon,
            nearZero, -nearZero, T.CreateChecked(0.001m), T.CreateChecked(-0.001m),
            T.CreateChecked(0.25m), T.One, -T.One, T.CreateChecked(10), T.CreateChecked(-10),
            T.CreateChecked(10000), T.CreateChecked(-10000), T.NaN, T.PositiveInfinity, T.NegativeInfinity })
        {
            SameBits(T.Exp2(value), GenericMath.Exp2(value));
            SameBits(T.Exp10(value), GenericMath.Exp10(value));
            // Preserve the IEEE zero sign even where the BCL returns positive zero.
            SameBits(T.IsZero(value) ? value : T.ExpM1(value), GenericMath.ExpM1(value));
        }
    }

    private static void SameBits<T>(T expected, T actual) where T : unmanaged
    {
        T[] left = [expected], right = [actual];
        Assert.Equal(MemoryMarshal.AsBytes(left.AsSpan()).ToArray(), MemoryMarshal.AsBytes(right.AsSpan()).ToArray());
    }
}
