// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp.LinearAlgebra;

/// <summary>Iterative solvers for the Poisson equation ∇²p = b on a regular two-dimensional grid <c>p[j, i]</c> (five-point stencil), for every IEEE Generic Math format.
/// The boundary values are the caller's: the solver updates the interior points <c>1 ≤ j, i ≤ n − 2</c> and, after each sweep, calls the <c>boundary</c> action so that
/// Neumann (copy the neighbour) or Dirichlet (fix a value) conditions are re-imposed in whatever order the caller needs.</summary>
public static class GridPoisson
{
    /// <summary>Successive over-relaxation: <paramref name="iterations"/> sweeps of Gauss-Seidel (row by row, left to right) with the relaxation factor
    /// <paramref name="omega"/> (1 is plain Gauss-Seidel; between 1 and 2 accelerates convergence; 1.7 suits grids of a few tens to a hundred points).
    /// For each interior point: <c>gs = ((p[j,i+1] + p[j,i−1]) dy² + (p[j+1,i] + p[j−1,i]) dx²) / (2 (dx² + dy²)) − dx² dy² / (2 (dx² + dy²)) · b[j,i]</c>,
    /// then <c>p[j,i] += ω (gs − p[j,i])</c>. <paramref name="p"/> is updated in place; it holds the initial guess.</summary>
    /// <param name="p">The unknown, with its boundary rows and columns, on input the initial guess.</param>
    /// <param name="b">The right-hand side (same shape).</param>
    /// <param name="dx">Spacing along the columns (x).</param>
    /// <param name="dy">Spacing along the rows (y).</param>
    /// <param name="omega">Relaxation factor.</param>
    /// <param name="iterations">Number of sweeps.</param>
    /// <param name="boundary">Called after each sweep (including the last) to impose the boundary conditions on <paramref name="p"/>; may be null.</param>
    public static void Sor<T>(T[,] p, T[,] b, T dx, T dy, T omega, int iterations, Action<T[,]>? boundary = null) where T : IFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNull(p);
        ArgumentNullException.ThrowIfNull(b);
        int rows = p.GetLength(0), cols = p.GetLength(1);
        if (b.GetLength(0) != rows || b.GetLength(1) != cols) throw new ArgumentException("p and b must have the same shape.");
        if (iterations < 0) throw new ArgumentOutOfRangeException(nameof(iterations));
        T two = T.CreateChecked(2);
        T dx2 = dx * dx, dy2 = dy * dy, denom = two * (dx2 + dy2);
        for (int it = 0; it < iterations; it++)
        {
            for (int j = 1; j < rows - 1; j++)
                for (int i = 1; i < cols - 1; i++)
                {
                    T gs = ((p[j, i + 1] + p[j, i - 1]) * dy2 + (p[j + 1, i] + p[j - 1, i]) * dx2) / denom
                           - dx2 * dy2 / denom * b[j, i];
                    p[j, i] += omega * (gs - p[j, i]);
                }
            boundary?.Invoke(p);
        }
    }

    /// <summary>Root mean square of the residual b − ∇²p over the interior points: a measure of how far <paramref name="p"/> is from solving the equation.</summary>
    public static T ResidualRms<T>(T[,] p, T[,] b, T dx, T dy) where T : IFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNull(p);
        ArgumentNullException.ThrowIfNull(b);
        int rows = p.GetLength(0), cols = p.GetLength(1);
        if (rows < 3 || cols < 3) return T.Zero;
        T two = T.CreateChecked(2), dx2 = dx * dx, dy2 = dy * dy, sum = T.Zero;
        for (int j = 1; j < rows - 1; j++)
            for (int i = 1; i < cols - 1; i++)
            {
                T lap = (p[j, i + 1] - two * p[j, i] + p[j, i - 1]) / dx2 + (p[j + 1, i] - two * p[j, i] + p[j - 1, i]) / dy2;
                T r = b[j, i] - lap;
                sum += r * r;
            }
        return T.Sqrt(sum / T.CreateChecked((rows - 2) * (cols - 2)));
    }
}
