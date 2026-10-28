# 10 · Algebra lineare, simulazione e facciata per PySharp

Codice: [MatrixAlgebra.cs](../../../src/libs/MathemaSharp.TimeSeries/MatrixAlgebra.cs), [Simulation.cs](../../../src/libs/MathemaSharp.TimeSeries/Simulation.cs), [Interop.cs](../../../src/libs/MathemaSharp.TimeSeries/Interop.cs).

## Minimi quadrati con QR di Householder

Si cerca $\hat\beta=\arg\min\|X\beta-y\|_2$ con $X\in\mathbb R^{m\times n},\,m\ge n$. La strada "scolastica" sono le **equazioni normali** $X^\top X\beta=X^\top y$, ma $\kappa(X^\top X)=\kappa(X)^2$: si **raddoppiano le cifre perse**. La QR lavora su $X$ e perde solo $\kappa(X)\,\varepsilon$.

**Riflessione di Householder.** Per annullare la colonna $k$ sotto la diagonale si usa $H=I-2\dfrac{vv^\top}{v^\top v}$ con

$$v=x-\alpha e_1,\qquad \alpha=-\operatorname{sign}(x_1)\,\|x\|_2$$

Il segno opposto a $x_1$ evita la **cancellazione** in $v_1=x_1-\alpha$. $H$ si applica alle colonne successive e al termine noto; alla fine si risolve il sistema triangolare per sostituzione all'indietro.

```csharp
var alpha = a[k, k] > T.Zero ? -norm : norm;
a[k, k] -= alpha;                                   // v₁ = x₁ − α, senza cancellazione
...
var f = two * s / vNorm2;                           // H = I − 2vvᵀ/‖v‖²
```

**Dipendenza lineare.** Se la norma del sotto-blocco di una colonna scende sotto $4096\,\varepsilon_{\text{mach}}\cdot\max(\text{scala},1)$ le colonne sono linearmente dipendenti (a precisione di macchina) e la libreria lancia `ArgumentException` invece di dividere per un numero spurio: è ciò che con `float` dichiara «singolare» la matrice di Hilbert $7\times7$ (cap. 1).

Verifiche: `LeastSquaresRecoversExactLine` (retta esatta + rifiuto di colonne dipendenti), `LeastSquaresInEveryFormat`.

È usata da: `ArimaModel.FitHannanRissanen` (due regressioni), `VarModel.Fit` (una per equazione), e disponibile direttamente.

## Simulazione

Servono processi con parametri **noti** per verificare gli stimatori. `Simulation` genera ARMA, passeggiate aleatorie, GARCH e VAR.

**Rumore normale (Box-Muller):**

$$z=\sqrt{-2\ln u_1}\,\cos(2\pi u_2),\qquad u_1=1-U_1,\;u_2=U_2$$

$1-U_1$ evita $\ln0$. **Burn-in:** i primi 200 valori si scartano per dimenticare lo stato iniziale. **Con lo stesso seme** formati diversi partono dallo stesso rumore (le estrazioni sono sempre `double`, poi convertite): la differenza fra i risultati è solo aritmetica. Su questo si basa `Decimal128AgreesWithDoubleOnTheSameNoise`.

> **Non è il generatore di NumPy.** `System.Random` più Box-Muller produce flussi diversi da `numpy.random` (PCG64 con `Generator`): stesso seme, numeri diversi. Per riprodurre una serie fra C# e PySharp, la facciata espone `Simulate.Arma(seed, …)`, che chiama lo stesso generatore.

## La facciata per PySharp

PySharp (un interprete Python in C#) carica DLL .NET con `clr.AddReference`, ma la sua reflection ha due limiti:

- **non può passare un `ReadOnlySpan<T>`** (una `ref struct` non si può inscatolare): provato, `Statistics.Mean(...)` su un metodo con span fallisce con «no overload matches»;
- **non fa inferenza di tipo sui metodi generici** (limite dichiarato nel README di PySharp), quindi `SeriesStatistics.Acf(y, 20)` non è chiamabile così com'è.

Per questo la libreria ha, accanto all'API generica:

1. **`T[]`** accanto a `ReadOnlySpan<T>` nei metodi di istanza dei modelli (`ArModel<T>.Forecast`, `ArimaModel<T>.Residuals/Forecast`, `Garch11<T>.ConditionalVariances/LogLikelihood/ForecastVariance`, `KalmanFilter<T>.Filter`);
2. il namespace **`MathemaSharp.TimeSeries.Interop`**: classi statiche **non generiche** su `double[]` che delegano all'API generica con `T = double`:

| Classe | Delega a |
|---|---|
| `Statistics` | `SeriesStatistics`, `SpecialFunctions` |
| `Forecasts` | `ReferenceForecasts` |
| `Metrics` | `Accuracy` |
| `Smoothing` | `ExponentialSmoothing` |
| `Arima` | `ArModel`, `ArimaModel` |
| `Kalman` | `KalmanFilter` |
| `Var` | `VarModel` (matrici **appiattite in ordine di riga**) |
| `Garch` | `Garch11` |
| `Decomposition` | `Stl` |
| `Linear` | `LinearAlgebra` |
| `Simulate` | `Simulation` (con il seme come argomento) |

I risultati sono **classi semplici** (`LevinsonResult`, `LjungBoxResult`, `KalmanResult`), non tuple: le proprietà si leggono da Python come `r.Pacf`.

> L'interop è verificata con PySharp reale: liste Python → `double[]`, array NumPy → `x.to_clr()`, e i risultati tornano in NumPy con `np.array(...)`.

Per C# conviene l'API generica, che accetta anche `float`, `Half` e `Decimal32/64/128`.

← [9 · LOESS e STL](09-stl-loess.md) · [Indice](README.md) · [Parte II — La libreria →](../01-libreria/README.md)
