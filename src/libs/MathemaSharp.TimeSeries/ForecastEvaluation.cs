using System.Numerics;

namespace MathemaSharp.TimeSeries;

/// <summary>Complete, time-ordered windows; no implicit padding.</summary>
public static class HistoryPatches
{
    public static T[][] Extract<T>(ReadOnlySpan<T> history, int length, int stride = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(stride, 1);
        if (history.Length < length) return [];
        var count = 1 + (history.Length - length) / stride;
        var patches = new T[count][];
        for (var i = 0; i < count; i++) patches[i] = history.Slice(i * stride, length).ToArray();
        return patches;
    }
}

/// <summary>Origin is the exclusive index of the training set; Actual and Forecast cover the same future.</summary>
public sealed record ForecastFold<T>(int Origin, T[] Actual, T[] Forecast, T Mae, T Rmse);

/// <summary>Expanding-window backtest, with an optional gap before the evaluated period.</summary>
public static class RollingOrigin
{
    /// <summary>
    /// The callback receives a copy of the past only and must forecast gap + horizon steps.
    /// It must redo fit and preprocessing on that past, without capturing future data.
    /// With a step smaller than horizon the tests overlap: the scores are not independent.
    /// </summary>
    public static ForecastFold<T>[] Evaluate<T>(ReadOnlySpan<T> series, int initialTraining,
        int horizon, Func<T[], int, T[]> fitForecast, int step = 1, int gap = 0)
        where T : IFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNull(fitForecast);
        ArgumentOutOfRangeException.ThrowIfLessThan(initialTraining, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(horizon, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(step, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(gap);
        var predictionLength = checked(gap + horizon);
        if (initialTraining > series.Length || predictionLength > series.Length - initialTraining)
            throw new ArgumentException("Insufficient data for a complete first origin.", nameof(series));
        foreach (var value in series)
            if (!T.IsFinite(value)) throw new ArgumentException("Only finite data are allowed.", nameof(series));
        var folds = new List<ForecastFold<T>>();
        var lastOrigin = series.Length - predictionLength;
        for (var origin = initialTraining; origin <= lastOrigin;)
        {
            var predicted = fitForecast(series[..origin].ToArray(), predictionLength);
            if (predicted is null || predicted.Length != predictionLength)
                throw new ArgumentException("The callback must return gap + horizon forecasts.", nameof(fitForecast));
            foreach (var value in predicted)
                if (!T.IsFinite(value)) throw new ArgumentException("Previsioni non finite.", nameof(fitForecast));
            var actual = series.Slice(origin + gap, horizon).ToArray();
            var forecast = predicted.AsSpan(gap, horizon).ToArray();
            folds.Add(new(origin, actual, forecast, Accuracy.Mae<T>(actual, forecast), Accuracy.Rmse<T>(actual, forecast)));
            if (step > lastOrigin - origin) break;
            origin += step;
        }
        return folds.ToArray();
    }
}
