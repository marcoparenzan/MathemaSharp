# 2 · Use of the library and of the single functions

Conventions valid everywhere, except where indicated:

- `T` is `T : IFloatingPointIeee754<T>`; array arguments can be passed as `T[]` (C# converts them to `ReadOnlySpan<T>` and **infers `T`**: `SeriesStatistics.Acf(y, 20)` is enough).
- Empty series, negative horizons and inconsistent dimensions throw `ArgumentException` / `ArgumentOutOfRangeException`.
- Each function is explained, with the formula, in the indicated chapter of [Part I](../00-formule-e-libreria/README.md).

## `SeriesStatistics` — ch. 2

| Function | Returns | Notes |
|---|---|---|
| `Mean(y)` | `T` | |
| `Autocovariance(y, maxLag)` | `T[maxLag+1]` | divisor $n$; `maxLag < n` |
| `Acf(y, maxLag)` | `T[maxLag+1]` | $\rho(0)=1$; constant series → exception |
| `Pacf(y, maxLag)` | `T[maxLag]` | lag 1..maxLag |
| `LevinsonDurbin(autocov, order)` | `(T[] Phi, T[] Pacf, T InnovationVariance)` | $O(p^2)$ |
| `Difference(y, lag = 1)` | `T[n−lag]` | $y_t-y_{t-\text{lag}}$ (it is not `np.diff(n=)`) |
| `Quantile(values, p)` | `T` | quantile with linear interpolation ($h=(n-1)p$): **coincides with `np.quantile`** (verified, difference 0) |
| `LjungBox(residuals, lags, fittedParameters = 0)` | `(T Statistic, int DegreesOfFreedom, T PValue)` | degrees of freedom $\ge1$ |

## `SpecialFunctions` — ch. 2

`LogGamma(x)` ($x>0$), `RegularizedGammaQ(a, x)`, `ChiSquareSurvival(x, dof)`. Accuracy ~$10^{-15}$ in every format.

## `ReferenceForecasts` and `Accuracy` — ch. 3

| Function | Returns |
|---|---|
| `Mean / Naive / Drift (y, horizon)` | `T[horizon]` |
| `SeasonalNaive(y, period, horizon)` | `T[horizon]`; at least one period needed |
| `Accuracy.Mae / Rmse / Mape (actual, forecast)` | `T`; `Mape` rejects zero observations |
| `Accuracy.Mase(actual, forecast, training, period = 1)` | `T`; constant training → exception |
| `Accuracy.PinballLoss(actual, quantileForecast, p)` | `T`: quantile loss, $\frac1n\sum\max(p\,e,(p-1)e)$; with $p=0.5$ it is MAE/2; $p\in(0,1)$ |
| `Accuracy.IntervalCoverage(actual, lower, upper)` | `T`: fraction of observations inside the interval (for P10–P90 ≈ 0.8) |

## `ArModel`, `ArimaModel` — ch. 4

| Function | Returns |
|---|---|
| `ArModel.FitYuleWalker(y, order)` | `ArModel<T>(Mean, Phi[], InnovationVariance)` |
| `ArModel<T>.Forecast(history, horizon)` | `T[]`; converges to the mean |
| `ArimaModel.FitHannanRissanen(y, p, d, q, longArOrder = null)` | `ArimaModel<T>(P, D, Q, Constant, Phi[], Theta[], InnovationVariance)` |
| `ArimaModel<T>.Residuals(y)` | `T[n−d]` (CSS) |
| `ArimaModel<T>.Forecast(y, horizon)` | `T[]`, already integrated $d$ times |

`new ArimaModel<double>(0, 1, 1, 0, [], [θ], 1).Forecast(y, h)` builds a model with known parameters.

## `ExponentialSmoothing` — ch. 5

All return `SmoothingResult<T>(Fitted[], Forecast[], Level, Trend, Season[], Sse)`.

| Function | Parameters |
|---|---|
| `Ses(y, alpha, horizon)` | $\alpha\in[0,1]$ |
| `Holt(y, alpha, beta, horizon)` / `Holt(…, phi)` | with `phi` the trend is damped ($\phi=1$ = linear) |
| `HoltWinters(y, period, alpha, beta, gamma, horizon)` / `(…, phi)` | at least 2 periods |
| `FitSesAlpha(y)` | `T`: optimal $\alpha$ by golden section |
| `GoldenSection(f, a, b)` / `(f, a, b, tolerance)` | minimum of a unimodal function |

Parameters outside $[0,1]$ → `ArgumentOutOfRangeException`.

## `KalmanFilter` — ch. 6

| Function | Returns |
|---|---|
| `KalmanFilter.LocalLevel(q, r, initialLevel, initialVariance)` | `KalmanFilter<T>` |
| `KalmanFilter.LocalLinearTrend(levelNoise, slopeNoise, r, initialLevel, initialSlope, initialVariance)` | `KalmanFilter<T>` |
| `KalmanFilter.LocalLevelSteadyStateGain(q, r)` | `T`: $K^*$ |
| `new KalmanFilter<T>(F, Q, H, R, x0, P0)` | general model with scalar observation |
| `filter.Predict()` / `filter.Update(y)` | `(Prediction, Variance)` / innovation (`NaN` if $y$ is `NaN`) |
| `filter.Filter(y)` | `(Predictions[], PredictionVariances[], FilteredFirstState[], FirstGain[], LogLikelihood)` |

The filter **keeps the state**: to filter another series a new one is created.

## `VarModel` — ch. 7

`VarModel.Fit(y, order)` with `y` of shape `T[time, series]` → `VarModel<T>(Intercept[], Coefficients[][,], ResidualCovariance[,])`. `model.Forecast(history, horizon)` → `T[horizon, series]`; `IsStableOrder1Bivariate()` only for VAR(1) $2\times2$.

## `Garch11` — ch. 8

| Function | Returns |
|---|---|
| `new Garch11<T>(omega, alpha, beta)` | model; `Persistence`, `UnconditionalVariance` |
| `Garch11.FitVarianceTargeting(shocks, gridSize = 40, refinements = 3)` | `Garch11<T>` |
| `g.ConditionalVariances(shocks)` | `T[n]` |
| `g.LogLikelihood(shocks)` | `T` |
| `g.ForecastVariance(shocks, horizon)` | `T[horizon]` |

## `Stl` — ch. 9

`Stl.Decompose(y, period, seasonalSpan = 7, trendSpan = null, innerIterations = 2, robustIterations = 0)` → `StlDecomposition<T>(Trend[], Seasonal[], Remainder[], RobustnessWeights[])` with $y=T+S+R$ exact. `Stl.Loess(y, robustWeights, span, x)` evaluates the LOESS at a point (even outside the series).

## `LinearAlgebra` and `Simulation` — ch. 10

| Function | Notes |
|---|---|
| `MatrixAlgebra.LeastSquares(X, y)` | Householder QR; dependent columns → exception |
| `Multiply`, `Transpose`, `Identity` | |
| `Simulation.Arma(random, n, constant, phi, theta, sigma, burnIn = 200)` | |
| `Simulation.RandomWalk(random, n)` / `(random, n, start, drift, sigma)` | |
| `Simulation.Garch(random, model, n)`, `Simulation.Var(random, model, n, sigma)` | |
| `Simulation.StandardNormal(random)` | `double`; `StandardNormal<T>` converts it |

## `Interop` — the same API on `double[]`

| Class | Functions |
|---|---|
| `Statistics` | `Mean`, `Autocovariance`, `Acf`, `Pacf`, `Difference`, `Quantile`, `LevinsonDurbin`, `LjungBox`, `LogGamma`, `ChiSquareSurvival` |
| `Forecasts` | `Mean`, `Naive`, `SeasonalNaive`, `Drift` |
| `Metrics` | `Mae`, `Rmse`, `Mape`, `Mase`, `PinballLoss`, `IntervalCoverage` |
| `Smoothing` | `Ses`, `Holt`, `HoltWinters`, `FitSesAlpha` |
| `Arima` | `FitYuleWalker`, `FitHannanRissanen`, `Model` |
| `Kalman` | `LocalLevel`, `LocalLinearTrend`, `SteadyStateGain`, `Filter` |
| `Var` | `Fit(flat, rows, cols, order)`, `Forecast`, `Coefficient`, `ResidualCovariance`, `IsStableOrder1Bivariate` |
| `Garch` | `Model`, `FitVarianceTargeting` |
| `Decomposition` | `Stl`, `Loess` |
| `Linear` | `LeastSquares(flat, rows, cols, y)` |
| `Simulate` | `Normal`, `Arma`, `RandomWalk`, `Garch` (with an integer **seed**) |

The **matrices** are vectors in **row order**: an $r\times c$ matrix is `[m00, m01, …, m0c−1, m10, …]`.

## `MathemaSharp.NDSharp` — the bridge

| Direction | Function | Copy? |
|---|---|---|
| `NDArray` → `double[]` / `double[,]` | `ToVector()`, `ToMatrix()` | yes (C order, dtype cast) |
| `NDArray` → `T[]` / `T[,]` | `ToVector<T>()`, `ToMatrix<T>()` | yes, going through `double` |
| `NDArray` → `Complex<double>[]` | `ToComplexVector()` | yes |
| `double[]` → `NDArray` | `ToNDArray()` | **no**: shares the buffer |
| `T[]`, `double[,]`, `Complex<double>[]` → `NDArray` | `ToNDArray()` | yes |

NDSharp does not know the decimal formats: a `Decimal128[]` goes through `double` and **loses digits**.

← [Part II](README.md) · [3 · Overlap with NumPy →](03-overlap-numpy.md)
