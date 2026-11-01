// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericBaseVariantsTests
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
        foreach (var result in new[] { GenericMath.Exp2M1(zero), GenericMath.Exp10M1(zero), GenericMath.Log2P1(zero), GenericMath.Log10P1(zero) })
        {
            Assert.True(T.IsZero(result)); Assert.Equal(T.IsNegative(zero), T.IsNegative(result));
        }
        Assert.Equal(-T.One, GenericMath.Exp2M1(T.NegativeInfinity));
        Assert.Equal(-T.One, GenericMath.Exp10M1(T.NegativeInfinity));
        Assert.Equal(T.PositiveInfinity, GenericMath.Exp2M1(T.PositiveInfinity));
        Assert.Equal(T.PositiveInfinity, GenericMath.Exp10M1(T.PositiveInfinity));
        Assert.Equal(T.NegativeInfinity, GenericMath.Log2P1(-T.One));
        Assert.Equal(T.NegativeInfinity, GenericMath.Log10P1(-T.One));
        foreach (var value in new[] { T.BitDecrement(-T.One), T.NegativeInfinity, T.NaN })
        {
            Assert.True(T.IsNaN(GenericMath.Log2P1(value))); Assert.True(T.IsNaN(GenericMath.Log10P1(value)));
        }
        Assert.True(T.IsNaN(GenericMath.Exp2M1(T.NaN))); Assert.True(T.IsNaN(GenericMath.Exp10M1(T.NaN)));
        T[] values = [T.Epsilon, -T.Epsilon, T.CreateChecked(0.00001m), T.CreateChecked(-0.00001m),
            T.CreateChecked(0.25m), T.CreateChecked(-0.25m), T.One, -T.One,
            T.BitIncrement(-T.One), T.BitDecrement(-T.One), T.CreateChecked(1000),
            T.CreateChecked(-1000), T.PositiveInfinity, T.NegativeInfinity, T.NaN];
        foreach (var value in values)
        {
            SameBits(T.Exp2M1(value), GenericMath.Exp2M1(value)); SameBits(T.Exp10M1(value), GenericMath.Exp10M1(value));
            SameBits(T.Log2P1(value), GenericMath.Log2P1(value)); SameBits(T.Log10P1(value), GenericMath.Log10P1(value));
        }
    }

    private static void SameBits<T>(T expected, T actual) where T : unmanaged
    {
        T[] left = [expected], right = [actual];
        Assert.Equal(MemoryMarshal.AsBytes(left.AsSpan()).ToArray(), MemoryMarshal.AsBytes(right.AsSpan()).ToArray());
    }
}
