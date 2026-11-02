// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using MathemaSharp.Kernels;
using Xunit;

namespace MathemaSharp.Kernels.Tests;

public class LaneTests
{
    [Fact]
    public void ColumnMajorDimensions()
    {
        int[] dims = [2, 3, 4];
        Assert.Equal(new Lane(2, 1, 12), Lane.ForColumnMajorDim(dims, 0));
        Assert.Equal(new Lane(3, 2, 4), Lane.ForColumnMajorDim(dims, 1));
        Assert.Equal(new Lane(4, 6, 1), Lane.ForColumnMajorDim(dims, 2));
        Assert.Equal(new Lane(1, 24, 1), Lane.ForColumnMajorDim(dims, 3));
    }

    [Fact]
    public void RowMajorAxes()
    {
        int[] shape = [2, 3, 4];
        Assert.Equal(new Lane(2, 12, 1), Lane.ForRowMajorAxis(shape, 0));
        Assert.Equal(new Lane(3, 4, 2), Lane.ForRowMajorAxis(shape, 1));
        Assert.Equal(new Lane(4, 1, 6), Lane.ForRowMajorAxis(shape, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => Lane.ForRowMajorAxis(shape, 3));
    }

    [Fact]
    public void RowMajorAxisAndTransposedColumnMajorDimAreTheSameLane()
    {
        // A row-major [2,3,4] array is the column-major [4,3,2] array with reversed axes: axis a <-> dim (rank-1-a).
        int[] shape = [2, 3, 4], reversed = [4, 3, 2];
        for (int a = 0; a < 3; a++)
            Assert.Equal(Lane.ForRowMajorAxis(shape, a), Lane.ForColumnMajorDim(reversed, 2 - a));
    }

    [Fact]
    public void StartVisitsEveryElementOnce()
    {
        var lane = new Lane(3, 2, 4);
        var seen = new bool[lane.Total];
        for (int l = 0; l < lane.Count; l++)
            for (int k = 0; k < lane.Length; k++) { Assert.False(seen[lane.Start(l) + k * lane.Inner]); seen[lane.Start(l) + k * lane.Inner] = true; }
        Assert.All(seen, Assert.True);
    }
}

public class ReductionTests
{
    [Fact] public void Half_() => AllChecks<Half>(1e-2);
    [Fact] public void Single() => AllChecks<float>(1e-5);
    [Fact] public void Double() => AllChecks<double>(1e-12);
    [Fact] public void Decimal32_() => AllChecks<Decimal32>(1e-5);
    [Fact] public void Decimal64_() => AllChecks<Decimal64>(1e-12);
    [Fact] public void Decimal128_() => AllChecks<Decimal128>(1e-12);

    static T V<T>(double x) where T : INumberBase<T> => T.CreateChecked(x);
    static double D<T>(T x) where T : INumberBase<T> => double.CreateChecked(x);

    // Gathers lane l of src as doubles, independently of the kernels.
    static double[] Gather<T>(T[] src, Lane lane, int l) where T : INumberBase<T>
    {
        var r = new double[lane.Length];
        int o = l / lane.Inner, i = l % lane.Inner;
        for (int k = 0; k < lane.Length; k++) r[k] = D(src[o * lane.Length * lane.Inner + k * lane.Inner + i]);
        return r;
    }

    static void Near(double expected, double actual, double tol, string what)
    {
        if (double.IsNaN(expected)) { Assert.True(double.IsNaN(actual), what + $": expected NaN, got {actual}"); return; }
        if (double.IsInfinity(expected)) { Assert.Equal(expected, actual); return; }
        Assert.True(Math.Abs(expected - actual) <= tol * Math.Max(1, Math.Abs(expected)), what + $": expected {expected}, got {actual}");
    }

    static void AllChecks<T>(double tol) where T : IFloatingPointIeee754<T>
    {
        // 2 x 3 x 4 data with small integers (exactly representable in every format) in the three column-major dims.
        int[] dims = [2, 3, 4];
        var data = new T[24];
        for (int i = 0; i < data.Length; i++) data[i] = V<T>(((i * 7) % 11) - 4);
        for (int dim = 0; dim < 4; dim++)
        {
            var lane = Lane.ForColumnMajorDim(dims, dim);
            int n = lane.Count;
            var sum = new T[n]; var prod = new T[n]; var mean = new T[n]; var v0 = new T[n]; var v1 = new T[n]; var sd = new T[n];
            var mx = new T[n]; var mn = new T[n]; var med = new T[n]; var rng = new T[n]; var ss = new T[n];
            var imx = new int[n]; var imn = new int[n]; var nz = new int[n]; var any = new bool[n]; var all = new bool[n];
            Reductions.Sum<T>(data, lane, sum); Reductions.Product<T>(data, lane, prod); Reductions.Mean<T>(data, lane, mean);
            Reductions.Variance<T>(data, lane, v0, 0); Reductions.Variance<T>(data, lane, v1, 1); Reductions.StandardDeviation<T>(data, lane, sd, 1);
            Reductions.Max<T>(data, lane, mx, imx, NanPolicy.Propagate); Reductions.Min<T>(data, lane, mn, imn, NanPolicy.Propagate);
            Reductions.Median<T>(data, lane, med); Reductions.Range<T>(data, lane, rng, NanPolicy.Propagate); Reductions.SumOfSquares<T>(data, lane, ss);
            Reductions.CountNonZero<T>(data, lane, nz); Reductions.Any<T>(data, lane, any); Reductions.All<T>(data, lane, all);
            for (int l = 0; l < n; l++)
            {
                var g = Gather(data, lane, l);
                string w = $"{typeof(T).Name} dim {dim} lane {l}";
                Near(g.Sum(), D(sum[l]), 0, w + " sum");
                Near(g.Aggregate(1.0, (a, b) => a * b), D(prod[l]), tol, w + " prod");
                Near(g.Sum(x => x * x), D(ss[l]), 0, w + " sumsq");
                Near(g.Average(), D(mean[l]), tol, w + " mean");
                double m = g.Average(), m2 = g.Sum(x => (x - m) * (x - m));
                Near(m2 / g.Length, D(v0[l]), tol, w + " var0");
                Near(g.Length > 1 ? m2 / (g.Length - 1) : double.NaN, D(v1[l]), tol, w + " var1");
                Near(g.Length > 1 ? Math.Sqrt(m2 / (g.Length - 1)) : double.NaN, D(sd[l]), tol, w + " std");
                Assert.Equal(g.Max(), D(mx[l])); Assert.Equal(Array.IndexOf(g, g.Max()), imx[l]);
                Assert.Equal(g.Min(), D(mn[l])); Assert.Equal(Array.IndexOf(g, g.Min()), imn[l]);
                Assert.Equal(g.Max() - g.Min(), D(rng[l]));
                var s = g.Order().ToArray();
                Near(s.Length % 2 == 1 ? s[s.Length / 2] : (s[s.Length / 2 - 1] + s[s.Length / 2]) / 2, D(med[l]), tol, w + " median");
                Assert.Equal(g.Count(x => x != 0), nz[l]);
                Assert.Equal(g.Any(x => x != 0), any[l]);
                Assert.Equal(g.All(x => x != 0), all[l]);
            }
        }
    }

    [Fact]
    public void SumPolicies()
    {
        double[] x = [1, double.NaN, 3, 4, 5, double.NaN];
        var d = new double[2];
        Reductions.Sum<double>(x, new Lane(3, 1, 2), d);
        Assert.True(double.IsNaN(d[0])); Assert.True(double.IsNaN(d[1]));
        Reductions.Sum<double>(x, new Lane(3, 1, 2), d, NanPolicy.Omit);
        Assert.Equal([4.0, 9.0], d);
        Reductions.Sum<double>([double.NaN, double.NaN], Lane.Flat(2), d, NanPolicy.Omit);
        Assert.Equal(0, d[0]);
        Reductions.Product<double>([double.NaN, double.NaN], Lane.Flat(2), d, NanPolicy.Omit);
        Assert.Equal(1, d[0]);
        Reductions.Mean<double>([double.NaN, double.NaN], Lane.Flat(2), d, NanPolicy.Omit);
        Assert.True(double.IsNaN(d[0]));
        Reductions.Mean<double>([1, double.NaN, 3], Lane.Flat(3), d, NanPolicy.Omit);
        Assert.Equal(2, d[0]);
    }

    [Fact]
    public void InfinitiesAndSignedZero()
    {
        var d = new double[1];
        Reductions.Sum<double>([double.PositiveInfinity, 1], Lane.Flat(2), d, compensated: true);
        Assert.Equal(double.PositiveInfinity, d[0]);
        Reductions.Sum<double>([double.PositiveInfinity, double.NegativeInfinity], Lane.Flat(2), d, compensated: true);
        Assert.True(double.IsNaN(d[0]));
        Reductions.Sum<double>([double.MaxValue, double.MaxValue, -double.MaxValue], Lane.Flat(3), d, compensated: true);
        Assert.Equal(double.PositiveInfinity, d[0]);   // overflow is not hidden by compensation
        var idx = new int[1];
        Reductions.Max<double>([-0.0, 0.0], Lane.Flat(2), d, idx, NanPolicy.Propagate);
        Assert.Equal(0, idx[0]); Assert.True(double.IsNegative(d[0]));   // the first of equal elements wins
        Reductions.Min<double>([0.0, -0.0], Lane.Flat(2), d, idx, NanPolicy.Propagate);
        Assert.Equal(0, idx[0]); Assert.False(double.IsNegative(d[0]));
    }

    [Fact]
    public void CompensationRecoversWhatPlainSummationLoses()
    {
        double[] x = [1e16, 1, -1e16];
        var d = new double[1];
        Reductions.Sum<double>(x, Lane.Flat(3), d);
        Assert.Equal(0, d[0]);
        Reductions.Sum<double>(x, Lane.Flat(3), d, compensated: true);
        Assert.Equal(1, d[0]);
        Reductions.Mean<double>(x, Lane.Flat(3), d, compensated: true);
        Assert.Equal(1.0 / 3, d[0]);
        var many = Enumerable.Repeat(0.1, 1_000_000).ToArray();
        Reductions.Sum<double>(many, Lane.Flat(many.Length), d, compensated: true);
        Assert.Equal(100000.0, d[0], 9);
    }

    [Fact]
    public void ExtremumNanPolicyAndIndices()
    {
        double[] x = [2, double.NaN, 5, 5, double.NaN];
        var v = new double[1]; var i = new int[1];
        Reductions.Max<double>(x, Lane.Flat(5), v, i, NanPolicy.Propagate);
        Assert.True(double.IsNaN(v[0])); Assert.Equal(1, i[0]);   // first NaN
        Reductions.Max<double>(x, Lane.Flat(5), v, i, NanPolicy.Omit);
        Assert.Equal(5, v[0]); Assert.Equal(2, i[0]);             // first of the ties
        Reductions.Min<double>(x, Lane.Flat(5), v, i, NanPolicy.Omit);
        Assert.Equal(2, v[0]); Assert.Equal(0, i[0]);
        Reductions.Min<double>([double.NaN, double.NaN], Lane.Flat(2), v, i, NanPolicy.Omit);
        Assert.True(double.IsNaN(v[0])); Assert.Equal(0, i[0]);
        Reductions.Max<double>([3, 1], Lane.Flat(2), v, Span<int>.Empty, NanPolicy.Omit);   // indices are optional
        Assert.Equal(3, v[0]);
        Assert.Throws<ArgumentException>(() => Reductions.Max<double>([], Lane.Flat(0), new double[1], Span<int>.Empty, NanPolicy.Omit));
    }

    [Fact]
    public void VarianceDivisorAndDegenerateLanes()
    {
        var d = new double[1];
        Reductions.Variance<double>([5], Lane.Flat(1), d, ddof: 1);
        Assert.True(double.IsNaN(d[0]));
        Reductions.Variance<double>([5], Lane.Flat(1), d, ddof: 0);
        Assert.Equal(0, d[0]);
        Reductions.Variance<double>([1, double.NaN, 3], Lane.Flat(3), d, ddof: 1, NanPolicy.Omit);
        Assert.Equal(2, d[0]);
        Reductions.Variance<double>([1, double.NaN, 3], Lane.Flat(3), d, ddof: 1);
        Assert.True(double.IsNaN(d[0]));
        Reductions.StandardDeviation<double>([2, 4, 4, 4, 5, 5, 7, 9], Lane.Flat(8), d, ddof: 0);
        Assert.Equal(2, d[0]);
        Assert.Throws<ArgumentOutOfRangeException>(() => Reductions.Variance<double>([1, 2], Lane.Flat(2), d, -1));
        // Welford stays accurate where the textbook sum-of-squares formula cancels completely.
        double[] big = [1e9 + 4, 1e9 + 7, 1e9 + 13, 1e9 + 16];
        Reductions.Variance<double>(big, Lane.Flat(4), d, ddof: 1);
        Assert.Equal(30.0, d[0], 6);
    }

    [Fact]
    public void MedianPolicies()
    {
        var d = new double[1];
        Reductions.Median<double>([3, 1, 2], Lane.Flat(3), d); Assert.Equal(2, d[0]);
        Reductions.Median<double>([4, 1, 2, 3], Lane.Flat(4), d); Assert.Equal(2.5, d[0]);
        Reductions.Median<double>([1, double.NaN, 3], Lane.Flat(3), d); Assert.True(double.IsNaN(d[0]));
        Reductions.Median<double>([1, double.NaN, 3], Lane.Flat(3), d, NanPolicy.Omit); Assert.Equal(2, d[0]);
        Reductions.Median<double>([double.NegativeInfinity, double.PositiveInfinity], Lane.Flat(2), d); Assert.True(double.IsNaN(d[0]));
        Reductions.Median<double>([double.PositiveInfinity, double.PositiveInfinity], Lane.Flat(2), d); Assert.Equal(double.PositiveInfinity, d[0]);
        Reductions.Median<double>([double.MaxValue, double.MaxValue], Lane.Flat(2), d); Assert.Equal(double.MaxValue, d[0]);   // no overflow
    }

    [Fact]
    public void AnyAllCountTreatNaNAsRequested()
    {
        double[] x = [0, double.NaN, 0];
        var b = new bool[1]; var n = new int[1];
        Reductions.Any<double>(x, Lane.Flat(3), b); Assert.True(b[0]);
        Reductions.Any<double>(x, Lane.Flat(3), b, nanIsNonZero: false); Assert.False(b[0]);
        Reductions.All<double>([1, double.NaN], Lane.Flat(2), b); Assert.True(b[0]);
        Reductions.All<double>([1, double.NaN], Lane.Flat(2), b, nanIsNonZero: false); Assert.False(b[0]);
        Reductions.CountNonZero<double>(x, Lane.Flat(3), n); Assert.Equal(1, n[0]);
        Reductions.All<int>([], Lane.Flat(0), b); Assert.True(b[0]);
        Reductions.Any<int>([], Lane.Flat(0), b); Assert.False(b[0]);
    }

    [Fact]
    public void IntegerTypesSumProductAndCount()
    {
        var s = new int[2]; var p = new long[2];
        Reductions.Sum<int>([1, 2, 3, 4, 5, 6], new Lane(3, 1, 2), s); Assert.Equal([6, 15], s);
        Reductions.Product<long>([1, 2, 3, 4, 5, 6], new Lane(3, 1, 2), p); Assert.Equal([6L, 120L], p);
    }

    [Fact]
    public void EmptyLanesGiveTheIdentity()
    {
        var d = new double[3];
        var lane = new Lane(0, 1, 3);
        Reductions.Sum<double>([], lane, d); Assert.Equal([0, 0, 0], d);
        Reductions.Product<double>([], lane, d); Assert.Equal([1, 1, 1], d);
        Reductions.Mean<double>([], lane, d); Assert.All(d, x => Assert.True(double.IsNaN(x)));
        Reductions.Median<double>([], lane, d); Assert.All(d, x => Assert.True(double.IsNaN(x)));
        Reductions.Sum<double>([], new Lane(3, 0, 1), d);   // no lanes at all: no-op
    }

    [Fact]
    public void ContractViolationsAreRejected()
    {
        var d = new double[2];
        Assert.Throws<ArgumentException>(() => Reductions.Sum<double>(new double[5], new Lane(3, 1, 2), d));       // source too short
        Assert.Throws<ArgumentException>(() => Reductions.Sum<double>(new double[6], new Lane(3, 1, 2), new double[1]));   // destination too short
        var both = new double[6];
        Assert.Throws<ArgumentException>(() => Reductions.Sum<double>(both, new Lane(3, 1, 2), both.AsSpan(1, 2)));   // overlap
        Assert.Throws<ArgumentException>(() => Reductions.Sum<double>(new double[6], new Lane(-3, 1, 2), d));
    }

    [Fact]
    public void LargerLaneLayoutsAgreeWithAnIndependentGather()
    {
        var rnd = new Random(12345);
        int[] shape = [3, 5, 2, 4];
        var x = Enumerable.Range(0, 120).Select(_ => rnd.NextDouble() * 20 - 10).ToArray();
        for (int axis = 0; axis < 4; axis++)
        {
            var lane = Lane.ForRowMajorAxis(shape, axis);
            var s = new double[lane.Count]; var mx = new double[lane.Count]; var ix = new int[lane.Count];
            Reductions.Sum<double>(x, lane, s); Reductions.Max<double>(x, lane, mx, ix, NanPolicy.Propagate);
            for (int l = 0; l < lane.Count; l++)
            {
                var g = Gather(x, lane, l);
                Assert.Equal(g.Sum(), s[l], 10); Assert.Equal(g.Max(), mx[l]); Assert.Equal(Array.IndexOf(g, g.Max()), ix[l]);
            }
        }
    }

    [Fact]
    public void WarmDoubleReductionsAllocateNoManagedBytes()
    {
        var x = new double[1000]; var d = new double[10]; var i = new int[10];
        var lane = new Lane(100, 1, 10);
        for (int r = 0; r < 100; r++) { Reductions.Sum<double>(x, lane, d); Reductions.Variance<double>(x, lane, d, 1); Reductions.Max<double>(x, lane, d, i, NanPolicy.Omit); }
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int r = 0; r < 100; r++) { Reductions.Sum<double>(x, lane, d); Reductions.Variance<double>(x, lane, d, 1); Reductions.Max<double>(x, lane, d, i, NanPolicy.Omit); }
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }
}
