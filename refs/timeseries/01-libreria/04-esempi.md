# 4 · Esempi in C# e in PySharp

Due notebook **gemelli** fanno le stesse operazioni sugli stessi dati (AR(2) simulato con seme 2026):

| | Notebook | Come usa la libreria |
|---|---|---|
| **C#** | [`notebooks/TimeSeries/10-ts-libreria-tour.ipynb`](../../../notebooks/TimeSeries/10-ts-libreria-tour.ipynb) | API **generica**, `T = double` (più una sezione con `float` e `Decimal128`) |
| **PySharp** | [`notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb`](../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb) | facciata **`Interop`** via `clr.AddReference` (più una sezione di confronto con NumPy) |

Ogni riga `etichetta: valori` stampata da uno compare **identica** nell'altro (27 etichette). Lo garantisce il test `LibraryTourNotebooksAgreeAcrossLanguages`.

Esecuzione: `pwsh scripts/run-notebooks.ps1 -Filter '10-*'`.

## C#: l'API generica

```csharp
#load "shared/MathemaSharp.csx"
using MathemaSharp.TimeSeries;

var y = Simulation.Arma<double>(new Random(2026), 600, 0, new[] { 0.6, -0.2 }, Array.Empty<double>(), 1);
double[] train = y[..580], test = y[580..];

var acf = SeriesStatistics.Acf(train, 5);                       // T dedotto da double[]
var ar  = ArModel.FitYuleWalker(train, 2);                      // ArModel<double>
Console.WriteLine(string.Join(" ", ar.Phi));                    // 0.599503 -0.200752
var f   = ar.Forecast(train, 5);

var alpha = ExponentialSmoothing.FitSesAlpha(train);
var holt  = ExponentialSmoothing.Holt(train, 0.4, 0.2, 5, 0.9); // damped, φ = 0.9

var kf = KalmanFilter.LocalLevel(0.1, 1.0, train[0], 10.0);
var kr = kf.Filter(train[..100]);                               // kr.FirstGain, kr.LogLikelihood
```

### Cambiare formato = cambiare `T`

```csharp
using System.Numerics;
var pacf128 = SeriesStatistics.Pacf(train.Select(Decimal128.CreateChecked).ToArray(), 1)[0];   // Decimal128
var pacf32  = SeriesStatistics.Pacf(train.Select(float.CreateChecked).ToArray(), 1)[0];        // float
```

Stessi dati, stesso algoritmo, risultati misurati nella sezione 9 del notebook C#:

| Formato | PACF al lag 1 | Scarto da `double` |
|---|---|---|
| `float` | 0.4992733896 | 1.4·10⁻⁷ (settima cifra) |
| `double` | 0.49927325354040686 | — |
| `Decimal128` | 0.49927325354040664 | 2·10⁻¹⁶ (sedicesima cifra) |

(il valore `Decimal128` è stampato dopo la conversione in `double`, quindi non mostra le sue cifre oltre la sedicesima).

### Quantistica con lo stesso schema

```csharp
var u = QuantumAlgorithms.TimeEvolution(Pauli<Decimal128>.X, Decimal128.Pi / Decimal128.CreateChecked(2));   // errore ~1e-33
```

## PySharp: la facciata `Interop`

```python
import os, clr
import numpy as np

root = os.getcwd()                                  # deve essere dentro il repository Numbers
while not os.path.exists(os.path.join(root, "src", "MathemaSharp.slnx")):
    root = os.path.dirname(root)
clr.AddReference(os.path.join(root, "artifacts", "bin", "MathemaSharp.TimeSeries", "release", "MathemaSharp.TimeSeries.dll"))
from MathemaSharp.TimeSeries.Interop import Statistics, Forecasts, Metrics, Smoothing, Arima, Kalman, Var, Garch, Decomposition, Simulate

y = list(Simulate.Arma(2026, 600, 0.0, [0.6, -0.2], [], 1.0))      # stesso seme del notebook C#
train, test = y[:580], y[580:]

acf = Statistics.Acf(train, 5)                       # una lista Python va bene come double[]
ar  = Arima.FitYuleWalker(train, 2)
print(list(ar.Phi))                                  # [0.599503..., -0.200752...]
print(list(ar.Forecast(train, 5)))                   # i modelli hanno metodi con array

kf = Kalman.LocalLevel(0.1, 1.0, train[0], 10.0)
kr = Kalman.Filter(kf, train[:100])                  # kr.FirstGain, kr.LogLikelihood
```

Regole pratiche:

- Un risultato `double[]` si legge con `list(...)` o `np.array(...)`; stampato direttamente mostra `System.Double[]`.
- **Un `NaN` è un dato mancante** per il Kalman: `float("nan")` in Python.
- Le **matrici** si passano **appiattite** in ordine di riga (`Var.Fit(flat, rows, cols, order)`).
- Eccezioni .NET diventano eccezioni Python (`ArgumentException` → `ValueError`).

## NumPy e libreria nella stessa cella

```python
x = np.array(train)
xc = x - np.mean(x)
gamma = np.array([np.sum(xc[:len(x) - k] * xc[k:]) / len(x) for k in range(6)])   # solo NumPy
lib   = np.array(Statistics.Autocovariance(train, 5))                              # libreria
print(np.max(np.abs(gamma - lib)))                                                 # 4.4e-16
```

Il quadro completo di cosa si sovrappone è nel [capitolo 3](03-overlap-numpy.md).

## Cosa fare quando serve un'altra serie

1. Esporta la serie dallo storico come CSV (timestamp, valore).
2. In C#: `File.ReadLines(...)` → `double[]`; in PySharp: `np.array([...])` o `list`.
3. Se ci sono buchi: lasciali come `NaN` e filtra con `Kalman`, oppure interpola prima con NumPy.
4. Valuta con `Metrics.Mase` rispetto a `Forecasts.SeasonalNaive`: se il modello non batte il riferimento, non serve.

Gli [scenari](../../../scenari/README.md) mostrano questo flusso su dati di impianto.

← [3 · Overlap con NumPy](03-overlap-numpy.md) · [Parte II](README.md)
