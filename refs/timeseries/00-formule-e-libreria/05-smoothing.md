# 5 · Exponential smoothing: SES, Holt, damped trend, Holt-Winters

Code: [ExponentialSmoothing.cs](../../../src/libs/MathemaSharp.TimeSeries/ExponentialSmoothing.cs).

The idea (1950s): **update the level, then add trend and seasonality.** Three ingredients, one assumption: recent data count more than remote data, with weights that decay geometrically.

## SES: the level only

$$\ell_t=\alpha\,y_t+(1-\alpha)\,\ell_{t-1},\qquad \hat y_{T+h}=\ell_T\quad(\ell_0=y_0)$$

Unrolling the recursion, the level is a **weighted average with geometric weights**:

$$\ell_T=\sum_{j=0}^{T-1}\alpha(1-\alpha)^j\,y_{T-j}+(1-\alpha)^T\ell_0$$

- $\alpha\to0$: forecast = mean (long memory);
- $\alpha\to1$: forecast = naive (zero memory).

Verification `SesIsGeometricWeightedAverage`: $\ell_2=\alpha y_2+\alpha(1-\alpha)y_1+(1-\alpha)^2y_0$ exact, and $\alpha=0\Rightarrow$ mean, $\alpha=1\Rightarrow$ last value.

### Estimating $\alpha$: golden section

The SSE of the one-step errors $\sum(y_t-\hat y_{t|t-1})^2$ is minimised on $[0,1]$. The **golden section** reduces the interval by the factor $1/\varphi\approx0.618$ at each step **reusing a point already evaluated** (a single evaluation per iteration):

$$c=b-\tfrac1\varphi(b-a),\quad d=a+\tfrac1\varphi(b-a)$$

It presupposes a **unimodal** SSE in $\alpha$: typical, not guaranteed. Default tolerance $\sqrt{\varepsilon_{\text{mach}}}$. Verification `FitSesAlphaFindsLowAlphaForNoiseAndHighForRandomWalk`: white noise → $\alpha<0.1$; random walk → $\alpha>0.9$.

## Holt (1957) and the damped trend (Gardner & McKenzie, 1985)

A slope $b_t$ is added. With damping $\phi\in[0,1]$:

$$\hat y_{t|t-1}=\ell_{t-1}+\phi\,b_{t-1}$$
$$\ell_t=\alpha\,y_t+(1-\alpha)(\ell_{t-1}+\phi\,b_{t-1})$$
$$b_t=\beta\,(\ell_t-\ell_{t-1})+(1-\beta)\,\phi\,b_{t-1}$$

**Forecast at $h$ steps:**

$$\hat y_{T+h}=\ell_T+\Big(\sum_{i=1}^{h}\phi^i\Big)b_T=\ell_T+\frac{\phi(1-\phi^h)}{1-\phi}\,b_T\;\xrightarrow{h\to\infty}\;\ell_T+\frac{\phi}{1-\phi}\,b_T$$

- $\phi=1$: **linear Holt**, the trend continues to infinity.
- $\phi<1$: the trend **dies out**: at long horizons the forecast tends to a constant.

Empirically (M competitions) it is one of the most robust methods: rarely the best, almost never a disaster.

```csharp
power *= phi;
cumulative += power;                            // φ + φ² + … + φʰ
result[h - 1] = level + cumulative * trend + season(h);
```

Verifications (`HoltReproducesLinesAndDampingConverges`): a line is reproduced with zero SSE; with $\phi=0.9$ the 500-step forecast coincides with the limit $\ell_T+\phi b_T/(1-\phi)$ to $10^{-9}$.

## Additive Holt-Winters (1960)

Adds $m$ seasonal values $s_j$ (period $m$):

$$\ell_t=\alpha\,(y_t-s_{t-m})+(1-\alpha)(\ell_{t-1}+\phi b_{t-1})$$
$$s_t=\gamma\,(y_t-\ell_{t-1}-\phi b_{t-1})+(1-\gamma)\,s_{t-m}$$
$$\hat y_{T+h}=\ell_T+\Big(\sum_{i=1}^{h}\phi^i\Big)b_T+s_{T+h-m(k+1)},\quad k=\lfloor(h-1)/m\rfloor$$

### The initialisation matters (and here it is exact)

With the first two cycles:

$$b_0=\frac{\bar y_{\text{2nd cycle}}-\bar y_{\text{1st cycle}}}{m},\qquad \ell_0=\bar y_{\text{1st cycle}}+b_0\,\frac{m-1}{2},\qquad s_j=y_j-\Big(\bar y_{\text{1st cycle}}+b_0\big(j-\tfrac{m-1}{2}\big)\Big)$$

**Why the shift by $(m-1)/2$.** The mean of the first cycle estimates the level **at the centre** of the cycle (time $(m-1)/2$), not at the end. Moving it along the trend to $t=m-1$ and removing both the level and the trend from the initial seasonality makes a line plus a zero-mean seasonality be **reproduced exactly**.

It was an error found precisely by the test: the first version used the cycle mean without the shift, and the exactness test (tolerance 0.05) failed. Now `HoltWintersExactInEveryFormat` verifies an error below $10^{-9}$ in `double` (tolerance $2\cdot10^{-3}$ in `float`) over 8 forecast steps.

## Relationships with other families

| Method | Equivalent to |
|---|---|
| SES | ARIMA(0,1,1), $\theta=\alpha-1$ (ch. 4) — and to the stationary Kalman of the local level (ch. 6) |
| Linear Holt | ARIMA(0,2,2) and Kalman local linear trend |
| Holt-Winters | ETS(A,A,A) model of the state-space framework (Hyndman et al., 2002) |

ETS and ARIMA are **complementary families**, not one the update of the other: they overlap only on the linear models.

## What it does not do

**Deterministic, not estimated** initialisations (ETS would estimate them as parameters); no multiplicative seasonality; no prediction intervals. See [ROADMAP](../../../ROADMAP.md).

← [4 · AR and ARIMA](04-ar-arima.md) · [Index](README.md) · [6 · Kalman filter →](06-kalman.md)
