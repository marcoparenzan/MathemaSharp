using System.Numerics;
using MathemaSharp.TimeSeries;

namespace MathemaSharp.TimeSeries.Tests;

public class NativeGarchTests
{
    private static void Verify<T>() where T : IFloatingPointIeee754<T>
    {
        T N(int n) => T.CreateChecked(n);
        T[] shocks = [T.One, -T.One, N(2), -N(2), T.One, -T.One, N(2), -N(2)];
        var model = new Garch11<T>(T.One / N(4), T.One / N(4), T.One / N(2));
        var variances = model.ConditionalVariances(shocks);
        Assert.Equal(N(5) / N(2), variances[0]);
        Assert.Equal(N(7) / N(4), variances[1]);
        Assert.True(T.IsFinite(model.LogLikelihood(shocks)));
        var forecast = model.ForecastVariance(shocks, 4);
        for (var i = 1; i < forecast.Length; i++)
            Assert.Equal(model.Omega + model.Persistence * forecast[i - 1], forecast[i]);
        var fitted = Garch11.FitVarianceTargeting<T>(shocks, gridSize: 8, refinements: 2);
        Assert.True(T.IsFinite(fitted.Omega) && fitted.Omega > T.Zero);
        Assert.True(fitted.Alpha > T.Zero && fitted.Beta >= T.Zero && fitted.Persistence < T.One);
        Assert.True(T.IsFinite(fitted.LogLikelihood(shocks)));
        Assert.Equal(fitted, Garch11.FitVarianceTargeting<T>(shocks, gridSize: 8, refinements: 2));
    }
    [Fact] public void HalfNative() => Verify<Half>();
    [Fact] public void FloatNative() => Verify<float>();
    [Fact] public void DoubleNative() => Verify<double>();
    [Fact] public void Decimal32Native() => Verify<Decimal32>();
    [Fact] public void Decimal64Native() => Verify<Decimal64>();
    [Fact] public void Decimal128Native() => Verify<Decimal128>();

    [Fact]
    public void InvalidFitInputsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Garch11.FitVarianceTargeting<double>([1, 2], 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Garch11.FitVarianceTargeting<double>([1, 2], 8, -1));
        Assert.Throws<ArgumentException>(() => Garch11.FitVarianceTargeting<double>([]));
        Assert.Throws<ArgumentException>(() => Garch11.FitVarianceTargeting<double>([0, 0]));
        Assert.Throws<ArgumentException>(() => Garch11.FitVarianceTargeting<double>([double.NaN, 1]));
        Assert.Throws<ArgumentException>(() => Garch11.FitVarianceTargeting<double>([double.MaxValue, 1]));
    }
}
