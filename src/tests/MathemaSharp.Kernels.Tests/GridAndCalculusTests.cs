// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using MathemaSharp.Kernels;
using Xunit;

namespace MathemaSharp.Kernels.Tests;

public class GridAndCalculusTests
{
    static double[,] Field(int n, Func<double, double, double> f, double h)
    {
        var a = new double[n, n];
        for (int j = 0; j < n; j++) for (int i = 0; i < n; i++) a[j, i] = f(i * h, j * h);
        return a;
    }

    [Fact]
    public void CentralDifferencesAreExactOnLinearFields()
    {
        var f = Field(5, (x, y) => 3 * x - 2 * y, 0.5);
        Assert.Equal(3.0, GridStencils.CentralX(f, 2, 2, 1.0), 12);
        Assert.Equal(-2.0, GridStencils.CentralY(f, 2, 2, 1.0), 12);
    }

    [Fact]
    public void LaplacianOfAQuadraticIsConstant()
    {
        var f = Field(7, (x, y) => x * x + 2 * y * y, 0.25);   // laplacian = 2 + 4
        Assert.Equal(6.0, GridStencils.Laplacian(f, 3, 3, 0.0625, 0.0625), 10);
    }

    [Fact]
    public void DivergenceAndCurlOfARigidRotation()
    {
        // u = -y, v = x: divergence 0, curl 2
        var u = Field(5, (x, y) => -y, 0.5); var v = Field(5, (x, y) => x, 0.5);
        Assert.Equal(0.0, GridStencils.Divergence(u, v, 2, 2, 1.0, 1.0), 12);
        Assert.Equal(2.0, GridStencils.Curl(u, v, 2, 2) / 1.0, 12);   // numerator over 2h, with 2h = 1
    }

    [Fact]
    public void RmsDivergenceOfADivergenceFreeFieldIsZero()
    {
        var u = Field(9, (x, y) => -y, 0.125); var v = Field(9, (x, y) => x, 0.125);
        Assert.Equal(0.0, GridStencils.RmsDivergence(u, v, 0.125, 0.125, 1, 7, 1, 7), 12);
        var expanding = Field(9, (x, y) => x, 0.125);   // u = x, v = 0: divergence 1
        Assert.Equal(1.0, GridStencils.RmsDivergence(expanding, new double[9, 9], 0.125, 0.125, 1, 7, 1, 7), 12);
    }

    [Fact]
    public void HybridAdvectionIsCentralBelowTheLimitAndUpwindAbove()
    {
        var f = Field(5, (x, y) => x * x, 1.0);   // f = x squared; at x = 2: central 4, backward 3, forward 5
        // small velocity: Peclet 0.1 is below 2, central difference
        Assert.Equal(0.1 * 4.0, GridStencils.HybridAdvection(f, 2, 2, 0.1, 0.0, 1.0, 1.0, 1.0, 2.0), 12);
        // large velocity: Peclet 10 is above 2, upwind: backward for u > 0, forward for u < 0
        Assert.Equal(10.0 * 3.0, GridStencils.HybridAdvection(f, 2, 2, 10.0, 0.0, 1.0, 1.0, 1.0, 2.0), 12);
        Assert.Equal(-10.0 * 5.0, GridStencils.HybridAdvection(f, 2, 2, -10.0, 0.0, 1.0, 1.0, 1.0, 2.0), 12);
    }

    [Fact]
    public void CumulativeTrapezoidOfAConstantAndOfALine()
    {
        var ones = Field(5, (x, y) => 1, 0.5);
        var s = Calculus.CumulativeTrapezoidAlongRows(ones, 0.5);
        for (int j = 0; j < 5; j++) Assert.Equal(0.5 * j, s[j, 2], 12);
        var ramp = Field(5, (x, y) => y, 0.5);   // integral of y is y squared over 2, exact for the trapezoid rule on a line
        var r = Calculus.CumulativeTrapezoidAlongRows(ramp, 0.5);
        for (int j = 0; j < 5; j++) Assert.Equal((0.5 * j) * (0.5 * j) / 2, r[j, 1], 12);
    }

    [Fact]
    public void LinearInterpolationClampsOutsideTheTable()
    {
        double[] xs = [0, 1, 3], ys = [0, 10, 30];
        Assert.Equal(0.0, Calculus.InterpolateLinear<double>(xs, ys, -5));
        Assert.Equal(5.0, Calculus.InterpolateLinear<double>(xs, ys, 0.5), 12);
        Assert.Equal(20.0, Calculus.InterpolateLinear<double>(xs, ys, 2), 12);
        Assert.Equal(30.0, Calculus.InterpolateLinear<double>(xs, ys, 99));
        Assert.Throws<ArgumentException>(() => Calculus.InterpolateLinear<double>([], [], 1));
    }

    [Fact]
    public void RmsAgainstAReference()
    {
        double[] xs = [0, 1, 2], ys = [0, 1, 2];
        Assert.Equal(0.0, Calculus.RmsAgainstReference<double>(xs, ys, [0.5, 1.5], [0.5, 1.5]), 12);
        Assert.Equal(1.0, Calculus.RmsAgainstReference<double>(xs, ys, [0.5, 1.5], [1.5, 2.5]), 12);
    }

    [Fact]
    public void UpwardCrossingsOfASineGiveItsPeriod()
    {
        const int n = 200; const double period = 20;
        var s = Enumerable.Range(0, n).Select(k => Math.Sin(2 * Math.PI * k / period)).ToArray();
        var crossings = Calculus.UpwardCrossings<double>(s, 0.0);
        Assert.True(crossings.Count >= 9);
        Assert.Equal(period, Calculus.MeanPeriod<double>(crossings)!.Value, 1);
        Assert.Null(Calculus.MeanPeriod<double>([1.0, 2.0]));
        // a start index shifts the positions of the crossings
        var shifted = Calculus.UpwardCrossings<double>(s.AsSpan(50), 0.0, 50);
        Assert.Contains(shifted, x => x > 50);
    }

    [Fact]
    public void TheStencilsWorkInSingleAndHalfPrecision()
    {
        var f = new float[5, 5]; var h = new Half[5, 5];
        for (int j = 0; j < 5; j++) for (int i = 0; i < 5; i++) { f[j, i] = i * i + j * j; h[j, i] = (Half)(i * i + j * j); }
        Assert.Equal(4f, GridStencils.Laplacian(f, 2, 2, 1f, 1f));
        Assert.Equal((Half)4, GridStencils.Laplacian(h, 2, 2, (Half)1, (Half)1));
    }
}
