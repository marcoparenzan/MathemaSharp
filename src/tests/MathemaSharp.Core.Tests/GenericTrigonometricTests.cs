// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericTrigonometricTests
{
    [Fact] public void HalfFormat() => Check<Half>();
    [Fact] public void SingleFormat() => Check<float>();
    [Fact] public void DoubleFormat() => Check<double>();
    [Fact] public void Decimal32Format() => Check<Decimal32>();
    [Fact] public void Decimal64Format() => Check<Decimal64>();
    [Fact] public void Decimal128Format() => Check<Decimal128>();

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        // Independent analytic anchors; all nonzero samples below test forwarding,
        // not the BCL's mathematical accuracy against an external oracle.
        Assert.Equal(T.One, GenericMath.Cos(T.Zero));
        Assert.Equal(T.One, GenericMath.Cos(T.NegativeZero));
        foreach (var zero in new[] { T.Zero, T.NegativeZero })
        {
            Assert.True(T.IsZero(GenericMath.Sin(zero)));
            Assert.True(T.IsZero(GenericMath.Tan(zero)));
            Assert.Equal(T.IsNegative(zero), T.IsNegative(GenericMath.Sin(zero)));
            Assert.Equal(T.IsNegative(zero), T.IsNegative(GenericMath.Tan(zero)));
        }
        foreach (var value in new[] { T.NaN, T.PositiveInfinity, T.NegativeInfinity })
        {
            Assert.True(T.IsNaN(GenericMath.Sin(value)));
            Assert.True(T.IsNaN(GenericMath.Cos(value)));
            Assert.True(T.IsNaN(GenericMath.Tan(value)));
        }
        foreach (var value in new[] { T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon,
            T.CreateChecked(0.25m), T.CreateChecked(-0.25m), T.One, -T.One,
            T.Pi / T.CreateChecked(2), T.Pi, T.CreateChecked(1000), T.CreateChecked(-1000) })
        {
            SameBits(T.Sin(value), GenericMath.Sin(value));
            SameBits(T.Cos(value), GenericMath.Cos(value));
            SameBits(T.Tan(value), GenericMath.Tan(value));
        }
    }

    private static void SameBits<T>(T expected, T actual) where T : unmanaged
    {
        T[] left = [expected], right = [actual];
        Assert.Equal(MemoryMarshal.AsBytes(left.AsSpan()).ToArray(), MemoryMarshal.AsBytes(right.AsSpan()).ToArray());
    }
}
