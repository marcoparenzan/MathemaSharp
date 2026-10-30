# 10 · Linear algebra, simulation and the facade for PySharp

Code: [MatrixAlgebra.cs](../../../src/libs/MathemaSharp.TimeSeries/MatrixAlgebra.cs), [Simulation.cs](../../../src/libs/MathemaSharp.TimeSeries/Simulation.cs), [Interop.cs](../../../src/libs/MathemaSharp.TimeSeries/Interop.cs).

## Least squares with Householder QR

One looks for $\hat\beta=\arg\min\|X\beta-y\|_2$ with $X\in\mathbb R^{m\times n},\,m\ge n$. The "textbook" way is the **normal equations** $X^\top X\beta=X^\top y$, but $\kappa(X^\top X)=\kappa(X)^2$: the **digits lost are doubled**. The QR works on $X$ and loses only $\kappa(X)\,\varepsilon$.

**Householder reflection.** To zero column $k$ below the diagonal one uses $H=I-2\dfrac{vv^\top}{v^\top v}$ with

$$v=x-\alpha e_1,\qquad \alpha=-\operatorname{sign}(x_1)\,\|x\|_2$$

The sign opposite to $x_1$ avoids **cancellation** in $v_1=x_1-\alpha$. $H$ is applied to the following columns and to the right-hand side; at the end the triangular system is solved by back substitution.

```csharp
var alpha = a[k, k] > T.Zero ? -norm : norm;
a[k, k] -= alpha;                                   // v₁ = x₁ − α, without cancellation
...
var f = two * s / vNorm2;                           // H = I − 2vvᵀ/‖v‖²
```

**Linear dependence.** If the norm of a column's sub-block falls below $4096\,\varepsilon_{\text{mach}}\cdot\max(\text{scale},1)$ the columns are linearly dependent (to machine precision) and the library throws `ArgumentException` instead of dividing by a spurious number: it is what, with `float`, declares the $7\times7$ Hilbert matrix "singular" (ch. 1).

Verifications: `LeastSquaresRecoversExactLine` (exact line + rejection of dependent columns), `LeastSquaresInEveryFormat`.

It is used by: `ArimaModel.FitHannanRissanen` (two regressions), `VarModel.Fit` (one per equation), and is available directly.

## Simulation

Processes with **known** parameters are needed to verify the estimators. `Simulation` generates ARMA, random walks, GARCH and VAR.

**Normal noise (Box-Muller):**

$$z=\sqrt{-2\ln u_1}\,\cos(2\pi u_2),\qquad u_1=1-U_1,\;u_2=U_2$$

$1-U_1$ avoids $\ln0$. **Burn-in:** the first 200 values are discarded to forget the initial state. **With the same seed** different formats start from the same noise (the draws are always `double`, then converted): the difference between the results is only arithmetic. `Decimal128AgreesWithDoubleOnTheSameNoise` rests on this.

> **It is not NumPy's generator.** `System.Random` plus Box-Muller produces different streams from `numpy.random` (PCG64 with `Generator`): same seed, different numbers. To reproduce a series between C# and PySharp, the facade exposes `Simulate.Arma(seed, …)`, which calls the same generator.

## The facade for PySharp

PySharp (a Python interpreter in C#) loads .NET DLLs with `clr.AddReference`, but its reflection has two limits:

- **it cannot pass a `ReadOnlySpan<T>`** (a `ref struct` cannot be boxed): tried, `Statistics.Mean(...)` on a method with a span fails with "no overload matches";
- **it does not do type inference on generic methods** (limit declared in PySharp's README), so `SeriesStatistics.Acf(y, 20)` cannot be called as it is.

This is why the library has, next to the generic API:

1. **`T[]`** alongside `ReadOnlySpan<T>` in the models' instance methods (`ArModel<T>.Forecast`, `ArimaModel<T>.Residuals/Forecast`, `Garch11<T>.ConditionalVariances/LogLikelihood/ForecastVariance`, `KalmanFilter<T>.Filter`);
2. the namespace **`MathemaSharp.TimeSeries.Interop`**: **non-generic** static classes on `double[]` that delegate to the generic API with `T = double`:

| Class | Delegates to |
|---|---|
| `Statistics` | `SeriesStatistics`, `SpecialFunctions` |
| `Forecasts` | `ReferenceForecasts` |
| `Metrics` | `Accuracy` |
| `Smoothing` | `ExponentialSmoothing` |
| `Arima` | `ArModel`, `ArimaModel` |
| `Kalman` | `KalmanFilter` |
| `Var` | `VarModel` (matrices **flattened in row order**) |
| `Garch` | `Garch11` |
| `Decomposition` | `Stl` |
| `Linear` | `LinearAlgebra` |
| `Simulate` | `Simulation` (with the seed as an argument) |

The results are **simple classes** (`LevinsonResult`, `LjungBoxResult`, `KalmanResult`), not tuples: the properties can be read from Python as `r.Pacf`.

> The interop is verified with the real PySharp: Python lists → `double[]`, NumPy arrays → `x.to_clr()`, and the results come back into NumPy with `np.array(...)`.

For C# the generic API is preferable, which also accepts `float`, `Half` and `Decimal32/64/128`.

← [9 · LOESS and STL](09-stl-loess.md) · [Index](README.md) · [Part II — The library →](../01-libreria/README.md)
