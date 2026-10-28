using System.Numerics;
using MathemaSharp.Core;
using MathemaSharp.TimeSeries;

namespace MathemaSharp.TimeSeries.Tests;

public class StatisticsTests
{
    [Fact] public void LevinsonDurbinOnTheoreticalAr1Acf()
    {
        // AR(1) con φ = 0.6: γ(k) ∝ φ^k. PACF = φ al lag 1, esattamente 0 dopo.
        var gamma = Enumerable.Range(0, 6).Select(k => Math.Pow(0.6, k)).ToArray();
        var (phi, pacf, variance) = SeriesStatistics.LevinsonDurbin(gamma, 5);
        Assert.True(Precision.NearlyEqual(0.6, pacf[0], 1e-12, 1e-12));
        Assert.All(pacf[1..], p => Assert.True(Math.Abs(p) < 1e-12));
        Assert.True(Precision.NearlyEqual(0.6, phi[0], 1e-12, 1e-12));
        Assert.True(Precision.NearlyEqual(1 - 0.36, variance, 1e-12, 1e-12));
    }

    [Fact] public void LevinsonDurbinMatchesDirectYuleWalkerSolve()
    {
        double[] gamma = [2.0, 1.2, 0.5, 0.1];
        var (phi, _, _) = SeriesStatistics.LevinsonDurbin(gamma, 3);
        var toeplitz = new double[3, 3];
        for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++) toeplitz[i, j] = gamma[Math.Abs(i - j)];
        var direct = MatrixAlgebra.LeastSquares(toeplitz, gamma.AsSpan(1));
        for (var i = 0; i < 3; i++) Assert.True(Precision.NearlyEqual(direct[i], phi[i], 1e-12, 1e-12));
    }

    [Fact] public void AcfStartsAtOneAndDifferenceShortens()
    {
        Assert.Equal(1.0, SeriesStatistics.Acf([1.0, 3, 2, 5, 4], 2)[0]);
        Assert.Equal([2.0, 3.0], SeriesStatistics.Difference([1.0, 3, 6]));
        Assert.Equal([5.0], SeriesStatistics.Difference([1.0, 3, 6], 2));
    }

    [Fact] public void ChiSquareSurvivalKnownValues()
    {
        // χ²_2: P(X ≥ x) = e^{−x/2}. χ²_1 al 95%: 3.841458820694124.
        Assert.True(Precision.NearlyEqual(Math.Exp(-1.5), SpecialFunctions.ChiSquareSurvival(3.0, 2), 1e-13, 1e-12));
        Assert.True(Precision.NearlyEqual(0.05, SpecialFunctions.ChiSquareSurvival(3.841458820694124, 1), 1e-12, 1e-10));
        Assert.True(Precision.NearlyEqual(Math.Log(24), SpecialFunctions.LogGamma(5.0), 1e-13, 1e-13));
        Assert.True(Precision.NearlyEqual(0.5 * Math.Log(Math.PI), SpecialFunctions.LogGamma(0.5), 1e-13, 1e-13));
    }

    [Fact] public void LjungBoxSeparatesWhiteNoiseFromAr()
    {
        var noise = Simulation.Arma<double>(new Random(1), 500, 0, [], [], 1);
        var ar = Simulation.Arma<double>(new Random(1), 500, 0, [0.7], [], 1);
        Assert.True(SeriesStatistics.LjungBox(noise, 10).PValue > 0.05);
        Assert.True(SeriesStatistics.LjungBox(ar, 10).PValue < 1e-10);
    }

    [Fact] public void LeastSquaresRecoversExactLine()
    {
        var x = new double[5, 2];
        var y = new double[5];
        for (var i = 0; i < 5; i++) { x[i, 0] = 1; x[i, 1] = i; y[i] = 3 - 2 * i; }
        var beta = MatrixAlgebra.LeastSquares(x, y);
        Assert.True(Precision.NearlyEqual(3, beta[0], 1e-12, 1e-12));
        Assert.True(Precision.NearlyEqual(-2, beta[1], 1e-12, 1e-12));
        x[0, 1] = x[0, 0] = 0;
        for (var i = 0; i < 5; i++) x[i, 1] = 2 * x[i, 0];
        Assert.Throws<ArgumentException>(() => MatrixAlgebra.LeastSquares(x, y));
    }
}

public class ReferenceForecastTests
{
    [Fact] public void Benchmarks()
    {
        double[] y = [1, 2, 3, 10, 20, 30];
        Assert.Equal([11.0, 11.0], ReferenceForecasts.Mean(y, 2));
        Assert.Equal([30.0, 30.0], ReferenceForecasts.Naive(y, 2));
        Assert.Equal([10.0, 20.0, 30.0, 10.0], ReferenceForecasts.SeasonalNaive(y, 3, 4));
        Assert.Equal([35.8, 41.6], ReferenceForecasts.Drift(y, 2).Select(v => Math.Round(v, 10)));
    }

    [Fact] public void MaseOfNaiveOnRandomWalkIsNearOne()
    {
        double[] training = [1, 2, 4, 7];
        Assert.Equal(1.0, Accuracy.Mase([9.0], [7.0], training));
        Assert.Throws<ArgumentException>(() => Accuracy.Mape([0.0, 1.0], [1.0, 1.0]));
        Assert.Equal(Math.Sqrt(2.5), Accuracy.Rmse([0.0, 0.0], [1.0, 2.0]));
    }
}

public class ArimaTests
{
    [Fact] public void YuleWalkerRecoversAr2()
    {
        var y = Simulation.Arma<double>(new Random(42), 20_000, 0, [0.5, -0.3], [], 1);
        var model = ArModel.FitYuleWalker(y, 2);
        Assert.InRange(model.Phi[0], 0.48, 0.52);
        Assert.InRange(model.Phi[1], -0.32, -0.28);
        Assert.InRange(model.InnovationVariance, 0.97, 1.03);
    }

    [Fact] public void ArForecastDecaysToMean()
    {
        var model = new ArModel<double>(10, [0.5], 1);
        var f = model.Forecast([14], 30);
        Assert.Equal(12, f[0]);
        Assert.Equal(11, f[1]);
        Assert.True(Math.Abs(f[^1] - 10) < 1e-6);
    }

    [Fact] public void HannanRissanenRecoversArma11()
    {
        var y = Simulation.Arma<double>(new Random(7), 20_000, 1, [0.6], [0.4], 1);
        var model = ArimaModel.FitHannanRissanen(y, 1, 0, 1);
        Assert.InRange(model.Phi[0], 0.56, 0.64);
        Assert.InRange(model.Theta[0], 0.36, 0.44);
        Assert.InRange(model.Constant, 0.9, 1.1);
        // Residui ≈ innovazioni: nessuna autocorrelazione residua significativa.
        Assert.True(SeriesStatistics.LjungBox(model.Residuals(y)[1..], 20, 2).PValue > 0.01);
    }

    [Fact] public void Arima010IsNaiveAndArima020ExtrapolatesLine()
    {
        double[] y = [3, 5, 4, 8];
        Assert.Equal([8.0, 8.0], new ArimaModel<double>(0, 1, 0, 0, [], [], 1).Forecast(y, 2));
        // Δ²y = 0 con costante nulla: la previsione prosegue l'ultima pendenza (8 − 4 = 4).
        Assert.Equal([12.0, 16.0], new ArimaModel<double>(0, 2, 0, 0, [], [], 1).Forecast(y, 2));
    }

    [Fact] public void SesEqualsArima011WithThetaAlphaMinusOne()
    {
        var y = Simulation.RandomWalk<double>(new Random(3), 200, 50, 0, 1);
        const double alpha = 0.3;
        var ses = ExponentialSmoothing.Ses(y, alpha, 5);
        var arima = new ArimaModel<double>(0, 1, 1, 0, [], [alpha - 1], 1);
        var forecast = arima.Forecast(y, 5);
        for (var h = 0; h < 5; h++) Assert.True(Precision.NearlyEqual(ses.Forecast[h], forecast[h], 1e-9, 1e-12));
    }
}

public class SmoothingTests
{
    [Fact] public void SesIsGeometricWeightedAverage()
    {
        double[] y = [10, 20, 30];
        var alpha = 0.5;
        // ℓ_2 = α y_2 + α(1−α) y_1 + (1−α)² y_0.
        var expected = 0.5 * 30 + 0.25 * 20 + 0.25 * 10;
        Assert.Equal(expected, ExponentialSmoothing.Ses(y, alpha, 1).Forecast[0]);
        Assert.Equal(10, ExponentialSmoothing.Ses(y, 0, 1).Forecast[0]);
        Assert.Equal(30, ExponentialSmoothing.Ses(y, 1, 1).Forecast[0]);
    }

    [Fact] public void HoltReproducesLinesAndDampingConverges()
    {
        var line = Enumerable.Range(0, 20).Select(t => 5 + 2.0 * t).ToArray();
        var holt = ExponentialSmoothing.Holt(line, 0.5, 0.3, 3);
        Assert.Equal([45.0, 47.0, 49.0], holt.Forecast);
        Assert.Equal(0, holt.Sse);

        const double phi = 0.9;
        var damped = ExponentialSmoothing.Holt(line, 0.5, 0.3, 500, phi);
        var limit = damped.Level + phi / (1 - phi) * damped.Trend;
        Assert.True(Precision.NearlyEqual(limit, damped.Forecast[^1], 1e-9, 1e-12));
        Assert.True(damped.Forecast[^1] < holt.Level + 500 * holt.Trend);
    }

    [Fact] public void HoltWintersIsExactOnLinePlusSeasonal()
    {
        double[] pattern = [1, -2, 3, -2];
        var y = Enumerable.Range(0, 40).Select(t => 10 + 0.5 * t + pattern[t % 4]).ToArray();
        var hw = ExponentialSmoothing.HoltWinters(y, 4, 0.3, 0.1, 0.2, 8);
        for (var h = 1; h <= 8; h++)
        {
            var t = 39 + h;
            Assert.True(Math.Abs(hw.Forecast[h - 1] - (10 + 0.5 * t + pattern[t % 4])) < 1e-9, $"h={h}");
        }
    }

    [Fact] public void FitSesAlphaFindsLowAlphaForNoiseAndHighForRandomWalk()
    {
        var noise = Simulation.Arma<double>(new Random(5), 1000, 0, [], [], 1);
        var walk = Simulation.RandomWalk<double>(new Random(5), 1000);
        Assert.True(ExponentialSmoothing.FitSesAlpha(noise) < 0.1);
        Assert.True(ExponentialSmoothing.FitSesAlpha(walk) > 0.9);
    }

    [Fact] public void GoldenSectionFindsParabolaMinimum() =>
        Assert.True(Math.Abs(ExponentialSmoothing.GoldenSection(x => (x - 0.3) * (x - 0.3), 0, 1, 1e-9) - 0.3) < 1e-8);
}

public class KalmanTests
{
    [Fact] public void LocalLevelGainConvergesToRiccatiSolution()
    {
        const double q = 0.5, r = 2;
        var kf = KalmanFilter.LocalLevel(q, r, 0, 1e6);
        var y = Simulation.RandomWalk<double>(new Random(11), 200);
        var result = kf.Filter(y);
        var steady = KalmanFilter.LocalLevelSteadyStateGain(q, r);
        Assert.True(Precision.NearlyEqual(steady, result.FirstGain[^1], 1e-12, 1e-12));
        // Prior diffuso: il primo guadagno ≈ 1, il filtro "crede" alla prima osservazione.
        Assert.True(result.FirstGain[0] > 0.999);
    }

    [Fact] public void SteadyStateKalmanIsSes()
    {
        const double q = 0.5, r = 2;
        var alpha = KalmanFilter.LocalLevelSteadyStateGain(q, r);
        var y = Simulation.RandomWalk<double>(new Random(12), 300);
        var kalman = KalmanFilter.LocalLevel(q, r, y[0], 0).Filter(y);
        var ses = ExponentialSmoothing.Ses(y, alpha, 1);
        // Dopo il transitorio la previsione a un passo del Kalman coincide con quella del SES.
        for (var t = 100; t < y.Length; t++) Assert.True(Math.Abs(kalman.Predictions[t] - ses.Fitted[t]) < 1e-9, $"t={t}");
    }

    [Fact] public void MissingValuesSkipUpdateAndGrowUncertainty()
    {
        var kf = KalmanFilter.LocalLevel(1.0, 1, 0, 1);
        var result = kf.Filter([1, double.NaN, double.NaN, 2]);
        Assert.Equal(result.FilteredFirstState[0], result.FilteredFirstState[2]);
        Assert.True(result.PredictionVariances[2] > result.PredictionVariances[1]);
        Assert.Equal(0, result.FirstGain[1]);
    }

    [Fact] public void JosephCovarianceStaysSymmetric()
    {
        var kf = KalmanFilter.LocalLinearTrend(0.1, 0.01, 1, 0, 0, 100);
        kf.Filter(Enumerable.Range(0, 50).Select(t => 2.0 * t).ToArray());
        Assert.Equal(kf.Covariance[0, 1], kf.Covariance[1, 0]);
        Assert.True(Math.Abs(kf.State[1] - 2) < 0.05);
    }
}

public class VarTests
{
    [Fact] public void FitRecoversBivariateVar1()
    {
        var truth = new VarModel<double>([0.5, -0.2], [new double[,] { { 0.5, 0.2 }, { -0.1, 0.3 } }], new double[2, 2]);
        var y = Simulation.Var(new Random(21), truth, 20_000, 1);
        var model = VarModel.Fit(y, 1);
        for (var i = 0; i < 2; i++)
            for (var j = 0; j < 2; j++) Assert.True(Math.Abs(model.Coefficients[0][i, j] - truth.Coefficients[0][i, j]) < 0.02, $"A[{i},{j}]");
        Assert.True(Math.Abs(model.ResidualCovariance[0, 0] - 1) < 0.05);
        Assert.True(Math.Abs(model.ResidualCovariance[0, 1]) < 0.05);
        Assert.True(model.IsStableOrder1Bivariate());
    }

    [Fact] public void ForecastConvergesToStationaryMean()
    {
        var a = new double[,] { { 0.5, 0.2 }, { -0.1, 0.3 } };
        var model = new VarModel<double>([1, 1], [a], new double[2, 2]);
        var f = model.Forecast(new double[,] { { 0, 0 } }, 200);
        // μ = (I − A)⁻¹ c con I − A = [[0.5, −0.2], [0.1, 0.7]], det = 0.37, inversa = [[0.7, 0.2], [−0.1, 0.5]]/det.
        double det = 0.5 * 0.7 + 0.2 * 0.1;
        double mu0 = (0.7 * 1 + 0.2 * 1) / det, mu1 = (-0.1 * 1 + 0.5 * 1) / det;
        Assert.True(Math.Abs(f[199, 0] - mu0) < 1e-9);
        Assert.True(Math.Abs(f[199, 1] - mu1) < 1e-9);
    }
}

public class GarchTests
{
    [Fact] public void VarianceForecastConvergesToUnconditional()
    {
        var model = new Garch11<double>(0.1, 0.1, 0.8);
        Assert.True(Precision.NearlyEqual(1, model.UnconditionalVariance, 1e-12, 1e-12));
        var f = model.ForecastVariance([3.0, -2.5, 4.0], 400);
        Assert.True(f[0] > 1);
        Assert.True(Math.Abs(f[^1] - 1) < 1e-9);
        // Decadimento geometrico con ragione α + β.
        Assert.True(Precision.NearlyEqual(0.9, (f[2] - 1) / (f[1] - 1), 1e-12, 1e-12));
    }

    [Fact] public void FitRecoversSimulatedParameters()
    {
        var truth = new Garch11<double>(0.05, 0.1, 0.85);
        var shocks = Simulation.Garch(new Random(31), truth, 10_000);
        var fit = Garch11.FitVarianceTargeting(shocks);
        Assert.InRange(fit.Alpha, 0.07, 0.13);
        Assert.InRange(fit.Persistence, 0.92, 0.98);
        Assert.True(fit.LogLikelihood(shocks) >= truth.LogLikelihood(shocks) - 5);
    }
}

public class StlTests
{
    [Fact] public void ExactOnLinearTrendPlusZeroMeanSeasonal()
    {
        // Il LOESS lineare riproduce le rette e le medie mobili di un periodo annullano una stagionalità a media nulla:
        // STL separa esattamente trend lineare e stagionalità periodica.
        const int period = 12;
        var y = Enumerable.Range(0, 120).Select(t => 3 + 0.05 * t + 2 * Math.Sin(2 * Math.PI * t / period)).ToArray();
        var stl = Stl.Decompose(y, period);
        for (var t = 0; t < y.Length; t++)
        {
            Assert.True(Math.Abs(stl.Trend[t] - (3 + 0.05 * t)) < 1e-9, $"trend t={t}");
            Assert.True(Math.Abs(stl.Seasonal[t] - 2 * Math.Sin(2 * Math.PI * t / period)) < 1e-9, $"seasonal t={t}");
            Assert.True(Math.Abs(stl.Remainder[t]) < 1e-9);
        }
    }

    [Fact] public void RobustIterationsIsolateOutlier()
    {
        const int period = 12;
        double Seasonal(int t) => 2 * Math.Sin(2 * Math.PI * t / period);
        var y = Enumerable.Range(0, 120).Select(t => 0.05 * t + Seasonal(t)).ToArray();
        y[60] += 30;
        var plain = Stl.Decompose(y, period);
        var robust = Stl.Decompose(y, period, innerIterations: 1, robustIterations: 5);
        double MaxError(StlDecomposition<double> d) => Enumerable.Range(0, y.Length).Where(t => t != 60).Max(t => Math.Abs(d.Seasonal[t] - Seasonal(t)));
        Assert.True(MaxError(robust) < MaxError(plain) / 3);
        Assert.True(robust.Remainder[60] > 25);
        Assert.True(robust.RobustnessWeights[60] < 0.01);
    }

    [Fact] public void LoessReproducesLinesEvenWhenExtrapolating()
    {
        double[] y = [1, 3, 5, 7, 9, 11];
        Assert.True(Precision.NearlyEqual(-1, Stl.Loess(y, [], 4, -1), 1e-12, 1e-12));
        Assert.True(Precision.NearlyEqual(13, Stl.Loess(y, [], 4, 6), 1e-12, 1e-12));
        Assert.True(Precision.NearlyEqual(5, Stl.Loess(y, [], 50, 2), 1e-12, 1e-12));
    }
}

public class QuantileTests
{
    [Fact] public void QuantileInterpolatesLinearlyLikeNumpy()
    {
        double[] x = [4, 1, 3, 2];                               // ordinati: 1 2 3 4
        Assert.Equal(2.5, SeriesStatistics.Quantile(x, 0.5));
        Assert.Equal(1.0, SeriesStatistics.Quantile(x, 0.0));
        Assert.Equal(4.0, SeriesStatistics.Quantile(x, 1.0));
        Assert.True(Precision.NearlyEqual(1.9, SeriesStatistics.Quantile(x, 0.3), 1e-12, 1e-12));   // h = 0.9 → 1 + 0.9
        Assert.Equal(7.0, SeriesStatistics.Quantile([7.0], 0.9));
        Assert.Throws<ArgumentOutOfRangeException>(() => SeriesStatistics.Quantile(x, 1.1));
        Assert.Throws<ArgumentException>(() => SeriesStatistics.Quantile(Array.Empty<double>(), 0.5));
    }

    [Fact] public void PinballLossIsAsymmetricAndHalfMaeAtTheMedian()
    {
        double[] actual = [10, 12, 8];
        double[] forecast = [9, 12, 11];                         // errori: +1, 0, −3
        Assert.True(Precision.NearlyEqual(Accuracy.Mae(actual, forecast) / 2, Accuracy.PinballLoss(actual, forecast, 0.5), 1e-12, 1e-12));
        // p = 0.9: sottostima (e > 0) costa 0.9·e, sovrastima (e < 0) costa 0.1·|e|.
        Assert.True(Precision.NearlyEqual((0.9 * 1 + 0 + 0.1 * 3) / 3, Accuracy.PinballLoss(actual, forecast, 0.9), 1e-12, 1e-12));
        Assert.Throws<ArgumentOutOfRangeException>(() => Accuracy.PinballLoss(actual, forecast, 1.0));
    }

    [Fact] public void TheTrueQuantileMinimizesPinballLoss()
    {
        var sample = Simulation.Arma<double>(new Random(3), 4000, 0, [], [], 1);
        var best = SeriesStatistics.Quantile<double>(sample, 0.9);
        double Loss(double q) => Accuracy.PinballLoss<double>(sample, Enumerable.Repeat(q, sample.Length).ToArray(), 0.9);
        Assert.True(Loss(best) <= Loss(best - 0.2) && Loss(best) <= Loss(best + 0.2));
        Assert.InRange(best, 1.15, 1.42);                         // quantile 0.9 di N(0,1) = 1.2816
    }

    [Fact] public void IntervalCoverageCountsObservationsInside() =>
        Assert.Equal(0.5, Accuracy.IntervalCoverage<double>([1, 5, 2, 9], [0, 0, 0, 0], [3, 3, 3, 3]));

    [Fact] public void QuantileWorksInEveryFormat()
    {
        Assert.Equal(V<Decimal128>(2.5), SeriesStatistics.Quantile<Decimal128>([V<Decimal128>(1), V<Decimal128>(2), V<Decimal128>(3), V<Decimal128>(4)], V<Decimal128>(0.5)));
        Assert.Equal(2.5f, SeriesStatistics.Quantile<float>([1f, 2f, 3f, 4f], 0.5f));
        static T V<T>(double x) where T : IFloatingPointIeee754<T> => T.CreateChecked(x);
    }
}
