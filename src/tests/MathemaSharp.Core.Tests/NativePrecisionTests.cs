using System.Numerics;
using MathemaSharp.Core;

namespace MathemaSharp.Core.Tests;

/// <summary>Fixtures and comparisons in T: no numeric value passes through double.</summary>
public class NativePrecisionTests
{
    private static void Verify<T>() where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        T N(int n) => T.CreateChecked(n);
        var epsilon = T.BitIncrement(T.One) - T.One;
        var tolerance = N(16) * epsilon;
        Assert.True(Precision.NearlyEqual(T.One, T.BitIncrement(T.One)));
        Assert.False(Precision.NearlyEqual(T.One, N(2)));
        Assert.False(Precision.NearlyEqual(T.Zero, T.One));
        Assert.False(Precision.NearlyEqual(T.NaN, T.NaN));
        Assert.True(Precision.NearlyEqual(T.PositiveInfinity, T.PositiveInfinity));
        Assert.False(Precision.NearlyEqual(T.PositiveInfinity, T.MaxValue));
        Assert.False(Precision.NearlyEqual(T.MaxValue, -T.MaxValue));
        Assert.True(Precision.NearlyEqual(T.MaxValue, T.BitDecrement(T.MaxValue), T.Zero, N(4) * epsilon));
        Assert.Throws<ArgumentOutOfRangeException>(() => Precision.NearlyEqual(T.One, T.One, -T.One, epsilon));
        Assert.Throws<ArgumentOutOfRangeException>(() => Precision.NearlyEqual(T.One, T.One, epsilon, T.NaN));

        T[] real = [T.One, N(2), N(3)];
        Assert.Equal(N(6), NumericAlgorithms.Sum<T>(real));
        Assert.Equal(N(2), NumericAlgorithms.Mean<T>(real));
        Assert.Equal(N(6), NumericAlgorithms.KahanSum<T>(real));
        Assert.Equal(N(14), Signals.VectorDot<T>(real, real));
        Assert.Equal(N(17), NumericAlgorithms.Horner<T>(real, N(2)));

        Complex<T>[] z = [new(N(3), N(4))];
        Assert.Equal(new Complex<T>(N(25), T.Zero), Signals.HermitianDot<T>(z, z));
        Assert.Equal(new Complex<T>(N(-7), N(24)), NumericAlgorithms.Dot<Complex<T>>(z, z));
        var impulse = Signals.Dft<T>([Complex<T>.One, Complex<T>.Zero, Complex<T>.Zero, Complex<T>.Zero]);
        foreach (var value in impulse) Assert.True((value - Complex<T>.One).GetMagnitude() <= tolerance);
        var constant = Signals.Dft<T>([Complex<T>.One, Complex<T>.One, Complex<T>.One, Complex<T>.One]);
        Assert.True((constant[0] - new Complex<T>(N(4), T.Zero)).GetMagnitude() <= tolerance);
        foreach (var value in constant.Skip(1)) Assert.True(value.GetMagnitude() <= tolerance);
    }

    [Fact] public void HalfNative() => Verify<Half>();
    [Fact] public void FloatNative() => Verify<float>();
    [Fact] public void DoubleNative() => Verify<double>();
    [Fact] public void Decimal32Native() => Verify<Decimal32>();
    [Fact] public void Decimal64Native() => Verify<Decimal64>();
    [Fact] public void Decimal128Native() => Verify<Decimal128>();

    [Fact]
    public void Decimal128RetainsDifferencesBelowDoubleResolution()
    {
        var tiny = Decimal128.One;
        for (var i = 0; i < 25; i++) tiny /= Decimal128.CreateChecked(10);
        var shifted = Decimal128.One + tiny;
        Assert.NotEqual(Decimal128.One, shifted);
        Assert.Equal(tiny, shifted - Decimal128.One);
        Assert.False(Precision.NearlyEqual(Decimal128.One, shifted, Decimal128.Zero, tiny / Decimal128.CreateChecked(2)));
        Assert.Equal(shifted, NumericAlgorithms.Sum<Decimal128>([Decimal128.One, tiny]));
        Assert.Equal(tiny, NumericAlgorithms.Dot<Decimal128>([shifted, -Decimal128.One], [Decimal128.One, Decimal128.One]));
    }
}
