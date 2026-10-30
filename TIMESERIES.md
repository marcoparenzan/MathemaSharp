# MathemaSharp.TimeSeries — statistical foundations of forecasting

> **This page is a summary.** The complete treatment — formulas, realisation, use function by function, overlap with NumPy in PySharp, examples in C# and PySharp, industrial scenarios — is in the **[book](refs/timeseries/README.md)**.

Library (NuGet package) in `src/libs/MathemaSharp.TimeSeries`. It depends only on `MathemaSharp.Core`, target net11.0, **generic over `T : IFloatingPointIeee754<T>`** (float, double, Half, Decimal32/64/128). It follows the chronology of refs/timeseries/time-series-forecasting-0-01/time-series-forecasting-0-01.jpg: *a chronology of ideas, not a ranking of models*. Each stage has an implementation, tests that verify its mathematical properties and an executed notebook.

## Analysis of infographics 03–05 and new building blocks

Visual analysis of the three original images in `refs/timeseries/`:

- **03 — Transformers and strong baselines:** Informer selects queries and distils the sequence; Autoformer separates components and aggregates lags; N-HiTS combines resolutions. DLinear applies two linear maps to trend and remainder (it is not STL followed by ARIMA). PatchTST uses a window of one channel as a token, iTransformer the history of one variable. So it is the axis of attention that changes, not just the window length.
- **04 — Language and pre-training:** Time-LLM adapts inputs to a frozen LLM; LLMTime serialises numbers into text. TimeGPT, TimesFM, Moirai and Chronos learn from time series. The image distinguishes continuous patches, masked encoder and quantisation into tokens: they are not interchangeable pipelines. Zero-shot still requires local validation.
- **05 — Efficiency and multivariate:** sparse routing (Time-MoE), direct quantiles (Chronos-Bolt), shared context (Chronos-2), simplified decoder (Moirai 2.0) and temporal/cross-variable attention (TimesFM-3). The dates and labels are those of the figure, **not verified here against primary sources**; the diagrams do not prove performance superiority.

### Verifiable implementation

`HistoryPatches.Extract<T>(history, length, stride)` extracts copies of complete windows, without padding. It is the representation building block illustrated by PatchTST, it does **not** implement embedding, attention or training.

`RollingOrigin.Evaluate<T>` redoes fit/forecast at successive origins with expanding training. Each callback receives only a copy of the past. The gap requires forecasting the intermediate steps too, which are excluded from the score. It returns MAE/RMSE per origin; with overlapping windows the scores are dependent. The callback must also estimate normalisation and hyperparameters exclusively on the training data (any selection in an inner validation).

```csharp
var patches = HistoryPatches.Extract<double>(series, length: 24, stride: 12);
var folds = RollingOrigin.Evaluate<double>(series, initialTraining: 168,
    horizon: 24, fitForecast: (past, h) =>
        ReferenceForecasts.SeasonalNaive<double>(past, 24, h), step: 24);
foreach (var fold in folds)
    Console.WriteLine($"Origin {fold.Origin}: MAE={fold.Mae}, RMSE={fold.Rmse}");
```

Compare baseline and model on the **same origins**, keeping a final test untouched. This backtest is not a significance test and gives no coverage guarantees. Automated tests: complete and independent patches, isolation of the past, gap alignment, analytic errors of drift/naive and invalid inputs. Transformer, trained DLinear and foundation models remain not implemented; no accuracy benchmark was produced by this analysis.

## From the chronology to the API

| Years | Idea | API | Notebook |
|---|---|---|---|
| — | Mean, naive, seasonal naive, drift; MAE/RMSE/MAPE/MASE | `ReferenceForecasts`, `Accuracy` | 06 |
| 1927–1938 | Yule's AR, Wold decomposition | `SeriesStatistics` (ACF, PACF, Levinson-Durbin, Ljung-Box), `ArModel.FitYuleWalker` | 06 |
| 1950s–1960 | SES, Holt (1957), Holt-Winters (1960) | `ExponentialSmoothing.Ses/Holt/HoltWinters`, `FitSesAlpha` | 07 |
| 1960 | Kalman filter | `KalmanFilter` (`LocalLevel`, `LocalLinearTrend`, `LocalLevelSteadyStateGain`) | 07 |
| 1985 | Damped trend | parameter `phi` of `Holt`/`HoltWinters` | 07 |
| 1970 | Box-Jenkins / ARIMA | `ArimaModel.FitHannanRissanen`, `Residuals`, `Forecast` | 08 |
| 1980 | VAR | `VarModel.Fit/Forecast` | 08 |
| 1982/1986 | ARCH/GARCH | `Garch11` (conditional variances, log-likelihood, forecast, `FitVarianceTargeting`) | 08 |
| 1990 | STL | `Stl.Decompose`, `Stl.Loess` | 09 |

In support: `MatrixAlgebra.LeastSquares` (Householder QR), `SpecialFunctions` (Lanczos log Γ, incomplete gamma, χ² survival), `Simulation` (ARMA, random walk, GARCH, VAR with explicit seed).

## Identities verified by the tests

They are the didactic core: the model families are not islands.

- **SES ≡ ARIMA(0,1,1) with θ = α − 1.** From ℓ_t = y_t − (1−α)e_t follows ŷ_{t+1} = y_t + (α−1)e_t. Identical forecasts to 1e-9.
- **SES ≡ stationary Kalman filter of the local level.** The Riccati P = P − P²/(P+r) + q gives P* = (q + √(q²+4qr))/2 and K* = P*/(P*+r). After the transient the forecasts coincide to 1e-9; α depends only on q/r.
- **Levinson-Durbin ≡ direct solution of Yule-Walker** Γφ = γ. On the theoretical ACF of an AR(1), the PACF equals φ at lag 1 and is exactly 0 afterwards.
- **Damped trend**: the forecasts converge to ℓ_T + φ b_T/(1−φ); with φ = 1 it is linear Holt, which reproduces straight lines with zero SSE.
- **Holt-Winters exact on a line + zero-mean seasonality**, thanks to the centred initialisation: the mean of the first cycle is the level at time (m−1)/2, not at m−1.
- **STL exact on a line + zero-mean periodic seasonality**: the linear LOESS reproduces lines (even when extrapolating) and the one-period moving average cancels the seasonality. With robust iterations an outlier ends up in the remainder with weight ≈ 0.
- **GARCH**: the forecast of σ² decays towards ω/(1−α−β) with ratio exactly α + β.
- **VAR(1)**: the forecasts converge to (I − A)⁻¹c.
- **Estimates on simulated data** with known parameters: Yule-Walker for AR(2), Hannan-Rissanen for ARMA(1,1) (white residuals according to Ljung-Box), OLS for VAR(1), variance targeting for GARCH(1,1).

## Contracts and choices

- Series as `ReadOnlySpan<double>`; multiple series (VAR) as `double[time, series]`. For NDSharp data: `MathemaSharp.NDSharp` (`ToVector`, `ToMatrix`).
- Autocovariance with divisor n: positive semi-definite Toeplitz matrix, hence |PACF| ≤ 1 and the Yule-Walker AR always stationary.
- `ArimaModel`: conditional residuals (CSS), with e_t = 0 before the first estimable value. Hannan-Rissanen is linear, without optimisation and consistent, but less efficient than maximum likelihood. Stationarity and invertibility are **not** imposed.
- Smoothing: parameters in [0,1] (`ArgumentOutOfRangeException` otherwise), deterministic initialisations that are not estimated. `FitSesAlpha` uses the golden section and presupposes a unimodal SSE.
- `KalmanFilter`: scalar observation (S is a number, no inversion); covariance in Joseph form; `NaN` = missing datum (the update is skipped); log-likelihood by prediction-error decomposition.
- `Garch11`: zero-mean shocks; σ²_0 = mean of ε²; the estimate by refined grid with variance targeting is robust but not efficient.
- `Stl`: faithful to the algorithm of Cleveland et al., not to the Fortran (no *jump*, only degree-1 LOESS); n_s odd, at least two periods.
- Regressions with Householder QR: dependent columns → `ArgumentException`.

## Not included

Exact maximum likelihood for ARIMA, Kalman smoother, multiplicative ETS and prediction intervals, automatic order selection, structural VARs, asymmetric GARCH. See [ROADMAP.md](ROADMAP.md).

## Running

```sh
dotnet test src/tests/MathemaSharp.TimeSeries.Tests -c Release
dotnet run --project src/apps/MathemaSharp.Lab -- --topic timeseries
pwsh scripts/run-notebooks.ps1 -Filter '0[6-9]*'
```

References: Hyndman & Athanasopoulos, *Forecasting: Principles and Practice* (2nd ed.), plus the original publications (Yule 1927, Wold 1938, Holt 1957, Winters 1960, Kalman 1960, Box & Jenkins 1970, Sims 1980, Engle 1982, Hannan & Rissanen 1982, Gardner & McKenzie 1985, Bollerslev 1986, Cleveland et al. 1990).
