// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using MathemaSharp.Kernels;
using Xunit;

namespace MathemaSharp.Kernels.Tests;

public class ProductsTests
{
    [Fact]
    public void MatMulKnownProduct()
    {
        // [[1,2,3],[4,5,6]] · [[7,8],[9,10],[11,12]] = [[58,64],[139,154]]
        var c = new double[4];
        Products.MatMul<double>([1, 2, 3, 4, 5, 6], [7, 8, 9, 10, 11, 12], c, 2, 3, 2);
        Assert.Equal(new double[] { 58, 64, 139, 154 }, c);
    }

    [Fact]
    public void MatMulOverwritesAndHandlesEmptyDimensions()
    {
        var c = new double[] { 9, 9, 9, 9 };
        Products.MatMul<double>([], [], c, 2, 0, 2);
        Assert.Equal(new double[] { 0, 0, 0, 0 }, c);   // k = 0: the zero matrix
        Products.MatMul<double>([1.0], [2.0], c, 1, 1, 1);
        Assert.Equal(2.0, c[0]);
        Products.MatMul<double>([], [], Span<double>.Empty, 0, 3, 0);   // nothing to do
    }

    [Fact]
    public void MatMulAccumulatesLeftToRightInTheTypesOwnArithmetic()
    {
        // 1e16 + 1 + 1 - 1e16: left to right every 1 is lost (1e16 + 1 = 1e16 in double); a pairwise or wider accumulator would keep them
        var c = new double[1];
        Products.MatMul<double>([1e16, 1, 1, -1e16], [1, 1, 1, 1], c, 1, 4, 1);
        Assert.Equal(0.0, c[0]);
        var h = new Half[1];
        Products.MatMul<Half>([(Half)2048, (Half)1, (Half)1], [(Half)1, (Half)1, (Half)1], h, 1, 3, 1);
        Assert.Equal((Half)2048, h[0]);   // 2048 + 1 rounds back to 2048 in half precision, twice
    }

    [Fact]
    public void MatMulWorksForIntegersAndComplex()
    {
        var i = new int[4];
        Products.MatMul<int>([1, 2, 3, 4], [5, 6, 7, 8], i, 2, 2, 2);
        Assert.Equal(new[] { 19, 22, 43, 50 }, i);
        var z = new Complex[1];
        Products.MatMul<Complex>([new Complex(1, 2)], [new Complex(3, -1)], z, 1, 1, 1);
        Assert.Equal(new Complex(5, 5), z[0]);   // (1+2i)(3-i) = 3 - i + 6i + 2 = 5 + 5i
    }

    [Fact]
    public void MatMulRejectsShortBuffersAndOverlap()
    {
        Assert.Throws<ArgumentException>(() => Products.MatMul<double>([1, 2], [1, 2, 3, 4], new double[4], 2, 2, 2));
        Assert.Throws<ArgumentException>(() => Products.MatMul<double>([1, 2, 3, 4], [1, 2, 3, 4], new double[3], 2, 2, 2));
        var shared = new double[4];
        Assert.Throws<ArgumentException>(() => Products.MatMul<double>(shared, new double[4], shared, 2, 2, 2));
    }

    [Fact]
    public void CovarianceOfTwoVariables()
    {
        // x = [1,2,3,4], y = [2,4,6,9]: means 2.5 and 5.25; deviations (-1.5,-.5,.5,1.5) and (-3.25,-1.25,.75,3.75)
        // sxx = 5, sxy = 4.875 + .625 + .375 + 5.625 = 11.5, syy = 10.5625 + 1.5625 + .5625 + 14.0625 = 26.75; divided by 3
        var cov = new double[4];
        Products.Covariance<double>([1, 2, 3, 4, 2, 4, 6, 9], 2, 4, cov, 1);
        Assert.Equal(5.0 / 3, cov[0], 14);
        Assert.Equal(11.5 / 3, cov[1], 14);
        Assert.Equal(cov[1], cov[2]);
        Assert.Equal(26.75 / 3, cov[3], 14);
        var biased = new double[4];
        Products.Covariance<double>([1, 2, 3, 4, 2, 4, 6, 9], 2, 4, biased, 0);
        Assert.Equal(5.0 / 4, biased[0], 14);
    }

    [Theory]
    [InlineData(SumMethod.Pairwise)]
    [InlineData(SumMethod.SequentialFromFirst)]
    public void CovarianceMeanSumMethodFollowsTheLayout(SumMethod method)
    {
        // 16 observations: 1e16 followed by fifteen ones. Left to right loses every 1; the pairwise grouping keeps some, so the two means differ.
        var x = new double[16];
        x[0] = 1e16; for (int i = 1; i < 16; i++) x[i] = 1;
        var mean = new double[1];
        Reductions.Mean<double>(x, Lane.Flat(16), mean, NanPolicy.Propagate, method);
        double expected = 0;
        for (int i = 0; i < 16; i++) { double d = x[i] - mean[0]; expected += d * d; }
        expected *= 1.0 / 15;
        var cov = new double[1];
        Products.Covariance<double>(x, 1, 16, cov, 1, method);
        Assert.Equal(expected, cov[0]);
        var pairwise = new double[1]; var sequential = new double[1];
        Reductions.Mean<double>(x, Lane.Flat(16), pairwise, NanPolicy.Propagate, SumMethod.Pairwise);
        Reductions.Mean<double>(x, Lane.Flat(16), sequential, NanPolicy.Propagate, SumMethod.SequentialFromFirst);
        Assert.NotEqual(pairwise[0], sequential[0]);   // the two layouts really give different means
    }

    [Fact]
    public void CovarianceWithNoDegreesOfFreedom()
    {
        var cov = new double[1];
        Products.Covariance<double>([5.0], 1, 1, cov, 1);   // one observation, ddof 1: 0 * inf
        Assert.True(double.IsNaN(cov[0]));
        Products.Covariance<double>([1.0, 2.0], 1, 2, cov, 3);   // positive sum of squares over zero: infinity
        Assert.True(double.IsPositiveInfinity(cov[0]));
    }

    [Fact]
    public void CorrelationIsClippedAndHasAUnitDiagonal()
    {
        var corr = new double[4];
        Products.Correlation<double>([4.0, 6.0, 6.0, 9.0], 2, corr);   // 6 / (2 * 3) = 1
        Assert.Equal(new double[] { 1, 1, 1, 1 }, corr);
        Products.Correlation<double>([4.0, 6.5, 6.5, 9.0], 2, corr);   // 6.5 / 6 > 1: clipped
        Assert.Equal(1.0, corr[1]);
        Products.Correlation<double>([4.0, -7.0, -7.0, 9.0], 2, corr);
        Assert.Equal(-1.0, corr[1]);
        Products.Correlation<double>([4.0, double.NaN, double.NaN, 9.0], 2, corr);
        Assert.True(double.IsNaN(corr[1]));
    }

    [Fact]
    public void CovarianceMatchesMatMulOfTheCentredRows()
    {
        double[] x = Enumerable.Range(0, 3 * 50).Select(i => Math.Sin(i * 0.37) * 10 + i % 7).ToArray();
        var cov = new double[9];
        Products.Covariance<double>(x, 3, 50, cov, 1);
        for (int a = 0; a < 3; a++)
            for (int b = 0; b < 3; b++)
            {
                double ma = x.Skip(a * 50).Take(50).Average(), mb = x.Skip(b * 50).Take(50).Average(), s = 0;
                for (int i = 0; i < 50; i++) s += (x[a * 50 + i] - ma) * (x[b * 50 + i] - mb);
                Assert.Equal(s / 49, cov[a * 3 + b], 9);
            }
    }
}
