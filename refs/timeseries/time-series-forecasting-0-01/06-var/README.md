# VAR: vector autoregressive model

*(Reference image not included in the repository: third-party material.)*

> **Infographic** `time-series-forecasting-0-01` · fifth row of the infographic (1980) · **1980** · status ✅ (legend in the [atlas](../../ATLANTE.md))

## Theoretical description

**The idea.** Several series together, each explained by **its own lags and those of the others**:

$$Y_t=c+A_1Y_{t-1}+\dots+A_pY_{t-p}+\varepsilon_t,\qquad Y_t\in\mathbb R^k.$$

The element $A_i[r,s]$ measures the effect of series $s$ at lag $i$ on series $r$. For two series and one lag:

$$x_t=c_x+a_{11}x_{t-1}+a_{12}y_{t-1}+\varepsilon^x_t,\qquad y_t=c_y+a_{21}x_{t-1}+a_{22}y_{t-1}+\varepsilon^y_t.$$

**Who invented it?** The infographic says it clearly: *"Sims: a landmark application, not VAR's invention"*. Sims (1980, *Macroeconomics and Reality*) used it in macroeconomics as a reference application; the model existed before him.

**Estimation.** OLS equation by equation: with the same regressors in every equation it coincides with the SUR/GLS estimator, so no efficiency is lost.

**Stability.** Stationary if the eigenvalues of $A_1$ lie within the unit circle; for $2\times2$, $\lambda=(\operatorname{tr}\pm\sqrt{\operatorname{tr}^2-4\det})/2$. The stationary mean is $\mu=(I-A_1)^{-1}c$.

**The impulse response** $A_1^h e_j$ shows how a perturbation on one series propagates to the other.

**Limits.** Many parameters ($k^2p$); only bivariate VAR(1) for the stability check; no choice of order, no Granger, no orthogonalised impulses.

## The story

*Storyboard for a short comic-strip video. The lines belong to the [recurring cast](../../PERSONAGGI.md); historical figures appear only in the captions.*

**Title:** *The thermostat and the compressor talk to each other* · **Duration:** ~80 s · **Characters:** Giulia, Anguria, Tommaso · [cast](../../PERSONAGGI.md)

| # | Shot | Text | What it conveys |
|---|---|---|---|
| 1 | Two curves on the monitor: the chamber temperature and the compressor current. They move together, but with an offset. | **Giulia:** "When one goes up, shortly after the other moves." | two linked series |
| 2 | Two comic characters: "TEMPERATURE" (a thermometer) and "CURRENT" (a socket). They speak with two balloons. | **Temperature:** "I am hot!" — **Current:** "Then I start!" | the cross dependence |
| 3 | The current rises: an arrow pointing down on the temperature. | **Current:** "Now I cool you down." | negative effect of the current on the temperature |
| 4 | Anguria on the board: two coupled equations, with four arrows crossing. | **Anguria:** "Each one looks at itself **and** the other, at the previous step." | form of the VAR(1) |
| 5 | The numbers on the arrows light up: "+9.06 A per degree" and "−0.033 degrees per ampere". | **Tommaso:** "How do you know?" — **Anguria:** "I did not write it. It rediscovered it from the data." | the VAR rediscovers the thermostat's logic (true numbers, scenario A) |
| 6 | In a corner, a scholar with a book: "Macroeconomics and Reality". | *Caption:* "1980. Sims uses it for the economy. But the model was already there: he made it famous." | application, not invention |
| 7 | A blow on the temperature: a wave that propagates to the current and dies out. | **Giulia:** "If the door stays open, how long does the compressor take to respond?" | impulse response |

**Moral:** *series do not live alone: their conversation is information.*

## In the repository

| | |
|---|---|
| **Status** | ✅ implemented (VAR($p$) by OLS; stability only for bivariate VAR(1)) |
| **Chapter** | [7 · VAR](../../00-formule-e-libreria/07-var.md) |
| **Library** | [`VarModel.Fit`](../../../../src/libs/MathemaSharp.TimeSeries/VectorAutoregression.cs), `Forecast`, `IsStableOrder1Bivariate`; `Interop.Var` for PySharp (flattened matrices) |
| **C# notebook** | [08](../../../../notebooks/TimeSeries/08-ts-arima-var-garch.ipynb), [10](../../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-csharp.ipynb) |
| **PySharp notebook** | [10](../../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb), [A1](../../../../notebooks/TimeSeries/Scenarios/A-magazzino-frigorifero/A1-magazzino-frigorifero-pysharp.ipynb) |
| **In the factory** | scenario A, section 5: current ← temperature **+9.06 A/°C**; temperature ← current **−0.033**: the thermostat's logic, rediscovered from the data |

## References

- Sims, C. A. (1980). *Macroeconomics and reality.* Econometrica.
- Lütkepohl, H. (2005). *New Introduction to Multiple Time Series Analysis.* Springer.
