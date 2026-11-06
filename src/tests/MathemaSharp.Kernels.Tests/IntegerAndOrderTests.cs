// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using MathemaSharp.Kernels;
using Xunit;

namespace MathemaSharp.Kernels.Tests;

/// <summary>Max, Min and scans accept any INumber (integers included); SequentialFromFirst is NumPy's order along a strided axis.</summary>
public class IntegerAndOrderTests
{
    [Fact]
    public void MaxMinOfIntegersWithIndices()
    {
        sbyte[] data = [3, -7, 9, 9, -128, 127];
        var v = new sbyte[1]; var i = new int[1];
        Reductions.Max<sbyte>(data, Lane.Flat(6), v, i, NanPolicy.Propagate);
        Assert.Equal(127, v[0]); Assert.Equal(5, i[0]);
        Reductions.Min<sbyte>(data, Lane.Flat(6), v, i, NanPolicy.Omit);
        Assert.Equal(-128, v[0]); Assert.Equal(4, i[0]);
        Reductions.Max<sbyte>(data[2..4], Lane.Flat(2), v, i, NanPolicy.Propagate);
        Assert.Equal(0, i[0]);   // the first of equal elements wins
    }

    [Fact]
    public void MaxMinOfUnsignedAndLongLanes()
    {
        ulong[] data = [1, ulong.MaxValue, 5, 2, 8, 3];   // two lanes of three
        var v = new ulong[2];
        Reductions.Max<ulong>(data, new Lane(3, 1, 2), v, Span<int>.Empty, NanPolicy.Propagate);
        Assert.Equal(new ulong[] { ulong.MaxValue, 8 }, v);
        Reductions.Min<ulong>(data, new Lane(3, 1, 2), v, Span<int>.Empty, NanPolicy.Propagate);
        Assert.Equal(new ulong[] { 1, 2 }, v);
    }

    [Fact]
    public void FloatNanBehaviourIsUnchanged()
    {
        var v = new double[1]; var i = new int[1];
        Reductions.Max<double>([1, double.NaN, 3], Lane.Flat(3), v, i, NanPolicy.Propagate);
        Assert.True(double.IsNaN(v[0])); Assert.Equal(1, i[0]);
        Reductions.Max<double>([double.NaN, double.NaN], Lane.Flat(2), v, i, NanPolicy.Omit);
        Assert.True(double.IsNaN(v[0])); Assert.Equal(0, i[0]);
        var h = new Half[1];
        Reductions.Min<Half>([(Half)2, Half.NaN, (Half)1], Lane.Flat(3), h, Span<int>.Empty, NanPolicy.Omit);
        Assert.Equal((Half)1, h[0]);
    }

    [Fact]
    public void ScansOfIntegersWrapLikeTheType()
    {
        var d = new int[4];
        Scans.Scan<int>([1, 2, 3, 4], Lane.Flat(4), d, ScanOp.Sum);
        Assert.Equal(new[] { 1, 3, 6, 10 }, d);
        Scans.Scan<int>([1, 2, 3, 4], Lane.Flat(4), d, ScanOp.Product);
        Assert.Equal(new[] { 1, 2, 6, 24 }, d);
        var b = new byte[3];
        Scans.Scan<byte>([200, 100, 1], Lane.Flat(3), b, ScanOp.Sum);
        Assert.Equal(new byte[] { 200, 44, 45 }, b);   // 300 wraps to 44
        Scans.Scan<int>([3, 1, 4, 1, 5], Lane.Flat(5), new int[5], ScanOp.Max);
    }

    [Fact]
    public void MedianAverageOfTheTwoMiddleValues()
    {
        var d = new double[1];
        Reductions.Median<double>([double.MaxValue, double.MaxValue], Lane.Flat(2), d);
        Assert.Equal(double.MaxValue, d[0]);                                           // overflow-safe (MATLAB, Octave)
        Reductions.Median<double>([double.MaxValue, double.MaxValue], Lane.Flat(2), d, NanPolicy.Propagate, MedianAverage.Mean);
        Assert.True(double.IsPositiveInfinity(d[0]));                                  // NumPy: (a + b) / 2 overflows
        Reductions.Median<double>([3, 1, 2], Lane.Flat(3), d, NanPolicy.Propagate, MedianAverage.Mean);
        Assert.Equal(2, d[0]);                                                          // odd length: the middle value, untouched
        Reductions.Median<double>([4, 1, 2, 3], Lane.Flat(4), d, NanPolicy.Propagate, MedianAverage.Mean);
        Assert.Equal(2.5, d[0]);
    }

    [Fact]
    public void SequentialFromFirstKeepsNegativeZero()
    {
        var d = new double[1];
        Reductions.Sum<double>([-0.0, -0.0], Lane.Flat(2), d, NanPolicy.Propagate, SumMethod.SequentialFromFirst);
        Assert.True(double.IsNegative(d[0]));           // numpy: add.reduce([-0.0, -0.0]) is -0.0
        Reductions.Sum<double>([-0.0, -0.0], Lane.Flat(2), d, NanPolicy.Propagate, SumMethod.Sequential);
        Assert.False(double.IsNegative(d[0]));          // from zero: 0 + -0 = +0
    }

    [Fact]
    public void SequentialFromFirstNeverUsesPairwiseGrouping()
    {
        // 1e16 + 1 + 1 + ... : left to right loses every 1; the pairwise grouping of 16 values does not.
        var data = new double[16];
        data[0] = 1e16; for (int k = 1; k < 16; k++) data[k] = 1;
        var seq = new double[1]; var pair = new double[1];
        Reductions.Sum<double>(data, Lane.Flat(16), seq, NanPolicy.Propagate, SumMethod.SequentialFromFirst);
        Reductions.Sum<double>(data, Lane.Flat(16), pair, NanPolicy.Propagate, SumMethod.Pairwise);
        Assert.Equal(1e16, seq[0]);
        Assert.NotEqual(seq[0], pair[0]);
    }
}
