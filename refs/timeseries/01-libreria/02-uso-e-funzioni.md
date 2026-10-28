# 2 · Uso della libreria e delle singole funzioni

Convenzioni valide ovunque, salvo dove indicato:

- `T` è `T : IFloatingPointIeee754<T>`; gli argomenti array si possono passare come `T[]` (C# li converte in `ReadOnlySpan<T>` e **deduce `T`**: `SeriesStatistics.Acf(y, 20)` basta).
- Serie vuote, orizzonti negativi e dimensioni incoerenti lanciano `ArgumentException` / `ArgumentOutOfRangeException`.
- Ogni funzione è spiegata, con la formula, nel capitolo indicato della [Parte I](../00-formule-e-libreria/README.md).

## `SeriesStatistics` — cap. 2

| Funzione | Restituisce | Note |
|---|---|---|
| `Mean(y)` | `T` | |
| `Autocovariance(y, maxLag)` | `T[maxLag+1]` | divisore $n$; `maxLag < n` |
| `Acf(y, maxLag)` | `T[maxLag+1]` | $\rho(0)=1$; serie costante → eccezione |
| `Pacf(y, maxLag)` | `T[maxLag]` | lag 1..maxLag |
| `LevinsonDurbin(autocov, order)` | `(T[] Phi, T[] Pacf, T InnovationVariance)` | $O(p^2)$ |
| `Difference(y, lag = 1)` | `T[n−lag]` | $y_t-y_{t-\text{lag}}$ (non è `np.diff(n=)`) |
| `Quantile(values, p)` | `T` | quantile con interpolazione lineare ($h=(n-1)p$): **coincide con `np.quantile`** (verificato, differenza 0) |
| `LjungBox(residuals, lags, fittedParameters = 0)` | `(T Statistic, int DegreesOfFreedom, T PValue)` | gradi di libertà $\ge1$ |

## `SpecialFunctions` — cap. 2

`LogGamma(x)` ($x>0$), `RegularizedGammaQ(a, x)`, `ChiSquareSurvival(x, dof)`. Accuratezza ~$10^{-15}$ in ogni formato.

## `ReferenceForecasts` e `Accuracy` — cap. 3

| Funzione | Restituisce |
|---|---|
| `Mean / Naive / Drift (y, horizon)` | `T[horizon]` |
| `SeasonalNaive(y, period, horizon)` | `T[horizon]`; serve almeno un periodo |
| `Accuracy.Mae / Rmse / Mape (actual, forecast)` | `T`; `Mape` rifiuta osservazioni nulle |
| `Accuracy.Mase(actual, forecast, training, period = 1)` | `T`; training costante → eccezione |
| `Accuracy.PinballLoss(actual, quantileForecast, p)` | `T`: perdita dei quantili, $rac1n\sum\max(p\,e,(p-1)e)$; con $p=0.5$ vale MAE/2; $p\in(0,1)$ |
| `Accuracy.IntervalCoverage(actual, lower, upper)` | `T`: frazione di osservazioni dentro l'intervallo (per P10–P90 ≈ 0.8) |

## `ArModel`, `ArimaModel` — cap. 4

| Funzione | Restituisce |
|---|---|
| `ArModel.FitYuleWalker(y, order)` | `ArModel<T>(Mean, Phi[], InnovationVariance)` |
| `ArModel<T>.Forecast(history, horizon)` | `T[]`; converge alla media |
| `ArimaModel.FitHannanRissanen(y, p, d, q, longArOrder = null)` | `ArimaModel<T>(P, D, Q, Constant, Phi[], Theta[], InnovationVariance)` |
| `ArimaModel<T>.Residuals(y)` | `T[n−d]` (CSS) |
| `ArimaModel<T>.Forecast(y, horizon)` | `T[]`, già integrata $d$ volte |

`new ArimaModel<double>(0, 1, 1, 0, [], [θ], 1).Forecast(y, h)` costruisce un modello a parametri noti.

## `ExponentialSmoothing` — cap. 5

Tutte restituiscono `SmoothingResult<T>(Fitted[], Forecast[], Level, Trend, Season[], Sse)`.

| Funzione | Parametri |
|---|---|
| `Ses(y, alpha, horizon)` | $\alpha\in[0,1]$ |
| `Holt(y, alpha, beta, horizon)` / `Holt(…, phi)` | con `phi` il trend è smorzato ($\phi=1$ = lineare) |
| `HoltWinters(y, period, alpha, beta, gamma, horizon)` / `(…, phi)` | almeno 2 periodi |
| `FitSesAlpha(y)` | `T`: $\alpha$ ottimo per sezione aurea |
| `GoldenSection(f, a, b)` / `(f, a, b, tolerance)` | minimo di una funzione unimodale |

Parametri fuori da $[0,1]$ → `ArgumentOutOfRangeException`.

## `KalmanFilter` — cap. 6

| Funzione | Restituisce |
|---|---|
| `KalmanFilter.LocalLevel(q, r, initialLevel, initialVariance)` | `KalmanFilter<T>` |
| `KalmanFilter.LocalLinearTrend(levelNoise, slopeNoise, r, initialLevel, initialSlope, initialVariance)` | `KalmanFilter<T>` |
| `KalmanFilter.LocalLevelSteadyStateGain(q, r)` | `T`: $K^*$ |
| `new KalmanFilter<T>(F, Q, H, R, x0, P0)` | modello generale a osservazione scalare |
| `filter.Predict()` / `filter.Update(y)` | `(Prediction, Variance)` / innovazione (`NaN` se $y$ è `NaN`) |
| `filter.Filter(y)` | `(Predictions[], PredictionVariances[], FilteredFirstState[], FirstGain[], LogLikelihood)` |

Il filtro **conserva lo stato**: per filtrare un'altra serie se ne crea uno nuovo.

## `VarModel` — cap. 7

`VarModel.Fit(y, order)` con `y` di forma `T[tempo, serie]` → `VarModel<T>(Intercept[], Coefficients[][,], ResidualCovariance[,])`. `model.Forecast(history, horizon)` → `T[horizon, serie]`; `IsStableOrder1Bivariate()` solo per VAR(1) $2\times2$.

## `Garch11` — cap. 8

| Funzione | Restituisce |
|---|---|
| `new Garch11<T>(omega, alpha, beta)` | modello; `Persistence`, `UnconditionalVariance` |
| `Garch11.FitVarianceTargeting(shocks, gridSize = 40, refinements = 3)` | `Garch11<T>` |
| `g.ConditionalVariances(shocks)` | `T[n]` |
| `g.LogLikelihood(shocks)` | `T` |
| `g.ForecastVariance(shocks, horizon)` | `T[horizon]` |

## `Stl` — cap. 9

`Stl.Decompose(y, period, seasonalSpan = 7, trendSpan = null, innerIterations = 2, robustIterations = 0)` → `StlDecomposition<T>(Trend[], Seasonal[], Remainder[], RobustnessWeights[])` con $y=T+S+R$ esatto. `Stl.Loess(y, robustWeights, span, x)` valuta il LOESS in un punto (anche fuori dalla serie).

## `LinearAlgebra` e `Simulation` — cap. 10

| Funzione | Note |
|---|---|
| `MatrixAlgebra.LeastSquares(X, y)` | QR di Householder; colonne dipendenti → eccezione |
| `Multiply`, `Transpose`, `Identity` | |
| `Simulation.Arma(random, n, constant, phi, theta, sigma, burnIn = 200)` | |
| `Simulation.RandomWalk(random, n)` / `(random, n, start, drift, sigma)` | |
| `Simulation.Garch(random, model, n)`, `Simulation.Var(random, model, n, sigma)` | |
| `Simulation.StandardNormal(random)` | `double`; `StandardNormal<T>` lo converte |

## `Interop` — la stessa API su `double[]`

| Classe | Funzioni |
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
| `Simulate` | `Normal`, `Arma`, `RandomWalk`, `Garch` (con **seme** intero) |

Le **matrici** sono vettori in **ordine di riga**: una matrice $r\times c$ è `[m00, m01, …, m0c−1, m10, …]`.

## `MathemaSharp.NDSharp` — il ponte

| Direzione | Funzione | Copia? |
|---|---|---|
| `NDArray` → `double[]` / `double[,]` | `ToVector()`, `ToMatrix()` | sì (ordine C, cast del dtype) |
| `NDArray` → `T[]` / `T[,]` | `ToVector<T>()`, `ToMatrix<T>()` | sì, passando da `double` |
| `NDArray` → `Complex<double>[]` | `ToComplexVector()` | sì |
| `double[]` → `NDArray` | `ToNDArray()` | **no**: condivide il buffer |
| `T[]`, `double[,]`, `Complex<double>[]` → `NDArray` | `ToNDArray()` | sì |

NDSharp non conosce i formati decimali: un `Decimal128[]` passa da `double` e **perde cifre**.

← [Parte II](README.md) · [3 · Overlap con NumPy →](03-overlap-numpy.md)
