// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericRoundingTests
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
        foreach (var (mode, positive, negative) in new[]
        {
            (RoundingMode.HalfToEven, 2, -2),
            (RoundingMode.HalfAwayFromZero, 3, -3),
            (RoundingMode.TowardZero, 2, -2),
            (RoundingMode.TowardNegative, 2, -3),
            (RoundingMode.TowardPositive, 3, -2)
        })
        {
            var value = T.CreateChecked(2.5m);
            Assert.Equal(T.CreateChecked(positive), GenericMath.Round(value, mode));
            Assert.Equal(T.CreateChecked(negative), GenericMath.Round(-value, mode));
            Assert.Equal(GenericMath.Round(value, mode), GenericMath.Round(value, 0, mode));
            var fraction = mode is RoundingMode.HalfAwayFromZero or RoundingMode.TowardPositive ? 1.3m : 1.2m;
            Assert.Equal(T.CreateChecked(fraction), GenericMath.Round(T.CreateChecked(1.25m), 1, mode));
        }
        Assert.Throws<ArgumentOutOfRangeException>(() => GenericMath.Round(T.One, (RoundingMode)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => GenericMath.Round(T.One, 0, (RoundingMode)99));
    }

    private static void CheckIeee<T>() where T : IFloatingPointIeee754<T>
    {
        CheckFinite<T>();
        foreach (var mode in Enum.GetValues<RoundingMode>())
        {
            Assert.True(T.IsNegative(GenericMath.Round(T.NegativeZero, mode)));
            Assert.True(T.IsNaN(GenericMath.Round(T.NaN, mode)));
            Assert.Equal(T.PositiveInfinity, GenericMath.Round(T.PositiveInfinity, mode));
            Assert.Equal(T.NegativeInfinity, GenericMath.Round(T.NegativeInfinity, mode));
            Assert.True(T.IsNegative(GenericMath.Round(T.NegativeZero, 0, mode)));
            Assert.True(T.IsNaN(GenericMath.Round(T.NaN, 0, mode)));
            var expected = mode == RoundingMode.TowardPositive ? T.One : T.Zero;
            Assert.Equal(expected, GenericMath.Round(T.Epsilon, mode));
        }
    }
}
