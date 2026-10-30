# 1 · Generic Math: the same formula, every format

## The problem

A numerical algorithm is a formula plus a choice of arithmetic. Writing $\hat y = \alpha y_t + (1-\alpha)\hat y_{t-1}$ with `double` fixes the choice forever. With **Generic Math** (.NET 7+, and the IEEE decimal formats of .NET 11) the choice becomes a parameter: the formula is written only once, on `T`.

```csharp
public static T[] Acf<T>(ReadOnlySpan<T> y, int maxLag) where T : IFloatingPointIeee754<T>
```

## The hierarchy of constraints

Each constraint adds operations. The library asks for the minimum that is needed.

| Constraint | Adds | Used by |
|---|---|---|
| `INumberBase<T>` | `+ − × /`, `T.Zero`, `T.One`, `CreateChecked` | `NumericAlgorithms.Sum`, `Dot`, `Horner` |
| `INumber<T>` | ordering: `<`, `Min`, `Max`, `Clamp` | — |
| `IFloatingPoint<T>` | `Round`, `Floor`, `Ceiling` | `NumericAlgorithms.Mean` |
| **`IFloatingPointIeee754<T>`** | `Exp`, `Log`, `Pow`, `Sqrt`, `Sin`, `Cos`, `Pi`, `Tau`, `NaN`, `PositiveInfinity`, `IsFinite`, `BitIncrement` | **all of `MathemaSharp.TimeSeries`**, `Precision`, `Signals.VectorDot`, `Kahan` |
| `IMinMaxValue<T>` | `MinValue`, `MaxValue` | required by `Complex<T>`: `Signals.Dft`, all of `MathemaSharp.Quantum` |

> `System.Decimal` does **not** implement `IFloatingPointIeee754`: no `Exp`, `Log`, `NaN`. This is why it is outside the generic libraries; the decimal formats are `Decimal32/64/128` of .NET 11.

## The formats

| Format | Bits | Digits | Machine ε | Notes |
|---|---|---|---|---|
| `Half` | 16 | ~3 | 9.8·10⁻⁴ | for storage only: the accumulators do not hold up |
| `float` | 32 | ~7 | 1.2·10⁻⁷ | |
| `double` | 64 | ~16 | 2.2·10⁻¹⁶ | the format of the numerical tests |
| `Decimal32` | 32 | 7 | 10⁻⁶ | base 10: 0.1 is exact |
| `Decimal64` | 64 | 16 | 10⁻¹⁵ | **not more precise** than `double` (ε 4.5 times larger) |
| `Decimal128` | 128 | 34 | 10⁻³³ | 17 orders of magnitude better than `double` |

The ε of `float`, `double`, `Decimal64` and `Decimal128` are measured by the code (`T.BitIncrement(T.One) - T.One`); those of `Half` and `Decimal32` derive from the definition of the format.

## What really changes: the `--topic formats` table

`dotnet run --project src/apps/MathemaSharp.Lab -- --topic formats` runs the same algorithms in four formats:

```text
format      ε (ULP at 1)  DFT error   U(π/2)|0⟩ error   LS Hilbert 7×7
Single      1.2E-007     0            1.2E-007           singular
Double      2.2E-016     1.8E-015     6.7E-016           1.4E-008
Decimal64   1.0E-015     5.0E-015     0                  3.2E-008
Decimal128  1.0E-033     5.0E-033     1.0E-033           1.3E-026
```

Three lessons:

1. **With an ill-conditioned matrix the error is $\kappa\cdot\varepsilon_{\text{mach}}$.** The $7\times7$ Hilbert matrix has $\kappa\approx4.8\cdot10^{8}$. For `double`: $4.8\cdot10^8\cdot2.2\cdot10^{-16}\approx10^{-7}$ (measured $1.4\cdot10^{-8}$). For `Decimal128`: $\approx5\cdot10^{-25}$ (measured $1.3\cdot10^{-26}$).
2. **For `float` the product $\kappa\cdot\varepsilon$ is 57, that is more than 1**: the problem is numerically singular and the QR detects it and declares it (`ArgumentException`), instead of returning a wrong number.
3. **`Decimal64` is not a shortcut to more precision**: it has 16 digits, `double` has almost 16 in binary. The advantage of decimals is the **exact representation of 0.1**, not the precision.

## The techniques used in the code

| Technique | Where | Why |
|---|---|---|
| Constants as converted `double`: `Num.C<T>(0.5)` | everywhere | a literal `0.5` is not a `T` |
| Tolerances tied to the format: $4096\,\varepsilon$ (comparisons), $\sqrt\varepsilon$ (boolean checks) | `Precision`, `ComplexMatrix` | a fixed `1e-9` is too tight for `float` and too loose for `Decimal128` |
| `tiny = T.Sqrt(T.Epsilon)` | continued fraction of `SpecialFunctions` | `1e-300` is 0 for `float`; $\sqrt{\text{smallest positive}}$ is positive and $1/\text{tiny}$ does not leave the range in any format |
| Random draws are always `double` and then converted | `Simulation` | with the same seed, different formats start from the same noise and differ only in the arithmetic |
| `Vector<T>.IsSupported` with a fallback to scalar | `Signals.VectorDot` | `Vector<Decimal64>` is not supported: there is no SIMD, but the algorithm works |
| Static class + generic record with the same name: `ArModel.FitYuleWalker(...)` → `ArModel<T>` | models | inferring `T` from the arguments avoids writing `<double>` at every call |
| `T[]` alongside `ReadOnlySpan<T>` in the models' instance methods | `ArModel<T>.Forecast` etc. | a `ref struct` cannot be passed through reflection (PySharp) |

## What can go wrong

- **Constants with `double` precision.** The Lanczos coefficients of $\ln\Gamma$ are written as `double`: even with `Decimal128` the Ljung-Box p-value is accurate to ~$10^{-15}$, not $10^{-33}$.
- **`Half` in the accumulators.** With ε ≈ $10^{-3}$ the linear-dependence threshold of the QR ($4096\,\varepsilon$) is ~4: almost every matrix would be declared singular. `Half` is covered only by a smoke test.
- **Cost.** The IEEE decimals have no SIMD. No performance measurement was collected: no speed ratios are deduced from them.
- **Overflow in `CreateChecked`.** Converting a constant out of range throws `OverflowException`; underestimation, instead, silently rounds to zero.

## Verification

`GenericFormatTests` in `MathemaSharp.Core.Tests`, `MathemaSharp.Quantum.Tests` and `MathemaSharp.TimeSeries.Tests`: the same identities with `float`, `double`, `Decimal64`, `Decimal128`.

← [Index](README.md) · [2 · Basic statistics →](02-statistica-di-base.md)
