# Box-Jenkins and ARIMA

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-01` · fourth row of the infographic (1970) · **1970** · status ◐ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The idea.** Not a model, but a **systematic method**: identify, estimate, check the residuals, and repeat until the residuals are white noise.

**The model.** One differences $d$ times to remove the non-stationarity, $w=\Delta^d y$, and fits an ARMA:

$$w_t=c+\sum_{i=1}^{p}\phi_i\,w_{t-i}+e_t+\sum_{j=1}^{q}\theta_j\,e_{t-j}.$$

> **MA = lagged innovations, not a moving average.** The term $\theta_j e_{t-j}$ is the *shock* of $j$ steps ago (an unobserved quantity), not an average of the data. The infographic reminds us: *"MA = lagged innovations, not a rolling average"*.

**The three steps.**

1. **Identification**: ACF and PACF suggest the orders (an AR($p$) has a PACF that vanishes after $p$; an MA($q$) has an ACF that vanishes after $q$).
2. **Estimation**: here with **Hannan-Rissanen** (two linear regressions: a long AR to reconstruct the innovations, then OLS on lags and innovations).
3. **Checking**: the residuals must be white noise; the **Ljung-Box** test verifies it.

**Forecast.** The ARMA is extended with zero future innovations and **integrated** $d$ times.

**Status: ◐ partial.** The library estimates with Hannan-Rissanen, which is an excellent initial estimator but **not exact maximum likelihood**; stationarity and invertibility are **not imposed**; no automatic selection of the order.

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The three acts of the method* · **Duration:** ~85 s · **Characters:** Tommaso, Giulia, Anguria, narrator · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | A curtain. On the stage, a sign: "Act 1 · Identify". | *Caption:* "1970. Box and Jenkins write a book: forecasting as a craft, not as intuition." | a method, not a trick |
| 2 | Giulia shows a series that rises without stopping, like a ramp. | **Anguria:** "It does not stay still: first we make it flat. Difference: today minus yesterday." | differencing to make it stationary |
| 3 | Two histograms, ACF and PACF, like fingerprints. Anguria reads them. | **Anguria:** "This one stops after two bars: it remembers two steps." | the identification |
| 4 | "Act 2 · Estimate". A scale puts the coefficients together. | *Caption:* "Two regressions: the first reconstructs the past shocks, the second uses them." | Hannan-Rissanen |
| 5 | A ghost on the stage with the sign "echo of yesterday's error". | **Anguria:** "The 'MA' is not a moving average. It is the echo of yesterday's errors." | MA = lagged innovations |
| 6 | "Act 3 · Check". Tommaso with the stamp in hand looks at the residuals: they must be monotonous like the noise of a refrigerator. | **Tommaso:** "If there is still a melody in the residuals, the model has not understood everything." | white residuals, Ljung-Box |
| 7 | A table of five models. The (1,1,1) has a p-value of 0.034, the (2,1,2) 0.015. | **Giulia:** "Even the best is not perfect." | choice of order with AIC and Ljung-Box (notebook 08) |
| 8 | The curtain closes again, then reopens on "Act 1". | **Anguria:** "It is a cycle: you start over until the residuals fall silent." | the method is iterative |

**Moral:** *a model is not trusted: it is interrogated, and the residuals are listened to.*

## In the repository

| | |
|---|---|
| **Status** | ◐ partial: Hannan-Rissanen, not exact maximum likelihood; no automatic selection |
| **Chapter** | [4 · AR and ARIMA](../../00-formule-e-libreria/04-ar-arima.md) |
| **Library** | [`ArimaModel.FitHannanRissanen`](../../../../src/libs/MathemaSharp.TimeSeries/Arima.cs), `Residuals`, `Forecast`; `SeriesStatistics.LjungBox` |
| **C# notebook** | [08](../../../../notebooks/TimeSeries/08-ts-arima-var-garch.ipynb), [10](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb) |
| **PySharp notebook** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb) |
| **Result** | notebook 08: ARIMA(1,1,1) on 400 points, out-of-sample RMSE 3.81 against 10.63 of the naive and 5.36 of the drift. With 400 points the single parameters are noisy ($\hat\phi=0.40$, $\hat\theta=0.54$ against the true 0.5 and 0.4) |

## References

- Box, G. E. P. & Jenkins, G. M. (1970). *Time Series Analysis: Forecasting and Control.* Holden-Day.
- Hannan, E. J. & Rissanen, J. (1982). *Recursive estimation of mixed autoregressive-moving average order.* Biometrika.
