# 6 · The Kalman filter

Code: [KalmanFilter.cs](../../../src/libs/MathemaSharp.TimeSeries/KalmanFilter.cs).

## The model

A hidden state $x_t$ evolves linearly and is observed only through a noisy measurement:

$$x_t=F\,x_{t-1}+w_t,\;\;w_t\sim\mathcal N(0,Q)\qquad y_t=H\,x_t+v_t,\;\;v_t\sim\mathcal N(0,R)$$

The library handles the case of **scalar observation**: $H$ is a row vector, $y_t$ and $R$ numbers. Then the innovation has a scalar variance and **no matrix needs to be inverted**.

## Predict and Update

**Predict** (propagates state and uncertainty):

$$\hat x^-=F\hat x,\qquad P^-=F\,P\,F^\top+Q,\qquad S=H P^-H^\top+R$$

`Predict()` returns the forecast of $y$, $H\hat x^-$, and its variance $S$.

**Update** (corrects with the observation $y$):

$$v=y-H\hat x^-,\qquad K=\frac{P^-H^\top}{S},\qquad \hat x=\hat x^-+K\,v$$

### The covariance in Joseph form

$$P=(I-KH)\,P^-\,(I-KH)^\top+K\,R\,K^\top$$

It is algebraically equal to the better-known $P=(I-KH)P^-$, but it **stays symmetric and positive semi-definite** even with rounding; the short form can lose the symmetry and produce negative variances. Verification `JosephCovarianceStaysSymmetric` (*exact* equality $P_{01}=P_{10}$).

```csharp
var joseph = Multiply(Multiply(a, Covariance), Transpose(a));     // (I−KH) P⁻ (I−KH)ᵀ
for (...) joseph[i, j] += Gain[i] * _r * Gain[j];                 // + K R Kᵀ
```

### Missing data

A `NaN` is a missing datum: **the update is skipped**. The state remains the forecast alone and the uncertainty grows by $Q$ at every empty step. This is the advantage of the state model over "recipe" smoothing (ch. 5): no artifice for the gaps. Verification `MissingValuesSkipUpdateAndGrowUncertainty`: during the gaps the gain is 0 and $S_t$ grows.

### Log-likelihood

By the prediction-error decomposition:

$$\ln L=-\tfrac12\sum_t\Big(\ln(2\pi S_t)+\frac{v_t^2}{S_t}\Big)$$

It is what is **maximised to estimate $Q$ and $R$**. `Filter` returns it (steps with a missing datum do not contribute).

## Local level and local linear trend

**Local level** (a random walk observed with noise): $F=1$, $H=1$, $Q=q$, $R=r$.

$$\ell_t=\ell_{t-1}+\eta_t,\qquad y_t=\ell_t+\varepsilon_t$$

**Local linear trend**: state $(\ell,b)$, $F=\begin{pmatrix}1&1\\0&1\end{pmatrix}$, $H=(1\;0)$, $Q=\mathrm{diag}(q_\ell,q_b)$. It is the state counterpart of Holt's method.

## The steady-state gain and the equivalence with SES

In the local level, the prior variance $P^-$ stabilises after many steps. With $P^+=P^-r/(P^-+r)$ (variance after the update) and $P^-=P^++q$:

$$P^-=\frac{P^-r}{P^-+r}+q\;\Longrightarrow\;(P^-)^2-q\,P^--q\,r=0$$

(scalar **Riccati** equation). The positive root and the gain:

$$P^*=\frac{q+\sqrt{q^2+4qr}}{2},\qquad K^*=\frac{P^*}{P^*+r}$$

$K^*$ depends **only on the signal-to-noise ratio $q/r$**. Now the level update is

$$\hat\ell=\hat\ell^-+K\,(y-\hat\ell^-)=K\,y+(1-K)\hat\ell^-$$

that is **exactly one line of SES with $\alpha=K^*$**. Therefore:

> **SES = stationary Kalman filter of the local level**, and estimating $\alpha$ by minimising the SSE is equivalent to estimating $q/r$.

With $q=0.5,\;r=2$: $q/r=0.25$, $K^*=0.3904$.

Verifications:

- `LocalLevelGainConvergesToRiccatiSolution`: the filter's gain converges to $K^*$ to $10^{-12}$; the first step, with a diffuse prior ($P_0=10^6$), is $K\approx1$;
- `SteadyStateKalmanIsSes`: after the transient the one-step forecasts of Kalman and of SES($\alpha=K^*$) coincide to $10^{-9}$;
- `KalmanSteadyStateGainInEveryFormat`.

In the notebook [07](../../../notebooks/TimeSeries/07-ts-smoothing-kalman.ipynb) the $\hat\alpha$ estimated with the golden section (0.4301, from data with $q/r=0.25$) is close to $K^*=0.3904$: same order, the difference due to the finite sample.

## What it does not do

Only scalar observation; no *smoother* (Rauch-Tung-Striebel); $Q$ and $R$ **are not estimated** (they are passed in); no non-linear filter (EKF, UKF, particles). See [ROADMAP](../../../ROADMAP.md).

← [5 · Exponential smoothing](05-smoothing.md) · [Index](README.md) · [7 · VAR →](07-var.md)
