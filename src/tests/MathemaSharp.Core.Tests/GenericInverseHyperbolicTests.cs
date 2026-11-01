// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericInverseHyperbolicTests
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
            Assert.True(T.IsZero(GenericMath.Asinh(zero)));
            Assert.True(T.IsZero(GenericMath.Atanh(zero)));
            Assert.Equal(T.IsNegative(zero), T.IsNegative(GenericMath.Asinh(zero)));
            Assert.Equal(T.IsNegative(zero), T.IsNegative(GenericMath.Atanh(zero)));
            Assert.True(T.IsNaN(GenericMath.Acosh(zero)));
        }
        Assert.True(T.IsZero(GenericMath.Acosh(T.One)));
        Assert.False(T.IsNegative(GenericMath.Acosh(T.One)));
        Assert.Equal(T.PositiveInfinity, GenericMath.Asinh(T.PositiveInfinity));
        Assert.Equal(T.NegativeInfinity, GenericMath.Asinh(T.NegativeInfinity));
        Assert.Equal(T.PositiveInfinity, GenericMath.Acosh(T.PositiveInfinity));
        Assert.True(T.IsNaN(GenericMath.Acosh(T.NegativeInfinity)));
        Assert.Equal(T.PositiveInfinity, GenericMath.Atanh(T.One));
        Assert.Equal(T.NegativeInfinity, GenericMath.Atanh(-T.One));
        foreach (var value in new[] { T.PositiveInfinity, T.NegativeInfinity, T.CreateChecked(2), T.CreateChecked(-2), T.NaN })
            Assert.True(T.IsNaN(GenericMath.Atanh(value)));
        foreach (var value in new[] { T.BitDecrement(T.One), -T.One, T.NaN })
            Assert.True(T.IsNaN(GenericMath.Acosh(value)));
        Assert.True(T.IsNaN(GenericMath.Asinh(T.NaN)));
        Assert.True(T.IsFinite(GenericMath.Atanh(T.BitDecrement(T.One))));
        Assert.True(T.IsFinite(GenericMath.Atanh(T.BitIncrement(-T.One))));
        Assert.True(T.IsNaN(GenericMath.Atanh(T.BitIncrement(T.One))));
        Assert.True(T.IsNaN(GenericMath.Atanh(T.BitDecrement(-T.One))));
        Assert.True(T.IsFinite(GenericMath.Acosh(T.BitIncrement(T.One))));
        // BCL parity tests do not establish independent mathematical accuracy.
        foreach (var value in new[] { T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon,
            T.CreateChecked(0.25m), T.CreateChecked(-0.25m), T.One, -T.One,
            T.BitDecrement(T.One), T.BitIncrement(T.One), T.BitDecrement(-T.One), T.BitIncrement(-T.One),
            T.CreateChecked(2), T.CreateChecked(-2), T.CreateChecked(1000), T.CreateChecked(-1000),
            T.NaN, T.PositiveInfinity, T.NegativeInfinity })
        {
            SameBits(T.Asinh(value), GenericMath.Asinh(value));
            SameBits(T.Acosh(value), GenericMath.Acosh(value));
            SameBits(T.Atanh(value), GenericMath.Atanh(value));
        }
    }

    private static void SameBits<T>(T expected, T actual) where T : unmanaged
    {
        T[] left = [expected], right = [actual];
        Assert.Equal(MemoryMarshal.AsBytes(left.AsSpan()).ToArray(), MemoryMarshal.AsBytes(right.AsSpan()).ToArray());
    }
}
