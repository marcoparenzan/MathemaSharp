# ETS framework and Auto ARIMA

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-02` · second row of the infographic (2002 / 2008) · **2002 / 2008** · status ⬜ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The idea.** Make the statistical models **easy to estimate and to choose at scale**. The infographic summarises it with two boxes: *ETS (2002) → likelihood + intervals* and *Auto ARIMA (2008) → selects the orders + estimation*.

**The ETS framework (Hyndman, Koehler, Snyder, Grose, 2002).** Each exponential-smoothing method (ch. 5) becomes a **state-space model with innovations error**, with a likelihood. The name is the triple (**E**rror, **T**rend, **S**eason):

| Component | Options |
|---|---|
| Error | additive (A), multiplicative (M) |
| Trend | none (N), additive (A), additive damped (A$_d$), multiplicative (M), multiplicative damped (M$_d$) |
| Season | none (N), additive (A), multiplicative (M) |

that is $2\times5\times3=30$ combinations. For each, parameters **and initial states** are estimated by maximum likelihood, and chosen with an information criterion (AICc). The gain compared with "recipe" smoothing: **prediction intervals** and an objective way of choosing.

*Example:* ETS(A,A$_d$,A) is additive Holt-Winters with damped trend.

**Auto ARIMA (Hyndman & Khandakar, 2008).** Automates the Box-Jenkins method (ch. 4): chooses $d$ (and $D$) with unit-root tests (KPSS), then looks for $(p,q,P,Q)$ with a **stepwise search** that minimises the AICc, and estimates by maximum likelihood.

**Warning.** The AICc compares models **on the same differenced series**: one cannot compare ARIMA with different $d$, nor ETS with ARIMA, with the AICc. Automation does not replace the checking of the residuals.

**Limits.** *Local* forecasts: one model per series, no sharing among series (the theme of the other half of the infographic, see DeepAR).

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The catalogue and the sommelier* · **Duration:** ~90 s · **Characters:** Giulia, Tommaso, Anguria, narrator · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | Giulia in front of a wall of 200 charts, one for each compressor and chamber of the plant. | **Giulia:** "Two hundred series. I cannot choose the model by hand for each." | the problem of scale |
| 2 | Anguria brings a catalogue with three drop-down columns: ERROR, TREND, SEASON. | *Caption:* "2002. Hyndman and colleagues: thirty combinations, each with its own likelihood." | the ETS framework: 2 × 5 × 3 |
| 3 | Anguria leafs through: "flat, no season" up to "damped trend, additive season". | **Anguria:** "They are the smoothing methods, but now each one is a real model." | smoothing → state-space model |
| 4 | A sommelier with a glass: tastes a model, gives a score that rewards fit and punishes complication. | **Tommaso:** "Compared to what?" — **Anguria:** "Compared to the AICc score: the lower, the better." | selection with an information criterion |
| 5 | A second catalogue: ARIMA orders. The sommelier tries one combination, then the neighbouring one, and moves where the score drops. | *Caption:* "2008. Auto ARIMA: searches stepwise among the orders and chooses the best." | stepwise search |
| 6 | The sommelier shows two glasses on different tables, one with a sign "one difference", the other "two differences". A red sign: "not comparable". | **Tommaso:** "You cannot compare models with different differences." — **Anguria:** "Right. The differences are chosen beforehand, with a test." | the AICc does not compare everything |
| 7 | On Giulia's table, 200 forecasts with the uncertainty band. Tommaso looks at one: the residuals. | **Tommaso:** "I looked at them anyway." | automation does not remove the checking |

**Moral:** *automating the choice does not mean ceasing to look at the residuals.*

## In the repository

| | |
|---|---|
| **Status** | ⬜ **not implemented**, but with a close kinship |
| **What is close** | `ExponentialSmoothing.HoltWinters` (additive, with $\phi$) gives the **same point forecasts as ETS(A,A,A)** and ETS(A,A$_d$,A), but with parameters **passed by hand**, deterministic initialisation and **no intervals**. In [notebook 08](../../../../notebooks/TimeSeries/08-ts-arima-var-garch.ipynb) the comparison between ARIMA orders is done **by hand** with an approximate AIC |
| **What is missing** | exact state-space likelihood, estimation of the initial states, automatic selection (AICc), KPSS test, stepwise search |
| **Chapters** | [5 · Smoothing](../../00-formule-e-libreria/05-smoothing.md), [4 · AR and ARIMA](../../00-formule-e-libreria/04-ar-arima.md) |

## References

- Hyndman, R. J., Koehler, A. B., Snyder, R. D. & Grose, S. (2002). *A state space framework for automatic forecasting using exponential smoothing methods.* International Journal of Forecasting.
- Hyndman, R. J. & Khandakar, Y. (2008). *Automatic time series forecasting: the forecast package for R.* Journal of Statistical Software.
- Hyndman, R. J. & Athanasopoulos, G. *Forecasting: Principles and Practice* (2nd ed.), ch. 7 and 8.
