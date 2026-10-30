using System.Numerics;
using MathemaSharp.TimeSeries;

namespace MathemaSharp.TimeSeries.Tests;

/// <summary>
/// The library is generic over <c>T : IFloatingPointIeee754&lt;T&gt;</c>: the same mathematical identities must hold with every .NET 11
/// Generic Math format, with tolerances proportional to the type's precision. Half is excluded from the numerical identities (about 3
/// decimal digits are not enough for the accumulators) and is covered only by a smoke test.
/// </summary>
public class GenericFormatTests
{
    private static T V<T>(double x) where T : IFloatingPointIeee754<T> => T.CreateChecked(x);
    private static double D<T>(T x) where T : IFloatingPointIeee754<T> => double.CreateChecked(x);
    private static void Close<T>(T expected, T actual, double tolerance, string what) where T : IFloatingPointIeee754<T> =>
        Assert.True(Math.Abs(D(expected) - D(actual)) <= tolerance, $"{typeof(T).Name} {what}: atteso {D(expected)}, ottenuto {D(actual)}, tolleranza {tolerance}");

    /// <summary>Runs a check on all the formats with the suitable tolerance (float, double, Decimal64, Decimal128).</summary>
    private static void AllFormats(Action<double> forFloat, Action<double> forDouble, Action<double> forDecimal64, Action<double> forDecimal128, double floatTol, double doubleTol, double decimalTol)
    {
        forFloat(floatTol);
        forDouble(doubleTol);
        forDecimal64(decimalTol);
        forDecimal128(decimalTol);
    }

    // ---------------------------------------------------------------------------------------------------- SES ≡ ARIMA(0,1,1)
    private static void SesEqualsArima011<T>(double tolerance) where T : IFloatingPointIeee754<T>
    {
        var y = Simulation.RandomWalk<T>(new Random(3), 150, V<T>(50), T.Zero, T.One);
        var alpha = V<T>(0.3);
        var ses = ExponentialSmoothing.Ses(y, alpha, 5).Forecast;
        var arima = new ArimaModel<T>(0, 1, 1, T.Zero, [], [alpha - T.One], T.One).Forecast(y, 5);
        for (var h = 0; h < 5; h++) Close(ses[h], arima[h], tolerance, $"SES≡ARIMA h={h}");
    }

    [Fact] public void SesEqualsArima011InEveryFormat() =>
        AllFormats(SesEqualsArima011<float>, SesEqualsArima011<double>, SesEqualsArima011<Decimal64>, SesEqualsArima011<Decimal128>, 5e-3, 1e-9, 1e-9);

    // ---------------------------------------------------------------------------------------------------- Levinson-Durbin su ACF teorica
    private static void LevinsonOnAr1<T>(double tolerance) where T : IFloatingPointIeee754<T>
    {
        var gamma = new T[6];
        for (var k = 0; k < 6; k++) gamma[k] = T.Pow(V<T>(0.6), T.CreateChecked(k));
        var (phi, pacf, variance) = SeriesStatistics.LevinsonDurbin<T>(gamma, 5);
        Close(V<T>(0.6), pacf[0], tolerance, "PACF lag 1");
        for (var k = 1; k < 5; k++) Close(T.Zero, pacf[k], tolerance, $"PACF lag {k + 1}");
        Close(V<T>(0.6), phi[0], tolerance, "φ1");
        Close(V<T>(0.64), variance, tolerance, "σ²");
    }

    [Fact] public void LevinsonDurbinInEveryFormat() =>
        AllFormats(LevinsonOnAr1<float>, LevinsonOnAr1<double>, LevinsonOnAr1<Decimal64>, LevinsonOnAr1<Decimal128>, 1e-5, 1e-12, 1e-12);

    // ---------------------------------------------------------------------------------------------------- Holt-Winters esatto
    private static void HoltWintersExact<T>(double tolerance) where T : IFloatingPointIeee754<T>
    {
        double[] pattern = [1, -2, 3, -2];
        var y = new T[40];
        for (var t = 0; t < y.Length; t++) y[t] = V<T>(10 + 0.5 * t + pattern[t % 4]);
        var hw = ExponentialSmoothing.HoltWinters(y, 4, V<T>(0.3), V<T>(0.1), V<T>(0.2), 8);
        for (var h = 1; h <= 8; h++) Close(V<T>(10 + 0.5 * (39 + h) + pattern[(39 + h) % 4]), hw.Forecast[h - 1], tolerance, $"HW h={h}");
    }

    [Fact] public void HoltWintersExactInEveryFormat() =>
        AllFormats(HoltWintersExact<float>, HoltWintersExact<double>, HoltWintersExact<Decimal64>, HoltWintersExact<Decimal128>, 2e-3, 1e-9, 1e-9);

    // ---------------------------------------------------------------------------------------------------- STL esatta
    private static void StlExact<T>(double tolerance) where T : IFloatingPointIeee754<T>
    {
        const int period = 12;
        var y = new T[96];
        for (var t = 0; t < y.Length; t++) y[t] = V<T>(3 + 0.05 * t + 2 * Math.Sin(2 * Math.PI * t / period));
        var stl = Stl.Decompose(y, period);
        for (var t = 0; t < y.Length; t++)
        {
            Close(V<T>(3 + 0.05 * t), stl.Trend[t], tolerance, $"trend t={t}");
            Close(V<T>(2 * Math.Sin(2 * Math.PI * t / period)), stl.Seasonal[t], tolerance, $"seasonality t={t}");
        }
    }

    [Fact] public void StlExactInEveryFormat() =>
        AllFormats(StlExact<float>, StlExact<double>, StlExact<Decimal64>, StlExact<Decimal128>, 5e-3, 1e-9, 1e-9);

    // ---------------------------------------------------------------------------------------------------- Kalman at steady state
    private static void KalmanGain<T>(double tolerance) where T : IFloatingPointIeee754<T>
    {
        var y = Simulation.RandomWalk<T>(new Random(11), 200);
        var result = KalmanFilter.LocalLevel(V<T>(0.5), V<T>(2), T.Zero, V<T>(1e6)).Filter(y);
        Close(KalmanFilter.LocalLevelSteadyStateGain(V<T>(0.5), V<T>(2)), result.FirstGain[^1], tolerance, "steady-state gain");
    }

    [Fact] public void KalmanSteadyStateGainInEveryFormat() =>
        AllFormats(KalmanGain<float>, KalmanGain<double>, KalmanGain<Decimal64>, KalmanGain<Decimal128>, 1e-3, 1e-9, 1e-9);

    // ---------------------------------------------------------------------------------------------------- Minimi quadrati e χ²
    private static void LeastSquaresLine<T>(double tolerance) where T : IFloatingPointIeee754<T>
    {
        var x = new T[8, 2];
        var y = new T[8];
        for (var i = 0; i < 8; i++) { x[i, 0] = T.One; x[i, 1] = V<T>(i); y[i] = V<T>(3 - 2.0 * i); }
        var beta = MatrixAlgebra.LeastSquares<T>(x, y);
        Close(V<T>(3), beta[0], tolerance, "intercetta");
        Close(V<T>(-2), beta[1], tolerance, "pendenza");
    }

    private static void ChiSquare<T>(double tolerance) where T : IFloatingPointIeee754<T>
    {
        // χ²_2: P(X ≥ x) = e^{−x/2}; χ²_1 al 95%: x = 3.841458820694124.
        Close(T.Exp(V<T>(-1.5)), SpecialFunctions.ChiSquareSurvival(V<T>(3), 2), tolerance, "χ²₂");
        Close(V<T>(0.05), SpecialFunctions.ChiSquareSurvival(V<T>(3.841458820694124), 1), tolerance * 10, "χ²₁ 95%");
    }

    [Fact] public void LeastSquaresInEveryFormat() =>
        AllFormats(LeastSquaresLine<float>, LeastSquaresLine<double>, LeastSquaresLine<Decimal64>, LeastSquaresLine<Decimal128>, 1e-4, 1e-12, 1e-12);

    [Fact] public void ChiSquareSurvivalInEveryFormat() =>
        AllFormats(ChiSquare<float>, ChiSquare<double>, ChiSquare<Decimal64>, ChiSquare<Decimal128>, 1e-5, 1e-12, 1e-12);

    // ---------------------------------------------------------------------------------------------------- Stesso seme, formati diversi
    [Fact] public void Decimal128AgreesWithDoubleOnTheSameNoise()
    {
        // The random draws are doubles in every format: with the same seed only the arithmetic changes, so two different formats
        // must produce the same coefficients up to the double's rounding.
        var yd = Simulation.Arma<double>(new Random(42), 600, 0, [0.5, -0.3], [], 1);
        var yq = Simulation.Arma<Decimal128>(new Random(42), 600, Decimal128.Zero, [Decimal128.CreateChecked(0.5), Decimal128.CreateChecked(-0.3)], [], Decimal128.One);
        var fd = ArModel.FitYuleWalker(yd, 2);
        var fq = ArModel.FitYuleWalker(yq, 2);
        for (var i = 0; i < 2; i++) Close(fq.Phi[i], V<Decimal128>(fd.Phi[i]), 1e-9, $"φ{i + 1}");
        Close(fq.InnovationVariance, V<Decimal128>(fd.InnovationVariance), 1e-9, "σ²");
    }

    [Fact] public void GarchAndVarInDecimal128()
    {
        var truth = new Garch11<Decimal128>(V<Decimal128>(0.05), V<Decimal128>(0.1), V<Decimal128>(0.85));
        var shocks = Simulation.Garch(new Random(31), truth, 1500);
        var forecast = truth.ForecastVariance(shocks, 900); // persistenza 0.95: 0.95^900 ≈ 1e-20
        Close(truth.UnconditionalVariance, forecast[^1], 1e-9, "varianza incondizionata");
        var a = new Decimal128[,] { { V<Decimal128>(0.5), V<Decimal128>(0.2) }, { V<Decimal128>(-0.1), V<Decimal128>(0.3) } };
        var var1 = new VarModel<Decimal128>([V<Decimal128>(1), V<Decimal128>(1)], [a], new Decimal128[2, 2]);
        Assert.True(var1.IsStableOrder1Bivariate());
    }

    [Fact] public void HalfAndDecimal32WorkForSimpleStatistics()
    {
        Close(V<Half>(3), SeriesStatistics.Mean<Half>([V<Half>(2), V<Half>(3), V<Half>(4)]), 1e-3, "Half mean");
        Close(V<Decimal32>(2.5), SeriesStatistics.Mean<Decimal32>([V<Decimal32>(2), V<Decimal32>(3)]), 1e-9, "Decimal32 mean");
        var naive = ReferenceForecasts.Naive<Decimal32>([V<Decimal32>(1.1), V<Decimal32>(2.2)], 3);
        Assert.Equal([V<Decimal32>(2.2), V<Decimal32>(2.2), V<Decimal32>(2.2)], naive);
    }
}
