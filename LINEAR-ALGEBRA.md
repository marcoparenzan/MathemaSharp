# Linear algebra for AI — guide to the infographic

> **Reference images not included.** The infographic this guide is based on is attributed to Greg Coquillo and is third-party material: it is not included in the repository (the guide reports its formulas and APIs in text). The mention of the images `1793027138639.jpg` and `infografica.png` is to be read in this sense.

The image `1793027138639.jpg`, attributed in the image to Greg Coquillo, is a didactic map, not a benchmark. This guide translates its nine panels into readable formulas and C# APIs. The original image remains unchanged.

*(Reference image not included in the repository: third-party material.)*

## 01 — Vectors

A real vector x has n components. Addition and multiplication by a scalar are component-wise operations. The dot product is **x · y = Σ xᵢ yᵢ**.

```csharp
using MathemaSharp.Core;
var dot = NumericAlgorithms.Dot<double>([1, 2], [3, 4]); // 11
```

Use: features and embeddings. **The dot product is not automatically the cosine similarity**: you must divide by the L2 norms and handle null vectors.

## 02 — Matrices

A ∈ ℝᵐˣⁿ transforms x ∈ ℝⁿ into y ∈ ℝᵐ: **y = Ax**. Rows = components of the output, columns = components of the input.

```csharp
using MathemaSharp.LinearAlgebra;
double[,] a = {{1, 2}, {3, 4}};
var y = MatrixAlgebra.Multiply<double>(a, new double[] {1, 2}); // [5, 11]
```

Use: weights of a linear layer (the bias must be added separately).

## 03 — Matrix product

**C = AB**, with A of size m×n and B n×p, produces C m×p. Each element cᵢⱼ is the dot product of row i and column j. In general **AB ≠ BA**; the reverse product may not even be defined.

```csharp
var squared = MatrixAlgebra.Multiply(a, a); // [[7,10],[15,22]]
```

Use: composition of transformations and forward pass. Didactic implementation, not an optimised BLAS.

## 04 — Transpose

**(Aᵀ)ᵢⱼ = Aⱼᵢ**, swaps rows and columns. (AB)ᵀ = BᵀAᵀ.

```csharp
var transpose = MatrixAlgebra.Transpose(a);
```

Use: QKᵀ in attention; this product alone is not enough to implement a Transformer. For complex matrices, the conjugate transpose is a different operation.

## 05 — Determinant

For a 2×2 matrix, **det(A) = ad − bc**. The absolute value measures the area scale factor; the sign indicates the orientation. For real square matrices, det(A) ≠ 0 is equivalent to invertibility in exact arithmetic.

```csharp
double[,] b = {{4, 7}, {2, 6}};
var determinant = MatrixAlgebra.Determinant2x2(b); // 10
```

The API added is only 2×2. The floating-point determinant is not a reliable test of conditioning and can suffer overflow or cancellation.

## 06 — Inverse and linear systems

**A⁻¹A = AA⁻¹ = I**. For 2×2: A⁻¹ = [[d,−b],[−c,a]] / det(A).

```csharp
var inverse = MatrixAlgebra.Inverse2x2(b);
var identity = MatrixAlgebra.Multiply(b, inverse);
// For regression: solve directly, without forming the inverse.
var coefficients = MatrixAlgebra.LeastSquares<double>(
    new double[,] {{1,0}, {1,1}, {1,2}}, new double[] {1,3,5}); // [1,2]
```

The inverse normalises the scale and rejects non-finite elements or a normalised determinant ≤ 32 ULP of 1. It is a didactic safeguard, not an estimate of the condition number. Extreme results may not be representable. `LeastSquares` uses Householder QR with independent columns; not a pseudo-inverse.

## 07 — Eigenvalues and eigenvectors

**Av = λv**, with v ≠ 0. The eigenvector keeps its direction, except for a reversal of sense for λ < 0. A real matrix can have complex eigenvalues and not be diagonalisable.

Example: diag(2,3) has eigenvectors [1,0], [0,1] with eigenvalues 2 and 3. Use: stability, PageRank and PCA on symmetric covariance matrices.

**Status: explanation and mathematical example; no eigensolver added.**

## 08 — Norms

| Norm | Formula | API | Example [3,−4] |
|---|---|---|---|
| L1 | Σ abs(xᵢ) | `NormL1<T>` | 7 |
| L2 | √(Σ xᵢ²) | `NormL2<T>` | 5 |
| L∞ | max abs(xᵢ) | `NormInfinity<T>` | 4 |
| Frobenius | √(Σᵢⱼ aᵢⱼ²) | `FrobeniusNorm<T>` | 5 on diag(3,−4) |

```csharp
var l2 = MatrixAlgebra.NormL2<double>([3, -4]); // 5
```

L2 and Frobenius use `T.Hypot` to avoid premature overflow of the squares. The norms of empty vectors are zero. Use: regularisation and gradient clipping. Frobenius is not the spectral norm.

## 09 — SVD

**A = UΣVᵀ** for real m×n matrices. In the full form U is m×m, Σ m×n, V n×n. The singular values are non-negative and sorted in decreasing order. In the reduced form, with k = min(m,n), the dimensions are m×k, k×k, n×k.

Use: PCA, compression and low-rank approximation. Aᵣ = UᵣΣᵣVᵣᵀ is the best approximation of rank at most r in the Frobenius/spectral norm. LoRA learns low-rank updates but does not require computing an SVD during training.

**Status: explanation; no SVD implemented.** Avoid a naive implementation via AᵀA: it worsens the conditioning. A future integration will have to use a dedicated numerical backend and test reconstruction and orthogonality.

## Generic Math — general LU

`LuFactorization<T>` adds LU with partial pivoting, solver, determinant and inverse for non-empty real square matrices. It is a Numbers kernel independent of NDArray, written with `IFloatingPointIeee754<T>`: no intermediate conversion to `double`. The NDSharp snapshot remains unchanged and outside the solution.

```csharp
using MathemaSharp.LinearAlgebra;
double[,] a = {{0,2,1}, {1,1,0}, {2,0,1}};
var lu = LuFactorization<double>.Create(a);
var x = lu.Solve(new double[] {7,3,5}); // [1,2,3]
var determinant = lu.Determinant(); // -4
var inverse = lu.Inverse();
```

The matrix is normalised by the global scale. Pivots ≤ n × ULP(1) are rejected: a conservative relative policy, not an estimate of the conditioning. `Create` rejects singular/nearly singular matrices, so `Determinant()` is not an API to obtain zero on singular matrices. Reusable factorisation, without mutating the inputs. Overflow and underflow remain possible in the chosen format, also in the determinant and for extreme scales.

The products and the identity now use `INumber<T>` and also accept integers and `decimal`; the transpose requires no numeric constraints. For integers the product does not promise protection from overflow. The decompositions require floating-point numbers: integer division is not suitable. `System.Decimal` does not satisfy `IFloatingPointIeee754<T>`.

**Complete conversion still to do:** FFT, general eigenvalues, wavelets and distributions of the snapshot remain NDSharp implementations, not MathemaSharp Generic Math kernels. The NDArray bridge also keeps using double and explicit conversions; it does not automatically preserve the precision of the IEEE decimal formats. The dynamic NDArray storage was not converted nor were the imported notebooks rerun.

## Verification and limits

The new APIs are in `MathemaSharp.LinearAlgebra.MatrixAlgebra`, next to the operations already present, generic over `IFloatingPointIeee754<T>`. The tests added cover norms, premature L2 overflow, inverse, relative scale and wrong inputs. No benchmarks or executed notebooks were added in this tranche.

```sh
dotnet test src/tests/MathemaSharp.TimeSeries.Tests/MathemaSharp.TimeSeries.Tests.csproj
```
