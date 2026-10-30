using System.Numerics;
using static MathemaSharp.TimeSeries.Num;

namespace MathemaSharp.TimeSeries;

/// <summary>
/// Generators of synthetic processes with an explicit seed: the tests and notebooks verify the estimators on known parameters.
/// The default overloads draw doubles (<see cref="Random"/>) and convert them to T: the same seed gives the same initial noise.
/// The overloads with innovationSource accept innovations directly in T, without intermediate conversions.
/// Distribution, independence and standardisation of the custom source are the caller's responsibility.
/// </summary>
public static class Simulation
{
    /// <summary>N(0,1) with Box-Muller. 1 − U avoids ln(0).</summary>
    public static double StandardNormal(Random random)
    {
        var u1 = 1 - random.NextDouble();
        var u2 = random.NextDouble();
        return Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
    }

    /// <summary>N(0,1) convertita in T.</summary>
    public static T StandardNormal<T>(Random random) where T : IFloatingPointIeee754<T> => T.CreateChecked(StandardNormal(random));

    /// <summary>ARMA(p,q): y_t = c + Σ φ_i y_{t−i} + e_t + Σ θ_j e_{t−j}, e ~ N(0, σ²). The first burnIn values are discarded to forget the initial state.</summary>
    public static T[] Arma<T>(Random random, int n, T constant, T[] phi, T[] theta, T sigma, int burnIn = 200) where T : IFloatingPointIeee754<T>
        => Arma(random, n, constant, phi, theta, sigma, StandardNormal<T>, burnIn);

    /// <summary>ARMA with standardised innovations supplied directly in T; the source is also called during the burn-in.</summary>
    public static T[] Arma<T>(Random random, int n, T constant, T[] phi, T[] theta, T sigma,
        Func<Random, T> innovationSource, int burnIn = 200) where T : IFloatingPointIeee754<T>
    {
        ValidateSource(random, n, burnIn, innovationSource);
        ArgumentNullException.ThrowIfNull(phi);
        ArgumentNullException.ThrowIfNull(theta);
        var total = checked(n + burnIn);
        var y = new T[total];
        var e = new T[total];
        for (var t = 0; t < total; t++)
        {
            e[t] = sigma * Sample(random, innovationSource);
            var value = constant + e[t];
            for (var i = 1; i <= phi.Length && t - i >= 0; i++) value += phi[i - 1] * y[t - i];
            for (var j = 1; j <= theta.Length && t - j >= 0; j++) value += theta[j - 1] * e[t - j];
            y[t] = value;
        }
        return y[burnIn..];
    }

    /// <summary>Random walk with drift: y_t = y_{t−1} + drift + e_t.</summary>
    public static T[] RandomWalk<T>(Random random, int n, T start, T drift, T sigma) where T : IFloatingPointIeee754<T>
        => RandomWalk(random, n, start, drift, sigma, StandardNormal<T>);

    /// <summary>Random walk with innovations standardised directly in T.</summary>
    public static T[] RandomWalk<T>(Random random, int n, T start, T drift, T sigma,
        Func<Random, T> innovationSource) where T : IFloatingPointIeee754<T>
    {
        ValidateSource(random, n, 0, innovationSource);
        var y = new T[n];
        var level = start;
        for (var t = 0; t < n; t++) y[t] = level += drift + sigma * Sample(random, innovationSource);
        return y;
    }

    /// <summary>Standard random walk: starts at 0, no drift, σ = 1.</summary>
    public static T[] RandomWalk<T>(Random random, int n) where T : IFloatingPointIeee754<T> => RandomWalk(random, n, T.Zero, T.Zero, T.One);

    /// <summary>GARCH(1,1) shocks ε_t = σ_t z_t, starting from the unconditional variance.</summary>
    public static T[] Garch<T>(Random random, Garch11<T> model, int n, int burnIn = 200) where T : IFloatingPointIeee754<T>
        => Garch(random, model, n, StandardNormal<T>, burnIn);

    /// <summary>GARCH with innovations standardised directly in T.</summary>
    public static T[] Garch<T>(Random random, Garch11<T> model, int n,
        Func<Random, T> innovationSource, int burnIn = 200) where T : IFloatingPointIeee754<T>
    {
        ValidateSource(random, n, burnIn, innovationSource);
        ArgumentNullException.ThrowIfNull(model);
        var total = checked(n + burnIn);
        var shocks = new T[total];
        var variance = model.UnconditionalVariance;
        if (!T.IsFinite(variance)) throw new ArgumentException("The model must have α + β < 1.");
        for (var t = 0; t < total; t++)
        {
            if (t > 0) variance = model.Omega + model.Alpha * shocks[t - 1] * shocks[t - 1] + model.Beta * variance;
            shocks[t] = T.Sqrt(variance) * Sample(random, innovationSource);
        }
        return shocks[burnIn..];
    }

    /// <summary>VAR with independent Gaussian noise of standard deviation sigma for each series. Result T[time, series].</summary>
    public static T[,] Var<T>(Random random, VarModel<T> model, int n, T sigma, int burnIn = 200) where T : IFloatingPointIeee754<T>
        => Var(random, model, n, sigma, StandardNormal<T>, burnIn);

    /// <summary>VAR with innovations in T; one call per series and instant, including the burn-in instants.</summary>
    public static T[,] Var<T>(Random random, VarModel<T> model, int n, T sigma,
        Func<Random, T> innovationSource, int burnIn = 200) where T : IFloatingPointIeee754<T>
    {
        ValidateSource(random, n, burnIn, innovationSource);
        ArgumentNullException.ThrowIfNull(model);
        var k = model.Dimension;
        var total = checked(n + burnIn);
        var y = new T[total, k];
        for (var t = 0; t < total; t++)
            for (var r = 0; r < k; r++)
            {
                var value = model.Intercept[r] + sigma * Sample(random, innovationSource);
                for (var lag = 1; lag <= model.Order && t - lag >= 0; lag++)
                    for (var c = 0; c < k; c++) value += model.Coefficients[lag - 1][r, c] * y[t - lag, c];
                y[t, r] = value;
            }
        var result = new T[n, k];
        for (var t = 0; t < n; t++)
            for (var r = 0; r < k; r++) result[t, r] = y[burnIn + t, r];
        return result;
    }

    private static void ValidateSource<T>(Random random, int n, int burnIn, Func<Random, T> source)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(n);
        ArgumentOutOfRangeException.ThrowIfNegative(burnIn);
    }

    private static T Sample<T>(Random random, Func<Random, T> source) where T : IFloatingPointIeee754<T>
    {
        var value = source(random);
        if (!T.IsFinite(value)) throw new ArgumentException("La sorgente deve produrre innovazioni finite.", nameof(source));
        return value;
    }
}
