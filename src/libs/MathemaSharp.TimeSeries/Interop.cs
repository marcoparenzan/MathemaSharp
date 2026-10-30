namespace MathemaSharp.TimeSeries.Interop;

// NON-generic facade over double for hosts that cannot call generic methods or pass ReadOnlySpan<T>:
// PySharp (clr.AddReference), PowerShell, F# scripts, reflection in general. Each method delegates to the generic API with T = double.
// Conventions: one-dimensional double[] arrays; matrices (VAR, least squares) are "flattened" in row order with explicit rows and columns,
// because creating double[,] from these hosts is awkward; the results are plain classes, not tuples.
// For C# use the generic API directly (MathemaSharp.TimeSeries), which also supports float, Half and Decimal32/64/128.

/// <summary>Outcome of Levinson-Durbin: AR coefficients, PACF and innovation variance.</summary>
public sealed record LevinsonResult(double[] Phi, double[] Pacf, double InnovationVariance);

/// <summary>Outcome of the Ljung-Box test.</summary>
public sealed record LjungBoxResult(double Statistic, int DegreesOfFreedom, double PValue);

/// <summary>Outcome of a Kalman filter with a scalar observation.</summary>
public sealed record KalmanResult(double[] Predictions, double[] PredictionVariances, double[] FilteredFirstState, double[] FirstGain, double LogLikelihood);

/// <summary>Basic statistics of a series: mean, autocorrelation, PACF, differences, Ljung-Box, special functions.</summary>
public static class Statistics
{
    public static double Mean(double[] y) => SeriesStatistics.Mean<double>(y);
    public static double[] Autocovariance(double[] y, int maxLag) => SeriesStatistics.Autocovariance<double>(y, maxLag);
    public static double[] Acf(double[] y, int maxLag) => SeriesStatistics.Acf<double>(y, maxLag);
    public static double[] Pacf(double[] y, int maxLag) => SeriesStatistics.Pacf<double>(y, maxLag);
    public static double[] Difference(double[] y, int lag) => SeriesStatistics.Difference<double>(y, lag);

    public static LevinsonResult LevinsonDurbin(double[] autocovariance, int order)
    {
        var (phi, pacf, variance) = SeriesStatistics.LevinsonDurbin<double>(autocovariance, order);
        return new LevinsonResult(phi, pacf, variance);
    }

    public static LjungBoxResult LjungBox(double[] residuals, int lags, int fittedParameters)
    {
        var (q, dof, p) = SeriesStatistics.LjungBox<double>(residuals, lags, fittedParameters);
        return new LjungBoxResult(q, dof, p);
    }

    /// <summary>Quantile with linear interpolation: matches <c>np.quantile</c> (linear method).</summary>
    public static double Quantile(double[] values, double p) => SeriesStatistics.Quantile<double>(values, p);

    public static double LogGamma(double x) => SpecialFunctions.LogGamma(x);
    public static double ChiSquareSurvival(double x, int degreesOfFreedom) => SpecialFunctions.ChiSquareSurvival(x, degreesOfFreedom);
}

/// <summary>Reference forecasts: mean, naive, seasonal naive, drift.</summary>
public static class Forecasts
{
    public static double[] Mean(double[] y, int horizon) => ReferenceForecasts.Mean<double>(y, horizon);
    public static double[] Naive(double[] y, int horizon) => ReferenceForecasts.Naive<double>(y, horizon);
    public static double[] SeasonalNaive(double[] y, int period, int horizon) => ReferenceForecasts.SeasonalNaive<double>(y, period, horizon);
    public static double[] Drift(double[] y, int horizon) => ReferenceForecasts.Drift<double>(y, horizon);
}

/// <summary>Out-of-sample accuracy measures: MAE, RMSE, MAPE, MASE.</summary>
public static class Metrics
{
    public static double Mae(double[] actual, double[] forecast) => Accuracy.Mae<double>(actual, forecast);
    public static double Rmse(double[] actual, double[] forecast) => Accuracy.Rmse<double>(actual, forecast);
    public static double Mape(double[] actual, double[] forecast) => Accuracy.Mape<double>(actual, forecast);
    public static double Mase(double[] actual, double[] forecast, double[] training, int period) => Accuracy.Mase<double>(actual, forecast, training, period);
    public static double PinballLoss(double[] actual, double[] quantileForecast, double p) => Accuracy.PinballLoss<double>(actual, quantileForecast, p);
    public static double IntervalCoverage(double[] actual, double[] lower, double[] upper) => Accuracy.IntervalCoverage<double>(actual, lower, upper);
}

/// <summary>Smoothing esponenziale: SES, Holt, damped trend, Holt-Winters.</summary>
public static class Smoothing
{
    public static SmoothingResult<double> Ses(double[] y, double alpha, int horizon) => ExponentialSmoothing.Ses<double>(y, alpha, horizon);
    public static SmoothingResult<double> Holt(double[] y, double alpha, double beta, int horizon, double phi) => ExponentialSmoothing.Holt<double>(y, alpha, beta, horizon, phi);
    public static SmoothingResult<double> HoltWinters(double[] y, int period, double alpha, double beta, double gamma, int horizon, double phi) =>
        ExponentialSmoothing.HoltWinters<double>(y, period, alpha, beta, gamma, horizon, phi);
    public static double FitSesAlpha(double[] y) => ExponentialSmoothing.FitSesAlpha<double>(y);
}

/// <summary>AR (Yule-Walker) and ARIMA (Hannan-Rissanen) models. The returned models have methods with arrays: Forecast, Residuals.</summary>
public static class Arima
{
    public static ArModel<double> FitYuleWalker(double[] y, int order) => ArModel.FitYuleWalker<double>(y, order);
    public static ArimaModel<double> FitHannanRissanen(double[] y, int p, int d, int q) => ArimaModel.FitHannanRissanen<double>(y, p, d, q);
    public static ArimaModel<double> Model(int p, int d, int q, double constant, double[] phi, double[] theta, double innovationVariance) =>
        new(p, d, q, constant, phi, theta, innovationVariance);
}

/// <summary>Kalman filter with a scalar observation: local level, local linear trend, steady-state gain.</summary>
public static class Kalman
{
    public static KalmanFilter<double> LocalLevel(double q, double r, double initialLevel, double initialVariance) =>
        KalmanFilter.LocalLevel(q, r, initialLevel, initialVariance);

    public static KalmanFilter<double> LocalLinearTrend(double levelNoise, double slopeNoise, double r, double initialLevel, double initialSlope, double initialVariance) =>
        KalmanFilter.LocalLinearTrend(levelNoise, slopeNoise, r, initialLevel, initialSlope, initialVariance);

    public static double SteadyStateGain(double q, double r) => KalmanFilter.LocalLevelSteadyStateGain(q, r);

    /// <summary>Filters the series (NaN = missing data). The filter keeps its state: create a new one for each series.</summary>
    public static KalmanResult Filter(KalmanFilter<double> filter, double[] y)
    {
        var (predictions, variances, filtered, gains, logLikelihood) = filter.Filter(y);
        return new KalmanResult(predictions, variances, filtered, gains, logLikelihood);
    }
}

/// <summary>VAR with matrices flattened in row order: y[time × series], k × k coefficients for each lag.</summary>
public static class Var
{
    public static VarModel<double> Fit(double[] flat, int rows, int cols, int order) => VarModel.Fit(Unflatten(flat, rows, cols), order);

    /// <summary>Recursive forecast: returns horizon × k values in row order.</summary>
    public static double[] Forecast(VarModel<double> model, double[] historyFlat, int historyRows, int horizon) =>
        Flatten(model.Forecast(Unflatten(historyFlat, historyRows, model.Dimension), horizon));

    /// <summary>Matrix A_lag (lag from 1) flattened k × k in row order.</summary>
    public static double[] Coefficient(VarModel<double> model, int lag) => Flatten(model.Coefficients[lag - 1]);

    public static double[] ResidualCovariance(VarModel<double> model) => Flatten(model.ResidualCovariance);

    public static bool IsStableOrder1Bivariate(VarModel<double> model) => model.IsStableOrder1Bivariate();

    internal static double[,] Unflatten(double[] flat, int rows, int cols)
    {
        if (flat.Length != rows * cols) throw new ArgumentException($"Expected {rows}×{cols} = {rows * cols} values, received {flat.Length}.");
        var result = new double[rows, cols];
        for (var i = 0; i < rows; i++)
            for (var j = 0; j < cols; j++) result[i, j] = flat[i * cols + j];
        return result;
    }

    internal static double[] Flatten(double[,] matrix)
    {
        int rows = matrix.GetLength(0), cols = matrix.GetLength(1);
        var result = new double[rows * cols];
        for (var i = 0; i < rows; i++)
            for (var j = 0; j < cols; j++) result[i * cols + j] = matrix[i, j];
        return result;
    }
}

/// <summary>GARCH(1,1). The returned models have methods with arrays: ConditionalVariances, LogLikelihood, ForecastVariance.</summary>
public static class Garch
{
    public static Garch11<double> Model(double omega, double alpha, double beta) => new(omega, alpha, beta);
    public static Garch11<double> FitVarianceTargeting(double[] shocks) => Garch11.FitVarianceTargeting<double>(shocks);
}

/// <summary>Decomposizione STL e LOESS.</summary>
public static class Decomposition
{
    public static StlDecomposition<double> Stl(double[] y, int period, int seasonalSpan, int innerIterations, int robustIterations) =>
        MathemaSharp.TimeSeries.Stl.Decompose<double>(y, period, seasonalSpan, null, innerIterations, robustIterations);

    public static double Loess(double[] y, int span, double x) => MathemaSharp.TimeSeries.Stl.Loess<double>(y, [], span, x);
}

/// <summary>Least squares (Householder QR) with the regressor matrix flattened in row order.</summary>
public static class Linear
{
    public static double[] LeastSquares(double[] flat, int rows, int cols, double[] y) => MatrixAlgebra.LeastSquares<double>(Var.Unflatten(flat, rows, cols), y);
}

/// <summary>Synthetic processes with an explicit seed.</summary>
public static class Simulate
{
    public static double[] Normal(int seed, int count)
    {
        var random = new Random(seed);
        var result = new double[count];
        for (var i = 0; i < count; i++) result[i] = Simulation.StandardNormal(random);
        return result;
    }

    public static double[] Arma(int seed, int n, double constant, double[] phi, double[] theta, double sigma) =>
        Simulation.Arma<double>(new Random(seed), n, constant, phi, theta, sigma);

    public static double[] RandomWalk(int seed, int n, double start, double drift, double sigma) =>
        Simulation.RandomWalk<double>(new Random(seed), n, start, drift, sigma);

    public static double[] Garch(int seed, Garch11<double> model, int n) => Simulation.Garch(new Random(seed), model, n);
}
