using System.Numerics;
using static MathemaSharp.TimeSeries.Num;

namespace MathemaSharp.TimeSeries;

/// <summary>
/// Yule's AR(p) (1927): y_t − μ = Σ φ_i (y_{t−i} − μ) + ε_t. By Wold (1938) every stationary process is an MA(∞) of innovations
/// plus a deterministic part: the finite AR is a parsimonious approximation of it.
/// </summary>
public sealed record ArModel<T>(T Mean, T[] Phi, T InnovationVariance) where T : IFloatingPointIeee754<T>
{
    /// <summary>Recursive forecast: the unknown future values are replaced by their forecasts; converges to μ as h → ∞.</summary>
    public T[] Forecast(ReadOnlySpan<T> history, int horizon)
    {
        if (history.Length < Phi.Length) throw new ArgumentException("History shorter than the order.");
        var buffer = new List<T>(history.Length + horizon);
        foreach (var v in history) buffer.Add(v);
        var result = new T[horizon];
        for (var h = 0; h < horizon; h++)
        {
            var value = Mean;
            for (var i = 0; i < Phi.Length; i++) value += Phi[i] * (buffer[^(i + 1)] - Mean);
            buffer.Add(value);
            result[h] = value;
        }
        return result;
    }

    // Overload with arrays for callers without ReadOnlySpan support (reflection, PySharp: a ref struct cannot be passed through reflection).
    public T[] Forecast(T[] history, int horizon) => Forecast(history.AsSpan(), horizon);
}

/// <summary>Estimate of an <see cref="ArModel{T}"/>.</summary>
public static class ArModel
{
    /// <summary>Estimate with the Yule-Walker equations solved by Levinson-Durbin: the estimated model is always stationary.</summary>
    public static ArModel<T> FitYuleWalker<T>(ReadOnlySpan<T> y, int order) where T : IFloatingPointIeee754<T>
    {
        if (order < 1 || order >= y.Length) throw new ArgumentOutOfRangeException(nameof(order));
        var (phi, _, variance) = SeriesStatistics.LevinsonDurbin<T>(SeriesStatistics.Autocovariance(y, order), order);
        return new ArModel<T>(SeriesStatistics.Mean(y), phi, variance);
    }
}

/// <summary>
/// Box-Jenkins ARIMA(p,d,q) (1970) on w = Δ^d y: w_t = c + Σ φ_i w_{t−i} + e_t + Σ θ_j e_{t−j}.
/// MA = lagged innovations, not a moving average. Stationarity and invertibility are not imposed by the estimate: check them if needed.
/// </summary>
public sealed record ArimaModel<T>(int P, int D, int Q, T Constant, T[] Phi, T[] Theta, T InnovationVariance) where T : IFloatingPointIeee754<T>
{
    /// <summary>
    /// Conditional residuals (CSS) on the differenced series: e_t = 0 for t &lt; p, then e_t = w_t − ŵ_{t|t−1}.
    /// Length n − d.
    /// </summary>
    public T[] Residuals(ReadOnlySpan<T> y)
    {
        var w = ArimaModel.Differences(y, D)[D];
        var e = new T[w.Length];
        for (var t = P; t < w.Length; t++) e[t] = w[t] - OneStep(w, e, t);
        return e;
    }

    // Overload with arrays for callers without ReadOnlySpan support (reflection, PySharp: a ref struct cannot be passed through reflection).
    public T[] Residuals(T[] y) => Residuals(y.AsSpan());

    // Overload with arrays for callers without ReadOnlySpan support (reflection, PySharp: a ref struct cannot be passed through reflection).
    public T[] Forecast(T[] y, int horizon) => Forecast(y.AsSpan(), horizon);

    /// <summary>Forecast: ARMA recursion with zero future innovations, then d integrations starting from the last values of each level.</summary>
    public T[] Forecast(ReadOnlySpan<T> y, int horizon)
    {
        var levels = ArimaModel.Differences(y, D);
        var w = new List<T>(levels[D]);
        var e = new List<T>(Residuals(y));
        for (var h = 0; h < horizon; h++)
        {
            w.Add(T.Zero);
            e.Add(T.Zero);
            w[^1] = OneStep(w, e, w.Count - 1);
        }
        var forecast = w.Skip(levels[D].Length).ToArray();
        for (var k = D - 1; k >= 0; k--)
        {
            var last = levels[k][^1];
            for (var h = 0; h < horizon; h++) forecast[h] = last += forecast[h];
        }
        return forecast;
    }

    private T OneStep(IReadOnlyList<T> w, IReadOnlyList<T> e, int t)
    {
        var value = Constant;
        for (var i = 1; i <= P && t - i >= 0; i++) value += Phi[i - 1] * w[t - i];
        for (var j = 1; j <= Q && t - j >= 0; j++) value += Theta[j - 1] * e[t - j];
        return value;
    }
}

/// <summary>Estimate of an <see cref="ArimaModel{T}"/>.</summary>
public static class ArimaModel
{
    /// <summary>
    /// Hannan-Rissanen (1982): (1) long AR estimated with OLS to reconstruct the innovations ê_t; (2) OLS of w_t on lagged w and lagged ê.
    /// Linear and optimisation-free: a good initial estimator, less efficient than maximum likelihood.
    /// </summary>
    public static ArimaModel<T> FitHannanRissanen<T>(ReadOnlySpan<T> y, int p, int d, int q, int? longArOrder = null) where T : IFloatingPointIeee754<T>
    {
        if (p < 0 || d < 0 || q < 0) throw new ArgumentOutOfRangeException(nameof(p), "Ordini negativi.");
        var w = Differences(y, d)[d];
        T[] innovations = [];
        var start = p;
        if (q > 0)
        {
            var m = longArOrder ?? Math.Max(p + q, (int)Math.Ceiling(10 * Math.Log10(w.Length)));
            var longAr = Regress(w, m, [], 0, m);
            innovations = new T[w.Length];
            for (var t = m; t < w.Length; t++)
            {
                var fitted = longAr[0];
                for (var i = 1; i <= m; i++) fitted += longAr[i] * w[t - i];
                innovations[t] = w[t] - fitted;
            }
            start = Math.Max(p, m + q);
        }
        var beta = Regress(w, p, innovations, q, start);
        var model = new ArimaModel<T>(p, d, q, beta[0], beta[1..(p + 1)], beta[(p + 1)..], T.Zero);
        var residuals = model.Residuals(y);
        var sumSquares = T.Zero;
        for (var t = Math.Max(p, q); t < residuals.Length; t++) sumSquares += residuals[t] * residuals[t];
        var count = Math.Max(1, residuals.Length - Math.Max(p, q) - p - q - 1);
        return model with { InnovationVariance = sumSquares / N<T>(count) };
    }

    /// <summary>levels[k] = Δ^k y, k = 0..d.</summary>
    internal static T[][] Differences<T>(ReadOnlySpan<T> y, int d) where T : IFloatingPointIeee754<T>
    {
        var levels = new T[d + 1][];
        levels[0] = y.ToArray();
        for (var k = 1; k <= d; k++) levels[k] = SeriesStatistics.Difference<T>(levels[k - 1]);
        return levels;
    }

    /// <summary>OLS of w_t on [1, w_{t−1..p}, e_{t−1..q}] for t ≥ start. Returns [c, φ..., θ...].</summary>
    private static T[] Regress<T>(T[] w, int p, T[] e, int q, int start) where T : IFloatingPointIeee754<T>
    {
        var rows = w.Length - start;
        if (rows < 1 + p + q + 1) throw new ArgumentException("Series too short for the requested orders.");
        var x = new T[rows, 1 + p + q];
        var target = new T[rows];
        for (var r = 0; r < rows; r++)
        {
            var t = start + r;
            x[r, 0] = T.One;
            for (var i = 1; i <= p; i++) x[r, i] = w[t - i];
            for (var j = 1; j <= q; j++) x[r, p + j] = e[t - j];
            target[r] = w[t];
        }
        return MatrixAlgebra.LeastSquares<T>(x, target);
    }
}
