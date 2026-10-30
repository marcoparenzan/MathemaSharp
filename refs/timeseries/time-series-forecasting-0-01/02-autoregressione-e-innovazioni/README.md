# Autoregression and innovations

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-01` · first row of the infographic (1927–1938) · **1927–1938** · status ✅ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The idea (Yule, 1927).** Predict a value from the previous values, plus a random shock:

$$y_t-\mu=\sum_{i=1}^{p}\phi_i\,(y_{t-i}-\mu)+\varepsilon_t,\qquad \varepsilon_t\sim\text{white noise}(0,\sigma^2).$$

Yule studied Wolfer's sunspot numbers, which oscillate with a period of about 11 years. The method of the time looked for a hidden sinusoid plus noise. The new idea: **a cycle can arise from the dynamics alone**, without any sinusoid, if every value depends on the two previous ones and is pushed by random shocks.

**The cycle in the roots (AR(2)).** With $\phi_1^2+4\phi_2<0$ the roots of the polynomial $1-\phi_1z-\phi_2z^2$ are complex: the autocorrelation is a damped sinusoid of period $2\pi/\theta$, with $\cos\theta=\phi_1/(2\sqrt{-\phi_2})$. With $\phi=(1.6,-0.9)$ the period is ≈ 11.07 steps.

**Stationarity.** The roots must lie **outside** the unit circle. The Yule-Walker estimate (with Levinson-Durbin) guarantees it.

**The innovations.** $\varepsilon_t$ is the part of $y_t$ that is **not predictable** from the past: the surprise.

**Wold (1938): a theorem, not a model.** Every zero-mean stationary process can be written as a sum of past shocks, $y_t=\sum_{j\ge0}\psi_j\varepsilon_{t-j}$ plus a deterministic part. The finite AR is a parsimonious approximation of it. The infographic says so: *"AR illustration; Wold is a theorem"*.

**Limits.** Linear; assumes stationarity (differencing is needed if there is a trend); the order $p$ must be chosen.

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The pendulum and the peas* · **Duration:** ~90 s · **Characters:** Anguria, Giulia, narrator · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | Sepia. A register of numbers: the sunspots, year by year, rising and falling. | *Caption:* "1927. Astronomers count the spots on the Sun: about every 11 years, they return." | an observed cycle |
| 2 | A board with a sinusoid and, above it, dots of noise. | *Caption:* "The method of the time: look for a perfect wave hidden in the disturbance." | the previous idea: periodic signal + noise |
| 3 | Anguria draws a pendulum. Around it, kids with a blowpipe shoot peas at random. | **Anguria:** "What if no wave were hidden? What if it were just the pendulum, pushed at random?" *Caption:* "The image, according to tradition, is Yule's own." | **a cycle without a sinusoid**: it arises from the dynamics |
| 4 | The pendulum swings with a rhythm of its own, even though the peas arrive at random. | **Anguria:** "The rhythm belongs to the pendulum. The pushes are random." | memory + random push |
| 5 | Cold-storage warehouse. Giulia looks at the chamber temperature: it goes up, down, up. A door opens, a compressor starts. | **Giulia:** "My chamber is a pendulum too. The door and the thermostat push it." | the same scheme on a plant |
| 6 | The formula appears on the board, with two "memory" boxes and a "push" lightning bolt. | **Anguria:** "$y(t)=\phi_1\,y(t-1)+\phi_2\,y(t-2)+\varepsilon(t)$. Two memories and a push." | the AR(2) formula |
| 7 | A clock turns to 1938. A drawer labelled "everything that is stationary". | *Caption:* "1938. Wold proves: any stationary process is a sum of past pushes." | Wold is a **theorem**, not a model |
| 8 | Anguria with the marker on a chart of an estimate: "0.5995 and −0.2008". | **Anguria:** "Estimated from 580 points. The true ones were 0.6 and −0.2." | the estimate recovers the parameters (true numbers of notebook 10) |

**Moral:** *a rhythm can arise from memory alone, pushed by random shocks.*

## In the repository

| | |
|---|---|
| **Status** | ✅ implemented (Yule-Walker); Wold is a theorem, not a function |
| **Chapters** | [2 · Basic statistics](../../00-formule-e-libreria/02-statistica-di-base.md) (ACF, PACF, Levinson-Durbin), [4 · AR and ARIMA](../../00-formule-e-libreria/04-ar-arima.md) |
| **Library** | [`ArModel.FitYuleWalker`](../../../../src/libs/MathemaSharp.TimeSeries/Arima.cs), `SeriesStatistics.Acf/Pacf/LevinsonDurbin` |
| **C# notebook** | [06](../../../../notebooks/TimeSeries/06-ts-reference-autoregression.ipynb), [10](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb) |
| **PySharp notebook** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb) |

## References

- Yule, G. U. (1927). *On a method of investigating periodicities in disturbed series, with special reference to Wolfer's sunspot numbers.* Philosophical Transactions of the Royal Society A.
- Wold, H. (1938). *A study in the analysis of stationary time series.* Uppsala.
