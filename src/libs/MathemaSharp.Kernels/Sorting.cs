// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp.Kernels;

/// <summary>Direction of a sort. NaN is the largest value: last when ascending (NumPy, MATLAB), first when descending (MATLAB).</summary>
public enum SortDirection { Ascending, Descending }

/// <summary>Which of several equal positions a binary search returns.</summary>
public enum SearchSide
{
    /// <summary>The first position where the value could be inserted keeping the order (NumPy <c>side='left'</c>): the number of elements strictly before it.</summary>
    Left,
    /// <summary>The last such position (<c>side='right'</c>): the number of elements before or equal to it.</summary>
    Right,
}

/// <summary>Sorting and binary search along a lane, for real element types. The order is the total order of the type with NaN as the largest value, so a lane containing
/// NaN sorts like NumPy's (NaN last); negative and positive zero compare equal. Complex ordering is not handled here.</summary>
public static class Sorting
{
    readonly struct Ascending<T> : IComparer<T> where T : INumber<T>
    {
        public int Compare(T? x, T? y)
        {
            bool nx = T.IsNaN(x!), ny = T.IsNaN(y!);
            if (nx || ny) return nx == ny ? 0 : nx ? 1 : -1;
            return x! < y! ? -1 : x! > y! ? 1 : 0;
        }
    }

    readonly struct Descending<T> : IComparer<T> where T : INumber<T>
    {
        public int Compare(T? x, T? y) => default(Ascending<T>).Compare(y, x);
    }

    static void Check<T>(ReadOnlySpan<T> src, Lane lane, int dstLength, bool dstOverlapsSrc, string dstName)
    {
        lane.CheckSource(src.Length, nameof(src));
        if (dstLength < lane.Total) throw new ArgumentException("Destination is shorter than the lane layout.", dstName);
        if (lane.Total > 0 && dstOverlapsSrc) throw new ArgumentException("Destination may only overlap the source exactly.", dstName);
    }

    /// <summary>Sorts each lane of <paramref name="src"/> into <paramref name="dst"/> (same layout). The destination may be the source itself (exact in-place) but not partly overlap it.
    /// The sort is not stable: equal elements (negative and positive zero) may swap, as in NumPy's default sort. It is the framework's introspective sort with this class's order,
    /// so a contiguous lane is sorted in place without a copy.</summary>
    public static void Sort<T>(ReadOnlySpan<T> src, Lane lane, Span<T> dst, SortDirection direction = SortDirection.Ascending) where T : INumber<T>
    {
        bool partial = lane.Total > 0 && src.Overlaps(dst, out int offset) && offset != 0;
        Check(src, lane, dst.Length, partial, nameof(dst));
        bool inPlace = lane.Total > 0 && src.Overlaps(dst, out int o2) && o2 == 0;
        if (!inPlace && lane.Total > 0) src.Slice(0, lane.Total).CopyTo(dst);
        var temp = lane.Inner == 1 ? null : new T[lane.Length];
        for (int l = 0; l < lane.Count; l++)
        {
            int p0 = lane.Start(l);
            Span<T> run;
            if (lane.Inner == 1) run = dst.Slice(p0, lane.Length);
            else
            {
                for (int k = 0, p = p0; k < lane.Length; k++, p += lane.Inner) temp![k] = dst[p];
                run = temp!;
            }
            if (direction == SortDirection.Ascending) run.Sort(default(Ascending<T>)); else run.Sort(default(Descending<T>));
            if (lane.Inner != 1) for (int k = 0, p = p0; k < lane.Length; k++, p += lane.Inner) dst[p] = run[k];
        }
    }

    /// <summary>For each lane, the 0-based positions along the lane that sort it: <c>dst</c> (lane layout, same length as the source) holds at its k-th place the index of the
    /// k-th smallest (largest when descending) element. The sort is stable: equal elements keep their order of appearance, also when descending. The destination must not overlap the source.</summary>
    public static void ArgSort<T>(ReadOnlySpan<T> src, Lane lane, Span<int> dst, SortDirection direction = SortDirection.Ascending) where T : INumber<T>
    {
        Check(src, lane, dst.Length, false, nameof(dst));
        var values = new T[lane.Length];
        var order = new int[lane.Length];
        for (int l = 0; l < lane.Count; l++)
        {
            int p0 = lane.Start(l);
            for (int k = 0, p = p0; k < lane.Length; k++, p += lane.Inner) { values[k] = src[p]; order[k] = k; }
            if (direction == SortDirection.Ascending) Array.Sort(order, (i, j) => Stable(default(Ascending<T>), values, i, j));
            else Array.Sort(order, (i, j) => Stable(default(Descending<T>), values, i, j));
            for (int k = 0, p = p0; k < lane.Length; k++, p += lane.Inner) dst[p] = order[k];
        }
    }

    static int Stable<T, TC>(TC comparer, T[] values, int i, int j) where TC : IComparer<T>
    {
        int c = comparer.Compare(values[i], values[j]);
        return c != 0 ? c : i.CompareTo(j);
    }

    /// <summary>For each value, the position at which it would be inserted into the ascending, NaN-last <paramref name="sorted"/> sequence to keep it ordered (binary search).
    /// <paramref name="result"/> receives one position per value. NaN values go after every number and before or after existing NaNs according to <paramref name="side"/>.
    /// The sequence is assumed sorted; the result for an unsorted one is unspecified.</summary>
    public static void SearchSorted<T>(ReadOnlySpan<T> sorted, ReadOnlySpan<T> values, Span<int> result, SearchSide side = SearchSide.Left) where T : INumber<T>
    {
        if (result.Length < values.Length) throw new ArgumentException("Result is shorter than the values.", nameof(result));
        var cmp = default(Ascending<T>);
        for (int v = 0; v < values.Length; v++)
        {
            int lo = 0, hi = sorted.Length;
            while (lo < hi)
            {
                int mid = (int)(((uint)lo + (uint)hi) >> 1);
                int c = cmp.Compare(sorted[mid], values[v]);
                bool goRight = side == SearchSide.Left ? c < 0 : c <= 0;
                if (goRight) lo = mid + 1; else hi = mid;
            }
            result[v] = lo;
        }
    }
}
