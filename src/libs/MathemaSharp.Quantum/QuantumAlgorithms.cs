using System.Numerics;
using MathemaSharp.Core;

namespace MathemaSharp.Quantum;

/// <summary>
/// Stati come ket, cioè Complex&lt;T&gt;[] / ReadOnlySpan&lt;Complex&lt;T&gt;&gt;; il bra ⟨ψ| non è un tipo a parte, è il coniugato implicito nel prodotto interno.
/// Operatori come ComplexMatrix&lt;T&gt;. Posizione e impulso (x̂, p̂=-iħ d/dx) sono operatori su uno spazio continuo e restano fuori scope: qui si lavora
/// solo su spazi di Hilbert a dimensione finita (qubit, spin, sistemi a N livelli). Generico su <c>T : IFloatingPointIeee754&lt;T&gt;, IMinMaxValue&lt;T&gt;</c>.
/// </summary>
public static class QuantumAlgorithms
{
    /// <summary>⟨φ|ψ⟩: antilineare nel bra, lineare nel ket.</summary>
    public static Complex<T> InnerProduct<T>(ReadOnlySpan<Complex<T>> bra, ReadOnlySpan<Complex<T>> ket) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> =>
        Signals.HermitianDot(bra, ket);

    /// <summary>Norma euclidea con Hypot sulle componenti: evita quadrati prematuri.
    /// Il risultato può essere infinito se la norma stessa supera il range di T.</summary>
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
                throw new ArgumentException("Lo stato deve avere componenti finite.", nameof(ket));
            scale = T.Max(scale, T.Max(T.Abs(value.Real), T.Abs(value.Imaginary)));
        }
        if (scale == T.Zero) throw new ArgumentException("Il vettore nullo non è normalizzabile.", nameof(ket));
        var result = new Complex<T>[ket.Length];
        for (var i = 0; i < ket.Length; i++)
            result[i] = new Complex<T>(ket[i].Real / scale, ket[i].Imaginary / scale);
        var norm = Norm<T>(result);
        if (!T.IsFinite(norm) || norm == T.Zero) throw new ArithmeticException("Norma dello stato scalato non rappresentabile.");
        for (var i = 0; i < result.Length; i++)
            result[i] = new Complex<T>(result[i].Real / norm, result[i].Imaginary / norm);
        return result;
    }

    /// <summary>|ψ⟩⟨φ|: operatore di rango 1.</summary>
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

    /// <summary>⟨ψ|Â|ψ⟩. Per Â hermitiano e stato normalizzato la parte immaginaria è nulla a meno di errori numerici: non viene scartata qui.</summary>
    public static Complex<T> ExpectationValue<T>(ComplexMatrix<T> operatorA, ReadOnlySpan<Complex<T>> state) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> =>
        InnerProduct(state, Apply(operatorA, state));

    /// <summary>Proiettore |a⟩⟨a| su uno stato normalizzato. P²=P, Tr(P)=1.</summary>
    public static ComplexMatrix<T> Projector<T>(ReadOnlySpan<Complex<T>> normalizedKet) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> =>
        OuterProduct(normalizedKet, normalizedKet);

    /// <summary>[Â,B̂] = ÂB̂ − B̂Â. Nullo se e solo se Â e B̂ sono simultaneamente diagonalizzabili (osservabili compatibili).</summary>
    public static ComplexMatrix<T> Commutator<T>(ComplexMatrix<T> a, ComplexMatrix<T> b) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> => a * b - b * a;

    /// <summary>Verifica Â|a⟩ = a|a⟩ con tolleranza di default (√ε del tipo).</summary>
    public static (bool IsEigenstate, Complex<T> Eigenvalue) CheckEigenstate<T>(ComplexMatrix<T> operatorA, ReadOnlySpan<Complex<T>> ket) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> =>
        CheckEigenstate(operatorA, ket, ComplexMatrix<T>.DefaultTolerance);

    /// <summary>
    /// Verifica Â|a⟩ = a|a⟩ con residuo relativo ||Âu−λu||₂/||Â||F ≤ tolerance, u stato normalizzato.
    /// Stato e operatore sono scalati separatamente. L'autovalore è il quoziente di Rayleigh, anche per operatori non hermitiani.
    /// Tolleranza finita non negativa; stato non nullo e componenti finite. Intermedi/autovalori fuori range sono segnalati.
    /// </summary>
    public static (bool IsEigenstate, Complex<T> Eigenvalue) CheckEigenstate<T>(ComplexMatrix<T> operatorA, ReadOnlySpan<Complex<T>> ket, T tolerance) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (!T.IsFinite(tolerance) || tolerance < T.Zero) throw new ArgumentOutOfRangeException(nameof(tolerance));
        if (operatorA.Rows != operatorA.Cols || operatorA.Cols != ket.Length)
            throw new ArgumentException("Operatore quadrato e dimensioni dello stato compatibili richiesti.", nameof(operatorA));
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
            throw new ArithmeticException("Residuo dell'autostato non rappresentabile.");
        var eigenvalue = new Complex<T>(lambda.Real * scale, lambda.Imaginary * scale);
        if (!T.IsFinite(eigenvalue.Real) || !T.IsFinite(eigenvalue.Imaginary))
            throw new ArithmeticException("Autovalore non rappresentabile.");
        return (residualNorm / operatorNorm <= tolerance, eigenvalue);
    }

    /// <summary>
    /// Diagonalizzazione analitica di una matrice hermitiana 2×2 (qubit, spin-1/2): H=[[a,b],[b*,d]], a,d reali.
    /// λ± = (a+d)/2 ± √(((a-d)/2)²+|b|²). Gli autovettori sono definiti a meno di una fase globale, come sempre in meccanica quantistica.
    /// </summary>
    public static (T LambdaPlus, Complex<T>[] VectorPlus, T LambdaMinus, Complex<T>[] VectorMinus) DiagonalizeHermitian2<T>(ComplexMatrix<T> h) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (h.Rows != 2 || h.Cols != 2) throw new ArgumentException("Richiede una matrice 2x2.");
        var scale = T.Zero;
        for (var i = 0; i < 2; i++)
            for (var j = 0; j < 2; j++)
            {
                var value = h[i, j];
                if (!T.IsFinite(value.Real) || !T.IsFinite(value.Imaginary))
                    throw new ArgumentException("La matrice deve avere componenti finite.", nameof(h));
                scale = T.Max(scale, T.Max(T.Abs(value.Real), T.Abs(value.Imaginary)));
            }
        if (!h.IsHermitian()) throw new ArgumentException("La matrice non è hermitiana.");
        if (scale == T.Zero)
            return (T.Zero, [Complex<T>.One, Complex<T>.Zero], T.Zero, [Complex<T>.Zero, Complex<T>.One]);
        if (h[0, 1].Real == T.Zero && h[0, 1].Imaginary == T.Zero)
        {
            // Nessuna sottrazione tra numeri grandi: conserva anche diagonali estremamente sbilanciate.
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
            throw new ArithmeticException("Autovalori non rappresentabili nel tipo richiesto.");
        Complex<T>[] plus;
        if (b.Real == T.Zero && b.Imaginary == T.Zero)
            plus = a >= d ? [Complex<T>.One, Complex<T>.Zero] : [Complex<T>.Zero, Complex<T>.One];
        else
        {
            // Sceglie radius + |halfDiff|, senza cancellazione; non divide per b.
            plus = halfDiff >= T.Zero
                ? Normalize<T>([new Complex<T>(radius + halfDiff, T.Zero), Complex<T>.Conjugate(b)])
                : Normalize<T>([b, new Complex<T>(radius - halfDiff, T.Zero)]);
        }
        // Complemento ortonormale, valido anche per autovalori coincidenti.
        Complex<T>[] minus = [-Complex<T>.Conjugate(plus[1]), Complex<T>.Conjugate(plus[0])];
        return (lambdaPlus, plus, lambdaMinus, minus);
    }

    /// <summary>
    /// Matrice esponenziale via serie di Taylor con scaling-and-squaring: scala la matrice finché la norma è piccola, somma la serie, poi
    /// eleva al quadrato per annullare la scala. Didattica come Signals.Dft: O(terms·n³), non Padé, pensata per matrici piccole e ben scalate.
    /// </summary>
    public static ComplexMatrix<T> MatrixExp<T>(ComplexMatrix<T> m, int taylorTerms = 24) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taylorTerms, 1);
        if (m.Rows != m.Cols) throw new ArgumentException("L'esponenziale richiede una matrice quadrata.");
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
            if (++scalingSteps > 100000) throw new ArithmeticException("Limite di scaling superato.");
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
                    throw new ArithmeticException("Esponenziale matriciale: intermedio non rappresentabile.");
            }
    }

    /// <summary>Û(t) = exp(−iĤt) con ħ = 1.</summary>
    public static ComplexMatrix<T> TimeEvolution<T>(ComplexMatrix<T> hamiltonian, T t) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> =>
        TimeEvolution(hamiltonian, t, T.One);

    /// <summary>Û(t) = exp(−iĤt/ħ). Û è unitaria se Ĥ è hermitiano (controllare con ComplexMatrix.IsUnitary se serve verificarlo).</summary>
    public static ComplexMatrix<T> TimeEvolution<T>(ComplexMatrix<T> hamiltonian, T t, T hbar) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (!T.IsFinite(t)) throw new ArgumentOutOfRangeException(nameof(t));
        if (!T.IsFinite(hbar) || hbar <= T.Zero) throw new ArgumentOutOfRangeException(nameof(hbar));
        var ratio = -t / hbar;
        if (!T.IsFinite(ratio)) throw new ArithmeticException("Rapporto t/hbar non rappresentabile.");
        return MatrixExp(new Complex<T>(T.Zero, ratio) * hamiltonian);
    }

    /// <summary>Matrice densità ρ=|ψ⟩⟨ψ| di uno stato puro normalizzato.</summary>
    public static ComplexMatrix<T> DensityMatrix<T>(ReadOnlySpan<Complex<T>> normalizedKet) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> => Projector(normalizedKet);

    /// <summary>Tr(ρ²): 1 per uno stato puro, &lt;1 per uno stato misto.</summary>
    public static T Purity<T>(ComplexMatrix<T> rho) where T : IFloatingPointIeee754<T>, IMinMaxValue<T> => (rho * rho).Trace().Real;

    /// <summary>Vettore di Bloch (x,y,z)=Tr(ρ·σ) di una matrice densità 2×2. |v|=1 ⇔ stato puro (sulla superficie della sfera di Bloch).</summary>
    public static (T X, T Y, T Z) BlochVector<T>(ComplexMatrix<T> rho) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (rho.Rows != 2 || rho.Cols != 2) throw new ArgumentException("Il vettore di Bloch è definito per qubit (matrici 2x2).");
        return ((rho * Pauli<T>.X).Trace().Real, (rho * Pauli<T>.Y).Trace().Real, (rho * Pauli<T>.Z).Trace().Real);
    }
}
