# Baseline forecasts: mean, naive, seasonal naive

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-01` · "Reference forecasts" band at the top of the infographic · **always valid** · status ✅ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**What they are.** Three ways of forecasting that require no model:

| Method | Rule | $\hat y_{T+h}$ | Optimal for |
|---|---|---|---|
| **Mean** | the historical mean | $\bar y$ | white noise around a mean |
| **Naive** | the last value | $y_T$ | random walk |
| **Seasonal naive** | the last value of the same season | $y_{T+h-m(k+1)}$, $k=\lfloor(h-1)/m\rfloor$ | stable seasonality without trend |

The infographic calls them *"still benchmarks today"*. They are not naive forecasts, they are the *optimal* models for very simple processes. This is why every more complex model must beat them to justify its own existence.

**How it is compared.** With the **MASE** (Hyndman & Koehler, 2006): the out-of-sample error divided by the in-sample error of the seasonal naive,

$$\text{MASE}=\frac{\text{MAE}_{\text{out of sample}}}{\frac{1}{n-m}\sum_{t=m+1}^{n}|y_t-y_{t-m}|}.$$

MASE $<1$ means "better than the seasonal naive". It is scale-independent and defined even with zeros (the MAPE is not).

**Limits.** The naive cannot do trend or seasonality; the seasonal naive inherits the noise of the last cycle (it copies its errors too); the mean ignores everything that changes over time.

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The consultant and the banal* · **Duration:** ~75 s · **Characters:** Giulia, Tommaso, Anguria · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | Office of the cold-storage warehouse. On the monitor, the hourly energy consumption: waves that repeat every day. A consultant points at a chart with many arrows and boxes. | *Caption:* "A proposal: a sophisticated model to forecast next week's energy." | there are those who sell complexity |
| 2 | Tommaso, arms crossed, "VERIFIED" stamp in hand. | **Tommaso:** "Better compared to what?" | the right question is always *compared to what* |
| 3 | Anguria climbs on a stool with three signs: MEAN, LAST VALUE, LIKE YESTERDAY AT THE SAME HOUR. | **Anguria:** "Three forecasts that cost nothing. Can they be beaten?" | the three benchmarks |
| 4 | The chart: the MEAN is a flat line; the LAST VALUE is a line that starts from the last point; LIKE LAST WEEK copies the waves. | **Giulia:** "The last one copies the wave. It already has the right shape." | seasonal naive = copies the cycle |
| 5 | A scale: on one side the consultant with his model, on the other a sign "MASE 0.918". Under the sign, in small print: "seasonal naive, 168 hours". | **Tommaso:** "Your model?" — **Consultant:** "Holt-Winters: 0.934." | **the banal wins**: 0.918 against 0.934 (true numbers of scenario A) |
| 6 | The consultant looks at the sign. Anguria hands him the marker. | **Anguria:** "It is not a defeat. Now you know *how much* it must improve." | the benchmark measures the model's value |
| 7 | Tommaso stamps the sheet. | **Tommaso:** "We start from here." | |

**Moral:** *the first opponent of every model is the banal.*

## In the repository

| | |
|---|---|
| **Status** | ✅ implemented |
| **Chapter** | [3 · Baseline forecasts and metrics](../../00-formule-e-libreria/03-riferimenti-e-metriche.md) |
| **Library** | [`ReferenceForecasts`](../../../../src/libs/MathemaSharp.TimeSeries/ReferenceForecasts.cs): `Mean`, `Naive`, `SeasonalNaive`, `Drift`; `Accuracy.Mase` |
| **C# notebook** | [06](../../../../notebooks/TimeSeries/06-ts-reference-autoregression.ipynb), [10 · library tour](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-csharp.ipynb), [D1](../../../../notebooks/TimeSeries/Scenarios/D-fotovoltaico/D1-fotovoltaico-csharp.ipynb) |
| **PySharp notebook** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-pysharp.ipynb), [D1](../../../../notebooks/TimeSeries/Scenarios/D-fotovoltaico/D1-fotovoltaico-pysharp.ipynb) |
| **In the factory** | scenario A, section 3: the weekly seasonal naive (MASE 0.918) beats Holt-Winters (0.934) on the hourly profile |

## References

- Hyndman, R. J. & Koehler, A. B. (2006). *Another look at measures of forecast accuracy.* International Journal of Forecasting.
- Hyndman, R. J. & Athanasopoulos, G. *Forecasting: Principles and Practice* (2nd ed.), ch. 3.
