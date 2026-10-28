using System.Numerics;

namespace MathemaSharp.TimeSeries;

/// <summary>Finestre complete ordinate temporalmente; nessun padding implicito.</summary>
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

/// <summary>Origine è l'indice esclusivo del training; Actual e Forecast coprono lo stesso futuro.</summary>
public sealed record ForecastFold<T>(int Origin, T[] Actual, T[] Forecast, T Mae, T Rmse);

/// <summary>Backtest expanding-window, con gap opzionale prima del periodo valutato.</summary>
public static class RollingOrigin
{
    /// <summary>
    /// Il callback riceve una copia del solo passato e deve prevedere gap + horizon passi.
    /// Deve rifare fit e preprocessing su quel passato, senza catturare dati futuri.
    /// Con step inferiore a horizon i test si sovrappongono: i punteggi non sono indipendenti.
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
            throw new ArgumentException("Dati insufficienti per una prima origine completa.", nameof(series));
        foreach (var value in series)
            if (!T.IsFinite(value)) throw new ArgumentException("Sono ammessi solo dati finiti.", nameof(series));
        var folds = new List<ForecastFold<T>>();
        var lastOrigin = series.Length - predictionLength;
        for (var origin = initialTraining; origin <= lastOrigin;)
        {
            var predicted = fitForecast(series[..origin].ToArray(), predictionLength);
            if (predicted is null || predicted.Length != predictionLength)
                throw new ArgumentException("Il callback deve restituire gap + horizon previsioni.", nameof(fitForecast));
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
