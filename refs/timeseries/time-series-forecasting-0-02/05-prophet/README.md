# Prophet

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-02` · fifth row of the infographic (2017) · **2017** · status ⬜ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The idea.** A **decomposable** model, *configurable by an analyst*: the series is the sum of three components plus noise,

$$y(t)=g(t)+s(t)+h(t)+\varepsilon_t.$$

The infographic specifies: *"A decomposable model, not a neural network"*.

| Component | What it is |
|---|---|
| **Trend** $g(t)$ | piecewise linear (or logistic, with a capacity) with **change points** (*changepoints*): the slope can change at certain instants, and the number and flexibility of the changes can be tuned |
| **Seasonality** $s(t)$ | sum of **Fourier series**: $s(t)=\sum_{n=1}^{N}\big(a_n\cos\frac{2\pi nt}{P}+b_n\sin\frac{2\pi nt}{P}\big)$, with $P$ = 7 days, 365.25 days… |
| **Holidays and events** $h(t)$ | an effect for each event (Christmas, the August closure, a promotion), with a window around the date |

It is estimated in a Bayesian way (MAP estimate or sampling with Stan) and **intervals are obtained** by simulation.

**Why it is popular.** Designed for non-specialist analysts with *business* series: it tolerates missing data and outliers, is **tuned with readable parameters** ("how much the trend changes", "how strong the seasonality is") and domain knowledge can be **entered by hand** (the dates of the shutdowns).

**Limits.** It does not model short-term dependence (no autoregressive term): the residuals may be autocorrelated; configurability is not a guarantee of accuracy: it must always be compared with a benchmark ([0-01, card 01](../../time-series-forecasting-0-01/01-previsioni-di-riferimento/README.md)).

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The Lego of trend, season and holidays* · **Duration:** ~80 s · **Characters:** Anguria, Giulia, Tommaso · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | The plant: a calendar with the mid-August holiday circled and a sign "closed for holidays". The production chart has a hole. | **Giulia:** "Every August the line stops. And every August the models get it wrong." | calendar events break the forecast |
| 2 | Anguria opens a box of bricks: three colours, TREND, SEASON, HOLIDAYS. | **Anguria:** "I build the series with three bricks." | additive decomposition |
| 3 | The TREND brick: a broken line. On one point, Giulia puts a flag: "here the slope changed". | **Giulia:** "Here we added a second shift." | trend change points |
| 4 | The SEASON brick: a smooth curve made by summing a few waves. | **Anguria:** "Week and year: a few waves summed." | Fourier series |
| 5 | The HOLIDAYS brick: vertical peaks, one for each circled date. Giulia adds them by hand. | **Giulia:** "This I know, the model cannot learn it." | domain knowledge entered by hand |
| 6 | The three bricks fit together and the sum imitates the production. Anguria raises the sign "it is not a neural network". | **Anguria:** "It is a Lego. Each piece can be looked at and tuned on its own." | transparency |
| 7 | Tommaso with a tape measure in hand: measures the gap with respect to "like last week". | **Tommaso:** "Nice to look at. But does it beat the banal?" — **Anguria:** "This question is asked of every model." | comparison with the benchmark |

**Moral:** *a model you can take apart into three pieces is a model you can trust, or doubt, with knowledge.*

## In the repository

| | |
|---|---|
| **Status** | ⬜ **not implemented** |
| **What is close** | the "decomposition" part is `Stl.Decompose` ([ch. 9](../../00-formule-e-libreria/09-stl-loess.md)); trend and Fourier seasonality could be estimated with `MatrixAlgebra.LeastSquares` by adding sine/cosine columns and indicator variables for the holidays, as covariates are added in [scenario D](../../../../scenari/D-fotovoltaico/README.md) |
| **What is missing** | piecewise linear trend with change points, Bayesian estimation, intervals by simulation |

## References

- Taylor, S. J. & Letham, B. (2018). *Forecasting at scale.* The American Statistician (preview of 2017).
