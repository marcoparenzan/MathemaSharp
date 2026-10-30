using System.Numerics;
using MathemaSharp.Core;

namespace MathemaSharp.Quantum;

/// <summary>
/// States as kets, that is Complex&lt;T&gt;[] / ReadOnlySpan&lt;Complex&lt;T&gt;&gt;; the bra ⟨ψ| is not a separate type, it is the conjugate implicit in the inner product.
/// Operators as ComplexMatrix&lt;T&gt;. Position and momentum (x̂, p̂=-iħ d/dx) are operators on a continuous space and stay out of scope: here we work
/// only on finite-dimensional Hilbert spaces (qubits, spins, N-level systems). Generic over <c>T : IFloatingPointIeee754&lt;T&gt;, IMinMaxValue&lt;T&gt;</c>.
/// </summary>
public static class QuantumAlgorithms
{
    /// <summary>⟨φ|ψ⟩: antilinear in the bra, linear in the ket.</summary>
    public static Complex<T> InnerProduct<T>(ReadOnlySpan<Complex<T>> bra, ReadOnlySpan<Complex<T>> ket) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> =>
        Signals.HermitianDot(bra, ket);

    /// <summary>Euclidean norm with Hypot on the components: avoids premature squares.
    /// The result can be infinite if the norm itself exceeds the range of T.</summary>
    public static T Norm<T>(ReadOnlySpan<Complex<T>> ket) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var norm = T.Zero;
        foreach (var value in ket)
        {
            norm = T.Hypot(norm, value.Real);
            norm = T.Hypot(norm, value.Imaginary);
        }
        return norm;
    }

    public static Complex<T>[] Normalize<T>(ReadOnlySpan<Complex<T>> ket) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var scale = T.Zero;
        foreach (var value in ket)
        {
            if (!T.IsFinite(value.Real) || !T.IsFinite(value.Imaginary))
                throw new ArgumentException("The state must have finite components.", nameof(ket));
            scale = T.Max(scale, T.Max(T.Abs(value.Real), T.Abs(value.Imaginary)));
        }
        if (scale == T.Zero) throw new ArgumentException("The zero vector cannot be normalised.", nameof(ket));
        var result = new Complex<T>[ket.Length];
        for (var i = 0; i < ket.Length; i++)
            result[i] = new Complex<T>(ket[i].Real / scale, ket[i].Imaginary / scale);
        var norm = Norm<T>(result);
        if (!T.IsFinite(norm) || norm == T.Zero) throw new ArithmeticException("Norm of the scaled state not representable.");
        for (var i = 0; i < result.Length; i++)
            result[i] = new Complex<T>(result[i].Real / norm, result[i].Imaginary / norm);
        return result;
    }

    /// <summary>|ψ⟩⟨φ|: rank-1 operator.</summary>
    public static ComplexMatrix<T> OuterProduct<T>(ReadOnlySpan<Complex<T>> ket, ReadOnlySpan<Complex<T>> bra) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var result = new ComplexMatrix<T>(ket.Length, bra.Length);
        for (var i = 0; i < ket.Length; i++)
            for (var j = 0; j < bra.Length; j++)
                result[i, j] = ket[i] * Complex<T>.Conjugate(bra[j]);
        return result;
    }

    /// <summary>Â|ψ⟩.</summary>
    public static Complex<T>[] Apply<T>(ComplexMatrix<T> operatorA, ReadOnlySpan<Complex<T>> ket) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> => operatorA * ket;

    /// <summary>⟨ψ|Â|ψ⟩. For a Hermitian Â and a normalised state the imaginary part is zero up to numerical errors: it is not discarded here.</summary>
    public static Complex<T> ExpectationValue<T>(ComplexMatrix<T> operatorA, ReadOnlySpan<Complex<T>> state) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> =>
        InnerProduct(state, Apply(operatorA, state));

    /// <summary>Proiettore |a⟩⟨a| su uno stato normalizzato. P²=P, Tr(P)=1.</summary>
    public static ComplexMatrix<T> Projector<T>(ReadOnlySpan<Complex<T>> normalizedKet) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> =>
        OuterProduct(normalizedKet, normalizedKet);

    /// <summary>[Â,B̂] = ÂB̂ − B̂Â. Zero if and only if Â and B̂ are simultaneously diagonalisable (compatible observables).</summary>
    public static ComplexMatrix<T> Commutator<T>(ComplexMatrix<T> a, ComplexMatrix<T> b) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> => a * b - b * a;

    /// <summary>Verifica Â|a⟩ = a|a⟩ con tolleranza di default (√ε del tipo).</summary>
    public static (bool IsEigenstate, Complex<T> Eigenvalue) CheckEigenstate<T>(ComplexMatrix<T> operatorA, ReadOnlySpan<Complex<T>> ket) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> =>
        CheckEigenstate(operatorA, ket, ComplexMatrix<T>.DefaultTolerance);

    /// <summary>
    /// Verifica Â|a⟩ = a|a⟩ con residuo relativo ||Âu−λu||₂/||Â||F ≤ tolerance, u stato normalizzato.
    /// State and operator are scaled separately. The eigenvalue is the Rayleigh quotient, also for non-Hermitian operators.
    /// Finite non-negative tolerance; non-null state and finite components. Out-of-range intermediates/eigenvalues are reported.
    /// </summary>
    public static (bool IsEigenstate, Complex<T> Eigenvalue) CheckEigenstate<T>(ComplexMatrix<T> operatorA, ReadOnlySpan<Complex<T>> ket, T tolerance) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (!T.IsFinite(tolerance) || tolerance < T.Zero) throw new ArgumentOutOfRangeException(nameof(tolerance));
        if (operatorA.Rows != operatorA.Cols || operatorA.Cols != ket.Length)
            throw new ArgumentException("A square operator and compatible state dimensions are required.", nameof(operatorA));
        var unit = Normalize(ket);
        var scale = T.Zero;
        for (var i = 0; i < operatorA.Rows; i++)
            for (var j = 0; j < operatorA.Cols; j++)
            {
                var value = operatorA[i, j];
                if (!T.IsFinite(value.Real) || !T.IsFinite(value.Imaginary))
                    throw new ArgumentException("Operatore finito richiesto.", nameof(operatorA));
                scale = T.Max(scale, T.Max(T.Abs(value.Real), T.Abs(value.Imaginary)));
            }
        if (scale == T.Zero) return (true, Complex<T>.Zero);
        var scaled = new ComplexMatrix<T>(operatorA.Rows, operatorA.Cols);
        var operatorNorm = T.Zero;
        for (var i = 0; i < scaled.Rows; i++)
            for (var j = 0; j < scaled.Cols; j++)
            {
                var value = operatorA[i, j];
                var entry = new Complex<T>(value.Real / scale, value.Imaginary / scale);
                scaled[i, j] = entry;
                operatorNorm = T.Hypot(T.Hypot(operatorNorm, entry.Real), entry.Imaginary);
            }
        var applied = Apply(scaled, unit);
        var lambda = InnerProduct<T>(unit, applied) / InnerProduct<T>(unit, unit).Real;
        var residual = new Complex<T>[unit.Length];
        for (var i = 0; i < unit.Length; i++) residual[i] = applied[i] - lambda * unit[i];
        var residualNorm = Norm<T>(residual);
        if (!T.IsFinite(residualNorm) || !T.IsFinite(operatorNorm))
            throw new ArithmeticException("Eigenstate residual not representable.");
        var eigenvalue = new Complex<T>(lambda.Real * scale, lambda.Imaginary * scale);
        if (!T.IsFinite(eigenvalue.Real) || !T.IsFinite(eigenvalue.Imaginary))
            throw new ArithmeticException("Eigenvalue not representable.");
        return (residualNorm / operatorNorm <= tolerance, eigenvalue);
    }

    /// <summary>
    /// Analytic diagonalisation of a 2×2 Hermitian matrix (qubit, spin-1/2): H=[[a,b],[b*,d]], a,d real.
    /// λ± = (a+d)/2 ± √(((a-d)/2)²+|b|²). The eigenvectors are defined up to a global phase, as always in quantum mechanics.
    /// </summary>
    public static (T LambdaPlus, Complex<T>[] VectorPlus, T LambdaMinus, Complex<T>[] VectorMinus) DiagonalizeHermitian2<T>(ComplexMatrix<T> h) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (h.Rows != 2 || h.Cols != 2) throw new ArgumentException("A 2x2 matrix is required.");
        var scale = T.Zero;
        for (var i = 0; i < 2; i++)
            for (var j = 0; j < 2; j++)
            {
                var value = h[i, j];
                if (!T.IsFinite(value.Real) || !T.IsFinite(value.Imaginary))
                    throw new ArgumentException("The matrix must have finite components.", nameof(h));
                scale = T.Max(scale, T.Max(T.Abs(value.Real), T.Abs(value.Imaginary)));
            }
        if (!h.IsHermitian()) throw new ArgumentException("The matrix is not Hermitian.");
        if (scale == T.Zero)
            return (T.Zero, [Complex<T>.One, Complex<T>.Zero], T.Zero, [Complex<T>.Zero, Complex<T>.One]);
        if (h[0, 1].Real == T.Zero && h[0, 1].Imaginary == T.Zero)
        {
            // No subtraction between large numbers: it also preserves extremely unbalanced diagonals.
            var first = h[0, 0].Real;
            var second = h[1, 1].Real;
            return first >= second
                ? (first, [Complex<T>.One, Complex<T>.Zero], second, [Complex<T>.Zero, Complex<T>.One])
                : (second, [Complex<T>.Zero, Complex<T>.One], first, [Complex<T>.One, Complex<T>.Zero]);
        }
        var two = T.CreateChecked(2);
        var a = h[0, 0].Real / scale;
        var d = h[1, 1].Real / scale;
        var b = new Complex<T>(h[0, 1].Real / scale, h[0, 1].Imaginary / scale);
        var halfDiff = a / two - d / two;
        var radius = T.Hypot(halfDiff, T.Hypot(b.Real, b.Imaginary));
        var mean = a / two + d / two;
        var determinant = T.FusedMultiplyAdd(a, d, -b.Real * b.Real);
        determinant = T.FusedMultiplyAdd(-b.Imaginary, b.Imaginary, determinant);
        T plusValue, minusValue;
        if (mean >= T.Zero)
        {
            plusValue = mean + radius;
            minusValue = plusValue == T.Zero ? T.Zero : determinant / plusValue;
        }
        else
        {
            minusValue = mean - radius;
            plusValue = minusValue == T.Zero ? T.Zero : determinant / minusValue;
        }
        var lambdaPlus = plusValue * scale;
        var lambdaMinus = minusValue * scale;
        if (!T.IsFinite(lambdaPlus) || !T.IsFinite(lambdaMinus))
            throw new ArithmeticException("Eigenvalues not representable in the requested type.");
        Complex<T>[] plus;
        if (b.Real == T.Zero && b.Imaginary == T.Zero)
            plus = a >= d ? [Complex<T>.One, Complex<T>.Zero] : [Complex<T>.Zero, Complex<T>.One];
        else
        {
            // Chooses radius + |halfDiff|, without cancellation; it does not divide by b.
            plus = halfDiff >= T.Zero
                ? Normalize<T>([new Complex<T>(radius + halfDiff, T.Zero), Complex<T>.Conjugate(b)])
                : Normalize<T>([b, new Complex<T>(radius - halfDiff, T.Zero)]);
        }
        // Orthonormal complement, also valid for coincident eigenvalues.
        Complex<T>[] minus = [-Complex<T>.Conjugate(plus[1]), Complex<T>.Conjugate(plus[0])];
        return (lambdaPlus, plus, lambdaMinus, minus);
    }

    /// <summary>
    /// Matrix exponential via Taylor series with scaling-and-squaring: scales the matrix until the norm is small, sums the series, then
    /// squares to undo the scaling. Educational like Signals.Dft: O(terms·n³), not Padé, meant for small, well-scaled matrices.
    /// </summary>
    public static ComplexMatrix<T> MatrixExp<T>(ComplexMatrix<T> m, int taylorTerms = 24) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taylorTerms, 1);
        if (m.Rows != m.Cols) throw new ArgumentException("The exponential requires a square matrix.");
        var scaled = new ComplexMatrix<T>(m.Rows, m.Cols);
        for (var i = 0; i < m.Rows; i++)
            for (var j = 0; j < m.Cols; j++)
            {
                var value = m[i, j];
                if (!T.IsFinite(value.Real) || !T.IsFinite(value.Imaginary))
                    throw new ArgumentException("L'esponenziale richiede componenti finite.", nameof(m));
                scaled[i, j] = value;
            }
        var two = T.CreateChecked(2);
        var half = T.One / two;
        var scalingSteps = 0;
        while (FrobeniusNorm(scaled) > half)
        {
            if (++scalingSteps > 100000) throw new ArithmeticException("Scaling limit exceeded.");
            for (var i = 0; i < scaled.Rows; i++)
                for (var j = 0; j < scaled.Cols; j++)
                {
                    var value = scaled[i, j];
                    scaled[i, j] = new Complex<T>(value.Real / two, value.Imaginary / two);
                }
        }
        var term = ComplexMatrix<T>.Identity(m.Rows);
        var sum = ComplexMatrix<T>.Identity(m.Rows);
        for (var k = 0; k < taylorTerms; k++)
        {
            var denominator = T.CreateChecked(k + 1);
            term = term * scaled;
            for (var i = 0; i < term.Rows; i++)
                for (var j = 0; j < term.Cols; j++)
                {
                    var value = term[i, j];
                    term[i, j] = new Complex<T>(value.Real / denominator, value.Imaginary / denominator);
                }
            sum += term;
            RequireFinite(sum);
        }
        for (var i = 0; i < scalingSteps; i++)
        {
            sum *= sum;
            RequireFinite(sum);
        }
        return sum;
    }

    private static T FrobeniusNorm<T>(ComplexMatrix<T> m) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var norm = T.Zero;
        for (var i = 0; i < m.Rows; i++)
            for (var j = 0; j < m.Cols; j++)
            {
                var value = m[i, j];
                norm = T.Hypot(T.Hypot(norm, value.Real), value.Imaginary);
            }
        return norm;
    }

    private static void RequireFinite<T>(ComplexMatrix<T> matrix) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        for (var i = 0; i < matrix.Rows; i++)
            for (var j = 0; j < matrix.Cols; j++)
            {
                var value = matrix[i, j];
                if (!T.IsFinite(value.Real) || !T.IsFinite(value.Imaginary))
                    throw new ArithmeticException("Matrix exponential: intermediate not representable.");
            }
    }

    /// <summary>Û(t) = exp(−iĤt) with ħ = 1.</summary>
    public static ComplexMatrix<T> TimeEvolution<T>(ComplexMatrix<T> hamiltonian, T t) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> =>
        TimeEvolution(hamiltonian, t, T.One);

    /// <summary>Û(t) = exp(−iĤt/ħ). Û is unitary if Ĥ is Hermitian (check with ComplexMatrix.IsUnitary if you need to verify it).</summary>
    public static ComplexMatrix<T> TimeEvolution<T>(ComplexMatrix<T> hamiltonian, T t, T hbar) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (!T.IsFinite(t)) throw new ArgumentOutOfRangeException(nameof(t));
        if (!T.IsFinite(hbar) || hbar <= T.Zero) throw new ArgumentOutOfRangeException(nameof(hbar));
        var ratio = -t / hbar;
        if (!T.IsFinite(ratio)) throw new ArithmeticException("Ratio t/hbar not representable.");
        return MatrixExp(new Complex<T>(T.Zero, ratio) * hamiltonian);
    }

    /// <summary>Density matrix ρ=|ψ⟩⟨ψ| of a normalised pure state.</summary>
    public static ComplexMatrix<T> DensityMatrix<T>(ReadOnlySpan<Complex<T>> normalizedKet) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> => Projector(normalizedKet);

    /// <summary>Tr(ρ²): 1 for a pure state, &lt;1 for a mixed state.</summary>
    public static T Purity<T>(ComplexMatrix<T> rho) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> => (rho * rho).Trace().Real;

    /// <summary>Bloch vector (x,y,z)=Tr(ρ·σ) of a 2×2 density matrix. |v|=1 ⇔ pure state (on the surface of the Bloch sphere).</summary>
    public static (T X, T Y, T Z) BlochVector<T>(ComplexMatrix<T> rho) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (rho.Rows != 2 || rho.Cols != 2) throw new ArgumentException("The Bloch vector is defined for qubits (2x2 matrices).");
        return ((rho * Pauli<T>.X).Trace().Real, (rho * Pauli<T>.Y).Trace().Real, (rho * Pauli<T>.Z).Trace().Real);
    }
}
