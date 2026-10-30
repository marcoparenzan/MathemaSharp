# TBATS: long, multiple and non-integer seasonalities

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-02` · third row of the infographic (2011) · **2011** · status ⬜ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The problem.** Holt-Winters and STL handle **one** seasonality, of **integer** period and not too long. But many series have more than one (a daily cycle and a weekly one) and sometimes with a **non-integer** period: a year has 365.25 days, not 365.

**The idea (De Livera, Hyndman, Snyder, 2011).** The name is the list of components: **T**rigonometric seasonality, **B**ox-Cox transform, **A**RMA errors, **T**rend, **S**easonal components. The seasonality is not a table of $m$ values to remember, but a **sum of sinusoids**,

$$s_t^{(i)}=\sum_{j=1}^{k_i}s_{j,t}^{(i)},\qquad s_{j,t}^{(i)}=s_{j,t-1}^{(i)}\cos\lambda_j+s^{*(i)}_{j,t-1}\sin\lambda_j+\gamma\,d_t,$$

(and the analogous one for the twin component $s^{*}$, shifted by a quarter turn), with frequency $\lambda_j=2\pi j/m_i$ **which can be any real number**, and amplitude and phase that **evolve over time** (the *seasonal states* of the infographic). The **Box-Cox** transformation stabilises the variance; the errors follow an **ARMA**.

**Why a sinusoid and not a table.** With period 365.25 a table of 365 values would be wrong, and for long periods it would be an enormous number of states: a few harmonics ($k_i$ small) are enough.

**Limits.** Slow estimation on long series; the seasonalities must be indicated by the analyst; it provides Gaussian prediction intervals.

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The calendar with two clocks* · **Duration:** ~80 s · **Characters:** Anguria, Giulia, narrator · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | The hourly consumption of the warehouse: waves that repeat every day, but the height of the waves changes from Monday to Sunday. | **Giulia:** "There is the rhythm of the day and there is the rhythm of the week." | two seasonalities |
| 2 | Anguria holds a clock with a single hand. | **Anguria:** "Holt-Winters has one clock. It chooses day or week." | a single period |
| 3 | A second clock, big as a wheel, with seven slices. | **Anguria:** "Here two are needed: the hour hand and the wheel of the days." | multiple periods |
| 4 | The calendar: a year is a circle with a small gap that does not close. | *Caption:* "A year has 365 days and a quarter. No integer table contains it." | non-integer period |
| 5 | A sinusoid on the table. Anguria makes it turn with an angle that is not a round number. | **Anguria:** "A sinusoid turns at any speed. Even one that does not come home in a whole number of steps." | trigonometric seasonality |
| 6 | Two sinusoids summed: the result imitates the daily cycle with its humps. | **Anguria:** "A few sinusoids are enough to make a complicated shape. And amplitude and phase can change slowly." | few harmonics, evolving states |
| 7 | The name TBATS written on five tiles: T, B, A, T, S. Anguria turns them one at a time. | *Caption:* "2011. Sinusoids, Box-Cox transformation, ARMA errors, trend, seasonal components." | the acronym |

**Moral:** *when the calendar does not divide into equal parts, the sinusoids do not need to either.*

## In the repository

| | |
|---|---|
| **Status** | ⬜ **not implemented** |
| **What is close** | one seasonality at a time with `ExponentialSmoothing.HoltWinters` ([ch. 5](../../00-formule-e-libreria/05-smoothing.md)) and with `Stl.Decompose` ([ch. 9](../../00-formule-e-libreria/09-stl-loess.md)); in [scenario A](../../../../scenari/A-magazzino-frigorifero/README.md) the daily (24 h) and weekly (168 h) cycles are tried **separately**, and period 168 beats 24 by MASE (0.918 against 1.871) |
| **What is missing** | trigonometric seasonality with real periods, multiple seasonalities in the same model, Box-Cox transformation, ARMA errors in the state space |

## References

- De Livera, A. M., Hyndman, R. J. & Snyder, R. D. (2011). *Forecasting time series with complex seasonal patterns using exponential smoothing.* Journal of the American Statistical Association.
