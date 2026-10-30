# 2 · Basic statistics: autocorrelation, PACF, Ljung-Box

Code: [SeriesStatistics.cs](../../../src/libs/MathemaSharp.TimeSeries/SeriesStatistics.cs), [SpecialFunctions.cs](../../../src/libs/MathemaSharp.TimeSeries/SpecialFunctions.cs).

## Autocovariance and autocorrelation

$$\hat\gamma(k)=\frac1n\sum_{t=1}^{n-k}(y_t-\bar y)(y_{t+k}-\bar y),\qquad \hat\rho(k)=\frac{\hat\gamma(k)}{\hat\gamma(0)}$$

**Why the divisor is $n$ and not $n-k$.** The estimator with $n$ is biased, but the Toeplitz matrix $\Gamma_{ij}=\hat\gamma(|i-j|)$ that derives from it is **positive semi-definite**: every sequence of autocovariances is that of a real stationary process. With $n-k$ this guarantee is lost, and with it the one that follows on the PACF.

```csharp
for (var k = 0; k <= maxLag; k++)
{
    var sum = T.Zero;
    for (var t = 0; t + k < y.Length; t++) sum += (y[t] - mean) * (y[t + k] - mean);
    result[k] = sum / N<T>(y.Length);
}
```

Verification: `AcfStartsAtOneAndDifferenceShortens`. Constant series → $\hat\gamma(0)=0$ and the ACF is not defined (`ArgumentException`).

## PACF and the Levinson-Durbin recursion

The **PACF** at lag $k$ is the last coefficient $\phi_{kk}$ of the best AR($k$): the correlation between $y_t$ and $y_{t-k}$ with the influences of the intermediate lags **removed**. It is obtained by solving the Yule-Walker equations

$$\gamma(j)=\sum_{i=1}^{p}\phi_i\,\gamma(j-i),\qquad j=1..p$$

that is $\Gamma\phi=\gamma$. Inverting $\Gamma$ costs $O(p^3)$; the Toeplitz structure allows $O(p^2)$ with the recursion:

$$\kappa_k=\frac{\gamma(k)-\sum_{j=1}^{k-1}\phi_{k-1,j}\,\gamma(k-j)}{v_{k-1}},\qquad \phi_{k,k}=\kappa_k,$$
$$\phi_{k,j}=\phi_{k-1,j}-\kappa_k\,\phi_{k-1,k-j}\;(j<k),\qquad v_k=v_{k-1}\,(1-\kappa_k^2),\quad v_0=\gamma(0)$$

$\kappa_k$ is the **PACF at lag $k$**; $v_k$ is the innovation variance of the AR($k$).

```csharp
var kappa = acc / variance;                      // κ_k
phi[k - 1] = kappa;
for (var j = 1; j < k; j++) phi[j - 1] = previous[j - 1] - kappa * previous[k - j - 1];
variance *= T.One - kappa * kappa;               // v_k
```

**Why $|\kappa_k|\le1$.** From the semi-definiteness of $\Gamma$: $v_k\ge0$ for every $k$, hence $1-\kappa_k^2\ge0$. It also follows that **the estimated Yule-Walker AR is always stationary** (no root of the characteristic polynomial on or inside the unit circle).

**Signature of an AR($p$)**: decaying ACF, PACF that **vanishes** after lag $p$. On a theoretical AR(1) with $\phi=0.6$, $\gamma(k)\propto0.6^k$: $\kappa_1=0.6$, $\kappa_{k>1}=0$, $v_1=1-0.36=0.64$.

Verifications: `LevinsonDurbinOnTheoreticalAr1Acf`, `LevinsonDurbinMatchesDirectYuleWalkerSolve` (comparison with the direct $O(p^3)$ solution via QR), `LevinsonDurbinInEveryFormat`.

## Differences

$$\Delta_m y_t=y_t-y_{t-m}$$

With $m=1$ it removes a linear trend, with $m$ equal to the period it removes a seasonality. It returns $n-m$ values.

> **Beware of NumPy.** `np.diff(y, n=2)` is the *second-order* difference ($\Delta\Delta y$), not the one at distance 2: the seasonal difference in NumPy is written `y[m:] - y[:-m]`.

## Ljung-Box test

If the residuals of a model are white noise, their autocorrelations are close to zero. The test sums them:

$$Q=n(n+2)\sum_{k=1}^{h}\frac{\hat\rho_k^{2}}{n-k}\;\sim\;\chi^2_{\,h-p-q}\quad\text{(approximately)}$$

with $h$ lags and $p+q$ estimated parameters. **Small p-value ⇒ structure remains in the residuals**: the model has not captured all the dynamics. The p-value is $P(\chi^2_{\nu}\ge Q)=Q(\nu/2,\,Q/2)$, with $Q(a,x)$ the regularised upper incomplete gamma.

Verification: `LjungBoxSeparatesWhiteNoiseFromAr`: white noise → $p>0.05$; AR(1) with $\phi=0.7$ → $p<10^{-10}$.

## Special functions (for the p-value)

**$\ln\Gamma$ with Lanczos** ($g=7$, 9 coefficients):

$$\ln\Gamma(x+1)\approx\tfrac12\ln(2\pi)+\left(x+\tfrac12\right)\ln t-t+\ln A_g(x),\quad t=x+g+\tfrac12$$

with $A_g(x)=c_0+\sum_{i=1}^{8}\frac{c_i}{x+i}$. Relative error ~$10^{-15}$. For $x<\tfrac12$ one uses $\Gamma(x)=\Gamma(x+1)/x$.

**Upper incomplete gamma** $Q(a,x)=\Gamma(a,x)/\Gamma(a)$:

- for $x<a+1$, **series** $P(a,x)=\dfrac{e^{-x}x^a}{\Gamma(a)}\displaystyle\sum_{n\ge0}\frac{x^n}{a(a+1)\cdots(a+n)}$ and $Q=1-P$;
- for $x\ge a+1$, **continued fraction** (Lentz algorithm) $Q=\dfrac{e^{-x}x^a}{\Gamma(a)}\cdot\cfrac{1}{x+1-a-\cfrac{1(1-a)}{x+3-a-\cfrac{2(2-a)}{x+5-a-\cdots}}}$.

Each converges quickly in its own region. When the term falls below $\max(\varepsilon_{\text{mach}},10^{-16})\cdot|\text{sum}|$ it stops: beyond that it makes no sense, because the Lanczos constants are worth ~$10^{-16}$.

Checks with known values, in every format (`ChiSquareSurvivalInEveryFormat`): $\chi^2_2$: $P(X\ge x)=e^{-x/2}$ exact; $\chi^2_1$ at 95%: $x=3.841458820694124$.

← [1 · Generic Math](01-generic-math.md) · [Index](README.md) · [3 · Baselines and metrics →](03-riferimenti-e-metriche.md)
