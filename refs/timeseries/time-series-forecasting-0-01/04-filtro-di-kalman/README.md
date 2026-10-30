# Kalman filter

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-01` · third row of the infographic (1960) · **1960** · status ✅ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The idea.** *Predict a hidden state; correct it with the new observations.* The state $x_t$ evolves linearly and is seen only through a noisy measurement:

$$x_t=F\,x_{t-1}+w_t,\;w_t\sim\mathcal N(0,Q)\qquad y_t=H\,x_t+v_t,\;v_t\sim\mathcal N(0,R).$$

**Predict** (propagates state and uncertainty): $\hat x^-=F\hat x$, $\;P^-=FPF^\top+Q$.

**Update** (corrects with the observation): $v=y-H\hat x^-$, $\;K=P^-H^\top/S$, $\;\hat x=\hat x^-+Kv$, with $S=HP^-H^\top+R$.

The **gain** $K$ is the trust given to the observation: high if the measurement is reliable ($R$ small) or if the state is uncertain ($P^-$ large).

**Missing data.** If $y_t$ is missing, the update is skipped: the state remains the forecast alone and the uncertainty **grows** by $Q$ at every step. This is the advantage over "recipe" smoothing.

**Equivalence with SES.** In the *local level* the gain converges to $K^*=P^*/(P^*+r)$, with $P^*=(q+\sqrt{q^2+4qr})/2$ (Riccati equation). The update becomes one line of SES with $\alpha=K^*$.

**Limits.** Linear and Gaussian; here with scalar observation and $Q$, $R$ **not estimated** (they are passed in). *It does not guess what it does not see*: during a gap it predicts, it does not discover.

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The detective and the clue* · **Duration:** ~90 s · **Characters:** Anguria, Giulia, Tommaso, narrator · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | Anguria in a raincoat and detective hat. In front of her, a cold chamber in the background, in the dark. | **Anguria:** "I am sure the chamber is at 2 degrees. But the sensor lies a little." | hidden state + noisy measurement |
| 2 | Giulia hands over a sheet: the sensor reading, 2.16, then 2.10, then 2.09: jerky. | **Giulia:** "Noise. The sensor is off by 0.08 degrees." | the measurement noise $R$ |
| 3 | Anguria draws an arrow: "where it should be now, in my opinion". | *Caption:* "**Predict.** From the previous point, and from how the chamber moves." | predict |
| 4 | A clue arrives, the new reading. The detective compares arrow and clue. | *Caption:* "**Correct.** How much you trust the clue depends on how sure you are of yourself." | update and gain $K$ |
| 5 | A "trust" knob with two extremes: "I only trust myself" / "I only trust the sensor". | **Anguria:** "The gain turns by itself. If the process noise is a quarter of the measurement noise, at steady state it stops at 0.39." | steady-state gain |
| 6 | The gateway loses the signal: the data panel goes empty for 12 minutes. Anguria keeps drawing arrows, but the arrows become more and more blurred. | **Anguria:** "Without clues I can only deduce. And the more time passes, the less sure I am." | missing data: the uncertainty grows |
| 7 | The signal returns: the temperature has jumped to 4.5 degrees. Anguria looks at the arrow, stuck at 2.6. | **Tommaso:** "When did it go above 4?" — **Anguria:** "I don't know. It was inside the dark. I say: between 7 and 19 minutes." | **you cannot guess what you cannot see**; the KPI as an interval (scenario A: truth 11) |
| 8 | Caption with a small space capsule in the background. | *Caption:* "1960, Kalman. The same method later guided the Apollo missions." | historical importance |

**Moral:** *predict, then correct: and when you cannot see, say so.*

## In the repository

| | |
|---|---|
| **Status** | ✅ implemented (scalar observation, local level, local linear trend, Joseph form, missing data) |
| **Chapter** | [6 · The Kalman filter](../../00-formule-e-libreria/06-kalman.md) |
| **Library** | [`KalmanFilter`](../../../../src/libs/MathemaSharp.TimeSeries/KalmanFilter.cs): `LocalLevel`, `LocalLinearTrend`, `LocalLevelSteadyStateGain`, `Filter` |
| **C# notebook** | [07](../../../../notebooks/TimeSeries/07-ts-smoothing-kalman.ipynb), [10](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-csharp.ipynb) |
| **PySharp notebook** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-pysharp.ipynb) |
| **In the factory** | scenario A: temperature with 6 communication gaps; observed 7 minutes above +4 °C, Kalman 8, honest interval 7–19, truth 11 |

## References

- Kalman, R. E. (1960). *A new approach to linear filtering and prediction problems.* Journal of Basic Engineering.
- Durbin, J. & Koopman, S. J. *Time Series Analysis by State Space Methods.* Oxford University Press.
