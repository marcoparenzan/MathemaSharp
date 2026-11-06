// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using MathemaSharp.Kernels;
using Xunit;

namespace MathemaSharp.Kernels.Tests;

public class QuantileTests
{
    static double Q(double[] data, double q, QuantileMethod m, NanPolicy nan = NanPolicy.Propagate)
    {
        var d = new double[1];
        Statistics.Quantile<double>(data, Lane.Flat(data.Length), [q], d, m, nan);
        return d[0];
    }

    // Worked out by hand from the definitions, for [1,2,3,4] at q = 0.25 (n q = 1, (n-1) q = 0.75).
    [Theory]
    [InlineData(QuantileMethod.InvertedCdf, 1.0)]
    [InlineData(QuantileMethod.AveragedInvertedCdf, 1.5)]
    [InlineData(QuantileMethod.ClosestObservation, 1.0)]
    [InlineData(QuantileMethod.InterpolatedInvertedCdf, 1.0)]
    [InlineData(QuantileMethod.Hazen, 1.5)]
    [InlineData(QuantileMethod.Weibull, 1.25)]
    [InlineData(QuantileMethod.Linear, 1.75)]
    [InlineData(QuantileMethod.MedianUnbiased, 1.4166666666666667)]   // position 1/3 + 1/12 + 1 - 1 + 1 = 1.41667 (1-based)
    [InlineData(QuantileMethod.NormalUnbiased, 1.4375)]               // 3/8 + 1/16 = 0.4375 above the first value
    [InlineData(QuantileMethod.Lower, 1.0)]
    [InlineData(QuantileMethod.Higher, 2.0)]
    [InlineData(QuantileMethod.Nearest, 2.0)]
    [InlineData(QuantileMethod.Midpoint, 1.5)]
    public void MethodsAtAQuarter(QuantileMethod method, double expected)
        => Assert.Equal(expected, Q([1, 2, 3, 4], 0.25, method), 12);

    [Theory]
    [InlineData(QuantileMethod.Hazen)]
    [InlineData(QuantileMethod.Linear)]
    [InlineData(QuantileMethod.Weibull)]
    [InlineData(QuantileMethod.Lower)]
    [InlineData(QuantileMethod.InvertedCdf)]
    public void EndsAreTheMinimumAndTheMaximum(QuantileMethod method)
    {
        double[] data = [7, 3, 9, 1, 5];
        Assert.Equal(1, Q(data, 0, method));
        Assert.Equal(9, Q(data, 1, method));
    }

    [Theory]
    [InlineData(0.0, 1.0)]     // n q = 0 -> max(0, 1) = position 1
    [InlineData(0.25, 1.0)]    // n q = 1
    [InlineData(0.375, 2.0)]   // n q = 1.5 rounds to 2 (even)
    [InlineData(0.625, 2.0)]   // n q = 2.5 rounds to 2 (even)
    [InlineData(0.875, 4.0)]   // n q = 3.5 rounds to 4 (even)
    [InlineData(1.0, 4.0)]
    public void ClosestObservationRoundsHalvesToEven(double q, double expected)
        => Assert.Equal(expected, Q([1, 2, 3, 4], q, QuantileMethod.ClosestObservation));

    [Fact]
    public void LinearMedianEqualsTheMedianKernel()
    {
        double[] data = [4, 1, 2, 3, 10, -3];
        var med = new double[1];
        Reductions.Median<double>(data, Lane.Flat(6), med);
        Assert.Equal(med[0], Q(data, 0.5, QuantileMethod.Linear), 15);
    }

    [Fact]
    public void ProbabilityMajorLayoutAcrossLanes()
    {
        double[] src = [1, 2, 3, 10, 20, 30];   // two lanes of three
        var d = new double[6];
        Statistics.Quantile<double>(src, new Lane(3, 1, 2), [0, 0.5, 1], d, QuantileMethod.Linear);
        Assert.Equal(new double[] { 1, 10, 2, 20, 3, 30 }, d);
    }

    [Fact]
    public void StridedLanes()
    {
        double[] src = [1, 10, 2, 20, 3, 30];   // lanes along the first axis of a 3 x 2 row-major array
        var d = new double[2];
        Statistics.Quantile<double>(src, Lane.ForRowMajorAxis([3, 2], 0), [0.5], d, QuantileMethod.Linear);
        Assert.Equal(new double[] { 2, 20 }, d);
    }

    [Fact]
    public void NanPolicies()
    {
        Assert.True(double.IsNaN(Q([1, double.NaN, 3], 0.5, QuantileMethod.Linear)));
        Assert.Equal(2, Q([1, double.NaN, 3], 0.5, QuantileMethod.Linear, NanPolicy.Omit));
        Assert.True(double.IsNaN(Q([double.NaN], 0.5, QuantileMethod.Linear, NanPolicy.Omit)));
        Assert.True(double.IsNaN(Q([], 0.5, QuantileMethod.Linear)));
    }

    [Fact]
    public void ProbabilityOutsideZeroOneIsRejected()
    {
        var d = new double[1];
        Assert.Throws<ArgumentOutOfRangeException>(() => Statistics.Quantile<double>([1, 2], Lane.Flat(2), [1.1], d, QuantileMethod.Linear));
        Assert.Throws<ArgumentOutOfRangeException>(() => Statistics.Quantile<double>([1, 2], Lane.Flat(2), [double.NaN], d, QuantileMethod.Linear));
    }

    [Fact]
    public void WorksForSingleAndDecimalTypes()
    {
        var f = new float[1];
        Statistics.Quantile<float>([1f, 2f, 3f, 4f], Lane.Flat(4), [0.25], f, QuantileMethod.Linear);
        Assert.Equal(1.75f, f[0]);
        var m = new Decimal128[1];
        Statistics.Quantile<Decimal128>([Decimal128.CreateChecked(1), Decimal128.CreateChecked(2), Decimal128.CreateChecked(3), Decimal128.CreateChecked(4)],
            Lane.Flat(4), [0.25], m, QuantileMethod.Linear);
        Assert.Equal(Decimal128.Parse("1.75", System.Globalization.CultureInfo.InvariantCulture), m[0]);
    }
}

public class ModeTests
{
    [Fact]
    public void SmallestOfTheTiedValuesWins()
    {
        var v = new double[1]; var c = new int[1];
        Statistics.Mode<double>([3, 1, 3, 1, 2], Lane.Flat(5), v, c, NanPolicy.Omit);
        Assert.Equal(1, v[0]); Assert.Equal(2, c[0]);
    }

    [Fact]
    public void NanPolicyAndEmptyLane()
    {
        var v = new double[1]; var c = new int[1];
        Statistics.Mode<double>([double.NaN, 5, 5, 2], Lane.Flat(4), v, c, NanPolicy.Omit);
        Assert.Equal(5, v[0]); Assert.Equal(2, c[0]);
        Statistics.Mode<double>([double.NaN, 5, 5, 2], Lane.Flat(4), v, c, NanPolicy.Propagate);
        Assert.True(double.IsNaN(v[0])); Assert.Equal(0, c[0]);
        Statistics.Mode<double>([], Lane.Flat(0), v, c, NanPolicy.Omit);
        Assert.True(double.IsNaN(v[0])); Assert.Equal(0, c[0]);
    }

    [Fact]
    public void LanesAndOptionalCounts()
    {
        var v = new double[2];
        // 3 x 2 row-major array [[1,9],[2,9],[2,8]]: lane 0 is [1,2,2] (mode 2), lane 1 is [9,9,8] (mode 9)
        Statistics.Mode<double>([1, 9, 2, 9, 2, 8], Lane.ForRowMajorAxis([3, 2], 0), v, Span<int>.Empty, NanPolicy.Omit);
        Assert.Equal(new double[] { 2, 9 }, v);
    }
}

public class HistogramTests
{
    static long[] H(double[] data, double[] edges, HistogramLastBin last)
    {
        var c = new long[Statistics.HistogramBins(edges.Length, last)];
        Statistics.Histogram<double>(data, Lane.Flat(data.Length), edges, c, last);
        return c;
    }

    [Fact]
    public void ClosedIntervalLikeNumpy()
        => Assert.Equal(new long[] { 2, 1, 2 }, H([0, 0.5, 1, 2, 3, 3.5, -1, double.NaN], [0, 1, 2, 3], HistogramLastBin.ClosedInterval));

    [Fact]
    public void ExactLastEdgeLikeHistc()
        => Assert.Equal(new long[] { 2, 1, 1, 1 }, H([0, 0.5, 1, 2, 3, 3.5, -1, double.NaN], [0, 1, 2, 3], HistogramLastBin.ExactLastEdge));

    [Fact]
    public void NonUniformEdgesAndDuplicates()
        => Assert.Equal(new long[] { 1, 0, 3 }, H([0.1, 5, 6, 7], [0, 1, 1, 10], HistogramLastBin.ClosedInterval));

    [Fact]
    public void UnsortedEdgesAreRejected()
        => Assert.Throws<ArgumentException>(() => H([1], [2, 1], HistogramLastBin.ClosedInterval));

    [Fact]
    public void PerLaneCounts()
    {
        var c = new long[4];
        Statistics.Histogram<double>([0.5, 1.5, 0.5, 0.5], new Lane(2, 1, 2), [0, 1, 2], c, HistogramLastBin.ClosedInterval);
        Assert.Equal(new long[] { 1, 1, 2, 0 }, c);
    }
}

public class IntegrationTests
{
    [Fact]
    public void TrapezoidUniformAndPositions()
    {
        var d = new double[1];
        Statistics.Trapezoid<double>([0, 1, 2, 3], Lane.Flat(4), 1.0, d);
        Assert.Equal(4.5, d[0]);
        Statistics.Trapezoid<double>([1, 1, 1, 1], Lane.Flat(4), [0, 1, 3, 6], d);
        Assert.Equal(6, d[0]);
        Statistics.Trapezoid<double>([5], Lane.Flat(1), 1.0, d);
        Assert.Equal(0, d[0]);
    }

    [Fact]
    public void TrapezoidSumMethodsAgreeOnExactData()
    {
        double[] y = Enumerable.Range(0, 300).Select(i => (double)(i % 7)).ToArray();
        var a = new double[1]; var b = new double[1];
        Statistics.Trapezoid<double>(y, Lane.Flat(300), 0.5, a, SumMethod.Sequential);
        Statistics.Trapezoid<double>(y, Lane.Flat(300), 0.5, b, SumMethod.Pairwise);
        Assert.Equal(a[0], b[0], 9);
    }

    [Fact]
    public void GradientUniform()
    {
        var d = new double[4];
        Statistics.Gradient<double>([1, 4, 9, 16], Lane.Flat(4), 1.0, d);
        Assert.Equal(new double[] { 3, 4, 6, 7 }, d);
    }

    [Fact]
    public void GradientNonUniformIsExactForAQuadratic()
    {
        double[] x = [0, 1, 3, 6];
        double[] y = x.Select(v => v * v).ToArray();
        var d = new double[4];
        Statistics.Gradient<double>(y, Lane.Flat(4), x, d);
        Assert.Equal(1, d[0], 12);          // one-sided: (1 - 0) / 1
        Assert.Equal(2, d[1], 12);          // 2x at x = 1
        Assert.Equal(6, d[2], 12);          // 2x at x = 3
        Assert.Equal(9, d[3], 12);          // one-sided: (36 - 9) / 3
    }

    [Fact]
    public void GradientCentralSchemeIsTheSimpleDifference()
    {
        double[] x = [0, 1, 3, 6];
        var d = new double[4];
        Statistics.Gradient<double>([1, 4, 9, 16], Lane.Flat(4), x, d, GradientScheme.Central);
        Assert.Equal(3, d[0]);
        Assert.Equal((9.0 - 1.0) / (3.0 - 0.0), d[1]);    // Octave: 2.66667
        Assert.Equal((16.0 - 4.0) / (6.0 - 1.0), d[2]);   // 2.4
        Assert.Equal((16.0 - 9.0) / (6.0 - 3.0), d[3]);   // 2.33333
    }

    [Fact]
    public void GradientAlongStridedLanes()
    {
        double[] src = [1, 10, 4, 40, 9, 90];   // 3 x 2 row-major, lanes along axis 0
        var d = new double[6];
        Statistics.Gradient<double>(src, Lane.ForRowMajorAxis([3, 2], 0), 1.0, d);
        Assert.Equal(new double[] { 3, 30, 4, 40, 5, 50 }, d);
    }

    [Fact]
    public void GradientNeedsTwoSamples()
        => Assert.Throws<ArgumentException>(() => Statistics.Gradient<double>([1], Lane.Flat(1), 1.0, new double[1]));
}
