# 8 · ARCH and GARCH: modelling the variance, not the mean

Code: [Garch.cs](../../../src/libs/MathemaSharp.TimeSeries/Garch.cs).

## The problem

The models of the previous chapters describe the conditional **mean**. Many series (financial returns, but also the vibrations of a machine) have an almost constant mean and a **volatility that changes**: calm periods and agitated periods, in clusters (*volatility clustering*). The shocks $\varepsilon_t$ are almost uncorrelated, but their **squares** are not: this shows in the ACF of $\varepsilon_t^2$.

## ARCH (Engle, 1982) and GARCH (Bollerslev, 1986)

$$\varepsilon_t=\sigma_t\,z_t,\;\;z_t\sim\mathcal N(0,1)\qquad\sigma_t^2=\omega+\alpha\,\varepsilon_{t-1}^2+\beta\,\sigma_{t-1}^2$$

- $\omega>0$: base level of the variance;
- $\alpha$: reaction to yesterday's shock (*ARCH*);
- $\beta$: memory of yesterday's variance (*GARCH*);
- **ARCH(1) is the case $\beta=0$.**

**Covariance stationarity**: $\alpha+\beta<1$ (the **persistence**). Then the variance returns to

$$\bar\sigma^2=\frac{\omega}{1-\alpha-\beta}$$

The mean is not modelled: the $\varepsilon$ are assumed zero-mean (residuals of another model, or centred data).

```csharp
variances[0] = MeanSquare(shocks);      // σ²₀ = mean of ε² (conventional choice)
variances[t] = Omega + Alpha * shocks[t - 1] * shocks[t - 1] + Beta * variances[t - 1];
```

## Variance forecast

Since $E[\varepsilon_t^2\mid\mathcal F_{t-1}]=\sigma_t^2$, the $h$-step forecast moves towards $\bar\sigma^2$ with **geometric decay of ratio $\alpha+\beta$**:

$$\hat\sigma^2_{T+1}=\omega+\alpha\,\varepsilon_T^2+\beta\,\sigma_T^2,\qquad \hat\sigma^2_{T+h}=\bar\sigma^2+(\alpha+\beta)^{h-1}\big(\hat\sigma^2_{T+1}-\bar\sigma^2\big)$$

The **persistence** measures how long a shock lasts: with $\alpha+\beta=0.95$ the effect halves in $\ln0.5/\ln0.95\approx13.5$ steps.

Verification `VarianceForecastConvergesToUnconditional`: with $(\omega,\alpha,\beta)=(0.1,0.1,0.8)$ one gets $\bar\sigma^2=1$, and $(\hat\sigma^2_{T+3}-1)/(\hat\sigma^2_{T+2}-1)=0.9$ to $10^{-12}$. With persistence 0.95 ($0.95^h$) about 900 steps are needed to reach $10^{-9}$: `GarchAndVarInDecimal128` shows it.

## Estimation: maximum likelihood with *variance targeting*

The Gaussian log-likelihood is

$$\ln L(\omega,\alpha,\beta)=-\tfrac12\sum_{t}\Big(\ln 2\pi+\ln\sigma_t^2+\frac{\varepsilon_t^2}{\sigma_t^2}\Big)$$

Three parameters are many for a naive search. **Variance targeting** fixes the unconditional variance to the sample one $s^2$:

$$\omega=s^2\,(1-\alpha-\beta)$$

and the problem reduces to two parameters $(\alpha,\beta)$ in the triangle $\alpha>0,\,\beta\ge0,\,\alpha+\beta<1$. The library searches for the maximum on a **grid** (step $1/40$), then **refines** it around the best (step ÷10, 3 times). It is robust and simple; **it is not efficient** (a Newton/BFGS would be faster) and gives no standard errors.

Verification `FitRecoversSimulatedParameters`: on 10 000 points simulated with $(0.05,0.10,0.85)$ the estimate has $\hat\alpha\in[0.07,0.13]$ and persistence in $[0.92,0.98]$.

## What it does not do

Only GARCH(1,1) with Gaussian innovations: no GARCH($p,q$), fat-tailed distributions (Student-$t$), leverage effects (GJR, EGARCH), component in the mean (ARMA-GARCH). See [ROADMAP](../../../ROADMAP.md).

← [7 · VAR](07-var.md) · [Index](README.md) · [9 · LOESS and STL →](09-stl-loess.md)
