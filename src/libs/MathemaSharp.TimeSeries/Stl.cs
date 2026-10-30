using System.Numerics;
using static MathemaSharp.TimeSeries.Num;

namespace MathemaSharp.TimeSeries;

/// <summary>y_t = Trend_t + Seasonal_t + Remainder_t, exact by construction: Remainder = y − Trend − Seasonal.</summary>
public sealed record StlDecomposition<T>(T[] Trend, T[] Seasonal, T[] Remainder, T[] RobustnessWeights) where T : IFloatingPointIeee754<T>;

/// <summary>
/// STL of Cleveland, Cleveland, McRae &amp; Terpenning (1990): seasonal-trend decomposition with LOESS, generic over
/// <c>T : IFloatingPointIeee754&lt;T&gt;</c>. Inner loop: detrend, smoothing of the cycle-subseries, low-pass filter,
/// deseasonalisation, trend smoothing. Optional outer loop: bisquare robust weights that downweight outliers.
/// Faithful to the algorithm, not to the original Fortran code: no evaluation jumps (jump) and no degree-0 LOESS.
/// </summary>
public static class Stl
{
    /// <param name="y">Observed series, at least two periods.</param>
    /// <param name="period">n_p, observations per cycle.</param>
    /// <param name="seasonalSpan">n_s, odd ≥ 3 (recommended ≥ 7): larger ⇒ seasonality more stable over time.</param>
    /// <param name="trendSpan">n_t; default the smallest odd number ≥ 1.5 n_p/(1 − 1.5/n_s).</param>
    /// <param name="innerIterations">n_i; 2 without robustness, 1 is enough with robustness.</param>
    /// <param name="robustIterations">n_o; 0 = non-robust.</param>
    public static StlDecomposition<T> Decompose<T>(ReadOnlySpan<T> y, int period, int seasonalSpan = 7, int? trendSpan = null,
        int innerIterations = 2, int robustIterations = 0) where T : IFloatingPointIeee754<T>
    {
        var n = y.Length;
        if (period < 2 || n < 2 * period) throw new ArgumentException("Servono almeno due periodi completi.");
        if (seasonalSpan < 3 || seasonalSpan % 2 == 0) throw new ArgumentOutOfRangeException(nameof(seasonalSpan), "Atteso dispari ≥ 3.");
        var nt = trendSpan ?? NextOdd(1.5 * period / (1 - 1.5 / seasonalSpan));
        var nl = NextOdd(period);
        var data = y.ToArray();
        var trend = new T[n];
        var seasonal = new T[n];
        var weights = new T[n];
        Array.Fill(weights, T.One);

        for (var outer = 0; outer <= robustIterations; outer++)
        {
            for (var inner = 0; inner < innerIterations; inner++)
            {
                // 1. Detrend.
                var detrended = new T[n];
                for (var t = 0; t < n; t++) detrended[t] = data[t] - trend[t];

                // 2. Cycle-subseries: each phase j is a series on its own, smoothed and extended by one cycle before and after → C of length n + 2 n_p.
                var cycle = new T[n + 2 * period];
                for (var j = 0; j < period; j++)
                {
                    var count = (n - j + period - 1) / period;
                    var sub = new T[count];
                    var subWeights = new T[count];
                    for (var i = 0; i < count; i++) { sub[i] = detrended[j + i * period]; subWeights[i] = weights[j + i * period]; }
                    for (var i = -1; i <= count; i++)
                        cycle[j + (i + 1) * period] = Loess<T>(sub, subWeights, seasonalSpan, N<T>(i));
                }

                // 3. Low-pass: MA(n_p), MA(n_p), MA(3), then LOESS(n_l). The three moving averages shorten by 2 n_p: back to length n.
                var smooth = MovingAverage(MovingAverage(MovingAverage(cycle, period), period), 3);
                var low = new T[n];
                for (var t = 0; t < n; t++) low[t] = Loess<T>(smooth, [], nl, N<T>(t));

                // 4. Seasonality = C − L, removing from C the slow level that the low-pass captured.
                for (var t = 0; t < n; t++) seasonal[t] = cycle[period + t] - low[t];

                // 5–6. Deseasonalise and smooth the trend.
                var deseasonalized = new T[n];
                for (var t = 0; t < n; t++) deseasonalized[t] = data[t] - seasonal[t];
                for (var t = 0; t < n; t++) trend[t] = Loess<T>(deseasonalized, weights, nt, N<T>(t));
            }
            if (outer < robustIterations) weights = RobustnessWeights(data, trend, seasonal);
        }

        var remainder = new T[n];
        for (var t = 0; t < n; t++) remainder[t] = data[t] - trend[t] - seasonal[t];
        return new StlDecomposition<T>(trend, seasonal, remainder, weights);
    }

    /// <summary>
    /// Local linear LOESS on equally spaced abscissae 0..n−1, evaluated at x (also outside the interval, to extrapolate).
    /// Uses the q nearest points with tricube weights (1 − (d/h)³)³ multiplied by the robust weights; if q &gt; n, h grows by (q − n)/2.
    /// A local linear polynomial reproduces lines exactly: the property the tests rely on.
    /// </summary>
    public static T Loess<T>(ReadOnlySpan<T> y, ReadOnlySpan<T> robustWeights, int span, T x) where T : IFloatingPointIeee754<T>
    {
        var n = y.Length;
        if (n == 0) throw new ArgumentException("Empty series.");
        if (span < 2) throw new ArgumentOutOfRangeException(nameof(span));
        if (!T.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x));
        if (!robustWeights.IsEmpty && robustWeights.Length != n)
            throw new ArgumentException("One weight per observation.", nameof(robustWeights));
        foreach (var value in y)
            if (!T.IsFinite(value)) throw new ArgumentException("Osservazioni finite richieste.", nameof(y));
        foreach (var weight in robustWeights)
            if (!T.IsFinite(weight) || weight < T.Zero)
                throw new ArgumentException("Finite, non-negative weights are required.", nameof(robustWeights));
        var q = Math.Min(span, n);
        var center = RoundToInt(x);
        var lo = Math.Clamp(center - (q - 1) / 2, 0, n - q);
        var hi = lo + q - 1;
        var h = T.Max(x - N<T>(lo), N<T>(hi) - x);
        if (span > n) h += N<T>(span - n) / N<T>(2);
        // Margin as in the original Fortran (0.999h): the points at the window edge have almost-zero weight, not exactly zero.
        h = T.Max(h, Epsilon<T>()) / (N<T>(999) / N<T>(1000));

        T sw = T.Zero, swx = T.Zero, swy = T.Zero, swxx = T.Zero, swxy = T.Zero;
        for (var i = lo; i <= hi; i++)
        {
            // Abscissa centred at x: avoids subtracting moments of large absolute indices.
            var xi = N<T>(i) - x;
            var u = T.Abs(xi) / h;
            var w = T.Zero;
            if (u < T.One)
            {
                var c = T.One - u * u * u;
                w = c * c * c;
            }
            if (!robustWeights.IsEmpty) w *= robustWeights[i];
            sw += w; swx += w * xi; swy += w * y[i]; swxx += w * xi * xi; swxy += w * xi * y[i];
        }
        if (sw <= T.Zero) return y[Math.Clamp(center, 0, n - 1)];
        var meanX = swx / sw;
        var meanY = swy / sw;
        var variance = swxx / sw - meanX * meanX;
        // Weighted line; if the points with weight are degenerate in x it falls back to the weighted mean (degree 0).
        var threshold = N<T>(16) * Epsilon<T>() * T.Max(T.One, swxx / sw);
        var slope = variance > threshold ? (swxy / sw - meanX * meanY) / variance : T.Zero;
        return meanY - slope * meanX;
    }

    private static T[] MovingAverage<T>(T[] x, int window) where T : IFloatingPointIeee754<T>
    {
        var result = new T[x.Length - window + 1];
        var sum = T.Zero;
        var w = N<T>(window);
        for (var i = 0; i < window; i++) sum += x[i];
        result[0] = sum / w;
        for (var i = window; i < x.Length; i++)
        {
            sum += x[i] - x[i - window];
            result[i - window + 1] = sum / w;
        }
        return result;
    }

    /// <summary>ρ_t = B(|R_t|/(6·median|R|)), with bisquare B(u) = (1 − u²)² for u &lt; 1, otherwise 0.</summary>
    private static T[] RobustnessWeights<T>(T[] y, T[] trend, T[] seasonal) where T : IFloatingPointIeee754<T>
    {
        var abs = new T[y.Length];
        for (var t = 0; t < y.Length; t++) abs[t] = T.Abs(y[t] - trend[t] - seasonal[t]);
        var sorted = (T[])abs.Clone();
        Array.Sort(sorted);
        var median = sorted.Length % 2 == 1 ? sorted[sorted.Length / 2] : (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]) / N<T>(2);
        var h = N<T>(6) * median;
        var weights = new T[y.Length];
        for (var t = 0; t < y.Length; t++)
        {
            if (h == T.Zero) { weights[t] = T.One; continue; }
            var u = abs[t] / h;
            weights[t] = u >= T.One ? T.Zero : (T.One - u * u) * (T.One - u * u);
        }
        return weights;
    }

    private static int NextOdd(double value)
    {
        var n = (int)Math.Ceiling(value);
        return n % 2 == 0 ? n + 1 : n;
    }
}
