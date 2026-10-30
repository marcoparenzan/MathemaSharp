# ARCH and GARCH

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-01` · sixth row of the infographic (1982 / 1986) · **1982 / 1986** · status ◐ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The idea.** The previous models describe the **mean**. Many series have an almost constant mean and a **variance that changes**: calm periods, agitated periods, in clusters (*volatility clustering*). The shocks are almost uncorrelated, but their **squares** are not.

**ARCH (Engle, 1982)** and **GARCH (Bollerslev, 1986):**

$$\varepsilon_t=\sigma_t z_t,\;z_t\sim\mathcal N(0,1)\qquad \sigma_t^2=\omega+\alpha\,\varepsilon_{t-1}^2+\beta\,\sigma_{t-1}^2.$$

- $\alpha$: reaction to yesterday's shock (the ARCH part); $\beta$: memory of yesterday's variance (the GARCH part); **ARCH(1) is the case $\beta=0$**.
- Covariance-stationary if $\alpha+\beta<1$; the variance returns to $\bar\sigma^2=\omega/(1-\alpha-\beta)$.

**Variance forecast.** It decays geometrically with ratio $\alpha+\beta$:

$$\hat\sigma^2_{T+h}=\bar\sigma^2+(\alpha+\beta)^{h-1}\big(\hat\sigma^2_{T+1}-\bar\sigma^2\big).$$

**Estimation.** Gaussian maximum likelihood with *variance targeting* ($\omega=s^2(1-\alpha-\beta)$) on a refined grid.

**What it does NOT do.** *"Volatility, not the mean"*, as the infographic says: it does not predict the value, it predicts **how uncertain it will be**.

**Status: ◐ partial.** Only GARCH(1,1) with Gaussian innovations: no GARCH($p,q$), fat tails (Student-$t$), leverage effects (GJR, EGARCH).

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The calm sea and the rough sea* · **Duration:** ~90 s · **Characters:** Giulia, Anguria, Tommaso, narrator · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | The harbour of a city. The sea is flat, then, at times, it ripples, then goes flat again. | *Caption:* "The mean value of the wave does not change. What changes is how much it churns." | constant mean, changing variance |
| 2 | On Giulia's monitor: the vibration of the compressor. Small oscillations, then a group of large oscillations, then small again. | **Giulia:** "The big shocks come together. After a big shock another one arrives." | volatility clustering |
| 3 | Anguria draws two tails: "yesterday was agitated" and "yesterday was calm". Today's agitation thermometer depends on both. | **Anguria:** "Today's agitation = a bit of base + yesterday's shock + the memory of yesterday's agitation." | $\sigma_t^2=\omega+\alpha\varepsilon^2+\beta\sigma^2$ |
| 4 | A spring that loads when there is a shock and then relaxes slowly. | *Caption:* "A shock takes time to die out: it depends on $\alpha+\beta$." | persistence |
| 5 | Caption with two dates: 1982 and 1986. A hand draws a small "A" and then a "G". | *Caption:* "1982, Engle: yesterday's agitation. 1986, Bollerslev: also its memory." | ARCH → GARCH |
| 6 | Three days of vibration compressed in a frame. The first 48 hours: the calm sea. Then something changes. | **Anguria:** "The bearing is failing. But nobody knows yet." | regime change (scenario A) |
| 7 | A bell. A clock shows 71 minutes after the start of the change. | **Tommaso:** "The change started at midnight. The alarm?" — **Giulia:** "71 minutes later. No false alarms before." | **the moment of the change is not the moment of the alarm** |
| 8 | The alarm chart: 0.5% of the minutes on the first day, 23% on the last. | **Anguria:** "It does not say what is breaking. It says that it has changed." | the GARCH as a bell |

**Moral:** *sometimes the signal is not the value, but how much it trembles.*

## In the repository

| | |
|---|---|
| **Status** | ◐ partial: only Gaussian GARCH(1,1) (ARCH(1) is the case β = 0) |
| **Chapter** | [8 · ARCH and GARCH](../../00-formule-e-libreria/08-garch.md) |
| **Library** | [`Garch11`](../../../../src/libs/MathemaSharp.TimeSeries/Garch.cs): `ConditionalVariances`, `LogLikelihood`, `ForecastVariance`, `FitVarianceTargeting` |
| **C# notebook** | [08](../../../../notebooks/TimeSeries/08-ts-arima-var-garch.ipynb), [10](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-csharp.ipynb) |
| **PySharp notebook** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-pysharp.ipynb) |
| **In the factory** | scenario A, section 4: estimate $(0.0019,\,0.083,\,0.809)$ against the true $(0.002,\,0.10,\,0.80)$; alarm in 0.5% of the minutes on the first day and 22.7% on the last; it goes off 71 minutes after the true change |

## References

- Engle, R. F. (1982). *Autoregressive conditional heteroscedasticity with estimates of the variance of United Kingdom inflation.* Econometrica.
- Bollerslev, T. (1986). *Generalized autoregressive conditional heteroskedasticity.* Journal of Econometrics.
