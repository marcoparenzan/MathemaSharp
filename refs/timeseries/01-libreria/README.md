# Part II — The library

> **Numbers**: a family of .NET 11 libraries for numerical mathematics, written as **Generic Math** (`T : IFloatingPointIeee754<T>`): the same algorithms work with `float`, `double`, `Half` and the decimal formats `Decimal32`, `Decimal64`, `Decimal128`.

This part answers four questions, in this order:

| | Question | Where |
|---|---|---|
| 1 | **What is the library?** Packages, dependencies, how to use it | this page |
| 2 | **How is it used, function by function?** | [02 · Use and functions](02-uso-e-funzioni.md) |
| 3 | **What overlaps with NumPy in PySharp?** | [03 · Overlap with NumPy](03-overlap-numpy.md) |
| 4 | **Examples in C# and in PySharp** | [04 · Examples](04-esempi.md) |

## 1 · The library at a glance

### The packages

| Package | What it contains | Depends on | Documentation |
|---|---|---|---|
| **`MathemaSharp.Core`** | `NumericAlgorithms` (sum, Kahan, Horner, GCD, factorial), `Precision` (comparisons with tolerance, `decimal` roundings), `Signals` (DFT, Hermitian product, SIMD dot product) | — | [ARCHITECTURE](../../../ARCHITECTURE.md) |
| **`MathemaSharp.TimeSeries`** | statistics, baseline forecasts, AR/ARIMA, smoothing, Kalman, VAR, GARCH, STL, simulation, **`Interop`** | `MathemaSharp.Core` | [Part I](../00-formule-e-libreria/README.md) |
| **`MathemaSharp.Quantum`** | kets, operators, Pauli, time evolution, density matrix | `MathemaSharp.Core` | [QUANTUM](../../../QUANTUM.md) |
| **`MathemaSharp.NDSharp`** | bridge between `NDArray` (PySharp's NumPy) and `T[]`, `T[,]`, `Complex<T>[]` | `NDSharp` package 2.5.0 | this part |

Version **0.1.0**, target **net11.0** (SDK and runtime 11 RC2, `global.json`), XML documentation included in the packages. The book mainly covers `MathemaSharp.TimeSeries`.

### The type constraints

| Where | Constraint on `T` |
|---|---|
| `MathemaSharp.TimeSeries`, `Precision`, `Signals.VectorDot` | `IFloatingPointIeee754<T>` |
| `Signals.Dft`, `Signals.HermitianDot`, `MathemaSharp.Quantum` | `IFloatingPointIeee754<T>, IMinMaxValue<T>` (those of `Complex<T>`) |

Allowed formats: `float`, `double`, `Half`, `Decimal32`, `Decimal64`, `Decimal128`. **`System.Decimal` is not allowed** (it does not implement `IFloatingPointIeee754`). Which to choose: [ch. 1](../00-formule-e-libreria/01-generic-math.md).

### How to get it

```powershell
dotnet build src/MathemaSharp.slnx -c Release          # artifacts/bin/<project>/release/*.dll
pwsh scripts/pack.ps1                              # artifacts/package/release/*.nupkg
pwsh scripts/pack.ps1 -Publish                     # also copies into the local feed D:\Dev\NuGetLocalFeed
```

```xml
<PackageReference Include="MathemaSharp.TimeSeries" Version="0.1.0" />
```

(with the local feed in `nuget.config`). From a JupyterNet notebook, `#load "shared/MathemaSharp.csx"` references the DLLs in `artifacts/` (see [USAGE](../../../USAGE.md#notebooks-jupyternet)).

### Two ways of using it

| | **Generic API** | **`Interop` facade** |
|---|---|---|
| For whom | C# (and F#) | PySharp, PowerShell, any host with reflection |
| Types | `T` of your choice; `ReadOnlySpan<T>` | only `double[]`; flattened matrices |
| Namespace | `MathemaSharp.TimeSeries` | `MathemaSharp.TimeSeries.Interop` |
| Why it exists | the natural way in .NET | PySharp's reflection does not pass `ReadOnlySpan<T>` nor resolve generic methods |
| What is lost | — | the other formats: `Interop` is `double` |

The two levels produce **identical numbers**: the test `LibraryTourNotebooksAgreeAcrossLanguages` verifies it on the [library tour](04-esempi.md).

### What the library is **not**

- It is not a high-performance linear-algebra library (no BLAS, no SIMD for the decimals): for N-dimensional arrays there is NDSharp, through `MathemaSharp.NDSharp`.
- It is not `statsmodels`: ARIMA is by **Hannan-Rissanen** (not maximum likelihood), the Kalman has scalar observation, the GARCH is Gaussian (1,1). The limits of each algorithm are in its chapter of Part I.
- No performance measurement was collected: **no speed ratio will be deduced** between formats or between libraries.

→ [02 · Use and functions](02-uso-e-funzioni.md)
