# 4 · AR and ARIMA: from Yule to Box-Jenkins

Code: [Arima.cs](../../../src/libs/MathemaSharp.TimeSeries/Arima.cs) (`ArModel`, `ArimaModel`).

## AR($p$): Yule's idea (1927)

$$y_t-\mu=\sum_{i=1}^{p}\phi_i\,(y_{t-i}-\mu)+\varepsilon_t,\qquad \varepsilon_t\sim\text{white noise}(0,\sigma^2)$$

Yule modelled sunspots as an AR(2) driven by random shocks: a **cycle without sinusoids**, only dynamics.

**Stationarity.** The process is stationary if the roots of $1-\phi_1z-\dots-\phi_pz^p$ are **outside** the unit circle. The Yule-Walker estimate (ch. 2) guarantees it.

**The cycle in the roots (AR(2)).** With $\phi_1^2+4\phi_2<0$ the roots are complex conjugates: the ACF is a damped sinusoid of period

$$P=\frac{2\pi}{\theta},\qquad \cos\theta=\frac{\phi_1}{2\sqrt{-\phi_2}}$$

With $\phi=(1.6,-0.9)$: $\cos\theta=0.843$, $\theta=0.567$, $P\approx11.07$ steps.

**Forecast.** Recursive: the unknown future values are replaced by their forecasts.

$$\hat y_{T+h}=\mu+\sum_{i=1}^{p}\phi_i\,(\hat y_{T+h-i}-\mu)\;\xrightarrow{h\to\infty}\;\mu$$

```csharp
var value = Mean;
for (var i = 0; i < Phi.Length; i++) value += Phi[i] * (buffer[^(i + 1)] - Mean);
buffer.Add(value);
```

Verification: `YuleWalkerRecoversAr2` (20 000 points: $\hat\phi=(0.50,-0.30)\pm0.02$), `ArForecastDecaysToMean`.

## Wold (1938): a theorem, not a model

Every zero-mean stationary process can be written

$$y_t=\sum_{j=0}^{\infty}\psi_j\,\varepsilon_{t-j}+\text{deterministic part},\qquad \psi_0=1$$

The finite AR is a parsimonious approximation of it. The weights $\psi_j$ (the response to a unit impulse) are computed from $\phi$ and $\theta$:

$$\psi_j=\theta_j+\sum_{i=1}^{\min(j,p)}\phi_i\,\psi_{j-i}\qquad(\theta_j=0\text{ for }j>q)$$

For ARMA(1,1): $\psi_1=\phi+\theta$, then geometric decay with ratio $\phi$. The notebook [08](../../../notebooks/TimeSeries/08-ts-arima-var-garch.ipynb) plots them.

## ARIMA($p,d,q$): Box-Jenkins (1970)

One differences $d$ times to remove the non-stationarity, $w=\Delta^d y$, then fits an ARMA:

$$w_t=c+\sum_{i=1}^{p}\phi_i\,w_{t-i}+e_t+\sum_{j=1}^{q}\theta_j\,e_{t-j}$$

> **MA = lagged innovations, not a moving average.** The term $\theta_j e_{t-j}$ is the *shock* of $j$ steps ago, an unobserved quantity, not an average of the data.

**Residuals (CSS).** With $e_t=0$ for $t<p$, then $e_t=w_t-\hat w_{t|t-1}$. `Residuals` returns $n-d$ values.

**Forecast.** $w$ is extended with the ARMA recursion (future innovations $=0$) and **integrated** $d$ times starting from the last value of each level:

```csharp
for (var k = D - 1; k >= 0; k--)
{
    var last = levels[k][^1];
    for (var h = 0; h < horizon; h++) forecast[h] = last += forecast[h];
}
```

Limit cases verified: ARIMA(0,1,0) without constant = **naive**; ARIMA(0,2,0) = continuation of the last slope (`Arima010IsNaiveAndArima020ExtrapolatesLine`).

## Estimation: Hannan-Rissanen (1982)

The MA term contains *unobserved* innovations: direct regression is not possible. Two linear steps:

1. **Long AR** (order $m=\max(p+q,\lceil10\log_{10}n\rceil)$) estimated by OLS: its residuals are the estimated innovations $\hat e_t$.
2. **OLS of $w_t$** on $[1,\,w_{t-1..p},\,\hat e_{t-1..q}]$.

Each regression uses the **Householder QR** (ch. 10), not the normal equations. The innovation variance is the sum of squared residuals divided by the degrees of freedom.

**What it is NOT.** It is not exact maximum likelihood: it is an **excellent initial estimator**, consistent but less efficient. Stationarity and invertibility are **not imposed**. On 20 000 points it recovers ARMA(1,1) $\phi=0.6,\theta=0.4$ within $\pm0.04$ (`HannanRissanenRecoversArma11`).

**Choice of the order.** Approximate AIC $n\ln\hat\sigma^2+2k$ plus Ljung-Box on the residuals (ch. 2). The notebook [08](../../../notebooks/TimeSeries/08-ts-arima-var-garch.ipynb) compares five orders.

## The identity that links ARIMA to SES

The SES $\ell_t=\alpha y_t+(1-\alpha)\ell_{t-1}$ can be rewritten, with the one-step error $e_t=y_t-\ell_{t-1}$, as

$$\ell_t=\ell_{t-1}+\alpha e_t\quad\Longrightarrow\quad\hat y_{t+1}=y_t-(1-\alpha)\,e_t$$

which is an ARIMA(0,1,1) $\;y_{t+1}=y_t+e_{t+1}+\theta e_t\;$ with **$\theta=\alpha-1$**. The forecasts coincide (same values to $10^{-9}$ in `double`, in every format with an adequate tolerance): `SesEqualsArima011InEveryFormat`.

← [3 · Baselines and metrics](03-riferimenti-e-metriche.md) · [Index](README.md) · [5 · Exponential smoothing →](05-smoothing.md)
