// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using MathemaSharp.LinearAlgebra;
using Xunit;

namespace MathemaSharp.LinearAlgebra.Tests;

public class GridPoissonTests
{
    // laplacian p = -2 pi^2 sin(pi x) sin(pi y) on the unit square with p = 0 on the border has the solution p = sin(pi x) sin(pi y).
    static (T[,] p, T[,] b, T[,] exact, T h) Problem<T>(int n) where T : IFloatingPointIeee754<T>
    {
        T h = T.One / T.CreateChecked(n - 1);
        var p = new T[n, n]; var b = new T[n, n]; var exact = new T[n, n];
        for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
            {
                double x = i / (double)(n - 1), y = j / (double)(n - 1);
                exact[j, i] = T.CreateChecked(Math.Sin(Math.PI * x) * Math.Sin(Math.PI * y));
                b[j, i] = T.CreateChecked(-2 * Math.PI * Math.PI * Math.Sin(Math.PI * x) * Math.Sin(Math.PI * y));
            }
        return (p, b, exact, h);
    }

    [Fact]
    public void SorConvergesToTheManufacturedSolution()
    {
        var (p, b, exact, h) = Problem<double>(33);
        GridPoisson.Sor(p, b, h, h, 1.8, 400);
        double worst = 0;
        for (int j = 0; j < 33; j++) for (int i = 0; i < 33; i++) worst = Math.Max(worst, Math.Abs(p[j, i] - exact[j, i]));
        Assert.True(worst < 2e-3, $"max error {worst}");   // discretisation error of the five-point stencil, O(h squared)
        Assert.True(GridPoisson.ResidualRms(p, b, h, h) < 1e-6);
    }

    [Fact]
    public void ResidualFallsWithTheSweeps()
    {
        var (p, b, _, h) = Problem<double>(17);
        double r0 = GridPoisson.ResidualRms(p, b, h, h);
        GridPoisson.Sor(p, b, h, h, 1.7, 10);
        double r10 = GridPoisson.ResidualRms(p, b, h, h);
        GridPoisson.Sor(p, b, h, h, 1.7, 100);
        double r110 = GridPoisson.ResidualRms(p, b, h, h);
        Assert.True(r10 < r0 && r110 < r10);
    }

    [Fact]
    public void TheBoundaryActionRunsAfterEverySweep()
    {
        var (p, b, _, h) = Problem<double>(9);
        int calls = 0;
        GridPoisson.Sor(p, b, h, h, 1.5, 7, _ => calls++);
        Assert.Equal(7, calls);
    }

    [Fact]
    public void NeumannBoundariesKeepTheConstantFreedom()
    {
        // zero right-hand side, copy-the-neighbour boundary, constant initial guess: the solution stays that constant
        const int n = 12;
        var p = new double[n, n]; var b = new double[n, n];
        for (int j = 0; j < n; j++) for (int i = 0; i < n; i++) p[j, i] = 3.5;
        GridPoisson.Sor(p, b, 0.1, 0.1, 1.7, 50, q =>
        {
            for (int k = 0; k < n; k++) { q[k, 0] = q[k, 1]; q[k, n - 1] = q[k, n - 2]; q[0, k] = q[1, k]; q[n - 1, k] = q[n - 2, k]; }
        });
        for (int j = 0; j < n; j++) for (int i = 0; i < n; i++) Assert.Equal(3.5, p[j, i], 12);
    }

    [Theory]
    [InlineData(typeof(float))]
    [InlineData(typeof(Half))]
    public void WorksInNarrowFormats(Type type)
        => typeof(GridPoissonTests).GetMethod(nameof(Narrow), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.MakeGenericMethod(type).Invoke(null, null);

    static void Narrow<T>() where T : IFloatingPointIeee754<T>
    {
        var (p, b, exact, h) = Problem<T>(17);
        GridPoisson.Sor(p, b, h, h, T.CreateChecked(1.7), 150);
        double worst = 0;
        for (int j = 0; j < 17; j++) for (int i = 0; i < 17; i++) worst = Math.Max(worst, Math.Abs(double.CreateChecked(p[j, i] - exact[j, i])));
        Assert.True(worst < 0.02, $"{typeof(T).Name} max error {worst}");
    }
}
