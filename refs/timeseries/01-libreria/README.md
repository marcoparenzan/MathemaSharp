# Parte II — La libreria

> **Numbers**: una famiglia di librerie .NET 11 per la matematica numerica, scritte come **Generic Math** (`T : IFloatingPointIeee754<T>`): gli stessi algoritmi funzionano con `float`, `double`, `Half` e i formati decimali `Decimal32`, `Decimal64`, `Decimal128`.

Questa parte risponde a quattro domande, in quest'ordine:

| | Domanda | Dove |
|---|---|---|
| 1 | **Qual è la libreria?** Pacchetti, dipendenze, come si usa | questa pagina |
| 2 | **Come si usa, funzione per funzione?** | [02 · Uso e funzioni](02-uso-e-funzioni.md) |
| 3 | **Cosa si sovrappone a NumPy in PySharp?** | [03 · Overlap con NumPy](03-overlap-numpy.md) |
| 4 | **Esempi in C# e in PySharp** | [04 · Esempi](04-esempi.md) |

## 1 · Indicazione della libreria

### I pacchetti

| Pacchetto | Cosa contiene | Dipende da | Documentazione |
|---|---|---|---|
| **`MathemaSharp.Core`** | `NumericAlgorithms` (somma, Kahan, Horner, GCD, fattoriale), `Precision` (confronti con tolleranza, arrotondamenti `decimal`), `Signals` (DFT, prodotto hermitiano, prodotto scalare SIMD) | — | [ARCHITECTURE](../../../ARCHITECTURE.md) |
| **`MathemaSharp.TimeSeries`** | statistiche, previsioni di riferimento, AR/ARIMA, smoothing, Kalman, VAR, GARCH, STL, simulazione, **`Interop`** | `MathemaSharp.Core` | [Parte I](../00-formule-e-libreria/README.md) |
| **`MathemaSharp.Quantum`** | ket, operatori, Pauli, evoluzione temporale, matrice densità | `MathemaSharp.Core` | [QUANTUM](../../../QUANTUM.md) |
| **`MathemaSharp.NDSharp`** | ponte fra `NDArray` (NumPy di PySharp) e `T[]`, `T[,]`, `Complex<T>[]` | pacchetto `NDSharp` 2.5.0 | questa parte |

Versione **0.1.0**, target **net11.0** (SDK e runtime 11 RC2, `global.json`), documentazione XML inclusa nei pacchetti. Il libro tratta soprattutto `MathemaSharp.TimeSeries`.

### I vincoli di tipo

| Dove | Vincolo su `T` |
|---|---|
| `MathemaSharp.TimeSeries`, `Precision`, `Signals.VectorDot` | `IFloatingPointIeee754<T>` |
| `Signals.Dft`, `Signals.HermitianDot`, `MathemaSharp.Quantum` | `IFloatingPointIeee754<T>, IMinMaxValue<T>` (quelli di `Complex<T>`) |

Formati ammessi: `float`, `double`, `Half`, `Decimal32`, `Decimal64`, `Decimal128`. **`System.Decimal` non è ammesso** (non implementa `IFloatingPointIeee754`). Quale scegliere: [cap. 1](../00-formule-e-libreria/01-generic-math.md).

### Come si ottiene

```powershell
dotnet build src/MathemaSharp.slnx -c Release          # artifacts/bin/<progetto>/release/*.dll
pwsh scripts/pack.ps1                              # artifacts/package/release/*.nupkg
pwsh scripts/pack.ps1 -Publish                     # copia anche nel feed locale D:\Dev\NuGetLocalFeed
```

```xml
<PackageReference Include="MathemaSharp.TimeSeries" Version="0.1.0" />
```

(con il feed locale in `nuget.config`). Da un notebook JupyterNet, `#load "shared/MathemaSharp.csx"` referenzia le DLL di `artifacts/` (vedi [USAGE](../../../USAGE.md#notebook-jupyternet)).

### Due modi di usarla

| | **API generica** | **Facciata `Interop`** |
|---|---|---|
| Per chi | C# (e F#) | PySharp, PowerShell, qualunque host con reflection |
| Tipi | `T` a scelta; `ReadOnlySpan<T>` | solo `double[]`; matrici appiattite |
| Namespace | `MathemaSharp.TimeSeries` | `MathemaSharp.TimeSeries.Interop` |
| Perché esiste | il modo naturale in .NET | la reflection di PySharp non passa `ReadOnlySpan<T>` né risolve i metodi generici |
| Cosa si perde | — | gli altri formati: l'`Interop` è `double` |

I due livelli producono **numeri identici**: lo verifica il test `LibraryTourNotebooksAgreeAcrossLanguages` sul [giro della libreria](04-esempi.md).

### Cosa la libreria **non** è

- Non è una libreria di algebra lineare ad alte prestazioni (niente BLAS, niente SIMD per i decimali): per array N-dimensionali c'è NDSharp, tramite `MathemaSharp.NDSharp`.
- Non è `statsmodels`: ARIMA è per **Hannan-Rissanen** (non massima verosimiglianza), il Kalman ha osservazione scalare, il GARCH è (1,1) gaussiano. I limiti di ogni algoritmo stanno nel suo capitolo della Parte I.
- Nessuna misura di prestazione è stata raccolta: **non si dedurrà nessun rapporto di velocità** fra formati o fra librerie.

→ [02 · Uso e funzioni](02-uso-e-funzioni.md)
