# 9 · LOESS and STL: decompose first, forecast later

Code: [Stl.cs](../../../src/libs/MathemaSharp.TimeSeries/Stl.cs).

## LOESS: a local line

For each point $x$ a **weighted line** is fitted to the $q$ nearest points; the estimated value is the value of the line at $x$. The weights are **tricube**:

$$w_i=\Big(1-\Big(\frac{|x_i-x|}{h}\Big)^{3}\Big)^{3}\;\text{ for }|x_i-x|<h,\qquad 0\text{ otherwise}$$

where $h$ is the distance of the $q$-th neighbour. With the weights $w_i$ (multiplied by the robustness weights) the weighted least-squares line has

$$\bar x=\frac{\sum w_ix_i}{\sum w_i},\quad\bar y=\frac{\sum w_iy_i}{\sum w_i},\quad \text{slope}=\frac{\sum w_ix_iy_i/\sum w_i-\bar x\bar y}{\sum w_ix_i^2/\sum w_i-\bar x^2},\quad \hat y(x)=\bar y+\text{slope}\,(x-\bar x)$$

```csharp
var slope = variance > C<T>(1e-12) * T.Max(T.One, meanX * meanX)
    ? (swxy / sw - meanX * meanY) / variance : T.Zero;     // degree 0 if the weighted points are degenerate
return meanY + slope * (x - meanX);
```

**Property used by the tests**: a line is reproduced *exactly*, even when extrapolating outside the interval (`LoessReproducesLinesEvenWhenExtrapolating`). If $q>n$, $h$ grows by $(q-n)/2$; the margin $h/0.999$ (as in the original Fortran) gives the points on the edge an almost zero weight, not exactly zero.

## STL (Cleveland, Cleveland, McRae, Terpenning, 1990)

Decomposes additively into **trend** $T_t$, **seasonality** $S_t$ and **remainder** $R_t$:

$$y_t=T_t+S_t+R_t$$

(the remainder is by definition $y-T-S$, so the sum is exact). An **inner loop**, repeated $n_i$ times, with period $n_p$:

| Step | Operation |
|---|---|
| 1 | remove the trend: $y_t-T_t$ |
| 2 | **cycle-subseries**: for each phase $j$ (all the "Januaries", all the "Februaries"…) a LOESS of width $n_s$, extended by **one cycle before and one after** → series $C$ of length $n+2n_p$ |
| 3 | **low-pass** of $C$: moving averages MA($n_p$), MA($n_p$), MA(3), then LOESS($n_l$) → $L$ of length $n$ |
| 4 | seasonality $S=C-L$ (the slow level that the low-pass captured is removed from $C$) |
| 5 | deseasonalise: $y_t-S_t$ |
| 6 | **trend** = LOESS($n_t$) of the deseasonalised series |

Parameters: $n_s$ **odd** (≥ 7 recommended): larger ⇒ seasonality more stable over time; $n_t$ is the smallest odd number $\ge\dfrac{1.5\,n_p}{1-1.5/n_s}$; $n_l$ is the smallest odd number $\ge n_p$.

**Why the three moving averages.** A moving average one period long cancels any zero-mean seasonality of period $n_p$; applied twice and then with MA(3) it makes the low-pass very smooth and shortens $C$ from $n+2n_p$ to $n$ ($-(n_p-1)-(n_p-1)-2=-2n_p$).

### Exactness

On $y_t=a+bt+s_{t\bmod n_p}$ with $\sum s=0$: the LOESS reproduce lines and the one-period MA cancels $s$, so STL separates trend and seasonality **exactly** (to $10^{-9}$ in `double`, $5\cdot10^{-3}$ in `float`; `StlExactInEveryFormat`).

## Robustness (outer loop)

After the inner loop the **bisquare weights** are computed

$$\rho_t=B\!\left(\frac{|R_t|}{6\,\operatorname{median}|R|}\right),\qquad B(u)=\begin{cases}(1-u^2)^2&u<1\\0&u\ge1\end{cases}$$

which enter as weights in the LOESS of steps 2 and 6 of the following iterations. An **outlier** (large $|R_t|$) has $\rho_t\approx0$ and no longer drags trend and seasonality: it ends up in the remainder. Verification `RobustIterationsIsolateOutlier`: with a +30 spike the robust seasonality errs 3 times less than the non-robust one, the remainder at $t=60$ stays $>25$ and its weight $<0.01$.

With robustness, $n_i=1$ is enough.

## Decompose first, forecast later

STL does not forecast. The classic scheme: deseasonalised trend → Holt with damping; seasonality → seasonal naive on the $S$ component; forecast = sum. The notebook [09](../../../notebooks/TimeSeries/09-ts-stl-ndsharp.ipynb) compares it with Holt-Winters on synthetic data with two outliers; the result is honest: in the case shown it does **not** beat Holt-Winters.

## What it does not do

Faithful to the *algorithm*, not to the original Fortran: no evaluation *jump* (the LOESS is evaluated at every point), no degree-0 LOESS, no multiplicative seasonality (use the logarithm upstream). No multiple seasonality (MSTL).

← [8 · ARCH and GARCH](08-garch.md) · [Index](README.md) · [10 · Algebra and simulation →](10-algebra-e-simulazione.md)
