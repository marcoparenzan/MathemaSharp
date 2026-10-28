# 1 · Generic Math: la stessa formula, ogni formato

## Il problema

Un algoritmo numerico è una formula più una scelta di aritmetica. Scrivere $\hat y = \alpha y_t + (1-\alpha)\hat y_{t-1}$ con `double` fissa la scelta per sempre. Con **Generic Math** (.NET 7+, e i formati decimali IEEE di .NET 11) la scelta diventa un parametro: la formula è scritta una volta sola, su `T`.

```csharp
public static T[] Acf<T>(ReadOnlySpan<T> y, int maxLag) where T : IFloatingPointIeee754<T>
```

## La gerarchia dei vincoli

Ogni vincolo aggiunge operazioni. La libreria chiede il minimo che serve.

| Vincolo | Aggiunge | Usato da |
|---|---|---|
| `INumberBase<T>` | `+ − × /`, `T.Zero`, `T.One`, `CreateChecked` | `NumericAlgorithms.Sum`, `Dot`, `Horner` |
| `INumber<T>` | ordinamento: `<`, `Min`, `Max`, `Clamp` | — |
| `IFloatingPoint<T>` | `Round`, `Floor`, `Ceiling` | `NumericAlgorithms.Mean` |
| **`IFloatingPointIeee754<T>`** | `Exp`, `Log`, `Pow`, `Sqrt`, `Sin`, `Cos`, `Pi`, `Tau`, `NaN`, `PositiveInfinity`, `IsFinite`, `BitIncrement` | **tutta `MathemaSharp.TimeSeries`**, `Precision`, `Signals.VectorDot`, `Kahan` |
| `IMinMaxValue<T>` | `MinValue`, `MaxValue` | richiesto da `Complex<T>`: `Signals.Dft`, tutta `MathemaSharp.Quantum` |

> `System.Decimal` **non** implementa `IFloatingPointIeee754`: niente `Exp`, `Log`, `NaN`. Per questo è fuori dalle librerie generiche; i formati decimali sono `Decimal32/64/128` di .NET 11.

## I formati

| Formato | Bit | Cifre | ε di macchina | Note |
|---|---|---|---|---|
| `Half` | 16 | ~3 | 9.8·10⁻⁴ | solo per memorizzare: gli accumulatori non reggono |
| `float` | 32 | ~7 | 1.2·10⁻⁷ | |
| `double` | 64 | ~16 | 2.2·10⁻¹⁶ | il formato dei test numerici |
| `Decimal32` | 32 | 7 | 10⁻⁶ | base 10: 0.1 è esatto |
| `Decimal64` | 64 | 16 | 10⁻¹⁵ | **non più preciso** di `double` (ε 4.5 volte maggiore) |
| `Decimal128` | 128 | 34 | 10⁻³³ | 17 ordini di grandezza meglio di `double` |

Gli ε di `float`, `double`, `Decimal64` e `Decimal128` sono misurati dal codice (`T.BitIncrement(T.One) - T.One`); quelli di `Half` e `Decimal32` derivano dalla definizione del formato.

## Cosa cambia davvero: la tabella `--topic formats`

`dotnet run --project src/apps/MathemaSharp.Lab -- --topic formats` esegue gli stessi algoritmi in quattro formati:

```text
formato     ε (ULP a 1)  errore DFT   errore U(π/2)|0⟩   LS Hilbert 7×7
Single      1.2E-007     0            1.2E-007           singolare
Double      2.2E-016     1.8E-015     6.7E-016           1.4E-008
Decimal64   1.0E-015     5.0E-015     0                  3.2E-008
Decimal128  1.0E-033     5.0E-033     1.0E-033           1.3E-026
```

Tre lezioni:

1. **Con una matrice mal condizionata l'errore è $\kappa\cdot\varepsilon_{\text{mach}}$.** La matrice di Hilbert $7\times7$ ha $\kappa\approx4.8\cdot10^{8}$. Per `double`: $4.8\cdot10^8\cdot2.2\cdot10^{-16}\approx10^{-7}$ (misurato $1.4\cdot10^{-8}$). Per `Decimal128`: $\approx5\cdot10^{-25}$ (misurato $1.3\cdot10^{-26}$).
2. **Per `float` il prodotto $\kappa\cdot\varepsilon$ vale 57, cioè più di 1**: il problema è numericamente singolare e la QR lo rileva e lo dichiara (`ArgumentException`), invece di restituire un numero sbagliato.
3. **`Decimal64` non è una scorciatoia verso più precisione**: ha 16 cifre, `double` ne ha quasi 16 in binario. Il vantaggio dei decimali è la **rappresentazione esatta di 0.1**, non la precisione.

## Le tecniche usate nel codice

| Tecnica | Dove | Perché |
|---|---|---|
| Costanti come `double` convertite: `Num.C<T>(0.5)` | ovunque | un letterale `0.5` non è un `T` |
| Tolleranze legate al formato: $4096\,\varepsilon$ (confronti), $\sqrt\varepsilon$ (controlli booleani) | `Precision`, `ComplexMatrix` | `1e-9` fisso è troppo stretto per `float` e troppo largo per `Decimal128` |
| `tiny = T.Sqrt(T.Epsilon)` | frazione continua di `SpecialFunctions` | `1e-300` vale 0 per `float`; $\sqrt{\text{min positivo}}$ è positivo e $1/\text{tiny}$ non esce dal range in nessun formato |
| Le estrazioni casuali sono sempre `double` e poi convertite | `Simulation` | con lo stesso seme, formati diversi partono dallo stesso rumore e differiscono solo per l'aritmetica |
| `Vector<T>.IsSupported` con ritorno allo scalare | `Signals.VectorDot` | `Vector<Decimal64>` non è supportato: la SIMD non c'è, ma l'algoritmo funziona |
| Classe statica + record generico con lo stesso nome: `ArModel.FitYuleWalker(...)` → `ArModel<T>` | modelli | l'inferenza di `T` dagli argomenti evita di scrivere `<double>` a ogni chiamata |
| `T[]` accanto a `ReadOnlySpan<T>` nei metodi di istanza dei modelli | `ArModel<T>.Forecast` ecc. | una `ref struct` non si passa via reflection (PySharp) |

## Cosa può andare storto

- **Costanti con precisione `double`.** I coefficienti di Lanczos di $\ln\Gamma$ sono scritti come `double`: anche con `Decimal128` il p-value di Ljung-Box è accurato a ~$10^{-15}$, non a $10^{-33}$.
- **`Half` negli accumulatori.** Con ε ≈ $10^{-3}$ la soglia di dipendenza lineare della QR ($4096\,\varepsilon$) vale ~4: quasi ogni matrice sarebbe dichiarata singolare. `Half` è coperto solo da uno smoke test.
- **Costo.** I decimali IEEE non hanno SIMD. Nessuna misura di prestazione è stata raccolta: non se ne deducono rapporti di velocità.
- **Overflow in `CreateChecked`.** Convertire una costante fuori range lancia `OverflowException`; la sottovalutazione, invece, arrotonda a zero in silenzio.

## Verifica

`GenericFormatTests` in `MathemaSharp.Core.Tests`, `MathemaSharp.Quantum.Tests` e `MathemaSharp.TimeSeries.Tests`: le stesse identità con `float`, `double`, `Decimal64`, `Decimal128`.

← [Indice](README.md) · [2 · Statistica di base →](02-statistica-di-base.md)
