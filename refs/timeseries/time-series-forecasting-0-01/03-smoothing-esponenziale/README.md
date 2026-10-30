# Exponential smoothing: SES, Holt, Holt-Winters

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-01` · second row of the infographic (1950s – 1960) · **1950s – 1960** · status ✅ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The idea.** *Update the level, then add trend and seasonality.* Recent data count more than remote data, with weights that decay geometrically.

**SES (1950s): the level only.**

$$\ell_t=\alpha\,y_t+(1-\alpha)\,\ell_{t-1},\qquad \hat y_{T+h}=\ell_T.$$

Unrolling: $\ell_T=\sum_{j\ge0}\alpha(1-\alpha)^j\,y_{T-j}+\dots$ that is a **weighted average with geometric weights**. With $\alpha\to0$ the forecast tends to the mean (long memory); with $\alpha\to1$, to the naive (zero memory).

**Holt (1957): + trend.** A slope $b_t$ is added with its own constant $\beta$; the forecast is a line $\hat y_{T+h}=\ell_T+h\,b_T$.

**Holt-Winters (1960): + seasonality** of period $m$, with its own constant $\gamma$:

$$s_t=\gamma\,(y_t-\ell_{t-1}-b_{t-1})+(1-\gamma)\,s_{t-m}.$$

**Relationships.** SES is equivalent to ARIMA(0,1,1) with $\theta=\alpha-1$, and to the stationary Kalman filter of the *local level*. Linear Holt is equivalent to ARIMA(0,2,2). ETS and ARIMA are complementary families.

**Limits.** Initialisation not estimated; no multiplicative seasonality; no prediction intervals.

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The memory that fades* · **Duration:** ~80 s · **Characters:** Anguria, Giulia, narrator · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | Giulia in front of a row of spare parts. On the desk, the consumption of the last weeks as a pile of sheets. | **Giulia:** "How many bearings do I order for next week?" | a stock problem, where the method is still used |
| 2 | Anguria leafs through the pile: the last sheet is sharp, the ones below ever more faded. | **Anguria:** "The last week counts a lot, the one a month ago almost nothing." | weight that decays over time |
| 3 | A cup of coffee cooling: the heat of the old sips disperses. | *Caption:* "Every new datum weighs $\alpha$; the whole past weighs $1-\alpha$." | the SES formula in one image |
| 4 | An $\alpha$ slider: on the left "long memory, forecast flat like the mean", on the right "zero memory, forecast like the last value". | **Anguria:** "The slider decides how much to trust the last datum." | the parameter $\alpha$ |
| 5 | Three matryoshka dolls. The smallest: "level". | *Caption:* "1950s: the level." | SES |
| 6 | The second opens: a tilted arrow. | *Caption:* "1957. Holt adds the slope: if consumption rises, the forecast rises." | trend |
| 7 | The third opens: a wave that repeats. | *Caption:* "1960. Winters adds the season: Monday looks like the Mondays." | seasonality |
| 8 | Giulia repeats the calculation on the warehouse consumption: the 168-hour model. | **Giulia:** "On the weekly total it is off by only 0.3%." | real case (scenario A: Holt-Winters 168 h, +0.3%) |

**Moral:** *remember everything, but forget with elegance.*

## In the repository

| | |
|---|---|
| **Status** | ✅ implemented (SES, Holt, additive Holt-Winters, with damping) |
| **Chapter** | [5 · Exponential smoothing](../../00-formule-e-libreria/05-smoothing.md) |
| **Library** | [`ExponentialSmoothing`](../../../../src/libs/MathemaSharp.TimeSeries/ExponentialSmoothing.cs): `Ses`, `Holt`, `HoltWinters`, `FitSesAlpha` |
| **C# notebook** | [07](../../../../notebooks/TimeSeries/07-ts-smoothing-kalman.ipynb), [10](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-csharp.ipynb) |
| **PySharp notebook** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-pysharp.ipynb) |
| **In the factory** | scenario A, section 3: the weekly total forecast with Holt-Winters 168 h is off by +0.3% |

## References

- Holt, C. C. (1957). *Forecasting seasonals and trends by exponentially weighted moving averages.* Office of Naval Research (published in International Journal of Forecasting, 2004).
- Winters, P. R. (1960). *Forecasting sales by exponentially weighted moving averages.* Management Science.
- Hyndman, R. J. et al. (2002). *A state space framework for automatic forecasting using exponential smoothing methods.* International Journal of Forecasting.
