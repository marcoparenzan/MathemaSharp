// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using MathemaSharp.Kernels;
using Xunit;

namespace MathemaSharp.Kernels.Tests;

public class SortingTests
{
    static double[] Sorted(double[] data, SortDirection d = SortDirection.Ascending)
    {
        var r = new double[data.Length];
        Sorting.Sort<double>(data, Lane.Flat(data.Length), r, d);
        return r;
    }

    [Fact]
    public void AscendingPutsNanLast()
        => Assert.Equal(new[] { -3.0, 0.0, 2.0, 5.0, double.PositiveInfinity, double.NaN, double.NaN },
                        Sorted([5, double.NaN, -3, double.PositiveInfinity, 0, double.NaN, 2]));

    [Fact]
    public void DescendingPutsNanFirst()
        => Assert.Equal(new[] { double.NaN, double.NaN, double.PositiveInfinity, 5.0, 2.0, 0.0, -3.0 },
                        Sorted([5, double.NaN, -3, double.PositiveInfinity, 0, double.NaN, 2], SortDirection.Descending));

    [Fact]
    public void IntegersAndHalf()
    {
        var i = new int[5];
        Sorting.Sort<int>([3, -1, 4, 1, 5], Lane.Flat(5), i);
        Assert.Equal(new[] { -1, 1, 3, 4, 5 }, i);
        var u = new byte[4];
        Sorting.Sort<byte>([200, 3, 255, 0], Lane.Flat(4), u, SortDirection.Descending);
        Assert.Equal(new byte[] { 255, 200, 3, 0 }, u);
        var h = new Half[3];
        Sorting.Sort<Half>([(Half)2, Half.NaN, (Half)1], Lane.Flat(3), h);
        Assert.Equal((Half)1, h[0]); Assert.Equal((Half)2, h[1]); Assert.True(Half.IsNaN(h[2]));
    }

    [Fact]
    public void StridedLanesAndInPlace()
    {
        // 3 x 2 row-major, lanes along axis 0: [3,1,2] and [30,10,20]
        double[] data = [3, 30, 1, 10, 2, 20];
        var lane = Lane.ForRowMajorAxis([3, 2], 0);
        var r = new double[6];
        Sorting.Sort<double>(data, lane, r);
        Assert.Equal(new double[] { 1, 10, 2, 20, 3, 30 }, r);
        Sorting.Sort<double>(data, lane, data, SortDirection.Descending);   // exact in place
        Assert.Equal(new double[] { 3, 30, 2, 20, 1, 10 }, data);
        Assert.Throws<ArgumentException>(() => Sorting.Sort<double>(data.AsSpan(0, 5), Lane.Flat(5), data.AsSpan(1, 5)));
    }

    [Fact]
    public void ArgSortIsStableAndSendsNanToTheEnd()
    {
        var idx = new int[6];
        Sorting.ArgSort<double>([2, 1, 2, double.NaN, 1, 2], Lane.Flat(6), idx);
        Assert.Equal(new[] { 1, 4, 0, 2, 5, 3 }, idx);   // equal values keep their order; NaN last
        Sorting.ArgSort<double>([2, 1, 2, double.NaN, 1, 2], Lane.Flat(6), idx, SortDirection.Descending);
        Assert.Equal(new[] { 3, 0, 2, 5, 1, 4 }, idx);   // NaN first, then 2s in order, then 1s in order
    }

    [Fact]
    public void ArgSortAlongAxis()
    {
        double[] data = [3, 30, 1, 10, 2, 20];
        var idx = new int[6];
        Sorting.ArgSort<double>(data, Lane.ForRowMajorAxis([3, 2], 0), idx);
        Assert.Equal(new[] { 1, 1, 2, 2, 0, 0 }, idx);
    }

    [Fact]
    public void EmptyAndSingle()
    {
        Sorting.Sort<double>([], Lane.Flat(0), Span<double>.Empty);
        Sorting.ArgSort<double>([], Lane.Flat(0), Span<int>.Empty);
        Assert.Equal(new[] { 7.0 }, Sorted([7]));
    }

    [Theory]
    [InlineData(SearchSide.Left, new[] { 0, 0, 1, 3, 4, 5, 5 })]
    [InlineData(SearchSide.Right, new[] { 0, 1, 3, 4, 5, 5, 5 })]
    public void SearchSortedSides(SearchSide side, int[] expected)
    {
        // sorted [1, 2, 2, 5, 7] searched for 0, 1, 2, 5, 7, 8, 100: left counts the elements strictly below, right those below or equal
        double[] sorted = [1, 2, 2, 5, 7];
        double[] values = [0.0, 1.0, 2.0, 5.0, 7.0, 8.0, 100.0];
        var r = new int[values.Length];
        Sorting.SearchSorted<double>(sorted, values, r, side);
        Assert.Equal(expected, r);
    }

    [Fact]
    public void SearchSortedWithNanAndEmpty()
    {
        double[] sorted = [1, 2, double.NaN, double.NaN];
        var r = new int[3];
        Sorting.SearchSorted<double>(sorted, [double.NaN, 3.0, 0.0], r, SearchSide.Left);
        Assert.Equal(new[] { 2, 2, 0 }, r);
        Sorting.SearchSorted<double>(sorted, [double.NaN], r, SearchSide.Right);
        Assert.Equal(4, r[0]);
        Sorting.SearchSorted<double>([], [1.0], r);
        Assert.Equal(0, r[0]);
    }

    [Fact]
    public void LargeSortMatchesTheFrameworkSort()
    {
        var rnd = new Random(7);
        var data = Enumerable.Range(0, 5000).Select(_ => Math.Round(rnd.NextDouble() * 100, 1)).ToArray();
        var expected = (double[])data.Clone();
        Array.Sort(expected);
        Assert.Equal(expected, Sorted(data));
        var idx = new int[data.Length];
        Sorting.ArgSort<double>(data, Lane.Flat(data.Length), idx);
        for (int k = 1; k < idx.Length; k++)
        {
            Assert.True(data[idx[k - 1]] <= data[idx[k]]);
            if (data[idx[k - 1]] == data[idx[k]]) Assert.True(idx[k - 1] < idx[k]);   // stable
        }
    }
}
