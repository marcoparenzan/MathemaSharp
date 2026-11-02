// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp.Kernels;

/// <summary>Reductions along a lane. Every method writes <see cref="Lane.Count"/> results to the destination prefix
/// (result of lane l at index l), leaves the tail untouched and is a no-op when there are no lanes. The destination must not
/// overlap the source. Semantic choices (NaN, normalisation, ties) are parameters; no method has a language default.
/// A lane of length zero gives the identity of the operation (sum 0, product 1, count 0, any false, all true) or NaN
/// (mean, variance, median) and throws for Min/Max, which have no identity.</summary>
public static class Reductions
{
    static void CheckMixed<TS, TD>(ReadOnlySpan<TS> src, Lane lane, Span<TD> dst)
    {
        lane.CheckSource(src.Length, nameof(src));
        if (dst.Length < lane.Count) throw new ArgumentException("Destination is shorter than the number of lanes.", nameof(dst));
    }

    static void Check<T>(ReadOnlySpan<T> src, Lane lane, Span<T> dst)
    {
        CheckMixed<T, T>(src, lane, dst);
        if (lane.Count > 0 && src.Overlaps(dst)) throw new ArgumentException("Destination must not overlap the source.", nameof(dst));
    }

    /// <summary>Sum of each lane, accumulated as <paramref name="method"/> says (see <see cref="SumMethod"/>); zero for an empty lane.
    /// Integer types wrap or throw as the type's own addition does; <see cref="SumMethod.Compensated"/> is meant for floating-point types.</summary>
    public static void Sum<T>(ReadOnlySpan<T> src, Lane lane, Span<T> dst, NanPolicy nan = NanPolicy.Propagate, SumMethod method = SumMethod.Sequential)
        where T : INumber<T>
    {
        Check(src, lane, dst);
        T[]? scratch = null;
        for (int l = 0; l < lane.Count; l++) dst[l] = SumLane(src, lane, lane.Start(l), nan, method, ref scratch, out _);
    }

    static T SumLane<T>(ReadOnlySpan<T> src, Lane lane, int p0, NanPolicy nan, SumMethod method, ref T[]? scratch, out int count) where T : INumber<T>
    {
        count = 0;
        int p = p0;
        if (method == SumMethod.Pairwise)
        {
            if (lane.Length == 0) return T.Zero;
            if (lane.Inner == 1 && nan == NanPolicy.Propagate) { count = lane.Length; return Pairwise(src.Slice(p0, lane.Length)); }
            scratch ??= new T[lane.Length];
            for (int k = 0; k < lane.Length; k++, p += lane.Inner)
            {
                T x = src[p];
                bool skip = nan == NanPolicy.Omit && T.IsNaN(x);
                scratch[k] = skip ? T.Zero : x;
                if (!skip) count++;
            }
            var run = scratch.AsSpan(0, lane.Length);
            return lane.Inner == 1 ? Pairwise(run) : SequentialFromFirst(run);
        }
        T s = T.Zero, c = T.Zero;
        for (int k = 0; k < lane.Length; k++, p += lane.Inner)
        {
            T x = src[p];
            if (nan == NanPolicy.Omit && T.IsNaN(x)) continue;
            count++;
            if (method == SumMethod.Sequential) { s += x; continue; }
            T t = s + x;
            if (T.IsFinite(t)) c += T.Abs(s) >= T.Abs(x) ? (s - t) + x : (x - t) + s;
            s = t;
        }
        return method == SumMethod.Compensated && T.IsFinite(s) ? s + c : s;
    }

    static T SequentialFromFirst<T>(ReadOnlySpan<T> x) where T : INumber<T>
    {
        T s = x[0];
        for (int i = 1; i < x.Length; i++) s += x[i];
        return s;
    }

    /// <summary>NumPy's pairwise summation of a contiguous run (n ≥ 1): below 8 elements plain; up to 128 eight interleaved accumulators
    /// combined as ((r0+r1)+(r2+r3))+((r4+r5)+(r6+r7)); above, two halves, the first one a multiple of 8 long.</summary>
    static T Pairwise<T>(ReadOnlySpan<T> x) where T : INumber<T>
    {
        int n = x.Length;
        if (n < 8) return SequentialFromFirst(x);
        if (n <= 128)
        {
            T r0 = x[0], r1 = x[1], r2 = x[2], r3 = x[3], r4 = x[4], r5 = x[5], r6 = x[6], r7 = x[7];
            int i = 8;
            for (; i < n - (n % 8); i += 8)
            {
                r0 += x[i]; r1 += x[i + 1]; r2 += x[i + 2]; r3 += x[i + 3];
                r4 += x[i + 4]; r5 += x[i + 5]; r6 += x[i + 6]; r7 += x[i + 7];
            }
            T res = ((r0 + r1) + (r2 + r3)) + ((r4 + r5) + (r6 + r7));
            for (; i < n; i++) res += x[i];
            return res;
        }
        int n2 = n / 2;
        n2 -= n2 % 8;
        return Pairwise(x[..n2]) + Pairwise(x[n2..]);
    }

    /// <summary>Sum of squares of each lane (MATLAB <c>sumsq</c>), plain left to right.</summary>
    public static void SumOfSquares<T>(ReadOnlySpan<T> src, Lane lane, Span<T> dst, NanPolicy nan = NanPolicy.Propagate)
        where T : INumber<T>
    {
        Check(src, lane, dst);
        for (int l = 0; l < lane.Count; l++)
        {
            int p = lane.Start(l);
            T s = T.Zero;
            for (int k = 0; k < lane.Length; k++, p += lane.Inner)
            {
                T x = src[p];
                if (nan == NanPolicy.Omit && T.IsNaN(x)) continue;
                s += x * x;
            }
            dst[l] = s;
        }
    }

    /// <summary>Product of each lane, plain left to right.</summary>
    public static void Product<T>(ReadOnlySpan<T> src, Lane lane, Span<T> dst, NanPolicy nan = NanPolicy.Propagate)
        where T : INumber<T>
    {
        Check(src, lane, dst);
        for (int l = 0; l < lane.Count; l++)
        {
            int p = lane.Start(l);
            T s = T.One;
            for (int k = 0; k < lane.Length; k++, p += lane.Inner)
            {
                T x = src[p];
                if (nan == NanPolicy.Omit && T.IsNaN(x)) continue;
                s *= x;
            }
            dst[l] = s;
        }
    }

    /// <summary>Arithmetic mean of each lane: the sum of the retained elements (accumulated as <paramref name="method"/>) divided by their count; NaN when none is retained.</summary>
    public static void Mean<T>(ReadOnlySpan<T> src, Lane lane, Span<T> dst, NanPolicy nan = NanPolicy.Propagate, SumMethod method = SumMethod.Sequential)
        where T : IFloatingPointIeee754<T>
    {
        Check(src, lane, dst);
        T[]? scratch = null;
        for (int l = 0; l < lane.Count; l++)
        {
            T s = SumLane(src, lane, lane.Start(l), nan, method, ref scratch, out int n);
            dst[l] = n == 0 ? T.NaN : s / T.CreateChecked(n);
        }
    }

    /// <summary>Variance of each lane in one pass (Welford), dividing by <c>n − ddof</c> where n is the number of retained elements.
    /// NaN when <c>n − ddof ≤ 0</c>. <paramref name="ddof"/> = 1 is MATLAB's default (<c>w = 0</c>), 0 is NumPy's default and MATLAB's <c>w = 1</c>.
    /// MATLAB's rule that the variance of a scalar is 0 is an adapter concern.</summary>
    public static void Variance<T>(ReadOnlySpan<T> src, Lane lane, Span<T> dst, int ddof, NanPolicy nan = NanPolicy.Propagate,
        VarianceMethod method = VarianceMethod.Welford) where T : IFloatingPointIeee754<T>
    {
        if (ddof < 0) throw new ArgumentOutOfRangeException(nameof(ddof));
        Check(src, lane, dst);
        T[]? scratch = null, deviations = null;
        for (int l = 0; l < lane.Count; l++)
            dst[l] = method == VarianceMethod.Welford ? VarianceLane(src, lane, l, ddof, nan) : TwoPassLane(src, lane, l, ddof, nan, ref scratch, ref deviations);
    }

    static T TwoPassLane<T>(ReadOnlySpan<T> src, Lane lane, int l, int ddof, NanPolicy nan, ref T[]? scratch, ref T[]? deviations) where T : IFloatingPointIeee754<T>
    {
        int p0 = lane.Start(l);
        T sum = SumLane(src, lane, p0, nan, SumMethod.Pairwise, ref scratch, out int n);
        if (n - ddof <= 0) return T.NaN;
        T mean = sum / T.CreateChecked(n);
        deviations ??= new T[lane.Length];
        for (int k = 0, p = p0; k < lane.Length; k++, p += lane.Inner)
        {
            T x = src[p];
            T d = x - mean;
            deviations[k] = nan == NanPolicy.Omit && T.IsNaN(x) ? T.Zero : d * d;
        }
        // The squared deviations form a new contiguous array of the same shape: adjacent elements exactly when the lane was.
        var dev = deviations.AsSpan(0, lane.Length);
        T total = lane.Inner == 1 ? Pairwise(dev) : SequentialFromFirst(dev);
        return total / T.CreateChecked(n - ddof);
    }

    /// <summary>Standard deviation of each lane: the square root of <see cref="Variance{T}"/>.</summary>
    public static void StandardDeviation<T>(ReadOnlySpan<T> src, Lane lane, Span<T> dst, int ddof, NanPolicy nan = NanPolicy.Propagate,
        VarianceMethod method = VarianceMethod.Welford) where T : IFloatingPointIeee754<T>
    {
        Variance(src, lane, dst, ddof, nan, method);
        for (int l = 0; l < lane.Count; l++) dst[l] = T.Sqrt(dst[l]);
    }

    static T VarianceLane<T>(ReadOnlySpan<T> src, Lane lane, int l, int ddof, NanPolicy nan) where T : IFloatingPointIeee754<T>
    {
        int p = lane.Start(l), n = 0;
        T mean = T.Zero, m2 = T.Zero;
        for (int k = 0; k < lane.Length; k++, p += lane.Inner)
        {
            T x = src[p];
            if (nan == NanPolicy.Omit && T.IsNaN(x)) continue;
            n++;
            T d = x - mean;
            mean += d / T.CreateChecked(n);
            m2 += d * (x - mean);
        }
        return n - ddof <= 0 ? T.NaN : m2 / T.CreateChecked(n - ddof);
    }

    /// <summary>Maximum minus minimum of each lane (MATLAB <c>range</c>, NumPy <c>ptp</c>). NaN policy as for Min/Max.
    /// Allocates one temporary buffer of one value per lane.</summary>
    public static void Range<T>(ReadOnlySpan<T> src, Lane lane, Span<T> dst, NanPolicy nan)
        where T : IFloatingPointIeee754<T>
    {
        Check(src, lane, dst);
        var hi = new T[lane.Count];
        Max(src, lane, hi, Span<int>.Empty, nan);
        Min(src, lane, dst, Span<int>.Empty, nan);
        for (int l = 0; l < lane.Count; l++) dst[l] = hi[l] - dst[l];
    }

    /// <summary>Largest element of each lane and, when <paramref name="indices"/> is not empty, its 0-based position along the lane.
    /// The first of equal elements wins (±0 compare equal). With <see cref="NanPolicy.Propagate"/> a lane containing NaN gives NaN at the first NaN;
    /// with <see cref="NanPolicy.Omit"/> NaN is ignored and a lane of only NaN gives NaN at index 0. The adapter adds the language's index base.
    /// Complex ordering is not handled here. Throws for a zero-length lane.</summary>
    public static void Max<T>(ReadOnlySpan<T> src, Lane lane, Span<T> values, Span<int> indices, NanPolicy nan)
        where T : IFloatingPointIeee754<T> => Extremum(src, lane, values, indices, nan, max: true);

    /// <summary>Smallest element of each lane; see <see cref="Max{T}"/> for ties, NaN and indices.</summary>
    public static void Min<T>(ReadOnlySpan<T> src, Lane lane, Span<T> values, Span<int> indices, NanPolicy nan)
        where T : IFloatingPointIeee754<T> => Extremum(src, lane, values, indices, nan, max: false);

    static void Extremum<T>(ReadOnlySpan<T> src, Lane lane, Span<T> values, Span<int> indices, NanPolicy nan, bool max)
        where T : IFloatingPointIeee754<T>
    {
        Check(src, lane, values);
        if (indices.Length != 0 && indices.Length < lane.Count) throw new ArgumentException("Indices must be empty or hold one entry per lane.", nameof(indices));
        if (lane.Count > 0 && lane.Length == 0) throw new ArgumentException("The extremum of an empty lane is undefined.", nameof(lane));
        for (int l = 0; l < lane.Count; l++)
        {
            int p = lane.Start(l), best = -1;
            T v = T.NaN;
            for (int k = 0; k < lane.Length; k++, p += lane.Inner)
            {
                T x = src[p];
                if (T.IsNaN(x))
                {
                    if (nan == NanPolicy.Propagate) { v = x; best = k; break; }
                    continue;
                }
                if (best < 0 || (max ? x > v : x < v)) { v = x; best = k; }
            }
            values[l] = v;
            if (indices.Length != 0) indices[l] = Math.Max(best, 0);
        }
    }

    /// <summary>Median of each lane (mean of the two middle elements for an even count, computed as <c>a + (b − a)/2</c>).
    /// With <see cref="NanPolicy.Propagate"/> any NaN gives NaN; with Omit NaNs are dropped first; no element gives NaN.
    /// Sorts a copy of each lane (O(n log n)), not a selection.</summary>
    public static void Median<T>(ReadOnlySpan<T> src, Lane lane, Span<T> dst, NanPolicy nan = NanPolicy.Propagate)
        where T : IFloatingPointIeee754<T>
    {
        Check(src, lane, dst);
        var buf = new T[lane.Length];
        T two = T.CreateChecked(2);
        for (int l = 0; l < lane.Count; l++)
        {
            int p = lane.Start(l), n = 0;
            bool hasNan = false;
            for (int k = 0; k < lane.Length; k++, p += lane.Inner)
            {
                T x = src[p];
                if (T.IsNaN(x)) { hasNan = true; continue; }
                buf[n++] = x;
            }
            if (n == 0 || (hasNan && nan == NanPolicy.Propagate)) { dst[l] = T.NaN; continue; }
            var s = buf.AsSpan(0, n);
            s.Sort();
            T a = s[(n - 1) / 2], b = s[n / 2];
            dst[l] = a == b ? a : a + (b - a) / two;
        }
    }

    /// <summary>Whether any element of each lane is non-zero. <paramref name="nanIsNonZero"/> says if NaN counts as non-zero (true in NumPy and MATLAB).</summary>
    public static void Any<T>(ReadOnlySpan<T> src, Lane lane, Span<bool> dst, bool nanIsNonZero = true) where T : INumberBase<T>
    {
        CheckMixed(src, lane, dst);
        for (int l = 0; l < lane.Count; l++)
        {
            int p = lane.Start(l);
            bool r = false;
            for (int k = 0; k < lane.Length && !r; k++, p += lane.Inner) r = NonZero(src[p], nanIsNonZero);
            dst[l] = r;
        }
    }

    /// <summary>Whether every element of each lane is non-zero (true for an empty lane). See <see cref="Any{T}"/> for NaN.</summary>
    public static void All<T>(ReadOnlySpan<T> src, Lane lane, Span<bool> dst, bool nanIsNonZero = true) where T : INumberBase<T>
    {
        CheckMixed(src, lane, dst);
        for (int l = 0; l < lane.Count; l++)
        {
            int p = lane.Start(l);
            bool r = true;
            for (int k = 0; k < lane.Length && r; k++, p += lane.Inner) r = NonZero(src[p], nanIsNonZero);
            dst[l] = r;
        }
    }

    /// <summary>Number of non-zero elements of each lane (MATLAB <c>nnz</c>, NumPy <c>count_nonzero</c>).</summary>
    public static void CountNonZero<T>(ReadOnlySpan<T> src, Lane lane, Span<int> dst, bool nanIsNonZero = true) where T : INumberBase<T>
    {
        CheckMixed(src, lane, dst);
        for (int l = 0; l < lane.Count; l++)
        {
            int p = lane.Start(l), n = 0;
            for (int k = 0; k < lane.Length; k++, p += lane.Inner) if (NonZero(src[p], nanIsNonZero)) n++;
            dst[l] = n;
        }
    }

    static bool NonZero<T>(T x, bool nanIsNonZero) where T : INumberBase<T> => T.IsNaN(x) ? nanIsNonZero : x != T.Zero;
}
