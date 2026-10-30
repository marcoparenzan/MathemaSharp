using System.Numerics;

namespace MathemaSharp.LinearAlgebra;

/// <summary>Solution and floating-point diagnostics in the infinity norm, not a certification of the forward error.</summary>
/// <param name="Solution">Soluzione indipendente dagli input.</param>
/// <param name="RelativeResidualInfinity">||Ax-b||∞/||b||∞; zero for a zero residual and right-hand side.</param>
/// <param name="NormwiseBackwardErrorInfinity">||Ax-b||∞/(||A||∞||x||∞+||b||∞).</param>
/// <param name="ConditionInfinityEstimate">||A||∞||A⁻¹||∞ approximated through n LU solves; not a cheap iterative estimate.</param>
/// <param name="ReciprocalConditionInfinityEstimate">Reciproco del condizionamento stimato; zero se la stima trabocca.</param>
/// <param name="IsIllConditioned">Warning: rcond ≤ √epsilon (potential loss of at least half of the digits). Not a universal judgement of accuracy.</param>
public sealed record LinearSolveReport<T>(T[] Solution, T RelativeResidualInfinity,
    T NormwiseBackwardErrorInfinity, T ConditionInfinityEstimate,
    T ReciprocalConditionInfinityEstimate, bool IsIllConditioned)
    where T : IFloatingPointIeee754<T>
{
    /// <summary>Alias of the normwise backward error in the infinity norm.</summary>
    public T NormwiseBackwardError => NormwiseBackwardErrorInfinity;
}

/// <summary>Diagnostica opt-in per piccoli sistemi densi quadrati, interamente in T.</summary>
/// <remarks>Reuses an LU and solves n systems to estimate the norm of the inverse without storing it.
/// The estimated conditioning depends on the quality of the LU. Residual computed in T, not at higher precision.
/// It does not estimate the error component by component and performs no refinement. Unrepresentable inputs and intermediates are rejected.</remarks>
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
        if (!T.IsFinite(normA)) throw new ArithmeticException("Norm of the normalised matrix not representable.");

        var b = new T[n];
        var normX = MatrixAlgebra.NormInfinity<T>(solution);
        var vectorScale = normX;
        for (var i = 0; i < n; i++)
        {
            b[i] = rhs[i] / scale;
            if (!T.IsFinite(b[i])) throw new ArithmeticException("Normalised right-hand side not representable.");
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
            if (!T.IsFinite(denominator)) throw new ArithmeticException("Backward denominator not representable.");
            backward = denominator == T.Zero ? T.Zero : residualNorm / denominator;
            relative = normB == T.Zero ? (residualNorm == T.Zero ? T.Zero : T.PositiveInfinity) : residualNorm / normB;
        }
        // Abar=A/scale; solving A*z=scale*e_j returns column j of Abar^-1.
        var inverseRowSums = new T[n];
        var basis = new T[n];
        for (var j = 0; j < n; j++)
        {
            basis[j] = scale;
            var column = factor.Solve(basis);
            basis[j] = T.Zero;
            for (var i = 0; i < n; i++)
            {
                if (!T.IsFinite(column[i])) throw new ArithmeticException("Norm of the inverse cannot be estimated in the requested type.");
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
