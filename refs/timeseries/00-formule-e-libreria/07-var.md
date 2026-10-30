# 7 · VAR: several series, cross lags

Code: [VectorAutoregression.cs](../../../src/libs/MathemaSharp.TimeSeries/VectorAutoregression.cs).

## The model (Sims, 1980)

With $k$ series gathered in a vector $Y_t\in\mathbb R^k$:

$$Y_t=c+A_1Y_{t-1}+\dots+A_pY_{t-p}+\varepsilon_t,\qquad \varepsilon_t\sim(0,\Sigma)$$

The element $A_i[r,s]$ measures the effect of series $s$ at lag $i$ on series $r$: **each series depends on its own lags and on those of the others**. Sims used it in macroeconomics as a flagship application: the VAR is not his invention.

For $k=2,\;p=1$: $\;x_t=c_x+a_{11}x_{t-1}+a_{12}y_{t-1}+\varepsilon^x_t$, $\;y_t=c_y+a_{21}x_{t-1}+a_{22}y_{t-1}+\varepsilon^y_t$.

## Estimation

**OLS equation by equation.** The $k$ equations have the **same regressors**: in this case the single OLS coincides with Zellner's GLS/SUR estimator, so no efficiency is lost by estimating them separately. The regressor matrix has $1+kp$ columns:

$$X_t=\big(1,\;Y_{t-1}^\top,\;\dots,\;Y_{t-p}^\top\big),\qquad \text{rows }t=p+1..n$$

Each equation is solved with the Householder QR (ch. 10). The covariance of the residuals uses the divisor $n-p-(1+kp)$ (degrees of freedom):

$$\hat\Sigma=\frac{1}{n-p-(1+kp)}\,\hat E^\top\hat E$$

Verification `FitRecoversBivariateVar1`: 20 000 points, $|\hat A-A|<0.02$ for each element, and $\hat\Sigma\approx I$ (off-diagonal covariance zero within 0.05).

## Stability and stationary mean

A VAR($1$) is **stationary** if all the eigenvalues of $A_1$ lie inside the unit circle. For $2\times2$, with $\operatorname{tr}$ and $\det$:

$$\lambda_{\pm}=\frac{\operatorname{tr}\pm\sqrt{\operatorname{tr}^2-4\det}}{2}$$

- discriminant $\ge0$: real eigenvalues, check $|\lambda_\pm|<1$;
- discriminant $<0$: complex conjugates with $|\lambda|^2=\det$, so the condition is $\det<1$ (no complex root to compute).

The **stationary mean** is $\mu=(I-A_1)^{-1}c$. The recursive forecast converges to it:

$$\hat Y_{T+h}=c+\sum_{i=1}^{p}A_i\hat Y_{T+h-i}\;\xrightarrow{h\to\infty}\;\mu$$

Verification `ForecastConvergesToStationaryMean`: with $A=\begin{pmatrix}0.5&0.2\\-0.1&0.3\end{pmatrix}$, $c=(1,1)$ one has $\det(I-A)=0.37$ and $\mu=\big(\tfrac{0.9}{0.37},\tfrac{0.4}{0.37}\big)$, reached to $10^{-9}$.

## The impulse response

A unit shock on series $j$ at time 0 produces at time $h$ the deviation $A_1^h e_j$ from the reference trajectory. The notebook [08](../../../notebooks/TimeSeries/08-ts-arima-var-garch.ipynb) obtains it by subtracting two forecasts (with and without the shock). It shows **how a perturbation on one series propagates to the other**: for a plant, "if the pressure rises, how much and when does the temperature rise?".

## What it does not do

No criteria for choosing the order $p$ (AIC/BIC), no Granger causality tests, no orthogonalised impulses (Cholesky factor of $\Sigma$), no cointegration (VECM). `IsStableOrder1Bivariate` covers only the bivariate VAR(1): for higher orders the eigenvalues of the companion matrix would be needed, and **`np.linalg.eigvals` does not even exist in PySharp's NumPy**.

← [6 · Kalman filter](06-kalman.md) · [Index](README.md) · [8 · ARCH and GARCH →](08-garch.md)
