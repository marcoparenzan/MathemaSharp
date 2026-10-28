using System.Numerics;
using static MathemaSharp.TimeSeries.Num;

namespace MathemaSharp.TimeSeries;

/// <summary>Statistiche di base, generiche su <c>T : IFloatingPointIeee754&lt;T&gt;</c>: autocovarianza, ACF, PACF (Levinson-Durbin), differenze, test di Ljung-Box.</summary>
public static class SeriesStatistics
{
    public static T Mean<T>(ReadOnlySpan<T> y) where T : IFloatingPointIeee754<T>
    {
        if (y.IsEmpty) throw new ArgumentException("Serie vuota.");
        var sum = T.Zero;
        foreach (var v in y) sum += v;
        return sum / N<T>(y.Length);
    }

    /// <summary>
    /// Quantile empirico di livello p ∈ [0,1] con interpolazione lineare fra le statistiche d'ordine (metodo 7 di Hyndman-Fan,
    /// il default di <c>np.quantile</c>): h = (n−1)p, Q = x₍⌊h⌋₎ + (h − ⌊h⌋)(x₍⌊h⌋+1₎ − x₍⌊h⌋₎). Con p = 0.5 è la mediana. Ignora l'ordine temporale.
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
    /// γ(k) = (1/n) Σ (y_t − ȳ)(y_{t+k} − ȳ), k = 0..maxLag. Divisore n e non n−k: lo stimatore distorto rende la matrice di Toeplitz
    /// semidefinita positiva, condizione che garantisce |PACF| ≤ 1 e un AR di Yule-Walker stazionario.
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
        if (gamma[0] == T.Zero) throw new ArgumentException("Serie costante: autocorrelazione non definita.");
        var result = new T[gamma.Length];
        for (var k = 0; k < gamma.Length; k++) result[k] = gamma[k] / gamma[0];
        return result;
    }

    /// <summary>
    /// Ricorsione di Levinson-Durbin: risolve le equazioni di Yule-Walker Γφ = γ in O(p²) sfruttando la struttura di Toeplitz.
    /// Il coefficiente di riflessione κ_k = φ_kk è la PACF al lag k; la varianza d'innovazione si aggiorna come v_k = v_{k−1}(1 − κ_k²).
    /// </summary>
    public static (T[] Phi, T[] Pacf, T InnovationVariance) LevinsonDurbin<T>(ReadOnlySpan<T> autocovariance, int order) where T : IFloatingPointIeee754<T>
    {
        if (order < 0 || order >= autocovariance.Length) throw new ArgumentOutOfRangeException(nameof(order));
        if (autocovariance[0] <= T.Zero) throw new ArgumentException("γ(0) deve essere positivo.");
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

    /// <summary>Differenza Δ_m y_t = y_t − y_{t−m}; lag 1 per il trend, lag m per la stagionalità. Restituisce n − lag valori.</summary>
    public static T[] Difference<T>(ReadOnlySpan<T> y, int lag = 1) where T : IFloatingPointIeee754<T>
    {
        if (lag < 1 || lag >= y.Length) throw new ArgumentOutOfRangeException(nameof(lag));
        var result = new T[y.Length - lag];
        for (var t = 0; t < result.Length; t++) result[t] = y[t + lag] - y[t];
        return result;
    }

    /// <summary>
    /// Ljung-Box: Q = n(n+2) Σ_{k=1..h} ρ̂_k²/(n−k), approssimativamente χ² con h − fittedParameters gradi di libertà se i residui
    /// sono rumore bianco. P-value piccolo ⇒ autocorrelazione residua: il modello non ha catturato tutta la dinamica.
    /// </summary>
    public static (T Statistic, int DegreesOfFreedom, T PValue) LjungBox<T>(ReadOnlySpan<T> residuals, int lags, int fittedParameters = 0) where T : IFloatingPointIeee754<T>
    {
        var dof = lags - fittedParameters;
        if (dof < 1) throw new ArgumentException("Gradi di libertà non positivi.");
        var rho = Acf(residuals, lags);
        var n = residuals.Length;
        var q = T.Zero;
        for (var k = 1; k <= lags; k++) q += rho[k] * rho[k] / N<T>(n - k);
        q *= N<T>(n) * N<T>(n + 2);
        return (q, dof, SpecialFunctions.ChiSquareSurvival(q, dof));
    }
}
