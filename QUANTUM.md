# MathemaSharp.Quantum — finite-dimensional quantum mathematics

Library (NuGet package) in `src/libs/MathemaSharp.Quantum` (depends only on `MathemaSharp.Core`, target net11.0). It covers the bra-ket formalism on finite Hilbert spaces: qubits, spin-1/2, N-level systems. All the arithmetic is .NET 11's `Complex<T>`, generic over `T : IFloatingPointIeee754<T>, IMinMaxValue<T>`: `float`, `double`, `Decimal32/64/128`. The methods are generic (`QuantumAlgorithms.TimeEvolution(h, t)` infers `T`); the operators and states are `Pauli<T>`, `QubitStates<T>`, `ComplexMatrix<T>`.

## Conventions

- **Ket** `|ψ⟩` = `Complex<T>[]` / `ReadOnlySpan<Complex<T>>`. There is no Bra type: `⟨φ|` is the conjugate implicit in the inner product (antilinear on the left, linear on the right, like `Signals.HermitianDot`).
- **Operator** `Â` = `ComplexMatrix<T>` (dense square matrix). The backing array is shared between copies of the struct: do not mutate an alias.
- Computational basis `{|0⟩, |1⟩}`: eigenstates of Z with eigenvalues +1 and −1.
- States must be normalised where indicated (`Projector`, `DensityMatrix`): use `Normalize`.
- The tolerances of the boolean checks are absolute and relative, default `1e-9`.

## From symbol to API

| Symbol | Meaning | API |
|---|---|---|
| `\|ψ⟩` | Ket | `Complex[]`; basis states in `QubitStates<T>` (`Zero`, `One`, `Plus`, `Minus`, `PlusI`, `MinusI`) |
| `⟨ψ\|` | Bra | conjugate implicit in the inner product |
| `⟨φ\|ψ⟩` | Inner product | `QuantumAlgorithms.InnerProduct(bra, ket)`; norm: `Norm`, `Normalize` |
| `\|ψ⟩⟨φ\|` | Outer product | `OuterProduct(ket, bra)` → `ComplexMatrix<T>` |
| `Â` | Operator | `ComplexMatrix<T>`; `Â\|ψ⟩` with `Apply(a, ket)` or `a * ket` |
| `Â\|a⟩ = a\|a⟩` | Eigenvalue equation | `CheckEigenstate(a, ket)` → `(IsEigenstate, Eigenvalue)`; `DiagonalizeHermitian2` for 2×2 |
| `⟨A⟩ = ⟨ψ\|Â\|ψ⟩` | Expectation value | `ExpectationValue(a, state)` (complex: the imaginary part is ~0 if `Â` is Hermitian) |
| `P̂ = \|a⟩⟨a\|` | Projector | `Projector(normalizedKet)` |
| `Û(t) = e^(−iĤt/ħ)` | Time evolution | `TimeEvolution(h, t, hbar = 1)`; `MatrixExp` |
| `[Â,B̂] = ÂB̂ − B̂Â` | Commutator | `Commutator(a, b)` |
| `X̂, Ŷ, Ẑ` | Pauli operators | `Pauli<T>.I`, `Pauli<T>.X`, `Pauli<T>.Y`, `Pauli<T>.Z` |
| `α\|0⟩ + β\|1⟩` | Superposition | `QubitStates.Superposition(alpha, beta)` (normalises) |
| `ρ` | Density matrix | `DensityMatrix(ket)`, `Purity(rho)` = Tr(ρ²), `BlochVector(rho)` |
| `x̂, p̂` | Position and momentum | **not supported** (continuous space) |

`ComplexMatrix<T>` also offers `+`, `−`, `*` (matrix, complex scalar, ket), `Dagger()`, `Trace()`, `IsHermitian()`, `IsUnitary()`, `Identity(n)`, `Zero(r, c)`.

## Examples

```csharp
using MathemaSharp.Quantum;
using static MathemaSharp.Quantum.QuantumAlgorithms;

using System.Numerics;

var psi = QubitStates<double>.Superposition(1, Complex<double>.ImaginaryOne); // (|0⟩ + i|1⟩)/√2

ExpectationValue(Pauli<double>.Y, psi).Real;   // 1: psi is an eigenstate of Y with eigenvalue +1
Commutator(Pauli<double>.X, Pauli<double>.Y);  // 2i·Z

var u = TimeEvolution(Pauli<double>.X, Math.PI / 2);   // exp(−i·X·π/2)
Apply(u, QubitStates<double>.Zero);                    // [0, −i]: rotation about X
u.IsUnitary();                                         // true (default tolerance √ε of the type)

// The same calculation in Decimal128: error ~1e-33 instead of ~1e-16.
var u128 = TimeEvolution(Pauli<Decimal128>.X, Decimal128.Pi / Decimal128.CreateChecked(2));

var rho = DensityMatrix(psi);
Purity(rho);                                   // 1: pure state
BlochVector(rho);                              // (0, 1, 0)
```

Runnable demo: `dotnet run --project src/apps/MathemaSharp.Lab -- --topic quantum`.

## Contracts and limits

- Incompatible dimensions → `ArgumentException`. `Normalize` and `CheckEigenstate` reject the null vector. `Trace` on a non-square matrix → `InvalidOperationException`.
- `DiagonalizeHermitian2` accepts only 2×2 Hermitian matrices. Eigenvalues λ± = (a+d)/2 ± √(((a−d)/2)² + |b|²); the eigenvectors are defined up to a global phase (e.g. for Y one gets −i·|i⟩, physically the same state).
- `MatrixExp` uses a Taylor series with scaling-and-squaring (24 terms by default): didactic, O(terms·n³), for small, well-scaled matrices. It is not Padé and not optimised.
- `TimeEvolution` is unitary only if `Ĥ` is Hermitian; the library does not check it, use `IsHermitian()`/`IsUnitary()`.
- Not included: N×N eigensolver, tensor product and multi-qubit gates, measurement with collapse, generic mixed states, continuous operators. Arithmetic in double: rounding errors accumulate over long evolutions.

## Tests

`src/tests/MathemaSharp.Quantum.Tests/QuantumTests.cs`: Pauli identities (`[X,Y] = 2iZ`), known spectra of X and Z, idempotence of the projectors, `U(π) = −I` for `H = Z`, purity and Bloch vector of the basis states. Run with `dotnet test src/MathemaSharp.slnx -c Release`.
