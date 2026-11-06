// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MathemaSharp.Kernels;

/// <summary>Finite-difference stencils on a regular two-dimensional grid stored as <c>T[j, i]</c> (<c>j</c> = row, the y axis; <c>i</c> = column, the x axis), for every IEEE
/// Generic Math format. Each method reads the neighbours of an interior point (<c>1 ≤ j, i ≤ n − 2</c>); the caller loops and decides what happens at the boundary.
/// They are the building blocks of the finite-difference solvers in <c>MathemaSharp.Fluids</c> and of any other PDE on a grid.
/// The expressions keep a fixed order of operations, so the result for <c>double</c> is reproducible bit for bit.</summary>
public static class GridStencils
{
    /// <summary>Central difference along x: (f[j, i+1] − f[j, i−1]) / (2h). <paramref name="twoH"/> is the precomputed 2h.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CentralX<T>(T[,] f, int j, int i, T twoH) where T : IFloatingPointIeee754<T> => (f[j, i + 1] - f[j, i - 1]) / twoH;

    /// <summary>Central difference along y: (f[j+1, i] − f[j−1, i]) / (2h).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CentralY<T>(T[,] f, int j, int i, T twoH) where T : IFloatingPointIeee754<T> => (f[j + 1, i] - f[j - 1, i]) / twoH;

    /// <summary>Five-point Laplacian: (f[j,i+1] − 2f + f[j,i−1]) / hx² + (f[j+1,i] − 2f + f[j−1,i]) / hy², with <paramref name="hx2"/> and <paramref name="hy2"/> the squared spacings.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Laplacian<T>(T[,] f, int j, int i, T hx2, T hy2) where T : IFloatingPointIeee754<T>
    {
        T two = T.CreateChecked(2), u = f[j, i];
        return (f[j, i + 1] - two * u + f[j, i - 1]) / hx2 + (f[j + 1, i] - two * u + f[j - 1, i]) / hy2;
    }

    /// <summary>The convective term (u·∇)f with a hybrid scheme: central differences where the cell Péclet number |u|h/ν is below <paramref name="pecletLimit"/>
    /// (2 for a second-order accurate, non-oscillating central scheme), first-order upwind otherwise (stable at high Reynolds number).</summary>
    public static T HybridAdvection<T>(T[,] f, int j, int i, T u, T v, T dx, T dy, T nu, T pecletLimit) where T : IFloatingPointIeee754<T>
    {
        T two = T.CreateChecked(2);
        T ddx = T.Abs(u) * dx / nu < pecletLimit
            ? (f[j, i + 1] - f[j, i - 1]) / (two * dx)
            : (u > T.Zero ? f[j, i] - f[j, i - 1] : f[j, i + 1] - f[j, i]) / dx;
        T ddy = T.Abs(v) * dy / nu < pecletLimit
            ? (f[j + 1, i] - f[j - 1, i]) / (two * dy)
            : (v > T.Zero ? f[j, i] - f[j - 1, i] : f[j + 1, i] - f[j, i]) / dy;
        return u * ddx + v * ddy;
    }

    /// <summary>Divergence ∂u/∂x + ∂v/∂y at an interior point by central differences.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Divergence<T>(T[,] u, T[,] v, int j, int i, T twoDx, T twoDy) where T : IFloatingPointIeee754<T>
        => CentralX(u, j, i, twoDx) + CentralY(v, j, i, twoDy);

    /// <summary>The numerator of the scalar curl (vorticity) ∂v/∂x − ∂u/∂y at an interior point by central differences: (v[j,i+1] − v[j,i−1]) − (u[j+1,i] − u[j−1,i]). Divide by 2h (equal spacing) for the curl.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Curl<T>(T[,] u, T[,] v, int j, int i) where T : IFloatingPointIeee754<T>
        => (v[j, i + 1] - v[j, i - 1]) - (u[j + 1, i] - u[j - 1, i]);

    /// <summary>Root mean square of the divergence over the points <c>j0 ≤ j &lt; j1</c>, <c>i0 ≤ i &lt; i1</c>: sqrt(Σ d² / count), summed left to right (row by row).</summary>
    public static T RmsDivergence<T>(T[,] u, T[,] v, T dx, T dy, int j0, int j1, int i0, int i1) where T : IFloatingPointIeee754<T>
    {
        T twoDx = T.CreateChecked(2) * dx, twoDy = T.CreateChecked(2) * dy, sum = T.Zero;
        int count = 0;
        for (int j = j0; j < j1; j++)
            for (int i = i0; i < i1; i++)
            {
                T d = Divergence(u, v, j, i, twoDx, twoDy);
                sum += d * d;
                count++;
            }
        return T.Sqrt(sum / T.CreateChecked(count));
    }
}
