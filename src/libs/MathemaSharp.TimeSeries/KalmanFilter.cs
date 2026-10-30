using System.Numerics;
using static MathemaSharp.TimeSeries.Num;

namespace MathemaSharp.TimeSeries;

/// <summary>
/// Linear Gaussian Kalman filter (1960) with a scalar observation, generic over <c>T : IFloatingPointIeee754&lt;T&gt;</c>:
/// x_t = F x_{t−1} + w_t, w ~ N(0,Q);  y_t = H x_t + v_t, v ~ N(0,R).
/// Predict propagates state and uncertainty, Update corrects with the gain K = P Hᵀ/S. With a scalar observation S is a number:
/// no matrix inversion.
/// </summary>
public sealed class KalmanFilter<T> where T : IFloatingPointIeee754<T>
{
    private readonly T[,] _f, _q;
    private readonly T[] _h;
    private readonly T _r;

    public T[] State { get; private set; }
    public T[,] Covariance { get; private set; }
    public T[] Gain { get; private set; }

    public KalmanFilter(T[,] transition, T[,] processNoise, T[] observation, T observationNoise, T[] initialState, T[,] initialCovariance)
    {
        var n = initialState.Length;
        if (transition.GetLength(0) != n || transition.GetLength(1) != n || processNoise.GetLength(0) != n || processNoise.GetLength(1) != n ||
            observation.Length != n || initialCovariance.GetLength(0) != n || initialCovariance.GetLength(1) != n)
            throw new ArgumentException("Dimensioni incoerenti.");
        if (observationNoise < T.Zero) throw new ArgumentOutOfRangeException(nameof(observationNoise));
        _f = transition;
        _q = processNoise;
        _h = observation;
        _r = observationNoise;
        State = (T[])initialState.Clone();
        Covariance = (T[,])initialCovariance.Clone();
        Gain = new T[n];
    }

    /// <summary>Predict: x ← F x, P ← F P Fᵀ + Q. Returns the forecast of y and its variance S = H P Hᵀ + R.</summary>
    public (T Prediction, T Variance) Predict()
    {
        State = MatrixAlgebra.Multiply<T>(_f, State);
        var p = MatrixAlgebra.Multiply(MatrixAlgebra.Multiply(_f, Covariance), MatrixAlgebra.Transpose(_f));
        for (var i = 0; i < p.GetLength(0); i++)
            for (var j = 0; j < p.GetLength(1); j++) p[i, j] += _q[i, j];
        Covariance = p;
        return (Dot(_h, State), Dot(_h, MatrixAlgebra.Multiply<T>(Covariance, _h)) + _r);
    }

    /// <summary>
    /// Update with the observation y: innovation v = y − Hx, K = P Hᵀ/S, x ← x + K v. Covariance in Joseph form
    /// P ← (I − KH) P (I − KH)ᵀ + K R Kᵀ: algebraically equal to (I − KH)P but it stays symmetric and positive semidefinite even with rounding.
    /// A NaN is a missing datum: the correction is skipped and the state remains the forecast alone (returns NaN).
    /// </summary>
    public T Update(T y)
    {
        var n = State.Length;
        if (T.IsNaN(y))
        {
            Gain = new T[n];
            return T.NaN;
        }
        var ph = MatrixAlgebra.Multiply<T>(Covariance, _h);
        var s = Dot(_h, ph) + _r;
        var innovation = y - Dot(_h, State);
        Gain = new T[n];
        for (var i = 0; i < n; i++) Gain[i] = ph[i] / s;
        for (var i = 0; i < n; i++) State[i] += Gain[i] * innovation;
        var a = MatrixAlgebra.Identity<T>(n);
        for (var i = 0; i < n; i++)
            for (var j = 0; j < n; j++) a[i, j] -= Gain[i] * _h[j];
        var joseph = MatrixAlgebra.Multiply(MatrixAlgebra.Multiply(a, Covariance), MatrixAlgebra.Transpose(a));
        for (var i = 0; i < n; i++)
            for (var j = 0; j < n; j++) joseph[i, j] += Gain[i] * _r * Gain[j];
        Covariance = joseph;
        return innovation;
    }

    /// <summary>
    /// Filters an entire series: for each t first Predict then Update. The first step predicts from the initial state.
    /// LogLikelihood = −½ Σ (ln 2πS_t + v_t²/S_t) by prediction-error decomposition: this is what is maximised to estimate q and r.
    /// </summary>
    public (T[] Predictions, T[] PredictionVariances, T[] FilteredFirstState, T[] FirstGain, T LogLikelihood) Filter(ReadOnlySpan<T> y)
    {
        var predictions = new T[y.Length];
        var variances = new T[y.Length];
        var filtered = new T[y.Length];
        var gains = new T[y.Length];
        var logLikelihood = T.Zero;
        var half = C<T>(0.5);
        for (var t = 0; t < y.Length; t++)
        {
            (predictions[t], variances[t]) = Predict();
            var innovation = Update(y[t]);
            if (!T.IsNaN(innovation))
                logLikelihood -= half * (T.Log(T.Tau * variances[t]) + innovation * innovation / variances[t]);
            filtered[t] = State[0];
            gains[t] = Gain[0];
        }
        return (predictions, variances, filtered, gains, logLikelihood);
    }

    // Overload with arrays for callers without ReadOnlySpan support (reflection, PySharp: a ref struct cannot be passed through reflection).
    public (T[] Predictions, T[] PredictionVariances, T[] FilteredFirstState, T[] FirstGain, T LogLikelihood) Filter(T[] y) => Filter(y.AsSpan());

    private static T Dot(ReadOnlySpan<T> a, ReadOnlySpan<T> b)
    {
        var sum = T.Zero;
        for (var i = 0; i < a.Length; i++) sum += a[i] * b[i];
        return sum;
    }
}

/// <summary>Ready-made state models and steady-state gain for <see cref="KalmanFilter{T}"/>.</summary>
public static class KalmanFilter
{
    /// <summary>
    /// Local level (random walk + noise): ℓ_t = ℓ_{t−1} + η_t, y_t = ℓ_t + ε_t, with variances q and r.
    /// The state model whose optimal forecast is simple exponential smoothing.
    /// </summary>
    public static KalmanFilter<T> LocalLevel<T>(T q, T r, T initialLevel, T initialVariance) where T : IFloatingPointIeee754<T> =>
        new(new T[,] { { T.One } }, new T[,] { { q } }, [T.One], r, [initialLevel], new T[,] { { initialVariance } });

    /// <summary>
    /// Local linear trend: level and slope, ℓ_t = ℓ_{t−1} + b_{t−1} + η, b_t = b_{t−1} + ζ. State counterpart of Holt's method.
    /// </summary>
    public static KalmanFilter<T> LocalLinearTrend<T>(T levelNoise, T slopeNoise, T r, T initialLevel, T initialSlope, T initialVariance) where T : IFloatingPointIeee754<T> =>
        new(new T[,] { { T.One, T.One }, { T.Zero, T.One } }, new T[,] { { levelNoise, T.Zero }, { T.Zero, slopeNoise } }, [T.One, T.Zero], r,
            [initialLevel, initialSlope], new T[,] { { initialVariance, T.Zero }, { T.Zero, initialVariance } });

    /// <summary>
    /// Steady-state gain of the local level. The prior covariance P satisfies the Riccati equation P = P − P²/(P+r) + q, that is P² − qP − qr = 0:
    /// P = (q + √(q² + 4qr))/2 and K = P/(P + r). It depends only on the signal-to-noise ratio q/r.
    /// </summary>
    public static T LocalLevelSteadyStateGain<T>(T q, T r) where T : IFloatingPointIeee754<T>
    {
        if (q <= T.Zero || r < T.Zero) throw new ArgumentOutOfRangeException(nameof(q));
        var p = (q + T.Sqrt(q * q + C<T>(4) * q * r)) / C<T>(2);
        return p / (p + r);
    }
}
