using System.Numerics;

namespace MathemaSharp.LinearAlgebra;

/// <summary>Soluzione e diagnostica floating-point in norma infinito, non una certificazione dell'errore forward.</summary>
/// <param name="Solution">Soluzione indipendente dagli input.</param>
/// <param name="RelativeResidualInfinity">||Ax-b||∞/||b||∞; zero per residuo e termine noto nulli.</param>
/// <param name="NormwiseBackwardErrorInfinity">||Ax-b||∞/(||A||∞||x||∞+||b||∞).</param>
/// <param name="ConditionInfinityEstimate">||A||∞||A⁻¹||∞ approssimata tramite n solve LU; non una stima iterativa economica.</param>
/// <param name="ReciprocalConditionInfinityEstimate">Reciproco del condizionamento stimato; zero se la stima trabocca.</param>
/// <param name="IsIllConditioned">Avviso: rcond ≤ √epsilon (potenziale perdita di almeno metà delle cifre). Non un giudizio universale di accuratezza.</param>
public sealed record LinearSolveReport<T>(T[] Solution, T RelativeResidualInfinity,
    T NormwiseBackwardErrorInfinity, T ConditionInfinityEstimate,
    T ReciprocalConditionInfinityEstimate, bool IsIllConditioned)
    where T : IFloatingPointIeee754<T>
{
    /// <summary>Alias dell'errore backward normwise in norma infinito.</summary>
    public T NormwiseBackwardError => NormwiseBackwardErrorInfinity;
}

/// <summary>Diagnostica opt-in per piccoli sistemi densi quadrati, interamente in T.</summary>
/// <remarks>Riusa una LU e risolve n sistemi per stimare la norma dell'inversa senza memorizzarla.
/// Il condizionamento stimato dipende dalla qualità della LU. Residuo calcolato in T, non a precisione superiore.
/// Non stima l'errore componente per componente e non esegue refinement. Input e intermedi non rappresentabili sono rifiutati.</remarks>
public static class LinearSystemDiagnostics
{
    public static LinearSolveReport<T> Solve<T>(T[,] matrix, ReadOnlySpan<T> rhs) where T : IFloatingPointIeee754<T>
    {
        var factor = LuFactorization<T>.Create(matrix);
        var n = factor.Order;
        var solution = factor.Solve(rhs);
        foreach (var value in solution)
            if (!T.IsFinite(value)) throw new ArithmeticException("Soluzione non rappresentabile.");
        var scale = T.Zero;
        foreach (var value in matrix) scale = T.Max(scale, T.Abs(value));
        var normalized = new T[n, n];
        var normA = T.Zero;
        for (var i = 0; i < n; i++)
        {
            var rowSum = T.Zero;
            for (var j = 0; j < n; j++)
            {
                normalized[i,j] = matrix[i,j] / scale;
                rowSum += T.Abs(normalized[i,j]);
            }
            normA = T.Max(normA, rowSum);
        }
        if (!T.IsFinite(normA)) throw new ArithmeticException("Norma della matrice normalizzata non rappresentabile.");

        var b = new T[n];
        var normX = MatrixAlgebra.NormInfinity<T>(solution);
        var vectorScale = normX;
        for (var i = 0; i < n; i++)
        {
            b[i] = rhs[i] / scale;
            if (!T.IsFinite(b[i])) throw new ArithmeticException("Termine noto normalizzato non rappresentabile.");
            vectorScale = T.Max(vectorScale, T.Abs(b[i]));
        }
        var residualNorm = T.Zero;
        var normB = T.Zero;
        var backward = T.Zero;
        var relative = T.Zero;
        if (vectorScale > T.Zero)
        {
            for (var i = 0; i < n; i++)
            {
                var value = -b[i] / vectorScale;
                for (var j = 0; j < n; j++)
                    value = T.FusedMultiplyAdd(normalized[i,j], solution[j] / vectorScale, value);
                if (!T.IsFinite(value)) throw new ArithmeticException("Residuo non rappresentabile.");
                residualNorm = T.Max(residualNorm, T.Abs(value));
                normB = T.Max(normB, T.Abs(b[i] / vectorScale));
            }
            var denominator = normA * (normX / vectorScale) + normB;
            if (!T.IsFinite(denominator)) throw new ArithmeticException("Denominatore backward non rappresentabile.");
            backward = denominator == T.Zero ? T.Zero : residualNorm / denominator;
            relative = normB == T.Zero ? (residualNorm == T.Zero ? T.Zero : T.PositiveInfinity) : residualNorm / normB;
        }
        // Abar=A/scale; solve A*z=scale*e_j restituisce colonna j di Abar^-1.
        var inverseRowSums = new T[n];
        var basis = new T[n];
        for (var j = 0; j < n; j++)
        {
            basis[j] = scale;
            var column = factor.Solve(basis);
            basis[j] = T.Zero;
            for (var i = 0; i < n; i++)
            {
                if (!T.IsFinite(column[i])) throw new ArithmeticException("Norma dell'inversa non stimabile nel tipo richiesto.");
                inverseRowSums[i] += T.Abs(column[i]);
            }
        }
        var inverseNorm = MatrixAlgebra.NormInfinity<T>(inverseRowSums);
        var condition = T.Max(T.One, normA * inverseNorm);
        var reciprocal = T.IsPositiveInfinity(condition) ? T.Zero : T.One / condition;
        var epsilon = T.BitIncrement(T.One) - T.One;
        return new(solution, relative, backward, condition, reciprocal, reciprocal <= T.Sqrt(epsilon));
    }
}
