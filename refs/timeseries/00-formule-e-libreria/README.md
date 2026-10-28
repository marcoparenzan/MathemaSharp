# Parte I — Le formule e la loro realizzazione

> Per ogni algoritmo della libreria: **la formula in teoria**, **come è scritta nel codice**, **quale test la verifica** e **cosa può andare storto**. Le formule usano la notazione di KaTeX (`$…$`), che VS Code e GitHub visualizzano.

La Parte I segue la cronologia della prima infografica ([time-series-forecasting-0-01](../time-series-forecasting-0-01/)): *una cronologia di idee, non una classifica di modelli*. Ogni capitolo è una tappa, ma sono scritti per essere letti anche separatamente.

| Cap. | Tappa | Contenuto | Codice |
|---|---|---|---|
| [1](01-generic-math.md) | — | Perché la libreria è generica; formati e precisione | `Num`, tutte le classi |
| [2](02-statistica-di-base.md) | 1927–1938 | Autocovarianza, ACF, PACF, Levinson-Durbin, Ljung-Box, funzioni speciali | `SeriesStatistics`, `SpecialFunctions` |
| [3](03-riferimenti-e-metriche.md) | — | Mean, naive, seasonal naive, drift; MAE, RMSE, MAPE, MASE | `ReferenceForecasts`, `Accuracy` |
| [4](04-ar-arima.md) | 1927–1970 | AR di Yule, Wold, ARIMA di Box-Jenkins, Hannan-Rissanen | `ArModel`, `ArimaModel` |
| [5](05-smoothing.md) | 1950s–1985 | SES, Holt, damped trend, Holt-Winters | `ExponentialSmoothing` |
| [6](06-kalman.md) | 1960 | Filtro di Kalman, Riccati, equivalenza con SES | `KalmanFilter` |
| [7](07-var.md) | 1980 | VAR: più serie, ritardi incrociati | `VarModel` |
| [8](08-garch.md) | 1982/1986 | ARCH/GARCH: la varianza condizionale | `Garch11` |
| [9](09-stl-loess.md) | 1990 | LOESS e STL | `Stl` |
| [10](10-algebra-e-simulazione.md) | — | Minimi quadrati (QR), simulazione, facciata per PySharp | `LinearAlgebra`, `Simulation`, `Interop` |

## Notazione

| Simbolo | Significato |
|---|---|
| $y_t$, $t=1..n$ | serie osservata |
| $\bar y$ | media campionaria |
| $\gamma(k)$, $\rho(k)$ | autocovarianza e autocorrelazione al lag $k$ |
| $\varepsilon_t$, $e_t$ | innovazioni (rumore bianco) |
| $\hat y_{T+h}$ | previsione a orizzonte $h$ dall'origine $T$ |
| $\varepsilon_{\text{mach}}$ | ε di macchina del formato: distanza fra 1 e il numero successivo |
| $T$ (nel codice) | il formato numerico: `float`, `double`, `Half`, `Decimal32/64/128` |

## Le identità che tengono insieme la Parte I

Le famiglie di modelli non sono isole: la libreria le verifica a vicenda con i test.

| Identità | Capitolo | Test |
|---|---|---|
| SES $\equiv$ ARIMA(0,1,1) con $\theta=\alpha-1$ | 4, 5 | `SesEqualsArima011InEveryFormat` |
| SES $\equiv$ Kalman stazionario del local level, con $\alpha=K^*$ | 5, 6 | `KalmanSteadyStateGainInEveryFormat`, `SteadyStateKalmanIsSes` |
| Levinson-Durbin $\equiv$ soluzione diretta di Yule-Walker | 2, 4 | `LevinsonDurbinMatchesDirectYuleWalkerSolve` |
| Holt-Winters esatto su retta + stagionalità | 5 | `HoltWintersExactInEveryFormat` |
| STL esatta su retta + stagionalità periodica | 9 | `StlExactInEveryFormat` |
| Previsione GARCH → varianza incondizionata con ragione $\alpha+\beta$ | 8 | `VarianceForecastConvergesToUnconditional` |

Tutte valgono con ogni formato, con tolleranze proporzionali a $\varepsilon_{\text{mach}}$.

→ [Parte II — La libreria](../01-libreria/README.md)
