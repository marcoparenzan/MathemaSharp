// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericIntegralRoundingTests
{
    [Fact] public void HalfFormat() => CheckIeee<Half>();
    [Fact] public void SingleFormat() => CheckIeee<float>();
    [Fact] public void DoubleFormat() => CheckIeee<double>();
    [Fact] public void Decimal32Format() => CheckIeee<Decimal32>();
    [Fact] public void Decimal64Format() => CheckIeee<Decimal64>();
    [Fact] public void Decimal128Format() => CheckIeee<Decimal128>();
    [Fact] public void BclDecimalFormat() => CheckFinite<decimal>();

    private static void CheckFinite<T>() where T : IFloatingPoint<T>
    {
        foreach (var (value, floor, ceiling, truncate) in new[]
        {
            (2.5m, 2, 3, 2), (-2.5m, -3, -2, -2),
            (0.25m, 0, 1, 0), (-0.25m, -1, 0, 0),
            (3m, 3, 3, 3), (-3m, -3, -3, -3), (0m, 0, 0, 0)
        })
        {
            var x = T.CreateChecked(value);
            Assert.Equal(T.CreateChecked(floor), GenericMath.Floor(x));
            Assert.Equal(T.CreateChecked(ceiling), GenericMath.Ceiling(x));
            Assert.Equal(T.CreateChecked(truncate), GenericMath.Truncate(x));
        }
    }

    private static void CheckIeee<T>() where T : IFloatingPointIeee754<T>
    {
        CheckFinite<T>();
        foreach (var operation in new Func<T, T>[] { GenericMath.Floor<T>, GenericMath.Ceiling<T>, GenericMath.Truncate<T> })
        {
            var positiveZero = operation(T.Zero);
            var negativeZero = operation(T.NegativeZero);
            Assert.True(T.IsZero(positiveZero));
            Assert.False(T.IsNegative(positiveZero));
            Assert.True(T.IsZero(negativeZero));
            Assert.True(T.IsNegative(negativeZero));
            Assert.True(T.IsNaN(operation(T.NaN)));
            Assert.Equal(T.PositiveInfinity, operation(T.PositiveInfinity));
            Assert.Equal(T.NegativeInfinity, operation(T.NegativeInfinity));
        }
        Assert.Equal(T.Zero, GenericMath.Floor(T.Epsilon));
        Assert.Equal(T.One, GenericMath.Ceiling(T.Epsilon));
        Assert.Equal(-T.One, GenericMath.Floor(-T.Epsilon));
        Assert.True(T.IsZero(GenericMath.Ceiling(-T.Epsilon)));
        Assert.True(T.IsNegative(GenericMath.Ceiling(-T.Epsilon)));
        Assert.True(T.IsZero(GenericMath.Truncate(-T.Epsilon)));
        Assert.True(T.IsNegative(GenericMath.Truncate(-T.Epsilon)));
    }
}
