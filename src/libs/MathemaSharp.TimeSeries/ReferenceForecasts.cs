using System.Numerics;
using static MathemaSharp.TimeSeries.Num;

namespace MathemaSharp.TimeSeries;

/// <summary>
/// Previsioni di riferimento, generiche su <c>T : IFloatingPointIeee754&lt;T&gt;</c>: ogni modello deve batterle per giustificare la propria complessità.
/// </summary>
public static class ReferenceForecasts
{
    /// <summary>ŷ_{T+h} = ȳ: media storica.</summary>
    public static T[] Mean<T>(ReadOnlySpan<T> y, int horizon) where T : IFloatingPointIeee754<T> => Repeat(SeriesStatistics.Mean(y), horizon);

    /// <summary>ŷ_{T+h} = y_T: ultimo valore. Ottimo per una passeggiata aleatoria.</summary>
    public static T[] Naive<T>(ReadOnlySpan<T> y, int horizon) where T : IFloatingPointIeee754<T>
    {
        if (y.IsEmpty) throw new ArgumentException("Serie vuota.");
        return Repeat(y[^1], horizon);
    }

    /// <summary>ŷ_{T+h} = y_{T+h−m(k+1)}, k = ⌊(h−1)/m⌋: ultimo valore della stessa stagione.</summary>
    public static T[] SeasonalNaive<T>(ReadOnlySpan<T> y, int period, int horizon) where T : IFloatingPointIeee754<T>
    {
        if (period < 1 || y.Length < period) throw new ArgumentException("Serve almeno una stagione completa.");
        var result = new T[horizon];
        for (var h = 1; h <= horizon; h++) result[h - 1] = y[y.Length - period + (h - 1) % period];
        return result;
    }

    /// <summary>ŷ_{T+h} = y_T + h(y_T − y_1)/(T − 1): naive con la pendenza media della retta fra prima e ultima osservazione.</summary>
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

/// <summary>Misure di accuratezza fuori campione, generiche su <c>T : IFloatingPointIeee754&lt;T&gt;</c>.</summary>
public static class Accuracy
{
    public static T Mae<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> forecast) where T : IFloatingPointIeee754<T> => MeanOf(actual, forecast, e => T.Abs(e));

    public static T Rmse<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> forecast) where T : IFloatingPointIeee754<T> => T.Sqrt(MeanOf(actual, forecast, e => e * e));

    /// <summary>MAPE in percentuale. Non definita se un valore osservato è zero, ed esplode vicino a zero: preferire MASE.</summary>
    public static T Mape<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> forecast) where T : IFloatingPointIeee754<T>
    {
        if (actual.Length != forecast.Length) throw new ArgumentException("Lunghezze diverse.");
        foreach (var a in actual)
            if (a == T.Zero) throw new ArgumentException("MAPE non definita con osservazioni nulle.");
        var sum = T.Zero;
        for (var i = 0; i < actual.Length; i++) sum += T.Abs((actual[i] - forecast[i]) / actual[i]);
        return N<T>(100) * sum / N<T>(actual.Length);
    }

    /// <summary>
    /// MASE (Hyndman &amp; Koehler, 2006): MAE diviso per il MAE in-sample del seasonal naive di periodo m. &lt; 1 ⇒ meglio del naive.
    /// Indipendente dalla scala e definita anche con zeri.
    /// </summary>
    public static T Mase<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> forecast, ReadOnlySpan<T> training, int period = 1) where T : IFloatingPointIeee754<T>
    {
        if (training.Length <= period) throw new ArgumentException("Training troppo corto per il periodo.");
        var scale = T.Zero;
        for (var t = period; t < training.Length; t++) scale += T.Abs(training[t] - training[t - period]);
        scale /= N<T>(training.Length - period);
        if (scale == T.Zero) throw new ArgumentException("Training costante sul periodo: scala nulla.");
        return Mae(actual, forecast) / scale;
    }

    /// <summary>
    /// Perdita pinball (quantile loss) di una previsione del quantile di livello p: media di max(p·e, (p−1)·e) con e = y − q.
    /// È la perdita **propria** per i quantili: il suo minimo atteso è il vero quantile. Con p = 0.5 vale MAE/2.
    /// Penalizza sotto-stima e sovra-stima in modo asimmetrico: per p = 0.9 sbagliare per difetto costa nove volte che per eccesso.
    /// </summary>
    public static T PinballLoss<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> quantileForecast, T p) where T : IFloatingPointIeee754<T>
    {
        if (T.IsNaN(p) || p <= T.Zero || p >= T.One) throw new ArgumentOutOfRangeException(nameof(p), "Atteso un livello in (0,1).");
        return MeanOf(actual, quantileForecast, e => T.Max(p * e, (p - T.One) * e));
    }

    /// <summary>Copertura empirica di un intervallo: frazione delle osservazioni in [lower, upper]. Per un intervallo P10–P90 ben calibrato vale ≈ 0.8.</summary>
    public static T IntervalCoverage<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> lower, ReadOnlySpan<T> upper) where T : IFloatingPointIeee754<T>
    {
        if (actual.Length != lower.Length || actual.Length != upper.Length) throw new ArgumentException("Lunghezze diverse.");
        if (actual.IsEmpty) throw new ArgumentException("Serie vuota.");
        var inside = 0;
        for (var i = 0; i < actual.Length; i++)
            if (actual[i] >= lower[i] && actual[i] <= upper[i]) inside++;
        return N<T>(inside) / N<T>(actual.Length);
    }

    private static T MeanOf<T>(ReadOnlySpan<T> actual, ReadOnlySpan<T> forecast, Func<T, T> loss) where T : IFloatingPointIeee754<T>
    {
        if (actual.Length != forecast.Length) throw new ArgumentException("Lunghezze diverse.");
        if (actual.IsEmpty) throw new ArgumentException("Serie vuota.");
        var sum = T.Zero;
        for (var i = 0; i < actual.Length; i++) sum += loss(actual[i] - forecast[i]);
        return sum / N<T>(actual.Length);
    }
}
