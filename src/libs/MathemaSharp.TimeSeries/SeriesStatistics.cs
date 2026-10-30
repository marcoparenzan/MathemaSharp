using System.Numerics;
using static MathemaSharp.TimeSeries.Num;

namespace MathemaSharp.TimeSeries;

/// <summary>Basic statistics, generic over <c>T : IFloatingPointIeee754&lt;T&gt;</c>: autocovariance, ACF, PACF (Levinson-Durbin), differences, Ljung-Box test.</summary>
public static class SeriesStatistics
{
    public static T Mean<T>(ReadOnlySpan<T> y) where T : IFloatingPointIeee754<T>
    {
        if (y.IsEmpty) throw new ArgumentException("Empty series.");
        var sum = T.Zero;
        foreach (var v in y) sum += v;
        return sum / N<T>(y.Length);
    }

    /// <summary>
    /// Empirical quantile of level p ∈ [0,1] with linear interpolation between the order statistics (Hyndman-Fan method 7,
    /// the default of <c>np.quantile</c>): h = (n−1)p, Q = x₍⌊h⌋₎ + (h − ⌊h⌋)(x₍⌊h⌋+1₎ − x₍⌊h⌋₎). With p = 0.5 it is the median. Ignores time order.
    /// </summary>
    public static T Quantile<T>(ReadOnlySpan<T> values, T p) where T : IFloatingPointIeee754<T>
    {
        if (values.IsEmpty) throw new ArgumentException("Serie vuota.");
        if (T.IsNaN(p) || p < T.Zero || p > T.One) throw new ArgumentOutOfRangeException(nameof(p), "Atteso un livello in [0,1].");
        var sorted = values.ToArray();
        Array.Sort(sorted);
        var h = N<T>(sorted.Length - 1) * p;
        var lower = int.CreateChecked(T.Floor(h));
        if (lower >= sorted.Length - 1) return sorted[^1];
        return sorted[lower] + (h - N<T>(lower)) * (sorted[lower + 1] - sorted[lower]);
    }

    /// <summary>
    /// γ(k) = (1/n) Σ (y_t − ȳ)(y_{t+k} − ȳ), k = 0..maxLag. Divisor n and not n−k: the biased estimator makes the Toeplitz matrix
    /// positive semidefinite, a condition that guarantees |PACF| ≤ 1 and a stationary Yule-Walker AR.
    /// </summary>
    public static T[] Autocovariance<T>(ReadOnlySpan<T> y, int maxLag) where T : IFloatingPointIeee754<T>
    {
        if (maxLag < 0 || maxLag >= y.Length) throw new ArgumentOutOfRangeException(nameof(maxLag));
        var mean = Mean(y);
        var result = new T[maxLag + 1];
        for (var k = 0; k <= maxLag; k++)
        {
            var sum = T.Zero;
            for (var t = 0; t + k < y.Length; t++) sum += (y[t] - mean) * (y[t + k] - mean);
            result[k] = sum / N<T>(y.Length);
        }
        return result;
    }

    /// <summary>ρ(k) = γ(k)/γ(0). ρ(0) = 1.</summary>
    public static T[] Acf<T>(ReadOnlySpan<T> y, int maxLag) where T : IFloatingPointIeee754<T>
    {
        var gamma = Autocovariance(y, maxLag);
        if (gamma[0] == T.Zero) throw new ArgumentException("Constant series: autocorrelation undefined.");
        var result = new T[gamma.Length];
        for (var k = 0; k < gamma.Length; k++) result[k] = gamma[k] / gamma[0];
        return result;
    }

    /// <summary>
    /// Levinson-Durbin recursion: solves the Yule-Walker equations Γφ = γ in O(p²) exploiting the Toeplitz structure.
    /// The reflection coefficient κ_k = φ_kk is the PACF at lag k; the innovation variance is updated as v_k = v_{k−1}(1 − κ_k²).
    /// </summary>
    public static (T[] Phi, T[] Pacf, T InnovationVariance) LevinsonDurbin<T>(ReadOnlySpan<T> autocovariance, int order) where T : IFloatingPointIeee754<T>
    {
        if (order < 0 || order >= autocovariance.Length) throw new ArgumentOutOfRangeException(nameof(order));
        if (autocovariance[0] <= T.Zero) throw new ArgumentException("γ(0) must be positive.");
        var phi = new T[order];
        var previous = new T[order];
        var pacf = new T[order];
        var variance = autocovariance[0];
        for (var k = 1; k <= order; k++)
        {
            var acc = autocovariance[k];
            for (var j = 1; j < k; j++) acc -= previous[j - 1] * autocovariance[k - j];
            var kappa = acc / variance;
            phi[k - 1] = kappa;
            for (var j = 1; j < k; j++) phi[j - 1] = previous[j - 1] - kappa * previous[k - j - 1];
            variance *= T.One - kappa * kappa;
            pacf[k - 1] = kappa;
            Array.Copy(phi, previous, k);
        }
        return (phi, pacf, variance);
    }

    public static T[] Pacf<T>(ReadOnlySpan<T> y, int maxLag) where T : IFloatingPointIeee754<T> => LevinsonDurbin<T>(Autocovariance(y, maxLag), maxLag).Pacf;

    /// <summary>Difference Δ_m y_t = y_t − y_{t−m}; lag 1 for the trend, lag m for seasonality. Returns n − lag values.</summary>
    public static T[] Difference<T>(ReadOnlySpan<T> y, int lag = 1) where T : IFloatingPointIeee754<T>
    {
        if (lag < 1 || lag >= y.Length) throw new ArgumentOutOfRangeException(nameof(lag));
        var result = new T[y.Length - lag];
        for (var t = 0; t < result.Length; t++) result[t] = y[t + lag] - y[t];
        return result;
    }

    /// <summary>
    /// Ljung-Box: Q = n(n+2) Σ_{k=1..h} ρ̂_k²/(n−k), approximately χ² with h − fittedParameters degrees of freedom if the residuals
    /// are white noise. Small p-value ⇒ residual autocorrelation: the model has not captured all the dynamics.
    /// </summary>
    public static (T Statistic, int DegreesOfFreedom, T PValue) LjungBox<T>(ReadOnlySpan<T> residuals, int lags, int fittedParameters = 0) where T : IFloatingPointIeee754<T>
    {
        var dof = lags - fittedParameters;
        if (dof < 1) throw new ArgumentException("Non-positive degrees of freedom.");
        var rho = Acf(residuals, lags);
        var n = residuals.Length;
        var q = T.Zero;
        for (var k = 1; k <= lags; k++) q += rho[k] * rho[k] / N<T>(n - k);
        q *= N<T>(n) * N<T>(n + 2);
        return (q, dof, SpecialFunctions.ChiSquareSurvival(q, dof));
    }
}
