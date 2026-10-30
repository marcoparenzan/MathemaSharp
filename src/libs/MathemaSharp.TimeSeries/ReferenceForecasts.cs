using System.Numerics;
using static MathemaSharp.TimeSeries.Num;

namespace MathemaSharp.TimeSeries;

/// <summary>
/// Reference forecasts, generic over <c>T : IFloatingPointIeee754&lt;T&gt;</c>: every model must beat them to justify its own complexity.
/// </summary>
public static class ReferenceForecasts
{
    /// <summary>ŷ_{T+h} = ȳ: historical mean.</summary>
    public static T[] Mean<T>(ReadOnlySpan<T> y, int horizon) where T : IFloatingPointIeee754<T> => Repeat(SeriesStatistics.Mean(y), horizon);

    /// <summary>ŷ_{T+h} = y_T: last value. Optimal for a random walk.</summary>
    public static T[] Naive<T>(ReadOnlySpan<T> y, int horizon) where T : IFloatingPointIeee754<T>
    {
        if (y.IsEmpty) throw new ArgumentException("Empty series.");
        return Repeat(y[^1], horizon);
    }

    /// <summary>ŷ_{T+h} = y_{T+h−m(k+1)}, k = ⌊(h−1)/m⌋: last value of the same season.</summary>
    public static T[] SeasonalNaive<T>(ReadOnlySpan<T> y, int period, int horizon) where T : IFloatingPointIeee754<T>
    {
        if (period < 1 || y.Length < period) throw new ArgumentException("At least one complete season is required.");
        var result = new T[horizon];
        for (var h = 1; h <= horizon; h++) result[h - 1] = y[y.Length - period + (h - 1) % period];
        return result;
    }

    /// <summary>ŷ_{T+h} = y_T + h(y_T − y_1)/(T − 1): naive with the mean slope of the line between the first and last observation.</summary>
    public static T[] Drift<T>(ReadOnlySpan<T> y, int horizon) where T : IFloatingPointIeee754<T>
    {
        if (y.Length < 2) throw new ArgumentException("Servono almeno due osservazioni.");
        var slope = (y[^1] - y[0]) / N<T>(y.Length - 1);
        var result = new T[horizon];
        for (var h = 1; h <= horizon; h++) result[h - 1] = y[^1] + N<T>(h) * slope;
        return result;
    }

    private static T[] Repeat<T>(T value, int horizon)
    {
        if (horizon < 0) throw new ArgumentOutOfRangeException(nameof(horizon));
        var result = new T[horizon];
        Array.Fill(result, value);
        return result;
    }
}

/// <summary>Out-of-sample accuracy measures, generic over <c>T : IFloatingPointIeee754&lt;T&gt;</c>.</summary>
public static class Accuracy
{
    public static T Mae<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> forecast) where T : IFloatingPointIeee754<T> => MeanOf(actual, forecast, e => T.Abs(e));

    public static T Rmse<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> forecast) where T : IFloatingPointIeee754<T> => T.Sqrt(MeanOf(actual, forecast, e => e * e));

    /// <summary>MAPE as a percentage. Undefined if an observed value is zero, and it explodes near zero: prefer MASE.</summary>
    public static T Mape<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> forecast) where T : IFloatingPointIeee754<T>
    {
        if (actual.Length != forecast.Length) throw new ArgumentException("Lunghezze diverse.");
        foreach (var a in actual)
            if (a == T.Zero) throw new ArgumentException("MAPE undefined with zero observations.");
        var sum = T.Zero;
        for (var i = 0; i < actual.Length; i++) sum += T.Abs((actual[i] - forecast[i]) / actual[i]);
        return N<T>(100) * sum / N<T>(actual.Length);
    }

    /// <summary>
    /// MASE (Hyndman &amp; Koehler, 2006): MAE divided by the in-sample MAE of the seasonal naive of period m. &lt; 1 ⇒ better than naive.
    /// Scale-independent and defined even with zeros.
    /// </summary>
    public static T Mase<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> forecast, ReadOnlySpan<T> training, int period = 1) where T : IFloatingPointIeee754<T>
    {
        if (training.Length <= period) throw new ArgumentException("Training set too short for the period.");
        var scale = T.Zero;
        for (var t = period; t < training.Length; t++) scale += T.Abs(training[t] - training[t - period]);
        scale /= N<T>(training.Length - period);
        if (scale == T.Zero) throw new ArgumentException("Training set constant over the period: zero scale.");
        return Mae(actual, forecast) / scale;
    }

    /// <summary>
    /// Pinball loss (quantile loss) of a forecast of the quantile of level p: mean of max(p·e, (p−1)·e) with e = y − q.
    /// It is the **proper** loss for quantiles: its expected minimum is the true quantile. With p = 0.5 it equals MAE/2.
    /// It penalises under- and over-estimation asymmetrically: for p = 0.9 erring on the low side costs nine times as much as on the high side.
    /// </summary>
    public static T PinballLoss<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> quantileForecast, T p) where T : IFloatingPointIeee754<T>
    {
        if (T.IsNaN(p) || p <= T.Zero || p >= T.One) throw new ArgumentOutOfRangeException(nameof(p), "Atteso un livello in (0,1).");
        return MeanOf(actual, quantileForecast, e => T.Max(p * e, (p - T.One) * e));
    }

    /// <summary>Empirical coverage of an interval: fraction of the observations in [lower, upper]. For a well-calibrated P10–P90 interval it is ≈ 0.8.</summary>
    public static T IntervalCoverage<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> lower, ReadOnlySpan<T> upper) where T : IFloatingPointIeee754<T>
    {
        if (actual.Length != lower.Length || actual.Length != upper.Length) throw new ArgumentException("Lunghezze diverse.");
        if (actual.IsEmpty) throw new ArgumentException("Empty series.");
        var inside = 0;
        for (var i = 0; i < actual.Length; i++)
            if (actual[i] >= lower[i] && actual[i] <= upper[i]) inside++;
        return N<T>(inside) / N<T>(actual.Length);
    }

    private static T MeanOf<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> forecast, Func<T, T> loss) where T : IFloatingPointIeee754<T>
    {
        if (actual.Length != forecast.Length) throw new ArgumentException("Lunghezze diverse.");
        if (actual.IsEmpty) throw new ArgumentException("Empty series.");
        var sum = T.Zero;
        for (var i = 0; i < actual.Length; i++) sum += loss(actual[i] - forecast[i]);
        return sum / N<T>(actual.Length);
    }
}
