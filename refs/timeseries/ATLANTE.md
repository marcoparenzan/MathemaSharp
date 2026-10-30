# Atlas of theories

> The **91 theories** of the 18 infographics in [`refs/timeseries`](.), with **what exists in the repository** for each. The infographics describe ideas and chronology, not a ranking of models: here we say only what has been **realised**.

## Status legend

| | Meaning |
|---|---|
| ✅ | **implemented** in the library, with tests that verify its properties |
| ◐ | **in part**: a subset, a basic building block, or only in a notebook's code |
| ⬜ | **not implemented**. If a text appears, it is that of Part I only for what is needed to understand it; the in-depth card of the theory is **still to be written** |

**Text** column: the chapter of the book that explains formulas and realisation ([Part I](00-formule-e-libreria/README.md)). **C#** and **PySharp** columns: the notebooks that show it at work ([`notebooks/`](../../notebooks/), [`scenari/`](../../scenari/README.md)). **Library** column: the class or function ([API](01-libreria/02-uso-e-funzioni.md)).

Notebook abbreviations: **06–09** = [`notebooks/06…09`](../../notebooks/) (C#); **10** = the [library tour](01-libreria/04-esempi.md), in C# and in PySharp; **A1**, **D1** = scenarios [A](../../scenari/A-magazzino-frigorifero/README.md) (cold-storage warehouse) and [D](../../scenari/D-fotovoltaico/README.md) (photovoltaic), in C# and in PySharp.

**Summary:** 13 ✅ · 12 ◐ · 66 ⬜ (out of 91, counted on the tables of this page).

---

## Statistical foundations — [`time-series-forecasting-0-01`](time-series-forecasting-0-01/README.md)

**Complete cards** (theoretical description + comic-strip story) for all 9 theories. For the other infographics the atlas is still the only index.

| Theory | Years | Status | Text | C# | PySharp | Library |
|---|---|---|---|---|---|---|
| [Baseline forecasts: mean, naive, seasonal naive](time-series-forecasting-0-01/01-previsioni-di-riferimento/README.md) | — | ✅ | [ch. 3](00-formule-e-libreria/03-riferimenti-e-metriche.md) | 06, 07, A1, D1 | 10, A1, D1 | `ReferenceForecasts`, `Accuracy` |
| [Autoregression and innovations (Yule; Wold)](time-series-forecasting-0-01/02-autoregressione-e-innovazioni/README.md) | 1927–38 | ✅ | [ch. 2](00-formule-e-libreria/02-statistica-di-base.md), [4](00-formule-e-libreria/04-ar-arima.md) | 06 | 10 | `ArModel`, `SeriesStatistics` |
| [Exponential smoothing: SES, Holt, Holt-Winters](time-series-forecasting-0-01/03-smoothing-esponenziale/README.md) | 1950s–60 | ✅ | [ch. 5](00-formule-e-libreria/05-smoothing.md) | 07, A1 | 10, A1 | `ExponentialSmoothing` |
| [Kalman filter](time-series-forecasting-0-01/04-filtro-di-kalman/README.md) | 1960 | ✅ | [ch. 6](00-formule-e-libreria/06-kalman.md) | 07, A1 | 10, A1 | `KalmanFilter` |
| [Box-Jenkins / ARIMA](time-series-forecasting-0-01/05-box-jenkins-arima/README.md) | 1970 | ◐ only Hannan-Rissanen, not exact maximum likelihood | [ch. 4](00-formule-e-libreria/04-ar-arima.md) | 08 | 10 | `ArimaModel` |
| [VAR in macroeconomics](time-series-forecasting-0-01/06-var/README.md) | 1980 | ✅ | [ch. 7](00-formule-e-libreria/07-var.md) | 08, A1 | 10, A1 | `VarModel` |
| [ARCH / GARCH](time-series-forecasting-0-01/07-arch-garch/README.md) | 1982/86 | ◐ only Gaussian GARCH(1,1) (ARCH(1) = β = 0) | [ch. 8](00-formule-e-libreria/08-garch.md) | 08, A1 | 10, A1 | `Garch11` |
| [Damped trend](time-series-forecasting-0-01/08-damped-trend/README.md) | 1985 | ✅ | [ch. 5](00-formule-e-libreria/05-smoothing.md) | 07 | 10 | `ExponentialSmoothing.Holt(…, phi)` |
| [STL decomposition](time-series-forecasting-0-01/09-stl/README.md) | 1990 | ✅ | [ch. 9](00-formule-e-libreria/09-stl-loess.md) | 09 | 10 | `Stl` |

## Automation and neural models — [`time-series-forecasting-0-02`](time-series-forecasting-0-02/README.md)

**Complete cards** (theoretical description + comic-strip story) for all 9 theories.

| Theory | Years | Status | Notes |
|---|---|---|---|
| [LSTM](time-series-forecasting-0-02/01-lstm/README.md) | 1997 | ⬜ | recurrent network with gated memory |
| [ETS framework and Auto ARIMA](time-series-forecasting-0-02/02-ets-e-auto-arima/README.md) | 2002/08 | ⬜ | additive Holt-Winters coincides with ETS(A,A,A) in the point forecasts; missing are likelihood estimation and automatic selection (in notebook 08 the comparison between orders is manual, with AIC) |
| [TBATS](time-series-forecasting-0-02/03-tbats/README.md) | 2011 | ⬜ | multiple and non-integer seasonalities |
| [Trees on lag features](time-series-forecasting-0-02/04-alberi-su-feature-di-ritardo/README.md) | 2010s | ⬜ | regression on lags and covariates exists (`MatrixAlgebra.LeastSquares`, used in D1) but not the trees |
| [Prophet](time-series-forecasting-0-02/05-prophet/README.md) | 2017 | ⬜ | trend + seasonality + holidays; STL is the "decomposition" part |
| [DeepAR](time-series-forecasting-0-02/06-deepar/README.md) | 2017 | ⬜ | |
| [Transformer](time-series-forecasting-0-02/07-transformer/README.md) | 2017 | ⬜ | |
| [N-BEATS](time-series-forecasting-0-02/08-n-beats/README.md) | 2019 | ⬜ | |
| [Temporal Fusion Transformer](time-series-forecasting-0-02/09-temporal-fusion-transformer/README.md) | 2019 | ⬜ | |

## Transformers and strong baselines — `time-series-forecasting-0-03`

| Theory | Years | Status | Notes |
|---|---|---|---|
| Informer | 2020 | ⬜ | |
| Autoformer | 2021 | ⬜ | uses a trend/seasonality decomposition like STL, but inside the network |
| N-HiTS | 2022 | ⬜ | |
| DLinear | 2022 | ⬜ | decomposition + two linear maps: the building blocks (`Stl`, `LeastSquares`) are there, the model is not |
| PatchTST | 2022 | ⬜ | |
| iTransformer | 2023 | ⬜ | |

## Language models and pre-training — `time-series-forecasting-0-04`

| Theory | Date | Status |
|---|---|---|
| Time-LLM | Oct 2023 | ⬜ |
| TimeGPT-1 | Oct 2023 | ⬜ |
| LLMTime | Oct 2023 | ⬜ |
| TimesFM | Oct 2023 | ⬜ |
| Moirai | Feb 2024 | ⬜ |
| Chronos | Mar 2024 | ⬜ |

## Efficient and multivariate models — `time-series-forecasting-0-05`

| Theory | Date | Status |
|---|---|---|
| Time-MoE | Sep 2024 | ⬜ |
| Chronos-Bolt | Nov 2024 | ⬜ |
| Chronos-2 | Oct 2025 | ⬜ (its use of covariates is the theme of scenario D, with a regression) |
| Moirai 2.0 | Nov 2025 | ⬜ |
| TimesFM-3 | Aug 2026 | ⬜ |

## Diagnostics: change points — `time-series-diagnostic-01`

| Theory | Status | Notes |
|---|---|---|
| PELT (offline) | ⬜ | |
| BOCPD (online) | ⬜ | |
| ClaSP (offline) | ⬜ | |

*The change is not the alarm*: the concept (detection delay, false alarms) is shown with GARCH in scenario A (alarm 71 minutes after the true change): ◐ as a demonstration, not as an algorithm.

## Diagnostics: matrix profile — `time-series-diagnostic-02`

| Theory | Status |
|---|---|
| Nearest neighbor (matrix profile) | ⬜ |
| Motif and discord | ⬜ |
| FLUSS / FLOSS | ⬜ |

## Validation of forecasts, 1982–2000 — `time-series-forecast-validation-01`

| Theory | Years | Status | Notes |
|---|---|---|---|
| M competition | 1982 | ◐ | baseline benchmarks and MASE (`ReferenceForecasts`, `Accuracy.Mase`) |
| Prequential principle | 1984 | ⬜ | |
| M2 competition | 1987–89 | ⬜ | |
| h-block CV | 1994 | ⬜ | |
| hv-block CV | 2000 | ⬜ | |
| Rolling-origin | 2000 | ✅ | `RollingOrigin.Evaluate`: expanding training, gap, MAE/RMSE per origin and test of the isolation of the past. The existing scenarios are not yet migrated to the multiple backtest |

## Validation: comparison between forecasts, 2000–2012 — `time-series-forecast-validation-02`

| Theory | Years | Status |
|---|---|---|
| Reality Check (White) | 2000 | ⬜ |
| SPA test (Hansen) | 2005 | ⬜ |
| Conditional ability (Giacomini-White) | 2006 | ⬜ |
| Clark-West | 2007 | ⬜ |
| Model Confidence Set | 2011 | ⬜ |
| CV for time series (Bergmeir-Benítez) | 2012 | ⬜ |

## Validation: beyond a single score, 2012–2025 — `time-series-forecast-validation-03`

| Theory | Years | Status | Notes |
|---|---|---|---|
| Window-robust tests | 2012 | ⬜ | |
| Murphy diagrams | 2016 | ⬜ | `Accuracy.PinballLoss` is the elementary quantile loss: ◐ as a building block |
| Approximate LFO | 2019/20 | ⬜ | |
| Always-valid comparison (*anytime-valid*) | 2021 | ⬜ | |
| Short-lived instability | 2024 | ⬜ | |
| Model prediction set (conformal) | 2025 | ⬜ | |

## Evaluating pre-trained models, 2024–2026 — `time-series-forecast-validation-04`

| Theory | Years | Status |
|---|---|---|
| GIFT-Eval | 2024 | ⬜ |
| fev-bench | 2025 | ⬜ |
| TIME | 2026 | ⬜ |

## Map of the architectures — `time-series-forecasting-1-01-a map of model architectures`

| Theory | Status | Notes |
|---|---|---|
| Linear / MLP | ◐ | a linear map from the window to the future is a regression: `MatrixAlgebra.LeastSquares` (scenario D), not an MLP |
| Temporal CNN | ⬜ | |
| RNN / LSTM | ⬜ | |
| Transformer | ⬜ | |
| State-space model | ◐ | the Kalman filter is the linear case (`KalmanFilter`); not those with learned dynamics |
| Graph neural network | ⬜ | |

## Representing the history — `time-series-forecasting-1-02-representing the history`

| Theory | Status | Text | Library |
|---|---|---|---|
| Single values | ✅ | — | the series itself (`T[]`) |
| Patch | ◐ | [Analysis and use](../../TIMESERIES.md) | `HistoryPatches.Extract`: complete windows with stride, without padding or learned embedding |
| Components (trend + seasonal + remainder) | ✅ | [ch. 9](00-formule-e-libreria/09-stl-loess.md) | `Stl` |
| Frequency domain | ◐ | | `Signals.Dft` (didactic, $O(N^2)$); in PySharp `np.fft` |
| Multiple scales | ⬜ | | |

## How the past reaches the forecast — `time-series-forecasting-1-03-how the past reaches the forecast`

| Theory | Status | Notes |
|---|---|---|
| Temporal convolution | ◐ | the moving average (STL's low-pass filter) is a convolution; there is no CNN |
| Recurrent state | ◐ | the Kalman state and the AR/ARIMA recursions carry the information through time; not an RNN |
| Self-attention | ⬜ | |

## One step or the whole horizon — `time-series-forecasting-1-one step or the whole horizon`

| Theory | Status | Notes |
|---|---|---|
| Recursive | ✅ | `ArModel.Forecast`, `ArimaModel.Forecast`, smoothing: the $h$-step forecasts are recursive |
| Direct | ⬜ | it can be done with a `LeastSquares` per horizon, but it is not a function of the library |
| MIMO | ⬜ | |
| Rectify | ⬜ | |

## Beyond recursive versus direct — `time-series-forecasting-1-beyond recursive vs direct`

| Theory | Year | Status |
|---|---|---|
| Stratify | 2025 | ⬜ |
| QDF | 2026 | ⬜ |
| FlowTime | 2025 | ⬜ |
| TSFlow | 2025 | ⬜ |

## Conditioning on an uncertain future — `time-series-forecasting-1-conditioning on an uncertain future`

| Theory | Status | Notes |
|---|---|---|
| Conditioning on a known input | ◐ | scenario D: regression on the forecast irradiance (`LeastSquares`) |
| Propagating the input uncertainty | ⬜ | in scenario D the weather error is absorbed by the residuals, not modelled separately |
| Intervening (observing ≠ doing) | ⬜ | |

## From points to possible futures — `time-series-forecasting-1-from points to possible futures`

| Theory | Status | Text | Library / notes |
|---|---|---|---|
| Point forecast | ✅ | all of Part I | all the forecasts |
| Quantiles and intervals | ✅ | [API](01-libreria/02-uso-e-funzioni.md) | `SeriesStatistics.Quantile`, `Accuracy.PinballLoss`, `Accuracy.IntervalCoverage`; D1 |
| Predictive distribution | ◐ | | the set of sampled trajectories of D1 approximates it |
| Sampled trajectories | ◐ | | resampling of blocks of residuals in D1 (in the notebook's code, not a function of the library) |

---

## Next natural implementations

By ratio of usefulness in the scenarios to cost, in order:

1. **Rolling origin** and **Diebold-Mariano / Clark-West**: they serve to say whether a model really beats another (A and D use a single test week).
2. **Conformal prediction** and **intervals for ARIMA/VAR**: intervals with guaranteed coverage (today from quantiles of the residuals, without guarantee).
3. **Matrix profile (STOMP)**: defrost cycles, anomalies of the machining cycle.
4. **PELT** and **BOCPD**: date setpoint or regime changes.
5. **State-space ETS with AIC selection** and ARIMA by exact maximum likelihood.

The neural models, the *foundation models* and the benchmarks (about forty entries of this atlas) require training and data that this library does not intend to provide: they remain described, not implemented.
