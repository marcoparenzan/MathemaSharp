# 3 · Overlap with NumPy in PySharp

In PySharp `import numpy as np` **is not CPython's NumPy** (no C extensions): it is a C# module on top of **NDSharp**, a native .NET `ndarray` library. It covers a broad but not complete subset of the API. This page says, function by function, **where the Numbers library and NumPy do the same thing**, where not, and where one or the other is missing.

> **Verified, not assumed.** The names available in PySharp were queried with `hasattr(np, …)` on the real kernel (JupyterNet, .NET 11 RC2). The numerical coincidences are in the notebook [PySharp 10](../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb), section 9.

## A · Where they coincide

| Operation | `MathemaSharp.TimeSeries` library | NumPy in PySharp | Verification |
|---|---|---|---|
| Mean | `Statistics.Mean` | `np.mean` | identical |
| Autocovariance (divisor $n$) | `Statistics.Autocovariance` | `np.sum(xc[:n-k]*xc[k:])/n`, with `xc = x - np.mean(x)` | max difference **4.4·10⁻¹⁶** |
| Yule-Walker | `Arima.FitYuleWalker` (Levinson-Durbin, $O(p^2)$) | `np.linalg.solve(Γ, γ)` ($O(p^3)$) | max difference **3.3·10⁻¹⁶** |
| First difference | `Statistics.Difference(y, 1)` | `np.diff(y)` | identical (0) |
| Quantiles | `Statistics.Quantile` | `np.quantile`, `np.percentile` | identical (0): same linear interpolation |
| Seasonal difference | `Statistics.Difference(y, m)` | `y[m:] - y[:-m]` | identical (0) |
| Cumulative sum (integrating the differences, ARIMA with $d\ge1$) | internal to `ArimaModel.Forecast` | `np.cumsum` | |
| Matrix product | `MatrixAlgebra.Multiply` | `np.dot`, `np.matmul` | |
| Least squares | `MatrixAlgebra.LeastSquares` (Householder QR) | `np.linalg.lstsq` | same problem; NDSharp's internal algorithm was not verified |
| Fourier transform | `Signals.Dft` ($O(N^2)$, **didactic**) | `np.fft.fft` | same values; for large $N$ use `np.fft` |
| Moving average (STL's low-pass filter) | internal to `Stl` | `np.convolve(x, np.ones(m)/m, 'valid')` | |
| Median (STL robustness weights) | internal to `Stl` | `np.median` | |
| Elementary functions | `T.Exp`, `T.Log`, `T.Sqrt`, `T.Cos`… | `np.exp`, `np.log`, `np.sqrt`, `np.cos`… | |

> **Trap: `np.diff(y, 2)` is not the difference at distance 2.** NumPy computes the *second-order* difference ($\Delta\Delta y$): `np.diff(np.array([1,2,4,7]), 2)` gives `[1, 1]`. `Statistics.Difference(y, 2)` is $y_t-y_{t-2}$. For the seasonal difference in NumPy use slices.

## B · Where the library adds (NumPy does not have it)

| Library | In Python it would live in |
|---|---|
| ACF, PACF, Levinson-Durbin, Ljung-Box, `ChiSquareSurvival` | `statsmodels.tsa` / `scipy.stats` |
| Baseline forecasts, MAE/RMSE/MAPE/**MASE** | `sktime`, `statsforecast` |
| SES, Holt, damped, Holt-Winters | `statsmodels.tsa.holtwinters` |
| Kalman filter, local level | `statsmodels.tsa.statespace` |
| ARIMA (Hannan-Rissanen), AR (Yule-Walker) | `statsmodels.tsa.arima` |
| VAR | `statsmodels.tsa.vector_ar` |
| GARCH(1,1) | `arch` |
| STL, LOESS | `statsmodels.tsa.seasonal.STL` |
| Pinball loss, interval coverage (`PinballLoss`, `IntervalCoverage`) | `properscoring`, `sktime.performance_metrics` |
| Computing in `Decimal32/64/128`, `float`, `Half` | — (NumPy has no IEEE decimal dtypes) |

Neither `statsmodels` nor `arch` exist in PySharp: **here the Numbers library is the only way** to do these things from Python, via `clr`.

## C · Where NumPy has what the library does not

Present in PySharp, absent from the library:

| NumPy | Typical use in time series |
|---|---|
| `np.std`, `np.var`, `np.cov`, `np.corrcoef` | descriptive statistics |
| `np.linalg.inv`, `pinv`, `svd`, `det`, `eigh`, `eigvalsh` | general linear algebra |
| `np.fft.rfft`, `ifft`, `fftfreq` | spectrum, frequency-domain filters |
| `np.random.default_rng`, `randn`, `normal`, `choice` | simulation, bootstrap |
| `np.sort`, `argsort`, `roll`, `where` | manipulation |

**Absent from both** (verified on PySharp): `np.correlate`, `np.polyfit`, `np.polyval`, `np.linalg.eig`, `np.linalg.eigvals`, `np.linalg.qr`, `np.linalg.cholesky`, `np.tril`, `np.triu`, `np.kron`, `np.searchsorted`. Concrete consequences:

- **Autocovariance with `np.correlate`** is not possible: it is written by hand (row of table A) or the library is used.
- **Stability of a VAR($p$)**: the eigenvalues of a *non-symmetric* matrix would be needed (`np.linalg.eigvals`): it is neither here nor in PySharp's NumPy. The library covers only the $2\times2$ VAR(1) in analytic form.
- **Cholesky factorisation** (for orthogonalised impulses) is missing everywhere.

## D · Differences that change the numbers

| Aspect | Library | NumPy in PySharp |
|---|---|---|
| Random generator | `System.Random` + Box-Muller | PCG64 (`default_rng`) |
| Consequence | **the same seed gives different numbers**: to reproduce a series between C# and PySharp use `Simulate.Arma(seed, …)`, not `np.random` | |
| Covariance divisor | $n$ (autocovariance) | `np.cov`: $n-1$ by default (`bias=True` → $n$) |
| Formats | `float`, `double`, `Half`, `Decimal32/64/128` | `float32`, `float64`, integers, complex; **no decimal** |
| Matrix layout | `T[,]` (or flattened in `Interop`) | `ndarray` with strides and views |

## E · How to use them together

The recommended flow: **NumPy to prepare and inspect, the library for the mathematics of time series.**

```python
import numpy as np

x = np.array(sensor_readings)            # NDArray float64
x = x[~np.isnan(x)]                      # cleaning with NumPy
d = np.diff(x)                           # exploration with NumPy

acf  = np.array(Statistics.Acf(x.to_clr(), 20))     # library: x.to_clr() → double[]; the result comes back into NumPy
model = Arima.FitYuleWalker(x.to_clr(), 2)
fc   = np.array(model.Forecast(x.to_clr(), 12))
```

- From NumPy to the library: **`x.to_clr()`** produces a `double[]` (copy); a Python list works as well.
- From the library to NumPy: **`np.array(clr_array)`**.
- From C#, the same exchange goes through the [`MathemaSharp.NDSharp`](02-uso-e-funzioni.md#mathemasharpndsharp--the-bridge) bridge: `ToVector()`, `ToNDArray()`.

← [2 · Use and functions](02-uso-e-funzioni.md) · [Part II](README.md) · [4 · Examples →](04-esempi.md)
