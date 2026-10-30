# Damped trend

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-01` · seventh row of the infographic (1985) · **1985** · status ✅ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The idea.** Holt's linear trend continues **to infinity**: an assumption that is almost never true. The *damped trend* **dampens** the contribution of the slope at long horizons, with a parameter $\phi\in[0,1]$.

$$\hat y_{t|t-1}=\ell_{t-1}+\phi\,b_{t-1},\quad \ell_t=\alpha y_t+(1-\alpha)(\ell_{t-1}+\phi b_{t-1}),\quad b_t=\beta(\ell_t-\ell_{t-1})+(1-\beta)\phi b_{t-1}.$$

**Forecast at $h$ steps:**

$$\hat y_{T+h}=\ell_T+\Big(\sum_{i=1}^{h}\phi^i\Big)b_T=\ell_T+\frac{\phi(1-\phi^h)}{1-\phi}\,b_T\;\xrightarrow{h\to\infty}\;\ell_T+\frac{\phi}{1-\phi}\,b_T.$$

- $\phi=1$: linear Holt, the line does not stop;
- $\phi<1$: the trend **dies out**, the forecast tends to a constant.

This is what the infographic shows: a dashed *"Linear trend"* that rises, a *"Damped"* one that bends. Empirically (M competitions) it is one of the most robust methods: rarely the best, almost never a disaster.

**Limits.** $\phi$ must be chosen or estimated (here it is passed in); it is not a probabilistic model (no intervals).

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The growth that gets tired* · **Duration:** ~70 s · **Characters:** Anguria, Giulia, Tommaso · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | A potted plant, grown so much that it touches the cardboard ceiling. On Anguria's board, a growth line that rises. | **Anguria:** "This plant grows 5 centimetres a month. How tall will it be in ten years?" | the linear trend |
| 2 | Anguria draws the line: it crosses the ceiling, the clouds, the moon. | **Tommaso:** "Six metres tall? It is not a building." | extrapolating to infinity is absurd |
| 3 | A plant: a new production line, the pieces per day grow in the first weeks and then the growth slows. | **Giulia:** "Our ramp-up does this: at the beginning it rises, then it settles." | real growth gets tired |
| 4 | A $\phi$ slider with a spring: at 1 the line continues; at 0.85 it bends. | **Anguria:** "Each month the growth weighs a little less than the month before: I multiply it by $\phi$." | the geometric damping |
| 5 | The chart: the dashed line (linear) shoots away, the solid line (damped) settles on a plateau. | *Caption:* "1985. Gardner and McKenzie dampen the trend." | $\ell+\phi b/(1-\phi)$ |
| 6 | A scale with two numbers: "Linear Holt: error 4.34" and "Damped: error 1.53". | **Tommaso:** "How do you know it works?" — **Anguria:** "On a growth that slows, the error drops by two thirds." | true numbers of notebook 07 (RMSE at 20 steps) |
| 7 | The limit: a horizontal line at 84.5 on the chart. | **Anguria:** "This is where the forecast stops. I already know it today." | the asymptotic limit |

**Moral:** *nothing grows forever: the forecast must not believe it.*

## In the repository

| | |
|---|---|
| **Status** | ✅ implemented (Holt and Holt-Winters with $\phi$) |
| **Chapter** | [5 · Exponential smoothing](../../00-formule-e-libreria/05-smoothing.md) |
| **Library** | [`ExponentialSmoothing.Holt(y, α, β, horizon, phi)`](../../../../src/libs/MathemaSharp.TimeSeries/ExponentialSmoothing.cs), `HoltWinters(…, phi)` |
| **C# notebook** | [07](../../../../notebooks/TimeSeries/07-ts-smoothing-kalman.ipynb), [10](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb) |
| **PySharp notebook** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb) |
| **Result** | notebook 07, slowing growth: RMSE at 20 steps **4.34** (linear Holt) against **1.53** (damped, $\phi=0.85$); asymptotic limit 84.5 |

## References

- Gardner, E. S. & McKenzie, E. (1985). *Forecasting trends in time series.* Management Science.
