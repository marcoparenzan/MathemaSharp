using MathemaSharp.Core;
using System.Numerics;

namespace MathemaSharp.Core.Tests;

public class NumericTests
{
    [Fact] public void SumAcrossTypes()
    {
        Assert.Equal(6, NumericAlgorithms.Sum<int>([1, 2, 3]));
        Assert.Equal(0.3m, NumericAlgorithms.Sum<decimal>([0.1m, 0.2m]));
        Assert.Equal(new Complex(4, 6), NumericAlgorithms.Sum<Complex>([new(1, 2), new(3, 4)]));
        Assert.Equal(BigInteger.Zero, NumericAlgorithms.Sum<BigInteger>([]));
    }
    [Fact] public void CheckedOverflow() => Assert.Throws<OverflowException>(() => NumericAlgorithms.SumChecked<int>([int.MaxValue, 1]));
    [Fact] public void ConversionPolicies()
    {
        Assert.Throws<OverflowException>(() => byte.CreateChecked(300));
        Assert.Equal((byte)255, byte.CreateSaturating(300));
        Assert.Equal((byte)44, byte.CreateTruncating(300));
    }
    [Fact] public void PolynomialAndMean()
    {
        Assert.Equal(16d, NumericAlgorithms.Horner<double>([1, 2, 1], 3));
        Assert.Equal(Complex.Zero, NumericAlgorithms.Horner<Complex>([Complex.One, Complex.Zero, Complex.One], Complex.ImaginaryOne));
        Assert.Equal(0, NumericAlgorithms.Horner<int>([], 3));
        Assert.Equal(1.5m, NumericAlgorithms.Mean<decimal>([1, 2]));
        Assert.Throws<ArgumentException>(() => NumericAlgorithms.Mean<double>([]));
    }
    [Fact] public void DotContracts()
    {
        Assert.Equal(11, NumericAlgorithms.Dot<int>([1, 2], [3, 4]));
        Assert.Throws<ArgumentException>(() => NumericAlgorithms.Dot<double>([1], []));
        Assert.Equal(0d, Signals.VectorDot<double>([], []));
        Assert.Throws<ArgumentException>(() => Signals.VectorDot<double>([1], []));
        Assert.Throws<ArgumentException>(() => Signals.HermitianDot<double>([Complex<double>.One], []));
    }
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(3)] [InlineData(19)] [InlineData(4099)]
    public void VectorIncludesTail(int n)
    {
        var random = new Random(1234);
        var left = Enumerable.Range(0, n).Select(_ => random.NextDouble() - 0.5).ToArray();
        var right = Enumerable.Range(0, n).Select(_ => random.NextDouble() - 0.5).ToArray();
        Assert.True(Precision.NearlyEqual(NumericAlgorithms.Dot<double>(left, right), Signals.VectorDot(left, right), 1e-11, 1e-11));
    }
    [Fact] public void KahanRecoversSmallTerms()
    {
        double[] values = [1e16, 1, 1, -1e16];
        Assert.Equal(0d, NumericAlgorithms.Sum<double>(values));
        Assert.Equal(2d, NumericAlgorithms.KahanSum<double>(values));
        Assert.Equal(0d, NumericAlgorithms.KahanSum<double>([]));
    }
    [Theory] [InlineData(0, 0, 0)] [InlineData(-48, 18, 6)] [InlineData(7, 13, 1)]
    public void Gcd(int a, int b, int result) => Assert.Equal(result, NumericAlgorithms.Gcd(a, b));
    [Fact] public void IntegerLimits()
    {
        Assert.Throws<OverflowException>(() => NumericAlgorithms.Gcd(int.MinValue, 0));
        Assert.Equal(BigInteger.One, NumericAlgorithms.Factorial(0));
        Assert.Equal(new BigInteger(3628800), NumericAlgorithms.Factorial(10));
        Assert.Throws<ArgumentOutOfRangeException>(() => NumericAlgorithms.Factorial(-1));
    }
    [Fact] public void DftImpulseAndConstant()
    {
        foreach (var bin in Signals.Dft<double>([Complex<double>.One, Complex<double>.Zero, Complex<double>.Zero, Complex<double>.Zero])) Assert.True((bin - Complex<double>.One).GetMagnitude() < 1e-12);
        var constant = Signals.Dft<double>([Complex<double>.One, Complex<double>.One, Complex<double>.One, Complex<double>.One]);
        Assert.True((constant[0] - new Complex<double>(4, 0)).GetMagnitude() < 1e-12);
        foreach (var bin in constant.Skip(1)) Assert.True(bin.GetMagnitude() < 1e-12);
        Assert.Empty(Signals.Dft<double>([]));
    }
    [Fact] public void HermitianIsNotBilinear()
    {
        Complex<double>[] z = [new(3, 4)];
        Assert.Equal(new Complex<double>(25, 0), Signals.HermitianDot<double>(z, z));
        Assert.Equal(new Complex<double>(-7, 24), NumericAlgorithms.Dot<Complex<double>>(z, z));
    }
    [Fact] public void NearlyEqualSpecialValues()
    {
        Assert.True(Precision.NearlyEqual(0.1 + 0.2, 0.3));
        Assert.False(Precision.NearlyEqual(double.NaN, double.NaN));
        Assert.True(Precision.NearlyEqual(double.PositiveInfinity, double.PositiveInfinity));
        Assert.False(Precision.NearlyEqual(double.PositiveInfinity, 1));
        Assert.False(Precision.NearlyEqual(double.MaxValue, -double.MaxValue));
        Assert.True(Precision.NearlyEqual(double.MaxValue, double.BitDecrement(double.MaxValue)));
        Assert.True(Precision.NearlyEqual(0, 1e-15));
        Assert.Throws<ArgumentOutOfRangeException>(() => Precision.NearlyEqual(1.0, 1.0, -1.0, 1.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Precision.NearlyEqual(1.0, 1.0, 1.0, double.NaN));
    }
    [Fact] public void MoneyRoundingAndScale()
    {
        decimal[] lines = [0.005m, 0.005m, 0.005m];
        Assert.Equal(0m, Precision.InvoiceTotal(lines, MidpointRounding.ToEven, true));
        Assert.Equal(0.02m, Precision.InvoiceTotal(lines, MidpointRounding.ToEven, false));
        Assert.Equal(0.03m, Precision.InvoiceTotal(lines, MidpointRounding.AwayFromZero, true));
        Assert.Equal(0.02m, Precision.InvoiceTotal(lines, MidpointRounding.AwayFromZero, false));
        Assert.Equal(2, Precision.DecimalScale(1.00m));
        Assert.Equal(0, Precision.DecimalScale(1m));
        Assert.Equal(0m, Precision.InvoiceTotal([], MidpointRounding.ToEven, false));
        Assert.Throws<OverflowException>(() => Precision.InvoiceTotal([decimal.MaxValue, 1m], MidpointRounding.ToEven, false));
    }
}
