# MathemaSharp.Quantum — matematica quantistica a dimensione finita

Libreria (pacchetto NuGet) in `src/libs/MathemaSharp.Quantum` (dipende solo da `MathemaSharp.Core`, target net11.0). Copre il formalismo di bra-ket su spazi di Hilbert finiti: qubit, spin-1/2, sistemi a N livelli. Tutta l'aritmetica è `Complex<T>` di .NET 11, generica su `T : IFloatingPointIeee754<T>, IMinMaxValue<T>`: `float`, `double`, `Decimal32/64/128`. I metodi sono generici (`QuantumAlgorithms.TimeEvolution(h, t)` deduce `T`); gli operatori e gli stati sono `Pauli<T>`, `QubitStates<T>`, `ComplexMatrix<T>`.

## Convenzioni

- **Ket** `|ψ⟩` = `Complex<T>[]` / `ReadOnlySpan<Complex<T>>`. Non esiste un tipo Bra: `⟨φ|` è il coniugato implicito nel prodotto interno (antilineare a sinistra, lineare a destra, come `Signals.HermitianDot`).
- **Operatore** `Â` = `ComplexMatrix<T>` (matrice densa quadrata). Il backing array è condiviso tra copie dello struct: non mutare un alias.
- Base computazionale `{|0⟩, |1⟩}`: autostati di Z con autovalori +1 e −1.
- Gli stati devono essere normalizzati dove indicato (`Projector`, `DensityMatrix`): usare `Normalize`.
- Le tolleranze dei controlli booleani sono assolute e relative, default `1e-9`.

## Dal simbolo all'API

| Simbolo | Significato | API |
|---|---|---|
| `\|ψ⟩` | Ket | `Complex[]`; stati base in `QubitStates<T>` (`Zero`, `One`, `Plus`, `Minus`, `PlusI`, `MinusI`) |
| `⟨ψ\|` | Bra | coniugato implicito nel prodotto interno |
| `⟨φ\|ψ⟩` | Prodotto interno | `QuantumAlgorithms.InnerProduct(bra, ket)`; norma: `Norm`, `Normalize` |
| `\|ψ⟩⟨φ\|` | Prodotto esterno | `OuterProduct(ket, bra)` → `ComplexMatrix<T>` |
| `Â` | Operatore | `ComplexMatrix<T>`; `Â\|ψ⟩` con `Apply(a, ket)` o `a * ket` |
| `Â\|a⟩ = a\|a⟩` | Equazione agli autovalori | `CheckEigenstate(a, ket)` → `(IsEigenstate, Eigenvalue)`; `DiagonalizeHermitian2` per 2×2 |
| `⟨A⟩ = ⟨ψ\|Â\|ψ⟩` | Valore atteso | `ExpectationValue(a, state)` (complesso: la parte immaginaria è ~0 se `Â` è hermitiano) |
| `P̂ = \|a⟩⟨a\|` | Proiettore | `Projector(normalizedKet)` |
| `Û(t) = e^(−iĤt/ħ)` | Evoluzione temporale | `TimeEvolution(h, t, hbar = 1)`; `MatrixExp` |
| `[Â,B̂] = ÂB̂ − B̂Â` | Commutatore | `Commutator(a, b)` |
| `X̂, Ŷ, Ẑ` | Operatori di Pauli | `Pauli<T>.I`, `Pauli<T>.X`, `Pauli<T>.Y`, `Pauli<T>.Z` |
| `α\|0⟩ + β\|1⟩` | Sovrapposizione | `QubitStates.Superposition(alpha, beta)` (normalizza) |
| `ρ` | Matrice densità | `DensityMatrix(ket)`, `Purity(rho)` = Tr(ρ²), `BlochVector(rho)` |
| `x̂, p̂` | Posizione e impulso | **non supportati** (spazio continuo) |

`ComplexMatrix<T>` offre inoltre `+`, `−`, `*` (matrice, scalare complesso, ket), `Dagger()`, `Trace()`, `IsHermitian()`, `IsUnitary()`, `Identity(n)`, `Zero(r, c)`.

## Esempi

```csharp
using MathemaSharp.Quantum;
using static MathemaSharp.Quantum.QuantumAlgorithms;

using System.Numerics;

var psi = QubitStates<double>.Superposition(1, Complex<double>.ImaginaryOne); // (|0⟩ + i|1⟩)/√2

ExpectationValue(Pauli<double>.Y, psi).Real;   // 1: psi è autostato di Y con autovalore +1
Commutator(Pauli<double>.X, Pauli<double>.Y);  // 2i·Z

var u = TimeEvolution(Pauli<double>.X, Math.PI / 2);   // exp(−i·X·π/2)
Apply(u, QubitStates<double>.Zero);                    // [0, −i]: rotazione attorno a X
u.IsUnitary();                                         // true (tolleranza di default √ε del tipo)

// Lo stesso calcolo in Decimal128: errore ~1e-33 invece di ~1e-16.
var u128 = TimeEvolution(Pauli<Decimal128>.X, Decimal128.Pi / Decimal128.CreateChecked(2));

var rho = DensityMatrix(psi);
Purity(rho);                                   // 1: stato puro
BlochVector(rho);                              // (0, 1, 0)
```

Demo eseguibile: `dotnet run --project src/apps/MathemaSharp.Lab -- --topic quantum`.

## Contratti e limiti

- Dimensioni incompatibili → `ArgumentException`. `Normalize` e `CheckEigenstate` rifiutano il vettore nullo. `Trace` su matrice non quadrata → `InvalidOperationException`.
- `DiagonalizeHermitian2` accetta solo matrici 2×2 hermitiane. Autovalori λ± = (a+d)/2 ± √(((a−d)/2)² + |b|²); gli autovettori sono definiti a meno di una fase globale (es. per Y si ottiene −i·|i⟩, fisicamente lo stesso stato).
- `MatrixExp` usa serie di Taylor con scaling-and-squaring (24 termini di default): didattica, O(termini·n³), per matrici piccole e ben scalate. Non è Padé e non è ottimizzata.
- `TimeEvolution` è unitaria solo se `Ĥ` è hermitiano; la libreria non lo verifica, usare `IsHermitian()`/`IsUnitary()`.
- Non incluso: autosolver N×N, prodotto tensore e porte a più qubit, misura con collasso, stati misti generici, operatori continui. Aritmetica in double: gli errori di arrotondamento si accumulano su evoluzioni lunghe.

## Test

`src/tests/MathemaSharp.Quantum.Tests/QuantumTests.cs`: identità di Pauli (`[X,Y] = 2iZ`), spettri noti di X e Z, idempotenza dei proiettori, `U(π) = −I` per `H = Z`, purezza e vettore di Bloch degli stati base. Eseguire con `dotnet test src/MathemaSharp.slnx -c Release`.
