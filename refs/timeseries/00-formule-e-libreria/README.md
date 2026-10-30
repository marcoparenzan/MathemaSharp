# Part I — The formulas and their realisation

> For every algorithm of the library: **the formula in theory**, **how it is written in the code**, **which test verifies it** and **what can go wrong**. The formulas use KaTeX notation (`$…$`), which VS Code and GitHub render.

Part I follows the chronology of the first infographic ([time-series-forecasting-0-01](../time-series-forecasting-0-01/)): *a chronology of ideas, not a ranking of models*. Each chapter is a stage, but they are written to be read separately too.

| Ch. | Stage | Contents | Code |
|---|---|---|---|
| [1](01-generic-math.md) | — | Why the library is generic; formats and precision | `Num`, all the classes |
| [2](02-statistica-di-base.md) | 1927–1938 | Autocovariance, ACF, PACF, Levinson-Durbin, Ljung-Box, special functions | `SeriesStatistics`, `SpecialFunctions` |
| [3](03-riferimenti-e-metriche.md) | — | Mean, naive, seasonal naive, drift; MAE, RMSE, MAPE, MASE | `ReferenceForecasts`, `Accuracy` |
| [4](04-ar-arima.md) | 1927–1970 | Yule's AR, Wold, Box-Jenkins ARIMA, Hannan-Rissanen | `ArModel`, `ArimaModel` |
| [5](05-smoothing.md) | 1950s–1985 | SES, Holt, damped trend, Holt-Winters | `ExponentialSmoothing` |
| [6](06-kalman.md) | 1960 | Kalman filter, Riccati, equivalence with SES | `KalmanFilter` |
| [7](07-var.md) | 1980 | VAR: several series, cross lags | `VarModel` |
| [8](08-garch.md) | 1982/1986 | ARCH/GARCH: the conditional variance | `Garch11` |
| [9](09-stl-loess.md) | 1990 | LOESS and STL | `Stl` |
| [10](10-algebra-e-simulazione.md) | — | Least squares (QR), simulation, facade for PySharp | `LinearAlgebra`, `Simulation`, `Interop` |

## Notation

| Symbol | Meaning |
|---|---|
| $y_t$, $t=1..n$ | observed series |
| $\bar y$ | sample mean |
| $\gamma(k)$, $\rho(k)$ | autocovariance and autocorrelation at lag $k$ |
| $\varepsilon_t$, $e_t$ | innovations (white noise) |
| $\hat y_{T+h}$ | forecast at horizon $h$ from origin $T$ |
| $\varepsilon_{\text{mach}}$ | machine ε of the format: distance between 1 and the next number |
| $T$ (in the code) | the numeric format: `float`, `double`, `Half`, `Decimal32/64/128` |

## The identities that hold Part I together

The model families are not islands: the library verifies them against each other with the tests.

| Identity | Chapter | Test |
|---|---|---|
| SES $\equiv$ ARIMA(0,1,1) with $\theta=\alpha-1$ | 4, 5 | `SesEqualsArima011InEveryFormat` |
| SES $\equiv$ stationary Kalman of the local level, with $\alpha=K^*$ | 5, 6 | `KalmanSteadyStateGainInEveryFormat`, `SteadyStateKalmanIsSes` |
| Levinson-Durbin $\equiv$ direct solution of Yule-Walker | 2, 4 | `LevinsonDurbinMatchesDirectYuleWalkerSolve` |
| Holt-Winters exact on a line + seasonality | 5 | `HoltWintersExactInEveryFormat` |
| STL exact on a line + periodic seasonality | 9 | `StlExactInEveryFormat` |
| GARCH forecast → unconditional variance with ratio $\alpha+\beta$ | 8 | `VarianceForecastConvergesToUnconditional` |

They all hold with every format, with tolerances proportional to $\varepsilon_{\text{mach}}$.

→ [Part II — The library](../01-libreria/README.md)
