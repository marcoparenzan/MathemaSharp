// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using MathemaSharp.Kernels;
using Xunit;

namespace MathemaSharp.Kernels.Tests;

public class ScanTests
{
    static double[] Run(double[] x, Lane lane, ScanOp op, ScanDirection dir = ScanDirection.Forward, NanPolicy nan = NanPolicy.Propagate)
    {
        var d = new double[lane.Total];
        Scans.Scan<double>(x, lane, d, op, dir, nan);
        return d;
    }

    [Fact]
    public void ForwardAndReverse()
    {
        double[] x = [3, 1, 4, 1, 5];
        Assert.Equal([3, 4, 8, 9, 14], Run(x, Lane.Flat(5), ScanOp.Sum));
        Assert.Equal([14, 11, 10, 6, 5], Run(x, Lane.Flat(5), ScanOp.Sum, ScanDirection.Reverse));
        Assert.Equal([3, 3, 12, 12, 60], Run(x, Lane.Flat(5), ScanOp.Product));
        Assert.Equal([3, 3, 4, 4, 5], Run(x, Lane.Flat(5), ScanOp.Max));
        Assert.Equal([3, 1, 1, 1, 1], Run(x, Lane.Flat(5), ScanOp.Min));
        Assert.Equal([1, 1, 1, 1, 5], Run(x, Lane.Flat(5), ScanOp.Min, ScanDirection.Reverse));
    }

    [Fact]
    public void LanesAlongTheInnerAndOuterDimensions()
    {
        // 2 x 3 column-major: [[1,3,5],[2,4,6]] stored 1,2,3,4,5,6. cumsum along dim 0 (length 2, inner 1, outer 3) and dim 1 (length 3, inner 2, outer 1).
        double[] x = [1, 2, 3, 4, 5, 6];
        Assert.Equal([1, 3, 3, 7, 5, 11], Run(x, new Lane(2, 1, 3), ScanOp.Sum));
        Assert.Equal([1, 2, 4, 6, 9, 12], Run(x, new Lane(3, 2, 1), ScanOp.Sum));
    }

    [Fact]
    public void NanPolicies()
    {
        double nan = double.NaN;
        double[] x = [1, nan, 2, 3];
        var p = Run(x, Lane.Flat(4), ScanOp.Sum);
        Assert.Equal(1, p[0]); Assert.True(double.IsNaN(p[1]) && double.IsNaN(p[2]) && double.IsNaN(p[3]));
        Assert.Equal([1, 1, 3, 6], Run(x, Lane.Flat(4), ScanOp.Sum, nan: NanPolicy.Omit));
        Assert.Equal([1, 1, 2, 6], Run(x, Lane.Flat(4), ScanOp.Product, nan: NanPolicy.Omit));
        Assert.Equal([1, 1, 2, 3], Run(x, Lane.Flat(4), ScanOp.Max, nan: NanPolicy.Omit));
        var lead = Run([nan, 5, 2], Lane.Flat(3), ScanOp.Max, nan: NanPolicy.Omit);
        Assert.True(double.IsNaN(lead[0])); Assert.Equal(5, lead[1]); Assert.Equal(5, lead[2]);
        Assert.Equal([0, 5, 7], Run([nan, 5, 2], Lane.Flat(3), ScanOp.Sum, nan: NanPolicy.Omit));
        var mx = Run(x, Lane.Flat(4), ScanOp.Max);
        Assert.Equal(1, mx[0]); Assert.True(double.IsNaN(mx[3]));
    }

    [Fact]
    public void ExactInPlaceIsAllowedPartialOverlapIsNot()
    {
        double[] x = [1, 2, 3, 4];
        Scans.Scan<double>(x, Lane.Flat(4), x, ScanOp.Sum);
        Assert.Equal([1, 3, 6, 10], x);
        double[] y = [1, 2, 3, 4];
        Scans.Scan<double>(y, Lane.Flat(4), y, ScanOp.Sum, ScanDirection.Reverse);
        Assert.Equal([10, 9, 7, 4], y);
        double[] z = new double[6];
        Assert.Throws<ArgumentException>(() => Scans.Scan<double>(z.AsSpan(0, 4), Lane.Flat(4), z.AsSpan(1, 4), ScanOp.Sum));
    }

    [Fact]
    public void ScansHoldForEveryFormat()
    {
        Check<Half>(); Check<float>(); Check<double>(); Check<Decimal32>(); Check<Decimal64>(); Check<Decimal128>();
        static void Check<T>() where T : IFloatingPointIeee754<T>
        {
            T[] x = [T.CreateChecked(2), T.CreateChecked(-1), T.CreateChecked(3)];
            var d = new T[3];
            Scans.Scan<T>(x, Lane.Flat(3), d, ScanOp.Sum);
            Assert.Equal([2, 1, 4], d.Select(double.CreateChecked).ToArray());
            Scans.Scan<T>(x, Lane.Flat(3), d, ScanOp.Min);
            Assert.Equal([2, -1, -1], d.Select(double.CreateChecked).ToArray());
        }
    }

    [Fact]
    public void DifferenceOrders()
    {
        var d = new double[4];
        Scans.Difference<double>([1, 4, 9, 16, 25], Lane.Flat(5), d);
        Assert.Equal([3, 5, 7, 9], d);
        Scans.Difference<double>([1, 4, 9, 16, 25], Lane.Flat(5), d, 2);
        Assert.Equal([2, 2, 2], d.Take(3).ToArray());
        var copy = new double[5];
        Scans.Difference<double>([1, 4, 9, 16, 25], Lane.Flat(5), copy, 0);
        Assert.Equal([1, 4, 9, 16, 25], copy);   // order 0 copies
    }

    [Fact]
    public void DifferenceAlongDimensions()
    {
        // 2 x 3 column-major 1,2,3,4,5,6: dim 1 differences (length 3, inner 2) -> 2 x 2 = 2,2,2,2; dim 0 (length 2, inner 1, outer 3) -> 1,1,1.
        var a = new double[4]; var b = new double[3];
        Scans.Difference<double>([1, 2, 3, 4, 5, 6], new Lane(3, 2, 1), a);
        Assert.Equal([2, 2, 2, 2], a);
        Scans.Difference<double>([1, 2, 3, 4, 5, 6], new Lane(2, 1, 3), b);
        Assert.Equal([1, 1, 1], b);
        var none = new double[0];
        Scans.Difference<double>([1, 2], Lane.Flat(2), none, 2);   // order >= length: no output, no error
        Scans.Difference<int>([1, 2, 3], Lane.Flat(3), new int[2], 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => Scans.Difference<double>([1, 2], Lane.Flat(2), new double[1], -1));
        Assert.Throws<ArgumentException>(() => Scans.Difference<double>([1, 2, 3], Lane.Flat(3), new double[1]));
    }
}
