namespace MathemaSharp.TimeSeries.Interop;

// Facciata NON generica su double per host che non sanno chiamare metodi generici né passare ReadOnlySpan<T>:
// PySharp (clr.AddReference), PowerShell, F# script, reflection in genere. Ogni metodo delega all'API generica con T = double.
// Convenzioni: array monodimensionali double[]; le matrici (VAR, minimi quadrati) sono "appiattite" in ordine di riga con righe e colonne
// esplicite, perché la creazione di double[,] da questi host è scomoda; i risultati sono classi semplici, non tuple.
// Per C# usare direttamente l'API generica (MathemaSharp.TimeSeries), che supporta anche float, Half e Decimal32/64/128.

/// <summary>Esito di Levinson-Durbin: coefficienti AR, PACF e varianza d'innovazione.</summary>
public sealed record LevinsonResult(double[] Phi, double[] Pacf, double InnovationVariance);

/// <summary>Esito del test di Ljung-Box.</summary>
public sealed record LjungBoxResult(double Statistic, int DegreesOfFreedom, double PValue);

/// <summary>Esito di un filtro di Kalman con osservazione scalare.</summary>
public sealed record KalmanResult(double[] Predictions, double[] PredictionVariances, double[] FilteredFirstState, double[] FirstGain, double LogLikelihood);

/// <summary>Statistiche di base di una serie: media, autocorrelazione, PACF, differenze, Ljung-Box, funzioni speciali.</summary>
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

    /// <summary>Quantile con interpolazione lineare: coincide con <c>np.quantile</c> (metodo lineare).</summary>
    public static double Quantile(double[] values, double p) => SeriesStatistics.Quantile<double>(values, p);

    public static double LogGamma(double x) => SpecialFunctions.LogGamma(x);
    public static double ChiSquareSurvival(double x, int degreesOfFreedom) => SpecialFunctions.ChiSquareSurvival(x, degreesOfFreedom);
}

/// <summary>Previsioni di riferimento: media, naive, seasonal naive, drift.</summary>
public static class Forecasts
{
    public static double[] Mean(double[] y, int horizon) => ReferenceForecasts.Mean<double>(y, horizon);
    public static double[] Naive(double[] y, int horizon) => ReferenceForecasts.Naive<double>(y, horizon);
    public static double[] SeasonalNaive(double[] y, int period, int horizon) => ReferenceForecasts.SeasonalNaive<double>(y, period, horizon);
    public static double[] Drift(double[] y, int horizon) => ReferenceForecasts.Drift<double>(y, horizon);
}

/// <summary>Misure di accuratezza fuori campione: MAE, RMSE, MAPE, MASE.</summary>
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

/// <summary>Modelli AR (Yule-Walker) e ARIMA (Hannan-Rissanen). I modelli restituiti hanno metodi con array: Forecast, Residuals.</summary>
public static class Arima
{
    public static ArModel<double> FitYuleWalker(double[] y, int order) => ArModel.FitYuleWalker<double>(y, order);
    public static ArimaModel<double> FitHannanRissanen(double[] y, int p, int d, int q) => ArimaModel.FitHannanRissanen<double>(y, p, d, q);
    public static ArimaModel<double> Model(int p, int d, int q, double constant, double[] phi, double[] theta, double innovationVariance) =>
        new(p, d, q, constant, phi, theta, innovationVariance);
}

/// <summary>Filtro di Kalman con osservazione scalare: local level, local linear trend, guadagno stazionario.</summary>
public static class Kalman
{
    public static KalmanFilter<double> LocalLevel(double q, double r, double initialLevel, double initialVariance) =>
        KalmanFilter.LocalLevel(q, r, initialLevel, initialVariance);

    public static KalmanFilter<double> LocalLinearTrend(double levelNoise, double slopeNoise, double r, double initialLevel, double initialSlope, double initialVariance) =>
        KalmanFilter.LocalLinearTrend(levelNoise, slopeNoise, r, initialLevel, initialSlope, initialVariance);

    public static double SteadyStateGain(double q, double r) => KalmanFilter.LocalLevelSteadyStateGain(q, r);

    /// <summary>Filtra la serie (NaN = dato mancante). Il filtro conserva lo stato: crearne uno nuovo per ogni serie.</summary>
    public static KalmanResult Filter(KalmanFilter<double> filter, double[] y)
    {
        var (predictions, variances, filtered, gains, logLikelihood) = filter.Filter(y);
        return new KalmanResult(predictions, variances, filtered, gains, logLikelihood);
    }
}

/// <summary>VAR con matrici appiattite in ordine di riga: y[tempo × serie], coefficienti k × k per ogni lag.</summary>
public static class Var
{
    public static VarModel<double> Fit(double[] flat, int rows, int cols, int order) => VarModel.Fit(Unflatten(flat, rows, cols), order);

    /// <summary>Previsione ricorsiva: restituisce horizon × k valori in ordine di riga.</summary>
    public static double[] Forecast(VarModel<double> model, double[] historyFlat, int historyRows, int horizon) =>
        Flatten(model.Forecast(Unflatten(historyFlat, historyRows, model.Dimension), horizon));

    /// <summary>Matrice A_lag (lag da 1) appiattita k × k in ordine di riga.</summary>
    public static double[] Coefficient(VarModel<double> model, int lag) => Flatten(model.Coefficients[lag - 1]);

    public static double[] ResidualCovariance(VarModel<double> model) => Flatten(model.ResidualCovariance);

    public static bool IsStableOrder1Bivariate(VarModel<double> model) => model.IsStableOrder1Bivariate();

    internal static double[,] Unflatten(double[] flat, int rows, int cols)
    {
        if (flat.Length != rows * cols) throw new ArgumentException($"Attesi {rows}×{cols} = {rows * cols} valori, ricevuti {flat.Length}.");
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

/// <summary>GARCH(1,1). I modelli restituiti hanno metodi con array: ConditionalVariances, LogLikelihood, ForecastVariance.</summary>
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

/// <summary>Minimi quadrati (QR di Householder) con la matrice dei regressori appiattita in ordine di riga.</summary>
public static class Linear
{
    public static double[] LeastSquares(double[] flat, int rows, int cols, double[] y) => MatrixAlgebra.LeastSquares<double>(Var.Unflatten(flat, rows, cols), y);
}

/// <summary>Processi sintetici con seme esplicito.</summary>
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
