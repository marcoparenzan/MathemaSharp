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

/// <summary>Sorting and binary search along a lane. Real element types are ordered with NaN as the largest value, so a lane containing NaN sorts like NumPy's (NaN last);
/// negative and positive zero compare equal. <see cref="Complex"/> has no order in .NET (it implements the arithmetic of generic math, not comparison), so the
/// <c>Complex</c> overloads (<see cref="SortComplex"/> and the others) define NumPy's: lexicographic by real then imaginary part, with the NaN cases last in the order
/// <c>[R + Rj, R + nanj, nan + Rj, nan + nanj]</c>.</summary>
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

    /// <summary>NumPy's complex order, ascending.</summary>
    readonly struct ComplexAscending : IComparer<Complex>
    {
        static int Rank(Complex z) => (double.IsNaN(z.Real) ? 2 : 0) + (double.IsNaN(z.Imaginary) ? 1 : 0);

        public int Compare(Complex x, Complex y)
        {
            int rx = Rank(x), ry = Rank(y);
            if (rx != ry) return rx < ry ? -1 : 1;
            switch (rx)
            {
                case 0:
                    if (x.Real != y.Real) return x.Real < y.Real ? -1 : 1;
                    return x.Imaginary < y.Imaginary ? -1 : x.Imaginary > y.Imaginary ? 1 : 0;
                case 1: return x.Real < y.Real ? -1 : x.Real > y.Real ? 1 : 0;                       // the imaginary part is NaN
                case 2: return x.Imaginary < y.Imaginary ? -1 : x.Imaginary > y.Imaginary ? 1 : 0;   // the real part is NaN
                default: return 0;
            }
        }
    }

    readonly struct ComplexDescending : IComparer<Complex>
    {
        public int Compare(Complex x, Complex y) => default(ComplexAscending).Compare(y, x);
    }

    static void Check<T>(ReadOnlySpan<T> src, Lane lane, int dstLength, bool dstOverlapsSrc, string dstName)
    {
        lane.CheckSource(src.Length, nameof(src));
        if (dstLength < lane.Total) throw new ArgumentException("Destination is shorter than the lane layout.", dstName);
        if (lane.Total > 0 && dstOverlapsSrc) throw new ArgumentException("Destination may only overlap the source exactly.", dstName);
    }

    // ------------------------------------------------------------------ sort

    /// <summary>Sorts each lane of <paramref name="src"/> into <paramref name="dst"/> (same layout). The destination may be the source itself (exact in-place) but not partly overlap it.
    /// The sort is not stable: equal elements (negative and positive zero) may swap, as in NumPy's default sort. It is the framework's introspective sort with this class's order,
    /// so a contiguous lane is sorted in place without a copy.</summary>
    public static void Sort<T>(ReadOnlySpan<T> src, Lane lane, Span<T> dst, SortDirection direction = SortDirection.Ascending) where T : INumber<T>
    {
        if (direction == SortDirection.Ascending) SortCore(src, lane, dst, default(Ascending<T>));
        else SortCore(src, lane, dst, default(Descending<T>));
    }

    /// <summary>As <see cref="Sort{T}"/> for complex numbers, in NumPy's lexicographic order.</summary>
    public static void SortComplex(ReadOnlySpan<Complex> src, Lane lane, Span<Complex> dst, SortDirection direction = SortDirection.Ascending)
    {
        if (direction == SortDirection.Ascending) SortCore(src, lane, dst, default(ComplexAscending));
        else SortCore(src, lane, dst, default(ComplexDescending));
    }

    static void SortCore<T, TC>(ReadOnlySpan<T> src, Lane lane, Span<T> dst, TC comparer) where TC : IComparer<T>
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
            run.Sort(comparer);
            if (lane.Inner != 1) for (int k = 0, p = p0; k < lane.Length; k++, p += lane.Inner) dst[p] = run[k];
        }
    }

    // ------------------------------------------------------------------ argsort

    /// <summary>For each lane, the 0-based positions along the lane that sort it: <c>dst</c> (lane layout, same length as the source) holds at its k-th place the index of the
    /// k-th smallest (largest when descending) element. The sort is stable: equal elements keep their order of appearance, also when descending. The destination must not overlap the source.</summary>
    public static void ArgSort<T>(ReadOnlySpan<T> src, Lane lane, Span<int> dst, SortDirection direction = SortDirection.Ascending) where T : INumber<T>
    {
        if (direction == SortDirection.Ascending) ArgSortCore(src, lane, dst, default(Ascending<T>));
        else ArgSortCore(src, lane, dst, default(Descending<T>));
    }

    /// <summary>As <see cref="ArgSort{T}"/> for complex numbers, in NumPy's lexicographic order.</summary>
    public static void ArgSortComplex(ReadOnlySpan<Complex> src, Lane lane, Span<int> dst, SortDirection direction = SortDirection.Ascending)
    {
        if (direction == SortDirection.Ascending) ArgSortCore(src, lane, dst, default(ComplexAscending));
        else ArgSortCore(src, lane, dst, default(ComplexDescending));
    }

    static void ArgSortCore<T, TC>(ReadOnlySpan<T> src, Lane lane, Span<int> dst, TC comparer) where TC : IComparer<T>
    {
        Check(src, lane, dst.Length, false, nameof(dst));
        var values = new T[lane.Length];
        var order = new int[lane.Length];
        for (int l = 0; l < lane.Count; l++)
        {
            int p0 = lane.Start(l);
            for (int k = 0, p = p0; k < lane.Length; k++, p += lane.Inner) { values[k] = src[p]; order[k] = k; }
            Array.Sort(order, (i, j) =>
            {
                int c = comparer.Compare(values[i], values[j]);
                return c != 0 ? c : i.CompareTo(j);
            });
            for (int k = 0, p = p0; k < lane.Length; k++, p += lane.Inner) dst[p] = order[k];
        }
    }

    // ------------------------------------------------------------------ binary search

    /// <summary>For each value, the position at which it would be inserted into the ascending, NaN-last <paramref name="sorted"/> sequence to keep it ordered (binary search).
    /// <paramref name="result"/> receives one position per value. NaN values go after every number and before or after existing NaNs according to <paramref name="side"/>.
    /// The sequence is assumed sorted; the result for an unsorted one is unspecified.</summary>
    public static void SearchSorted<T>(ReadOnlySpan<T> sorted, ReadOnlySpan<T> values, Span<int> result, SearchSide side = SearchSide.Left) where T : INumber<T>
        => SearchCore(sorted, values, result, side, default(Ascending<T>));

    /// <summary>As <see cref="SearchSorted{T}"/> for complex numbers, in NumPy's lexicographic order.</summary>
    public static void SearchSortedComplex(ReadOnlySpan<Complex> sorted, ReadOnlySpan<Complex> values, Span<int> result, SearchSide side = SearchSide.Left)
        => SearchCore(sorted, values, result, side, default(ComplexAscending));

    static void SearchCore<T, TC>(ReadOnlySpan<T> sorted, ReadOnlySpan<T> values, Span<int> result, SearchSide side, TC comparer) where TC : IComparer<T>
    {
        if (result.Length < values.Length) throw new ArgumentException("Result is shorter than the values.", nameof(result));
        for (int v = 0; v < values.Length; v++)
        {
            int lo = 0, hi = sorted.Length;
            while (lo < hi)
            {
                int mid = (int)(((uint)lo + (uint)hi) >> 1);
                int c = comparer.Compare(sorted[mid], values[v]);
                bool goRight = side == SearchSide.Left ? c < 0 : c <= 0;
                if (goRight) lo = mid + 1; else hi = mid;
            }
            result[v] = lo;
        }
    }
}
