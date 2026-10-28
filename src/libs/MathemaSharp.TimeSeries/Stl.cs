using System.Numerics;
using static MathemaSharp.TimeSeries.Num;

namespace MathemaSharp.TimeSeries;

/// <summary>y_t = Trend_t + Seasonal_t + Remainder_t, per costruzione esatta: Remainder = y − Trend − Seasonal.</summary>
public sealed record StlDecomposition<T>(T[] Trend, T[] Seasonal, T[] Remainder, T[] RobustnessWeights) where T : IFloatingPointIeee754<T>;

/// <summary>
/// STL di Cleveland, Cleveland, McRae &amp; Terpenning (1990): decomposizione stagionale-trend con LOESS, generica su
/// <c>T : IFloatingPointIeee754&lt;T&gt;</c>. Ciclo interno: detrend, smoothing delle sottoserie cicliche, filtro passa-basso,
/// destagionalizzazione, smoothing del trend. Ciclo esterno opzionale: pesi robusti bisquare che declassano gli outlier.
/// Fedele all'algoritmo, non al codice Fortran originale: niente salti di valutazione (jump) né LOESS di grado 0.
/// </summary>
public static class Stl
{
    /// <param name="y">Serie osservata, almeno due periodi.</param>
    /// <param name="period">n_p, osservazioni per ciclo.</param>
    /// <param name="seasonalSpan">n_s, dispari ≥ 3 (consigliato ≥ 7): più grande ⇒ stagionalità più stabile nel tempo.</param>
    /// <param name="trendSpan">n_t; default il minimo dispari ≥ 1.5 n_p/(1 − 1.5/n_s).</param>
    /// <param name="innerIterations">n_i; 2 senza robustezza, 1 è sufficiente con robustezza.</param>
    /// <param name="robustIterations">n_o; 0 = non robusto.</param>
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

                // 2. Sottoserie cicliche: ogni fase j è una serie a sé, lisciata ed estesa di un ciclo prima e dopo → C di lunghezza n + 2 n_p.
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

                // 3. Passa-basso: MA(n_p), MA(n_p), MA(3), poi LOESS(n_l). Le tre medie mobili accorciano di 2 n_p: torna a lunghezza n.
                var smooth = MovingAverage(MovingAverage(MovingAverage(cycle, period), period), 3);
                var low = new T[n];
                for (var t = 0; t < n; t++) low[t] = Loess<T>(smooth, [], nl, N<T>(t));

                // 4. Stagionalità = C − L, togliendo al C il livello lento che il passa-basso ha catturato.
                for (var t = 0; t < n; t++) seasonal[t] = cycle[period + t] - low[t];

                // 5–6. Destagionalizza e liscia il trend.
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
    /// LOESS locale lineare su ascisse equispaziate 0..n−1, valutato in x (anche fuori dall'intervallo, per estrapolare).
    /// Usa i q punti più vicini con pesi tricubici (1 − (d/h)³)³ moltiplicati per i pesi robusti; se q &gt; n, h cresce di (q − n)/2.
    /// Un polinomio locale lineare riproduce esattamente le rette: proprietà su cui si basano i test.
    /// </summary>
    public static T Loess<T>(ReadOnlySpan<T> y, ReadOnlySpan<T> robustWeights, int span, T x) where T : IFloatingPointIeee754<T>
    {
        var n = y.Length;
        if (n == 0) throw new ArgumentException("Serie vuota.");
        if (span < 2) throw new ArgumentOutOfRangeException(nameof(span));
        if (!T.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x));
        if (!robustWeights.IsEmpty && robustWeights.Length != n)
            throw new ArgumentException("Un peso per ogni osservazione.", nameof(robustWeights));
        foreach (var value in y)
            if (!T.IsFinite(value)) throw new ArgumentException("Osservazioni finite richieste.", nameof(y));
        foreach (var weight in robustWeights)
            if (!T.IsFinite(weight) || weight < T.Zero)
                throw new ArgumentException("Pesi finiti e non negativi richiesti.", nameof(robustWeights));
        var q = Math.Min(span, n);
        var center = RoundToInt(x);
        var lo = Math.Clamp(center - (q - 1) / 2, 0, n - q);
        var hi = lo + q - 1;
        var h = T.Max(x - N<T>(lo), N<T>(hi) - x);
        if (span > n) h += N<T>(span - n) / N<T>(2);
        // Margine come nel Fortran originale (0.999h): i punti al bordo della finestra hanno peso quasi nullo, non esattamente nullo.
        h = T.Max(h, Epsilon<T>()) / (N<T>(999) / N<T>(1000));

        T sw = T.Zero, swx = T.Zero, swy = T.Zero, swxx = T.Zero, swxy = T.Zero;
        for (var i = lo; i <= hi; i++)
        {
            // Ascissa centrata in x: evita sottrarre momenti di indici assoluti grandi.
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
        // Retta pesata; se i punti con peso sono degeneri in x si ripiega sulla media pesata (grado 0).
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

    /// <summary>ρ_t = B(|R_t|/(6·mediana|R|)), con bisquare B(u) = (1 − u²)² per u &lt; 1, altrimenti 0.</summary>
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
