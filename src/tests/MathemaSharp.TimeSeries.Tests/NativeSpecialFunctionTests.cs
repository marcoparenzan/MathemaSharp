using System.Globalization;
using System.Numerics;
using MathemaSharp.TimeSeries;

namespace MathemaSharp.TimeSeries.Tests;

public class NativeSpecialFunctionTests
{
    private static void Verify<T>() where T : IFloatingPointIeee754<T>
    {
        T N(int n) => T.CreateChecked(n);
        var epsilon = T.BitIncrement(T.One) - T.One;
        var tolerance = T.Max(N(64) * epsilon, T.Parse("1e-12", CultureInfo.InvariantCulture));
        Assert.Equal(T.Zero, SpecialFunctions.LogGamma(T.One));
        Assert.Equal(T.Zero, SpecialFunctions.LogGamma(N(2)));
        Assert.Equal(T.Log(T.Pi) / N(2), SpecialFunctions.LogGamma(T.One / N(2)));
        Assert.True(T.Abs(SpecialFunctions.LogGamma(N(3)) - T.Log(N(2))) <= tolerance);
        var x = N(3) / N(2);
        var recurrence = SpecialFunctions.LogGamma(x + T.One) - SpecialFunctions.LogGamma(x);
        Assert.True(T.Abs(recurrence - T.Log(x)) <= tolerance);
        foreach (var point in new T[] {T.One / N(2), T.One, N(4)})
        {
            Assert.Equal(T.Exp(-point), SpecialFunctions.RegularizedGammaQ(T.One, point));
            // Q(2,x) = exp(-x)(1+x): couvre serie e frazione continua.
            var q = SpecialFunctions.RegularizedGammaQ(N(2), point);
            Assert.True(q >= T.Zero && q <= T.One);
            Assert.True(T.Abs(q - T.Exp(-point) * (T.One + point)) <= tolerance);
        }
        Assert.Equal(T.One, SpecialFunctions.RegularizedGammaQ(N(3), T.Zero));
        Assert.Equal(T.Zero, SpecialFunctions.RegularizedGammaQ(N(3), T.PositiveInfinity));
        Assert.Equal(T.PositiveInfinity, SpecialFunctions.LogGamma(T.PositiveInfinity));
        Assert.Equal(T.Exp(-T.One), SpecialFunctions.ChiSquareSurvival(N(2), 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => SpecialFunctions.LogGamma(T.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => SpecialFunctions.LogGamma(T.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => SpecialFunctions.RegularizedGammaQ(T.PositiveInfinity, T.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => SpecialFunctions.RegularizedGammaQ(T.One, T.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => SpecialFunctions.RegularizedGammaQ(T.One, -T.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => SpecialFunctions.ChiSquareSurvival(T.One, 0));
    }
    [Fact] public void HalfNative() => Verify<Half>();
    [Fact] public void FloatNative() => Verify<float>();
    [Fact] public void DoubleNative() => Verify<double>();
    [Fact] public void Decimal32Native() => Verify<Decimal32>();
    [Fact] public void Decimal64Native() => Verify<Decimal64>();
    [Fact] public void Decimal128Native() => Verify<Decimal128>();

    [Fact]
    public void Decimal128ExponentialCasePreservesSubDoubleDifferences()
    {
        var tiny = Decimal128.One;
        for (var i = 0; i < 25; i++) tiny /= Decimal128.CreateChecked(10);
        var q = SpecialFunctions.RegularizedGammaQ(Decimal128.One, tiny);
        Assert.NotEqual(Decimal128.One, q);
        var epsilon = Decimal128.BitIncrement(Decimal128.One) - Decimal128.One;
        Assert.True(Decimal128.Abs((Decimal128.One - q) - tiny) <= Decimal128.CreateChecked(4) * epsilon);
    }

    [Fact]
    public void IterationLimitIsReported()
        => Assert.Throws<ArithmeticException>(() => SpecialFunctions.RegularizedGammaQ(1000000d, 1000000d));

    [Fact]
    public void UnrepresentableLogGammaIsReported()
        => Assert.Throws<ArithmeticException>(() => SpecialFunctions.LogGamma(double.MaxValue));
}
