using System.Numerics;
using static MathemaSharp.TimeSeries.Num;

namespace MathemaSharp.TimeSeries;

/// <summary>
/// Outcome of a smoothing method. Fitted[t] is the one-step forecast ŷ_{t|t−1}, Forecast the forecasts h = 1..H after the last observation.
/// Sse sums the one-step errors from the first observation that is actually forecast.
/// </summary>
public sealed record SmoothingResult<T>(T[] Fitted, T[] Forecast, T Level, T Trend, T[] Season, T Sse) where T : IFloatingPointIeee754<T>;

/// <summary>
/// Smoothing esponenziale additivo (Brown/Holt 1957/Winters 1960, damped trend di Gardner &amp; McKenzie 1985) in forma a componenti,
/// generic over <c>T : IFloatingPointIeee754&lt;T&gt;</c>. Simple, deterministic initialisations, not estimated: the first residuals are affected by the choice.
/// </summary>
public static class ExponentialSmoothing
{
    /// <summary>
    /// SES: ℓ_t = α y_t + (1−α) ℓ_{t−1}, ŷ_{T+h} = ℓ_T. Weighted mean with geometric weights α(1−α)^j.
    /// Equivalent to ARIMA(0,1,1) with θ = α − 1 and to the stationary Kalman filter of the local level. ℓ_0 = y_0.
    /// </summary>
    public static SmoothingResult<T> Ses<T>(ReadOnlySpan<T> y, T alpha, int horizon) where T : IFloatingPointIeee754<T> =>
        HoltCore(y, alpha, T.Zero, horizon, T.One, withTrend: false);

    /// <summary>Linear Holt (φ = 1): the trend continues to infinity. ℓ_0 = y_0, b_0 = y_1 − y_0.</summary>
    public static SmoothingResult<T> Holt<T>(ReadOnlySpan<T> y, T alpha, T beta, int horizon) where T : IFloatingPointIeee754<T> =>
        HoltCore(y, alpha, beta, horizon, T.One, withTrend: true);

    /// <summary>
    /// Holt with damping φ: ŷ_{t|t−1} = ℓ + φb; ℓ_t = α y_t + (1−α)(ℓ + φb); b_t = β(ℓ_t − ℓ_{t−1}) + (1−β)φb.
    /// ŷ_{T+h} = ℓ_T + (φ + φ² + … + φ^h) b_T → ℓ_T + φ b_T/(1−φ) for φ &lt; 1: the trend fades out instead of continuing to infinity.
    /// φ = 1 is linear Holt. ℓ_0 = y_0, b_0 = y_1 − y_0.
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

    /// <summary>Additive Holt-Winters without damping (φ = 1), period m. See the overload with φ for the formulas.</summary>
    public static SmoothingResult<T> HoltWinters<T>(ReadOnlySpan<T> y, int period, T alpha, T beta, T gamma, int horizon) where T : IFloatingPointIeee754<T> =>
        HoltWinters(y, period, alpha, beta, gamma, horizon, T.One);

    /// <summary>
    /// Additive Holt-Winters with damping, period m:
    /// ℓ_t = α(y_t − s_{t−m}) + (1−α)(ℓ + φb); b_t = β(ℓ_t − ℓ_{t−1}) + (1−β)φb; s_t = γ(y_t − ℓ_{t−1} − φb_{t−1}) + (1−γ)s_{t−m}.
    /// Initialisation on the first two cycles: b_0 = (mean of 2nd − mean of 1st)/m, ℓ_0 = mean of 1st + b_0(m−1)/2, s_j = y_j − (mean of 1st + b_0(j − (m−1)/2)).
    /// Fitted[t] = y_t for t &lt; m. Season[j] corresponds to t ≡ j (mod m).
    /// </summary>
    public static SmoothingResult<T> HoltWinters<T>(ReadOnlySpan<T> y, int period, T alpha, T beta, T gamma, int horizon, T phi) where T : IFloatingPointIeee754<T>
    {
        CheckUnit(alpha, nameof(alpha));
        CheckUnit(beta, nameof(beta));
        CheckUnit(gamma, nameof(gamma));
        CheckUnit(phi, nameof(phi));
        if (period < 2 || y.Length < 2 * period) throw new ArgumentException("Servono almeno due stagioni complete.");
        // The mean of the first cycle estimates the level at its centre (m−1)/2, not at the end: it is shifted along the trend up to t = m−1
        // and both the level and the trend are removed from the initial seasonality. So a line + zero-mean seasonality is reproduced exactly.
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
    /// Estimates the SES α by minimising the one-step SSE with the golden section on [0,1]. Presupposes a unimodal SSE in α, typical but not guaranteed.
    /// </summary>
    public static T FitSesAlpha<T>(ReadOnlySpan<T> y) where T : IFloatingPointIeee754<T>
    {
        var data = y.ToArray();
        return GoldenSection<T>(a => Ses<T>(data, a, 0).Sse, T.Zero, T.One);
    }

    /// <summary>Golden section with tolerance √ε of the type (≈ 1.5e-8 for double).</summary>
    public static T GoldenSection<T>(Func<T, T> f, T a, T b) where T : IFloatingPointIeee754<T> => GoldenSection(f, a, b, T.Sqrt(Epsilon<T>()));

    /// <summary>Minimum of a unimodal function on [a,b]: each step shrinks the interval by the factor 1/φ ≈ 0.618, reusing a point.</summary>
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
