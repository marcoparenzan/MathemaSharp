# 4 · Examples in C# and in PySharp

Two **twin** notebooks do the same operations on the same data (simulated AR(2) with seed 2026):

| | Notebook | How it uses the library |
|---|---|---|
| **C#** | [`notebooks/TimeSeries/10-ts-libreria-tour.ipynb`](../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb) | **generic** API, `T = double` (plus a section with `float` and `Decimal128`) |
| **PySharp** | [`notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb`](../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb) | **`Interop`** facade via `clr.AddReference` (plus a comparison section with NumPy) |

Every `label: values` line printed by one appears **identical** in the other (27 labels). The test `LibraryTourNotebooksAgreeAcrossLanguages` guarantees it.

Run: `pwsh scripts/run-notebooks.ps1 -Filter '10-*'`.

## C#: the generic API

```csharp
#load "shared/MathemaSharp.csx"
using MathemaSharp.TimeSeries;

var y = Simulation.Arma<double>(new Random(2026), 600, 0, new[] { 0.6, -0.2 }, Array.Empty<double>(), 1);
double[] train = y[..580], test = y[580..];

var acf = SeriesStatistics.Acf(train, 5);                       // T inferred from double[]
var ar  = ArModel.FitYuleWalker(train, 2);                      // ArModel<double>
Console.WriteLine(string.Join(" ", ar.Phi));                    // 0.599503 -0.200752
var f   = ar.Forecast(train, 5);

var alpha = ExponentialSmoothing.FitSesAlpha(train);
var holt  = ExponentialSmoothing.Holt(train, 0.4, 0.2, 5, 0.9); // damped, φ = 0.9

var kf = KalmanFilter.LocalLevel(0.1, 1.0, train[0], 10.0);
var kr = kf.Filter(train[..100]);                               // kr.FirstGain, kr.LogLikelihood
```

### Changing format = changing `T`

```csharp
using System.Numerics;
var pacf128 = SeriesStatistics.Pacf(train.Select(Decimal128.CreateChecked).ToArray(), 1)[0];   // Decimal128
var pacf32  = SeriesStatistics.Pacf(train.Select(float.CreateChecked).ToArray(), 1)[0];        // float
```

Same data, same algorithm, results measured in section 9 of the C# notebook:

| Format | PACF at lag 1 | Deviation from `double` |
|---|---|---|
| `float` | 0.4992733896 | 1.4·10⁻⁷ (seventh digit) |
| `double` | 0.49927325354040686 | — |
| `Decimal128` | 0.49927325354040664 | 2·10⁻¹⁶ (sixteenth digit) |

(the `Decimal128` value is printed after conversion to `double`, so it does not show its digits beyond the sixteenth).

### Quantum with the same scheme

```csharp
var u = QuantumAlgorithms.TimeEvolution(Pauli<Decimal128>.X, Decimal128.Pi / Decimal128.CreateChecked(2));   // error ~1e-33
```

## PySharp: the `Interop` facade

```python
import os, clr
import numpy as np

root = os.getcwd()                                  # must be inside the Numbers repository
while not os.path.exists(os.path.join(root, "src", "MathemaSharp.slnx")):
    root = os.path.dirname(root)
clr.AddReference(os.path.join(root, "artifacts", "bin", "MathemaSharp.TimeSeries", "release", "MathemaSharp.TimeSeries.dll"))
from MathemaSharp.TimeSeries.Interop import Statistics, Forecasts, Metrics, Smoothing, Arima, Kalman, Var, Garch, Decomposition, Simulate

y = list(Simulate.Arma(2026, 600, 0.0, [0.6, -0.2], [], 1.0))      # same seed as the C# notebook
train, test = y[:580], y[580:]

acf = Statistics.Acf(train, 5)                       # a Python list is fine as double[]
ar  = Arima.FitYuleWalker(train, 2)
print(list(ar.Phi))                                  # [0.599503..., -0.200752...]
print(list(ar.Forecast(train, 5)))                   # the models have methods with arrays

kf = Kalman.LocalLevel(0.1, 1.0, train[0], 10.0)
kr = Kalman.Filter(kf, train[:100])                  # kr.FirstGain, kr.LogLikelihood
```

Practical rules:

- A `double[]` result is read with `list(...)` or `np.array(...)`; printed directly it shows `System.Double[]`.
- **A `NaN` is a missing datum** for the Kalman: `float("nan")` in Python.
- The **matrices** are passed **flattened** in row order (`Var.Fit(flat, rows, cols, order)`).
- .NET exceptions become Python exceptions (`ArgumentException` → `ValueError`).

## NumPy and the library in the same cell

```python
x = np.array(train)
xc = x - np.mean(x)
gamma = np.array([np.sum(xc[:len(x) - k] * xc[k:]) / len(x) for k in range(6)])   # NumPy only
lib   = np.array(Statistics.Autocovariance(train, 5))                              # library
print(np.max(np.abs(gamma - lib)))                                                 # 4.4e-16
```

The complete picture of what overlaps is in [chapter 3](03-overlap-numpy.md).

## What to do when another series is needed

1. Export the series from the historian as CSV (timestamp, value).
2. In C#: `File.ReadLines(...)` → `double[]`; in PySharp: `np.array([...])` or `list`.
3. If there are gaps: leave them as `NaN` and filter with `Kalman`, or interpolate first with NumPy.
4. Evaluate with `Metrics.Mase` against `Forecasts.SeasonalNaive`: if the model does not beat the baseline, it is not needed.

The [scenarios](../../../scenari/README.md) show this flow on plant data.

← [3 · Overlap with NumPy](03-overlap-numpy.md) · [Part II](README.md)
