// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericRootsTests
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
            var cube = GenericMath.Cbrt(zero);
            Assert.True(T.IsZero(cube)); Assert.Equal(T.IsNegative(zero), T.IsNegative(cube));
            var length = GenericMath.Hypot(zero, zero);
            Assert.True(T.IsZero(length)); Assert.False(T.IsNegative(length));
        }
        Assert.Equal(T.One, GenericMath.Cbrt(T.One));
        Assert.Equal(-T.One, GenericMath.Cbrt(-T.One));
        Assert.Equal(T.CreateChecked(5), GenericMath.Hypot(T.CreateChecked(3), T.CreateChecked(4)));
        Assert.True(T.IsNaN(GenericMath.Cbrt(T.NaN)));
        Assert.Equal(T.PositiveInfinity, GenericMath.Cbrt(T.PositiveInfinity));
        Assert.Equal(T.NegativeInfinity, GenericMath.Cbrt(T.NegativeInfinity));
        T[] values = [T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon, T.One, -T.One,
            T.CreateChecked(0.25m), T.CreateChecked(-0.25m), T.CreateChecked(8),
            T.CreateChecked(-8), T.CreateChecked(1000), T.PositiveInfinity, T.NegativeInfinity, T.NaN];
        foreach (var value in values)
        {
            SameBits(T.Cbrt(value), GenericMath.Cbrt(value));
            foreach (int n in new[] { int.MinValue, -3, -2, -1, 0, 1, 2, 3, 4, int.MaxValue })
                SameBits(T.RootN(value, n), GenericMath.RootN(value, n));
            foreach (var other in values)
                SameBits(T.Hypot(value, other), GenericMath.Hypot(value, other));
        }
    }

    private static void SameBits<T>(T expected, T actual) where T : unmanaged
    {
        T[] left = [expected], right = [actual];
        Assert.Equal(MemoryMarshal.AsBytes(left.AsSpan()).ToArray(), MemoryMarshal.AsBytes(right.AsSpan()).ToArray());
    }
}
