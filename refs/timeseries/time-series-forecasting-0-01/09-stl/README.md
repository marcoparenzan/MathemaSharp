# STL decomposition

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-01` · eighth row of the infographic (1990) · **1990** · status ✅ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The idea.** *Separate trend-cycle, seasonality and remainder with LOESS.* Then, says the infographic, **decompose first, forecast later**: *"Decompose first; forecast components separately"*.

$$y_t=T_t+S_t+R_t.$$

**LOESS.** At each point a **weighted line** is fitted to the $q$ nearby points, with tricube weights $w_i=(1-(|x_i-x|/h)^3)^3$. A line is reproduced exactly, even when extrapolating.

**The inner loop** (6 steps): remove the trend; **cycle-subseries** (all the "Januaries", all the "Februaries"…, smoothed with LOESS and extended by one cycle); **low-pass** of that series (three moving averages + LOESS); seasonality $S=C-L$; deseasonalise; LOESS of the trend.

**Robustness.** After each inner loop bisquare weights are computed from the residuals: an **outlier** has weight ≈ 0 and ends up in the remainder, without dragging trend and seasonality.

**Exactness.** On a line plus a zero-mean periodic seasonality, STL separates the components **exactly**: the LOESS reproduce lines and the one-period moving average cancels the seasonality.

**Limits.** Faithful to the *algorithm*, not to the original Fortran (no *jump*, only degree-1 LOESS); a single seasonality; additive (for the multiplicative one use the logarithm).

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The three-channel mixer* · **Duration:** ~85 s · **Characters:** Anguria, Giulia, Tommaso, narrator · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | A recording studio. A three-channel mixer: "BASS", "RHYTHM", "HISS". A song enters through a cable. | **Anguria:** "The song is all together. But inside there are three things." | y = trend + seasonality + remainder |
| 2 | The BASS channel: a slow line that rises. | **Anguria:** "The bass is the trend: slow, it changes gently." | trend |
| 3 | The RHYTHM channel: a beat that repeats every 12 bars. | **Anguria:** "The rhythm comes back the same: the seasonality." | seasonality |
| 4 | The HISS channel: irregular dots. | **Giulia:** "And the hiss is what remains: the remainder." | remainder |
| 5 | A hand erases the season: all the Januaries one after the other form a small series of their own. | *Caption:* "1990, Bell Labs. STL looks at the Januaries together, the Februaries together: each month is a series." | cycle-subseries |
| 6 | On Giulia's monitor, the warehouse consumption with two anomalous peaks (two interruptions). The peaks end up straight in the HISS channel. | **Tommaso:** "And if a failure dirties the song?" — **Anguria:** "With the robust version, the failure ends up in the hiss. Rhythm and bass stay clean." | robustness: weights $\approx0$ on the outliers |
| 7 | A table: error on the rhythm 0.69 (normal) against 0.39 (robust). | **Tommaso:** "Are you sure?" — **Anguria:** "True numbers: the error on the seasonality drops by 43%." | notebook 09 |
| 8 | The mixer: the three channels forecast separately and then put back together. | **Anguria:** "First separate, then forecast each channel, then recompose." | decompose first, forecast later |
| 9 | Giulia shrugs: in one test the forecast by components has MASE 0.39, Holt-Winters 0.34. | **Giulia:** "Here Holt-Winters won anyway." | honesty: it does not always win |

**Moral:** *understanding what a signal is made of is already half of the forecast.*

## In the repository

| | |
|---|---|
| **Status** | ✅ implemented (STL with robustness, linear LOESS) |
| **Chapter** | [9 · LOESS and STL](../../00-formule-e-libreria/09-stl-loess.md) |
| **Library** | [`Stl.Decompose`](../../../../src/libs/MathemaSharp.TimeSeries/Stl.cs), `Stl.Loess` |
| **C# notebook** | [09](../../../../notebooks/TimeSeries/09-ts-stl-ndsharp.ipynb), [10](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb) |
| **PySharp notebook** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb) |
| **Result** | notebook 09: with two outliers, error on the seasonality 0.687 (non-robust) against 0.394 (robust, 6 iterations); robust weight of the outliers 0. Forecast by components: MASE 0.390 against 0.337 of damped Holt-Winters and 0.372 of the seasonal naive |

## References

- Cleveland, R. B., Cleveland, W. S., McRae, J. E. & Terpenning, I. (1990). *STL: a seasonal-trend decomposition procedure based on loess.* Journal of Official Statistics.
