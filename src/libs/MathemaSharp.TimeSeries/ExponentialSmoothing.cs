using System.Numerics;
using static MathemaSharp.TimeSeries.Num;

namespace MathemaSharp.TimeSeries;

/// <summary>
/// Esito di un metodo di smoothing. Fitted[t] è la previsione a un passo ŷ_{t|t−1}, Forecast le previsioni h = 1..H dopo l'ultima osservazione.
/// Sse somma gli errori a un passo dalla prima osservazione realmente prevista.
/// </summary>
public sealed record SmoothingResult<T>(T[] Fitted, T[] Forecast, T Level, T Trend, T[] Season, T Sse) where T : IFloatingPointIeee754<T>;

/// <summary>
/// Smoothing esponenziale additivo (Brown/Holt 1957/Winters 1960, damped trend di Gardner &amp; McKenzie 1985) in forma a componenti,
/// generico su <c>T : IFloatingPointIeee754&lt;T&gt;</c>. Inizializzazioni semplici e deterministiche, non stimate: i primi residui risentono della scelta.
/// </summary>
public static class ExponentialSmoothing
{
    /// <summary>
    /// SES: ℓ_t = α y_t + (1−α) ℓ_{t−1}, ŷ_{T+h} = ℓ_T. Media pesata con pesi geometrici α(1−α)^j.
    /// Equivale ad ARIMA(0,1,1) con θ = α − 1 e al filtro di Kalman stazionario del local level. ℓ_0 = y_0.
    /// </summary>
    public static SmoothingResult<T> Ses<T>(ReadOnlySpan<T> y, T alpha, int horizon) where T : IFloatingPointIeee754<T> =>
        HoltCore(y, alpha, T.Zero, horizon, T.One, withTrend: false);

    /// <summary>Holt lineare (φ = 1): il trend prosegue all'infinito. ℓ_0 = y_0, b_0 = y_1 − y_0.</summary>
    public static SmoothingResult<T> Holt<T>(ReadOnlySpan<T> y, T alpha, T beta, int horizon) where T : IFloatingPointIeee754<T> =>
        HoltCore(y, alpha, beta, horizon, T.One, withTrend: true);

    /// <summary>
    /// Holt con damping φ: ŷ_{t|t−1} = ℓ + φb; ℓ_t = α y_t + (1−α)(ℓ + φb); b_t = β(ℓ_t − ℓ_{t−1}) + (1−β)φb.
    /// ŷ_{T+h} = ℓ_T + (φ + φ² + … + φ^h) b_T → ℓ_T + φ b_T/(1−φ) per φ &lt; 1: il trend si spegne invece di proseguire all'infinito.
    /// φ = 1 è Holt lineare. ℓ_0 = y_0, b_0 = y_1 − y_0.
    /// </summary>
    public static SmoothingResult<T> Holt<T>(ReadOnlySpan<T> y, T alpha, T beta, int horizon, T phi) where T : IFloatingPointIeee754<T> =>
        HoltCore(y, alpha, beta, horizon, phi, withTrend: true);

    private static SmoothingResult<T> HoltCore<T>(ReadOnlySpan<T> y, T alpha, T beta, int horizon, T phi, bool withTrend) where T : IFloatingPointIeee754<T>
    {
        CheckUnit(alpha, nameof(alpha));
        CheckUnit(beta, nameof(beta));
        CheckUnit(phi, nameof(phi));
        if (y.Length < 2) throw new ArgumentException("Servono almeno due osservazioni.");
        var level = y[0];
        var trend = withTrend ? y[1] - y[0] : T.Zero;
        var fitted = new T[y.Length];
        fitted[0] = y[0];
        var sse = T.Zero;
        for (var t = 1; t < y.Length; t++)
        {
            var prediction = level + phi * trend;
            fitted[t] = prediction;
            var error = y[t] - prediction;
            sse += error * error;
            var previousLevel = level;
            level = prediction + alpha * error;
            if (withTrend) trend = beta * (level - previousLevel) + (T.One - beta) * phi * trend;
        }
        return new SmoothingResult<T>(fitted, DampedPath(level, trend, phi, horizon, _ => T.Zero), level, trend, [], sse);
    }

    /// <summary>Holt-Winters additivo senza damping (φ = 1), periodo m. Vedi l'overload con φ per le formule.</summary>
    public static SmoothingResult<T> HoltWinters<T>(ReadOnlySpan<T> y, int period, T alpha, T beta, T gamma, int horizon) where T : IFloatingPointIeee754<T> =>
        HoltWinters(y, period, alpha, beta, gamma, horizon, T.One);

    /// <summary>
    /// Holt-Winters additivo con damping, periodo m:
    /// ℓ_t = α(y_t − s_{t−m}) + (1−α)(ℓ + φb); b_t = β(ℓ_t − ℓ_{t−1}) + (1−β)φb; s_t = γ(y_t − ℓ_{t−1} − φb_{t−1}) + (1−γ)s_{t−m}.
    /// Inizializzazione sui primi due cicli: b_0 = (media 2° − media 1°)/m, ℓ_0 = media 1° + b_0(m−1)/2, s_j = y_j − (media 1° + b_0(j − (m−1)/2)).
    /// Fitted[t] = y_t per t &lt; m. Season[j] corrisponde a t ≡ j (mod m).
    /// </summary>
    public static SmoothingResult<T> HoltWinters<T>(ReadOnlySpan<T> y, int period, T alpha, T beta, T gamma, int horizon, T phi) where T : IFloatingPointIeee754<T>
    {
        CheckUnit(alpha, nameof(alpha));
        CheckUnit(beta, nameof(beta));
        CheckUnit(gamma, nameof(gamma));
        CheckUnit(phi, nameof(phi));
        if (period < 2 || y.Length < 2 * period) throw new ArgumentException("Servono almeno due stagioni complete.");
        // La media del primo ciclo stima il livello al suo centro (m−1)/2, non alla fine: si sposta lungo il trend fino a t = m−1
        // e si tolgono dalla stagionalità iniziale sia il livello sia il trend. Così retta + stagionalità a media nulla è riprodotta esattamente.
        var firstMean = SeriesStatistics.Mean(y[..period]);
        var trend = (SeriesStatistics.Mean(y[period..(2 * period)]) - firstMean) / N<T>(period);
        var center = N<T>(period - 1) / N<T>(2);
        var level = firstMean + trend * center;
        var season = new T[period];
        for (var j = 0; j < period; j++) season[j] = y[j] - (firstMean + trend * (N<T>(j) - center));
        var fitted = new T[y.Length];
        y[..period].CopyTo(fitted);
        var sse = T.Zero;
        for (var t = period; t < y.Length; t++)
        {
            var s = season[t % period];
            var damped = level + phi * trend;
            var prediction = damped + s;
            fitted[t] = prediction;
            sse += (y[t] - prediction) * (y[t] - prediction);
            var previousLevel = level;
            level = alpha * (y[t] - s) + (T.One - alpha) * damped;
            trend = beta * (level - previousLevel) + (T.One - beta) * phi * trend;
            season[t % period] = gamma * (y[t] - damped) + (T.One - gamma) * s;
        }
        var n = y.Length;
        var seasonal = season;
        var forecast = DampedPath(level, trend, phi, horizon, h => seasonal[(n + h - 1) % period]);
        return new SmoothingResult<T>(fitted, forecast, level, trend, season, sse);
    }

    /// <summary>
    /// Stima α del SES minimizzando l'SSE a un passo con la sezione aurea su [0,1]. Presuppone SSE unimodale in α, tipico ma non garantito.
    /// </summary>
    public static T FitSesAlpha<T>(ReadOnlySpan<T> y) where T : IFloatingPointIeee754<T>
    {
        var data = y.ToArray();
        return GoldenSection<T>(a => Ses<T>(data, a, 0).Sse, T.Zero, T.One);
    }

    /// <summary>Sezione aurea con tolleranza √ε del tipo (≈ 1.5e-8 per double).</summary>
    public static T GoldenSection<T>(Func<T, T> f, T a, T b) where T : IFloatingPointIeee754<T> => GoldenSection(f, a, b, T.Sqrt(Epsilon<T>()));

    /// <summary>Minimo di una funzione unimodale su [a,b]: ogni passo riduce l'intervallo del fattore 1/φ ≈ 0.618 riusando un punto.</summary>
    public static T GoldenSection<T>(Func<T, T> f, T a, T b, T tolerance) where T : IFloatingPointIeee754<T>
    {
        var invPhi = (T.Sqrt(N<T>(5)) - T.One) / N<T>(2);
        T c = b - invPhi * (b - a), d = a + invPhi * (b - a);
        T fc = f(c), fd = f(d);
        while (b - a > tolerance)
        {
            if (fc < fd) { b = d; d = c; fd = fc; c = b - invPhi * (b - a); fc = f(c); }
            else { a = c; c = d; fc = fd; d = a + invPhi * (b - a); fd = f(d); }
        }
        return (a + b) / N<T>(2);
    }

    private static T[] DampedPath<T>(T level, T trend, T phi, int horizon, Func<int, T> season) where T : IFloatingPointIeee754<T>
    {
        if (horizon < 0) throw new ArgumentOutOfRangeException(nameof(horizon));
        var result = new T[horizon];
        T cumulative = T.Zero, power = T.One;
        for (var h = 1; h <= horizon; h++)
        {
            power *= phi;
            cumulative += power;
            result[h - 1] = level + cumulative * trend + season(h);
        }
        return result;
    }

    private static void CheckUnit<T>(T value, string name) where T : IFloatingPointIeee754<T>
    {
        if (!(value >= T.Zero && value <= T.One)) throw new ArgumentOutOfRangeException(name, "Atteso un valore in [0,1].");
    }
}
