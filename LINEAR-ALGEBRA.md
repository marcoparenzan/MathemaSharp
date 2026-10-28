# Algebra lineare per AI — guida all’infografica

L’immagine `1793027138639.jpg`, attribuita nell’immagine a Greg Coquillo, è una mappa didattica, non un benchmark. Questa guida ne traduce i nove riquadri in formule leggibili e API C#. L’immagine originale rimane invariata.

![Linear Algebra Formulas for AI — infografica di riferimento](refs/linear-algebra/infografica.png)

## 01 — Vettori

Un vettore reale x ha n componenti. Somma e moltiplicazione per scalare sono operazioni componente per componente. Il prodotto scalare è **x · y = Σ xᵢ yᵢ**.

```csharp
using MathemaSharp.Core;
var dot = NumericAlgorithms.Dot<double>([1, 2], [3, 4]); // 11
```

Uso: feature ed embedding. **Il prodotto scalare non è automaticamente la similarità coseno**: occorre dividere per le norme L2 e gestire i vettori nulli.

## 02 — Matrici

A ∈ ℝᵐˣⁿ trasforma x ∈ ℝⁿ in y ∈ ℝᵐ: **y = Ax**. Righe = componenti dell’uscita, colonne = componenti dell’ingresso.

```csharp
using MathemaSharp.LinearAlgebra;
double[,] a = {{1, 2}, {3, 4}};
var y = MatrixAlgebra.Multiply<double>(a, new double[] {1, 2}); // [5, 11]
```

Uso: pesi di uno strato lineare (il bias va aggiunto separatamente).

## 03 — Prodotto matriciale

**C = AB**, con A di dimensione m×n e B n×p, produce C m×p. Ogni elemento cᵢⱼ è il prodotto scalare tra riga i e colonna j. In generale **AB ≠ BA**; il prodotto inverso può persino non essere definito.

```csharp
var squared = MatrixAlgebra.Multiply(a, a); // [[7,10],[15,22]]
```

Uso: composizione di trasformazioni e forward pass. Implementazione didattica, non una BLAS ottimizzata.

## 04 — Trasposta

**(Aᵀ)ᵢⱼ = Aⱼᵢ**, scambia righe e colonne. (AB)ᵀ = BᵀAᵀ.

```csharp
var transpose = MatrixAlgebra.Transpose(a);
```

Uso: QKᵀ nell’attenzione; non basta questo prodotto per implementare un Transformer. Per matrici complesse, la trasposta coniugata è un’operazione diversa.

## 05 — Determinante

Per una matrice 2×2, **det(A) = ad − bc**. Il valore assoluto misura il fattore di scala dell’area; il segno indica l’orientamento. Per matrici quadrate reali, det(A) ≠ 0 equivale all’invertibilità in aritmetica esatta.

```csharp
double[,] b = {{4, 7}, {2, 6}};
var determinant = MatrixAlgebra.Determinant2x2(b); // 10
```

L’API aggiunta è solo 2×2. Il determinante floating-point non è un test affidabile del condizionamento e può subire overflow o cancellazione.

## 06 — Inversa e sistemi lineari

**A⁻¹A = AA⁻¹ = I**. Per 2×2: A⁻¹ = [[d,−b],[−c,a]] / det(A).

```csharp
var inverse = MatrixAlgebra.Inverse2x2(b);
var identity = MatrixAlgebra.Multiply(b, inverse);
// Per regressione: risolvere direttamente, senza formare l’inversa.
var coefficients = MatrixAlgebra.LeastSquares<double>(
    new double[,] {{1,0}, {1,1}, {1,2}}, new double[] {1,3,5}); // [1,2]
```

L’inversa normalizza la scala e rifiuta elementi non finiti o un determinante normalizzato ≤ 32 ULP di 1. È una salvaguardia didattica, non una stima del numero di condizionamento. Risultati estremi possono non essere rappresentabili. `LeastSquares` usa QR Householder con colonne indipendenti; non una pseudo-inversa.

## 07 — Autovalori e autovettori

**Av = λv**, con v ≠ 0. L’autovettore conserva la propria direzione, salvo inversione del verso per λ < 0. Una matrice reale può avere autovalori complessi e non essere diagonalizzabile.

Esempio: diag(2,3) ha autovettori [1,0], [0,1] con autovalori 2 e 3. Uso: stabilità, PageRank e PCA su matrici di covarianza simmetriche.

**Stato: spiegazione ed esempio matematico; nessun eigensolver aggiunto.**

## 08 — Norme

| Norma | Formula | API | Esempio [3,−4] |
|---|---|---|---|
| L1 | Σ abs(xᵢ) | `NormL1<T>` | 7 |
| L2 | √(Σ xᵢ²) | `NormL2<T>` | 5 |
| L∞ | max abs(xᵢ) | `NormInfinity<T>` | 4 |
| Frobenius | √(Σᵢⱼ aᵢⱼ²) | `FrobeniusNorm<T>` | 5 su diag(3,−4) |

```csharp
var l2 = MatrixAlgebra.NormL2<double>([3, -4]); // 5
```

L2 e Frobenius usano `T.Hypot` per evitare l’overflow prematuro dei quadrati. Le norme dei vettori vuoti sono zero. Uso: regolarizzazione e gradient clipping. Frobenius non è la norma spettrale.

## 09 — SVD

**A = UΣVᵀ** per matrici reali m×n. Nella forma completa U è m×m, Σ m×n, V n×n. I valori singolari sono non negativi e ordinati in senso decrescente. Nella forma ridotta, con k = min(m,n), le dimensioni sono m×k, k×k, n×k.

Uso: PCA, compressione e approssimazione a basso rango. Aᵣ = UᵣΣᵣVᵣᵀ è la migliore approssimazione di rango al più r in norma Frobenius/spettrale. LoRA apprende aggiornamenti a basso rango ma non richiede di calcolare una SVD durante l’addestramento.

**Stato: spiegazione; nessuna SVD implementata.** Evitare un’implementazione ingenua via AᵀA: peggiora il condizionamento. Una futura integrazione dovrà usare un backend numerico dedicato e testare la ricostruzione e l’ortogonalità.

## Generic Math — LU generale

`LuFactorization<T>` aggiunge LU con pivoting parziale, solver, determinante e inversa per matrici quadrate reali non vuote. È un kernel Numbers indipendente da NDArray, scritto con `IFloatingPointIeee754<T>`: nessuna conversione intermedia a `double`. Lo snapshot NDSharp rimane immutato e fuori dalla soluzione.

```csharp
using MathemaSharp.LinearAlgebra;
double[,] a = {{0,2,1}, {1,1,0}, {2,0,1}};
var lu = LuFactorization<double>.Create(a);
var x = lu.Solve(new double[] {7,3,5}); // [1,2,3]
var determinant = lu.Determinant(); // -4
var inverse = lu.Inverse();
```

La matrice è normalizzata per la scala globale. Pivot ≤ n × ULP(1) sono rifiutati: policy conservativa relativa, non stima del condizionamento. `Create` rifiuta matrici singolari/quasi singolari, quindi `Determinant()` non è un’API per ottenere zero su matrici singolari. Fattorizzazione riutilizzabile, senza mutare gli input. Overflow e underflow restano possibili nel formato scelto, anche nel determinante e per scale estreme.

I prodotti e l’identità usano ora `INumber<T>` e accettano anche interi e `decimal`; trasposta non richiede vincoli numerici. Per gli interi il prodotto non promette protezione dall’overflow. Le decomposizioni richiedono numeri floating-point: la divisione intera non è adatta. `System.Decimal` non soddisfa `IFloatingPointIeee754<T>`.

**Conversione completa ancora da fare:** FFT, autovalori generali, wavelet e distribuzioni dello snapshot restano implementazioni NDSharp, non kernel Generic Math MathemaSharp. Anche il ponte NDArray continua a usare double e conversioni esplicite; non preserva automaticamente la precisione dei formati decimali IEEE. Non è stato convertito lo storage dinamico NDArray né sono stati rieseguiti i notebook importati.

## Verifica e limiti

Le nuove API sono in `MathemaSharp.LinearAlgebra.MatrixAlgebra`, accanto alle operazioni già presenti, generiche su `IFloatingPointIeee754<T>`. I test aggiunti coprono norme, overflow prematuro L2, inversa, scala relativa e input errati. Non sono stati aggiunti benchmark o notebook eseguiti in questa tranche.

```sh
dotnet test src/tests/MathemaSharp.TimeSeries.Tests/MathemaSharp.TimeSeries.Tests.csproj
```
