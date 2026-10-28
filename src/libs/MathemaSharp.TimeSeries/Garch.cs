using System.Numerics;
using static MathemaSharp.TimeSeries.Num;

namespace MathemaSharp.TimeSeries;

/// <summary>
/// GARCH(1,1) di Bollerslev (1986), estensione dell'ARCH di Engle (1982): ε_t = σ_t z_t, σ²_t = ω + α ε²_{t−1} + β σ²_{t−1}.
/// Modella la varianza condizionale (volatilità a grappoli), non la media: gli shock ε sono assunti a media nulla.
/// ARCH(1) è il caso β = 0. Stazionarietà in covarianza se α + β &lt; 1. Generico su <c>T : IFloatingPointIeee754&lt;T&gt;</c>.
/// </summary>
public sealed record Garch11<T>(T Omega, T Alpha, T Beta) where T : IFloatingPointIeee754<T>
{
    public T Persistence => Alpha + Beta;

    /// <summary>σ̄² = ω/(1 − α − β): livello a cui la volatilità ritorna; +∞ se α + β ≥ 1.</summary>
    public T UnconditionalVariance => Persistence < T.One ? Omega / (T.One - Persistence) : T.PositiveInfinity;

    /// <summary>Varianze condizionali σ²_t per t = 0..n−1, con σ²_0 = media campionaria di ε² (scelta convenzionale).</summary>
    public T[] ConditionalVariances(ReadOnlySpan<T> shocks)
    {
        if (shocks.IsEmpty) throw new ArgumentException("Serie vuota.");
        var variances = new T[shocks.Length];
        variances[0] = Garch11.MeanSquare(shocks);
        for (var t = 1; t < shocks.Length; t++)
            variances[t] = Omega + Alpha * shocks[t - 1] * shocks[t - 1] + Beta * variances[t - 1];
        return variances;
    }

    // Overload con array per i chiamanti senza supporto a ReadOnlySpan (reflection, PySharp: una ref struct non si può passare via reflection).
    public T[] ConditionalVariances(T[] shocks) => ConditionalVariances(shocks.AsSpan());

    // Overload con array per i chiamanti senza supporto a ReadOnlySpan (reflection, PySharp: una ref struct non si può passare via reflection).
    public T LogLikelihood(T[] shocks) => LogLikelihood(shocks.AsSpan());

    // Overload con array per i chiamanti senza supporto a ReadOnlySpan (reflection, PySharp: una ref struct non si può passare via reflection).
    public T[] ForecastVariance(T[] shocks, int horizon) => ForecastVariance(shocks.AsSpan(), horizon);

    /// <summary>Log-verosimiglianza gaussiana: −½ Σ (ln 2π + ln σ²_t + ε²_t/σ²_t).</summary>
    public T LogLikelihood(ReadOnlySpan<T> shocks)
    {
        var variances = ConditionalVariances(shocks);
        var sum = T.Zero;
        var logTau = T.Log(T.Tau);
        for (var t = 0; t < shocks.Length; t++)
            sum += logTau + T.Log(variances[t]) + shocks[t] * shocks[t] / variances[t];
        return -C<T>(0.5) * sum;
    }

    /// <summary>
    /// Previsione della varianza: σ²_{T+1} = ω + α ε²_T + β σ²_T, poi σ²_{T+h} = σ̄² + (α+β)^{h−1}(σ²_{T+1} − σ̄²):
    /// decadimento geometrico verso la varianza incondizionata con velocità α + β.
    /// </summary>
    public T[] ForecastVariance(ReadOnlySpan<T> shocks, int horizon)
    {
        var variances = ConditionalVariances(shocks);
        var next = Omega + Alpha * shocks[^1] * shocks[^1] + Beta * variances[^1];
        var result = new T[horizon];
        for (var h = 0; h < horizon; h++)
        {
            result[h] = next;
            next = Omega + Persistence * next;
        }
        return result;
    }
}

/// <summary>Stima di un <see cref="Garch11{T}"/>.</summary>
public static class Garch11
{
    /// <summary>
    /// Stima per massima verosimiglianza con variance targeting: ω = s²(1 − α − β) fissa la varianza incondizionata a quella campionaria
    /// e riduce il problema a (α, β). Griglia grossolana poi raffinata attorno al migliore: semplice e robusto, non efficiente.
    /// </summary>
    public static Garch11<T> FitVarianceTargeting<T>(ReadOnlySpan<T> shocks, int gridSize = 40, int refinements = 3) where T : IFloatingPointIeee754<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(gridSize, 2);
        ArgumentOutOfRangeException.ThrowIfNegative(refinements);
        if (shocks.IsEmpty) throw new ArgumentException("Serie vuota.", nameof(shocks));
        foreach (var value in shocks)
            if (!T.IsFinite(value)) throw new ArgumentException("Shock finiti richiesti.", nameof(shocks));
        var data = shocks.ToArray();
        var s2 = MeanSquare<T>(data);
        if (!T.IsFinite(s2) || s2 <= T.Zero)
            throw new ArgumentException("Media dei quadrati positiva e finita richiesta.", nameof(shocks));
        T bestAlpha = T.Zero, bestBeta = T.Zero, step = T.One / N<T>(gridSize), best = T.NegativeInfinity;
        T aLo = T.Zero, bLo = T.Zero;
        // Margine espresso in T; su Half 9999/10000 può arrotondarsi a 1.
        var limit = T.Min(N<T>(9999) / N<T>(10000), T.BitDecrement(T.One));
        var points = checked(gridSize + 1);
        for (var round = 0; round <= refinements; round++)
        {
            if (!T.IsFinite(step) || step <= T.Zero) break;
            // Indici interi: niente accumulo a += step, che può smettere di avanzare.
            for (var i = 0; i < points; i++)
            {
                var a = aLo + N<T>(i) * step;
                for (var j = 0; j < points; j++)
                {
                    var b = bLo + N<T>(j) * step;
                    var persistence = a + b;
                    if (a <= T.Zero || b < T.Zero || persistence > limit) continue;
                    var ll = new Garch11<T>(s2 * (T.One - persistence), a, b).LogLikelihood(data);
                    if (T.IsFinite(ll) && ll > best) (best, bestAlpha, bestBeta) = (ll, a, b);
                }
            }
            if (!T.IsFinite(best)) throw new ArithmeticException("Nessun candidato con likelihood finita.");
            (aLo, bLo) = (bestAlpha - step, bestBeta - step);
            step /= N<T>(10);
            points = 21; // ± il passo precedente, diviso in 20 intervalli.
        }
        return new Garch11<T>(s2 * (T.One - bestAlpha - bestBeta), bestAlpha, bestBeta);
    }

    internal static T MeanSquare<T>(ReadOnlySpan<T> x) where T : IFloatingPointIeee754<T>
    {
        var sum = T.Zero;
        foreach (var v in x) sum += v * v;
        return sum / N<T>(x.Length);
    }
}
