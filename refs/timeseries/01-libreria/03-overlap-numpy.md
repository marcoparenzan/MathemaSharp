# 3 · Overlap con NumPy in PySharp

In PySharp `import numpy as np` **non è il NumPy di CPython** (niente estensioni C): è un modulo C# sopra **NDSharp**, una libreria di `ndarray` nativa .NET. Copre un sottoinsieme ampio ma non completo dell'API. Questa pagina dice, funzione per funzione, **dove la libreria Numbers e NumPy fanno la stessa cosa**, dove no, e dove manca l'uno o l'altro.

> **Verificato, non supposto.** I nomi disponibili in PySharp sono stati interrogati con `hasattr(np, …)` sul kernel reale (JupyterNet, .NET 11 RC2). Le coincidenze numeriche sono nel notebook [PySharp 10](../../../notebooks/TimeSeries/pysharp/10-ts-libreria-tour.ipynb), sezione 9.

## A · Dove coincidono

| Operazione | Libreria `MathemaSharp.TimeSeries` | NumPy in PySharp | Verifica |
|---|---|---|---|
| Media | `Statistics.Mean` | `np.mean` | identici |
| Autocovarianza (divisore $n$) | `Statistics.Autocovariance` | `np.sum(xc[:n-k]*xc[k:])/n`, con `xc = x - np.mean(x)` | differenza max **4.4·10⁻¹⁶** |
| Yule-Walker | `Arima.FitYuleWalker` (Levinson-Durbin, $O(p^2)$) | `np.linalg.solve(Γ, γ)` ($O(p^3)$) | differenza max **3.3·10⁻¹⁶** |
| Differenza prima | `Statistics.Difference(y, 1)` | `np.diff(y)` | identici (0) |
| Quantili | `Statistics.Quantile` | `np.quantile`, `np.percentile` | identici (0): stessa interpolazione lineare |
| Differenza stagionale | `Statistics.Difference(y, m)` | `y[m:] - y[:-m]` | identici (0) |
| Somma cumulata (integrare le differenze, ARIMA con $d\ge1$) | interno a `ArimaModel.Forecast` | `np.cumsum` | |
| Prodotto matrice | `MatrixAlgebra.Multiply` | `np.dot`, `np.matmul` | |
| Minimi quadrati | `MatrixAlgebra.LeastSquares` (QR di Householder) | `np.linalg.lstsq` | stesso problema; l'algoritmo interno di NDSharp non è stato verificato |
| Trasformata di Fourier | `Signals.Dft` ($O(N^2)$, **didattica**) | `np.fft.fft` | stessi valori; per $N$ grande usare `np.fft` |
| Media mobile (filtro passa-basso di STL) | interno a `Stl` | `np.convolve(x, np.ones(m)/m, 'valid')` | |
| Mediana (pesi di robustezza STL) | interno a `Stl` | `np.median` | |
| Funzioni elementari | `T.Exp`, `T.Log`, `T.Sqrt`, `T.Cos`… | `np.exp`, `np.log`, `np.sqrt`, `np.cos`… | |

> **Trappola: `np.diff(y, 2)` non è la differenza a distanza 2.** NumPy calcola la differenza del *secondo ordine* ($\Delta\Delta y$): `np.diff(np.array([1,2,4,7]), 2)` dà `[1, 1]`. `Statistics.Difference(y, 2)` è $y_t-y_{t-2}$. Per la differenza stagionale in NumPy si usano le slice.

## B · Dove la libreria aggiunge (NumPy non ce l'ha)

| Libreria | In Python vivrebbe in |
|---|---|
| ACF, PACF, Levinson-Durbin, Ljung-Box, `ChiSquareSurvival` | `statsmodels.tsa` / `scipy.stats` |
| Previsioni di riferimento, MAE/RMSE/MAPE/**MASE** | `sktime`, `statsforecast` |
| SES, Holt, damped, Holt-Winters | `statsmodels.tsa.holtwinters` |
| Filtro di Kalman, local level | `statsmodels.tsa.statespace` |
| ARIMA (Hannan-Rissanen), AR (Yule-Walker) | `statsmodels.tsa.arima` |
| VAR | `statsmodels.tsa.vector_ar` |
| GARCH(1,1) | `arch` |
| STL, LOESS | `statsmodels.tsa.seasonal.STL` |
| Perdita pinball, copertura degli intervalli (`PinballLoss`, `IntervalCoverage`) | `properscoring`, `sktime.performance_metrics` |
| Calcolo in `Decimal32/64/128`, `float`, `Half` | — (NumPy non ha dtype decimali IEEE) |

Né `statsmodels` né `arch` esistono in PySharp: **qui la libreria Numbers è l'unico modo** di fare queste cose da Python, via `clr`.

## C · Dove NumPy ha ciò che la libreria non ha

Presente in PySharp, assente nella libreria:

| NumPy | Uso tipico in serie temporali |
|---|---|
| `np.std`, `np.var`, `np.cov`, `np.corrcoef` | statistiche descrittive |
| `np.linalg.inv`, `pinv`, `svd`, `det`, `eigh`, `eigvalsh` | algebra lineare generale |
| `np.fft.rfft`, `ifft`, `fftfreq` | spettro, filtri in frequenza |
| `np.random.default_rng`, `randn`, `normal`, `choice` | simulazione, bootstrap |
| `np.sort`, `argsort`, `roll`, `where` | manipolazione |

**Assenti in entrambi** (verificato su PySharp): `np.correlate`, `np.polyfit`, `np.polyval`, `np.linalg.eig`, `np.linalg.eigvals`, `np.linalg.qr`, `np.linalg.cholesky`, `np.tril`, `np.triu`, `np.kron`, `np.searchsorted`. Conseguenze concrete:

- **Autocovarianza con `np.correlate`** non è possibile: si scrive a mano (riga della tabella A) o si usa la libreria.
- **Stabilità di un VAR($p$)**: servirebbero gli autovalori di una matrice *non simmetrica* (`np.linalg.eigvals`): non c'è né qui né in NumPy-PySharp. La libreria copre solo il VAR(1) $2\times2$ in forma analitica.
- **Fattorizzazione di Cholesky** (per impulsi ortogonalizzati) manca ovunque.

## D · Differenze che cambiano i numeri

| Aspetto | Libreria | NumPy in PySharp |
|---|---|---|
| Generatore casuale | `System.Random` + Box-Muller | PCG64 (`default_rng`) |
| Conseguenza | **lo stesso seme dà numeri diversi**: per riprodurre una serie fra C# e PySharp si usa `Simulate.Arma(seed, …)`, non `np.random` | |
| Divisore della covarianza | $n$ (autocovarianza) | `np.cov`: $n-1$ di default (`bias=True` → $n$) |
| Formati | `float`, `double`, `Half`, `Decimal32/64/128` | `float32`, `float64`, interi, complessi; **nessun decimale** |
| Layout delle matrici | `T[,]` (o appiattite in `Interop`) | `ndarray` con strides e viste |

## E · Come usarli insieme

Il flusso consigliato: **NumPy per preparare e ispezionare, la libreria per la matematica delle serie temporali.**

```python
import numpy as np

x = np.array(sensor_readings)            # NDArray float64
x = x[~np.isnan(x)]                      # pulizia con NumPy
d = np.diff(x)                           # esplorazione con NumPy

acf  = np.array(Statistics.Acf(x.to_clr(), 20))     # libreria: x.to_clr() → double[]; il risultato torna in NumPy
model = Arima.FitYuleWalker(x.to_clr(), 2)
fc   = np.array(model.Forecast(x.to_clr(), 12))
```

- Da NumPy alla libreria: **`x.to_clr()`** produce un `double[]` (copia); una lista Python va bene lo stesso.
- Dalla libreria a NumPy: **`np.array(clr_array)`**.
- Da C#, lo stesso scambio passa dal ponte [`MathemaSharp.NDSharp`](02-uso-e-funzioni.md#numbersndsharp--il-ponte): `ToVector()`, `ToNDArray()`.

← [2 · Uso e funzioni](02-uso-e-funzioni.md) · [Parte II](README.md) · [4 · Esempi →](04-esempi.md)
