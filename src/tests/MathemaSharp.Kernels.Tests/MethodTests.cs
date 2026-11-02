// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using MathemaSharp.Kernels;
using Xunit;

namespace MathemaSharp.Kernels.Tests;

public class MethodTests
{
    static double One(double[] x, SumMethod m, NanPolicy nan = NanPolicy.Propagate)
    {
        var d = new double[1];
        Reductions.Sum<double>(x, Lane.Flat(x.Length), d, nan, m);
        return d[0];
    }

    [Fact]
    public void PairwiseBelowEightIsSequentialFromTheFirstElement()
    {
        double[] x = [0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7];
        double expected = x[0];
        for (int i = 1; i < x.Length; i++) expected += x[i];
        Assert.Equal(expected, One(x, SumMethod.Pairwise));
        Assert.True(double.IsNegative(One([-0.0], SumMethod.Pairwise)));      // seeded by the first element, as numpy
        Assert.False(double.IsNegative(One([-0.0], SumMethod.Sequential)));   // seeded by zero
    }

    [Fact]
    public void PairwiseBetweenEightAndOneHundredTwentyEightUsesEightAccumulators()
    {
        var rnd = new Random(7);
        for (int n = 8; n <= 128; n += 5)
        {
            var x = Enumerable.Range(0, n).Select(_ => rnd.NextDouble() * 2 - 1).ToArray();
            double[] r = new double[8];
            Array.Copy(x, r, 8);
            int i = 8;
            for (; i < n - n % 8; i += 8) for (int j = 0; j < 8; j++) r[j] += x[i + j];
            double expected = ((r[0] + r[1]) + (r[2] + r[3])) + ((r[4] + r[5]) + (r[6] + r[7]));
            for (; i < n; i++) expected += x[i];
            Assert.Equal(expected, One(x, SumMethod.Pairwise));
        }
    }

    [Fact]
    public void PairwiseAboveOneHundredTwentyEightSplitsInHalvesOfMultiplesOfEight()
    {
        var rnd = new Random(8);
        var x = Enumerable.Range(0, 1000).Select(_ => rnd.NextDouble() * 2 - 1).ToArray();
        int n2 = 1000 / 2; n2 -= n2 % 8;   // 496
        Assert.Equal(One(x[..n2], SumMethod.Pairwise) + One(x[n2..], SumMethod.Pairwise), One(x, SumMethod.Pairwise));
    }

    [Fact]
    public void PairwiseIsMoreAccurateThanSequentialOnLongLanes()
    {
        var x = Enumerable.Repeat(0.1, 1_000_000).ToArray();
        double exact = One(x, SumMethod.Compensated);
        Assert.True(Math.Abs(One(x, SumMethod.Pairwise) - exact) < Math.Abs(One(x, SumMethod.Sequential) - exact) / 100);
    }

    [Fact]
    public void PairwiseOnAStridedLaneIsSequentialFromTheFirstElement()
    {
        // Two lanes of 10 interleaved (inner = 2): numpy adds such a lane in order, not pairwise.
        var rnd = new Random(9);
        var x = Enumerable.Range(0, 20).Select(_ => rnd.NextDouble() * 2 - 1).ToArray();
        var d = new double[2];
        Reductions.Sum<double>(x, new Lane(10, 2, 1), d, method: SumMethod.Pairwise);
        for (int lane = 0; lane < 2; lane++)
        {
            double e = x[lane];
            for (int k = 1; k < 10; k++) e += x[lane + 2 * k];
            Assert.Equal(e, d[lane]);
        }
    }

    [Fact]
    public void PairwiseOmitReplacesNaNByZeroLikeNansum()
    {
        double[] x = [1, double.NaN, 2, 3, double.NaN, 4, 5, 6, 7, 8];
        double[] zeroed = [1, 0, 2, 3, 0, 4, 5, 6, 7, 8];
        Assert.Equal(One(zeroed, SumMethod.Pairwise), One(x, SumMethod.Pairwise, NanPolicy.Omit));
        var d = new double[1];
        Reductions.Mean<double>(x, Lane.Flat(10), d, NanPolicy.Omit, SumMethod.Pairwise);
        Assert.Equal(One(zeroed, SumMethod.Pairwise) / 8, d[0]);
    }

    [Fact]
    public void MeanFollowsTheSumMethod()
    {
        var x = Enumerable.Range(0, 1000).Select(i => 0.1 * (i % 7)).ToArray();
        var d = new double[1];
        Reductions.Mean<double>(x, Lane.Flat(1000), d, method: SumMethod.Pairwise);
        Assert.Equal(One(x, SumMethod.Pairwise) / 1000, d[0]);
    }

    [Fact]
    public void TwoPassMatchesTheTextbookAlgorithmAndWelfordWithinRounding()
    {
        var rnd = new Random(10);
        foreach (int n in new[] { 1, 2, 5, 8, 50, 300 })
        {
            var x = Enumerable.Range(0, n).Select(_ => rnd.NextDouble() * 10).ToArray();
            double mean = One(x, SumMethod.Pairwise) / n;
            var sq = x.Select(v => (v - mean) * (v - mean)).ToArray();
            double expected = One(sq, SumMethod.Pairwise) / n;
            var two = new double[1]; var wel = new double[1];
            Reductions.Variance<double>(x, Lane.Flat(n), two, 0, method: VarianceMethod.TwoPass);
            Reductions.Variance<double>(x, Lane.Flat(n), wel, 0);
            Assert.Equal(expected, two[0]);
            Assert.Equal(expected, wel[0], 10);
        }
    }

    [Fact]
    public void TwoPassDegenerateAndPolicies()
    {
        var d = new double[1];
        Reductions.Variance<double>([5], Lane.Flat(1), d, 1, method: VarianceMethod.TwoPass);
        Assert.True(double.IsNaN(d[0]));
        Reductions.Variance<double>([1, double.NaN, 3], Lane.Flat(3), d, 1, method: VarianceMethod.TwoPass);
        Assert.True(double.IsNaN(d[0]));
        Reductions.Variance<double>([1, double.NaN, 3], Lane.Flat(3), d, 1, NanPolicy.Omit, VarianceMethod.TwoPass);
        Assert.Equal(2, d[0]);
        Reductions.StandardDeviation<double>([2, 4, 4, 4, 5, 5, 7, 9], Lane.Flat(8), d, 0, method: VarianceMethod.TwoPass);
        Assert.Equal(2, d[0]);
        // strided lanes
        var v = new double[2];
        Reductions.Variance<double>([1, 10, 2, 20, 3, 30], new Lane(3, 2, 1), v, 0, method: VarianceMethod.TwoPass);
        Assert.Equal(2.0 / 3, v[0], 12); Assert.Equal(200.0 / 3, v[1], 12);
    }
}
