// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp.Kernels;

/// <summary>How a quantile is read from the sorted values. The continuous methods follow Hyndman and Fan (1996), numbered as there; the
/// others are the names of <c>numpy.quantile</c>'s <c>method</c> argument. MATLAB's <c>quantile</c> and Octave's default (method 5) are
/// <see cref="Hazen"/>; NumPy's default is <see cref="Linear"/>. There is no default here. Not verified: that every method matches the other
/// language on a numerical corpus (the kernel tests check values worked out by hand from the definitions).</summary>
public enum QuantileMethod
{
    /// <summary>Hyndman-Fan 1: the smallest value whose empirical CDF is at least q (<c>inverted_cdf</c>).</summary>
    InvertedCdf,
    /// <summary>Hyndman-Fan 2: as <see cref="InvertedCdf"/>, but the mean of two neighbours where the CDF equals q exactly (<c>averaged_inverted_cdf</c>).</summary>
    AveragedInvertedCdf,
    /// <summary>Hyndman-Fan 3: the nearest order statistic, ties to the even one: the value at position max(round-half-even(n·q), 1) (<c>closest_observation</c>; SAS, Octave method 3).</summary>
    ClosestObservation,
    /// <summary>Hyndman-Fan 4: linear interpolation of the empirical CDF, position n·q (<c>interpolated_inverted_cdf</c>).</summary>
    InterpolatedInvertedCdf,
    /// <summary>Hyndman-Fan 5: position n·q + 1/2 (<c>hazen</c>). MATLAB and Octave default.</summary>
    Hazen,
    /// <summary>Hyndman-Fan 6: position (n + 1)·q (<c>weibull</c>).</summary>
    Weibull,
    /// <summary>Hyndman-Fan 7: position (n − 1)·q + 1 (<c>linear</c>). NumPy default.</summary>
    Linear,
    /// <summary>Hyndman-Fan 8: position (n + 1/3)·q + 1/3 (<c>median_unbiased</c>).</summary>
    MedianUnbiased,
    /// <summary>Hyndman-Fan 9: position (n + 1/4)·q + 3/8 (<c>normal_unbiased</c>).</summary>
    NormalUnbiased,
    /// <summary>The value at position ⌊(n − 1)·q⌋ (<c>lower</c>).</summary>
    Lower,
    /// <summary>The value at position ⌈(n − 1)·q⌉ (<c>higher</c>).</summary>
    Higher,
    /// <summary>The value at position (n − 1)·q rounded to the nearest integer, halves to even (<c>nearest</c>).</summary>
    Nearest,
    /// <summary>The mean of the values at ⌊(n − 1)·q⌋ and ⌈(n − 1)·q⌉ (<c>midpoint</c>).</summary>
    Midpoint,
}

/// <summary>Which values the last histogram bin takes.</summary>
public enum HistogramLastBin
{
    /// <summary>The last bin is closed: <c>edge[n−1] ≤ x ≤ edge[n]</c>; there are <c>edges − 1</c> bins (<c>numpy.histogram</c>).</summary>
    ClosedInterval,
    /// <summary>An extra last bin counts only the values equal to the last edge; there are as many bins as edges (MATLAB and Octave <c>histc</c>).</summary>
    ExactLastEdge,
}

/// <summary>Order statistics, counting and numerical integration or differentiation along a lane. Same conventions as <see cref="Reductions"/>:
/// semantic choices are parameters, results are written lane by lane, destinations must not overlap sources.</summary>
public static class Statistics
{
    // ------------------------------------------------------------------ quantile

    /// <summary>Quantiles of each lane. <paramref name="q"/> holds probabilities in [0, 1], always as <c>double</c> (the positions in the sorted lane are computed in
    /// double, as NumPy and MATLAB do, and only the interpolation weight is converted to <typeparamref name="T"/>); the result for lane <c>l</c> and probability <c>j</c> is
    /// written at <c>dst[j * lane.Count + l]</c> (probability-major, as NumPy puts the probability axis first); the adapter moves the axis where its
    /// language wants it. Each lane is copied and sorted (O(n log n)). With <see cref="NanPolicy.Propagate"/> a lane containing NaN gives NaN;
    /// with Omit NaNs are dropped first; a lane with no value left gives NaN. Throws for a probability outside [0, 1] or NaN.</summary>
    public static void Quantile<T>(ReadOnlySpan<T> src, Lane lane, ReadOnlySpan<double> q, Span<T> dst, QuantileMethod method, NanPolicy nan = NanPolicy.Propagate)
        where T : IFloatingPointIeee754<T>
    {
        lane.CheckSource(src.Length, nameof(src));
        for (int j = 0; j < q.Length; j++)
            if (!(q[j] >= 0 && q[j] <= 1)) throw new ArgumentOutOfRangeException(nameof(q), "Probabilities must lie in [0, 1].");
        if (dst.Length < checked(lane.Count * q.Length)) throw new ArgumentException("Destination is shorter than lanes × probabilities.", nameof(dst));
        if (lane.Count > 0 && q.Length > 0 && src.Overlaps(dst)) throw new ArgumentException("Destination must not overlap the source.", nameof(dst));
        var buf = new T[lane.Length];
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
            if (n == 0 || (hasNan && nan == NanPolicy.Propagate))
            {
                for (int j = 0; j < q.Length; j++) dst[j * lane.Count + l] = T.NaN;
                continue;
            }
            var sorted = buf.AsSpan(0, n);
            sorted.Sort();
            for (int j = 0; j < q.Length; j++) dst[j * lane.Count + l] = QuantileOfSorted<T>(sorted, q[j], method);
        }
    }

    static T QuantileOfSorted<T>(ReadOnlySpan<T> x, double q, QuantileMethod method) where T : IFloatingPointIeee754<T>
    {
        int n = x.Length;
        double nn = n;
        switch (method)
        {
            case QuantileMethod.InvertedCdf:
                return x[Index(Math.Ceiling(nn * q) - 1, n)];
            case QuantileMethod.AveragedInvertedCdf:
            {
                double nq = nn * q, c = Math.Ceiling(nq);
                if (nq == c && nq >= 1 && nq < nn) return Lerp(x[Index(c - 1, n)], x[Index(c, n)], 0.5);
                return x[Index(c - 1, n)];
            }
            case QuantileMethod.ClosestObservation:
                return x[Index(Math.Max(Math.Round(nn * q, MidpointRounding.ToEven), 1) - 1, n)];
            case QuantileMethod.Lower: return x[Index(Math.Floor((nn - 1) * q), n)];
            case QuantileMethod.Higher: return x[Index(Math.Ceiling((nn - 1) * q), n)];
            case QuantileMethod.Nearest: return x[Index(Math.Round((nn - 1) * q, MidpointRounding.ToEven), n)];
            case QuantileMethod.Midpoint:
            {
                double v = (nn - 1) * q;
                return Lerp(x[Index(Math.Floor(v), n)], x[Index(Math.Ceiling(v), n)], 0.5);
            }
        }
        // continuous methods: virtual 0-based index, clipped to [0, n-1], then linear interpolation between neighbours
        double virt = method switch
        {
            QuantileMethod.InterpolatedInvertedCdf => nn * q - 1,
            QuantileMethod.Hazen => nn * q + 0.5 - 1,
            QuantileMethod.Weibull => nn * q + q - 1,
            QuantileMethod.Linear => (nn - 1) * q,
            QuantileMethod.MedianUnbiased => nn * q + (1.0 / 3.0) + q / 3.0 - 1,
            QuantileMethod.NormalUnbiased => nn * q + 0.375 + q / 4.0 - 1,
            _ => throw new ArgumentOutOfRangeException(nameof(method)),
        };
        if (virt < 0) virt = 0;
        if (virt > nn - 1) virt = nn - 1;
        int prev = (int)Math.Floor(virt), next = Math.Min(prev + 1, n - 1);
        return Lerp(x[prev], x[next], virt - prev);   // like NumPy, also at the last element: with infinities inf - inf gives NaN
    }

    static int Index(double index0, int n) => index0 <= 0 ? 0 : index0 >= n - 1 ? n - 1 : (int)index0;

    static T Step<T>(bool uniform, T h, ReadOnlySpan<T> x, int k) where T : IFloatingPointIeee754<T> => uniform ? h : x[k + 1] - x[k];

    /// <summary>NumPy's interpolation with the weight <paramref name="gamma"/> in [0, 1] kept in double: <c>a + (b − a)·γ</c>, or <c>b − (b − a)·(1 − γ)</c> when γ ≥ 1/2, so that
    /// the result is monotone in γ and exact at both ends. Both weights are computed in double and then converted to <typeparamref name="T"/>.</summary>
    static T Lerp<T>(T a, T b, double gamma) where T : IFloatingPointIeee754<T>
    {
        T d = b - a;
        return gamma >= 0.5 ? b - d * T.CreateChecked(1 - gamma) : a + d * T.CreateChecked(gamma);
    }

    // ------------------------------------------------------------------ mode

    /// <summary>Most frequent value of each lane; the smallest of the tied values wins (MATLAB <c>mode</c>, <c>scipy.stats.mode</c>).
    /// <paramref name="counts"/> receives how many times it occurs (may be empty if not wanted). A lane with no value gives NaN and count 0.
    /// With <see cref="NanPolicy.Omit"/> NaNs are ignored; with <see cref="NanPolicy.Propagate"/> a lane containing NaN gives NaN and count 0
    /// (not verified against MATLAB, which ignores NaN, nor against SciPy). Sorts a copy of each lane.</summary>
    public static void Mode<T>(ReadOnlySpan<T> src, Lane lane, Span<T> values, Span<int> counts, NanPolicy nan)
        where T : IFloatingPointIeee754<T>
    {
        lane.CheckSource(src.Length, nameof(src));
        if (values.Length < lane.Count) throw new ArgumentException("Values is shorter than the number of lanes.", nameof(values));
        if (counts.Length != 0 && counts.Length < lane.Count) throw new ArgumentException("Counts must be empty or hold one entry per lane.", nameof(counts));
        if (lane.Count > 0 && src.Overlaps(values)) throw new ArgumentException("Destination must not overlap the source.", nameof(values));
        var buf = new T[lane.Length];
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
            T best = T.NaN;
            int bestCount = 0;
            if (n > 0 && !(hasNan && nan == NanPolicy.Propagate))
            {
                var s = buf.AsSpan(0, n);
                s.Sort();
                for (int i = 0; i < n;)
                {
                    int j = i + 1;
                    while (j < n && s[j] == s[i]) j++;
                    if (j - i > bestCount) { bestCount = j - i; best = s[i]; }   // strict >: the first (smallest) run wins ties
                    i = j;
                }
            }
            values[l] = best;
            if (counts.Length != 0) counts[l] = bestCount;
        }
    }

    // ------------------------------------------------------------------ histogram

    /// <summary>The number of counts <see cref="Histogram{T}"/> writes per lane for <paramref name="edgeCount"/> edges.</summary>
    public static int HistogramBins(int edgeCount, HistogramLastBin lastBin)
        => edgeCount < 1 ? throw new ArgumentOutOfRangeException(nameof(edgeCount))
         : lastBin == HistogramLastBin.ClosedInterval ? edgeCount - 1 : edgeCount;

    /// <summary>Counts of each lane in the bins given by <paramref name="edges"/>, which must be non-decreasing and not NaN. Bin <c>b</c> holds
    /// <c>edges[b] ≤ x &lt; edges[b+1]</c>; the last bin is as <paramref name="lastBin"/> says. Values outside the edges and NaN are not counted.
    /// Counts of lane <c>l</c> are written at <c>counts[l * bins + b]</c>, with <c>bins</c> from <see cref="HistogramBins"/>. Uses the edges
    /// themselves (binary search), so no floating-point bin-width arithmetic is involved. Weights are not supported.</summary>
    public static void Histogram<T>(ReadOnlySpan<T> src, Lane lane, ReadOnlySpan<T> edges, Span<long> counts, HistogramLastBin lastBin)
        where T : IFloatingPointIeee754<T>
    {
        lane.CheckSource(src.Length, nameof(src));
        int bins = HistogramBins(edges.Length, lastBin);
        for (int i = 0; i < edges.Length; i++)
        {
            if (T.IsNaN(edges[i])) throw new ArgumentException("Edges must not be NaN.", nameof(edges));
            if (i > 0 && edges[i] < edges[i - 1]) throw new ArgumentException("Edges must be non-decreasing.", nameof(edges));
        }
        if (counts.Length < checked(lane.Count * bins)) throw new ArgumentException("Counts is shorter than lanes × bins.", nameof(counts));
        counts.Slice(0, lane.Count * bins).Clear();
        if (edges.Length == 0) return;
        T first = edges[0], last = edges[^1];
        for (int l = 0; l < lane.Count; l++)
        {
            int p = lane.Start(l);
            int baseIndex = l * bins;
            for (int k = 0; k < lane.Length; k++, p += lane.Inner)
            {
                T x = src[p];
                if (T.IsNaN(x) || x < first || x > last) continue;
                if (x == last)
                {
                    if (lastBin == HistogramLastBin.ClosedInterval) { if (bins > 0) counts[baseIndex + bins - 1]++; }
                    else counts[baseIndex + bins - 1]++;
                    continue;
                }
                // greatest b with edges[b] <= x (x < last, so b < edges.Length - 1)
                int lo = 0, hi = edges.Length - 1;
                while (hi - lo > 1)
                {
                    int mid = (lo + hi) >>> 1;
                    if (edges[mid] <= x) lo = mid; else hi = mid;
                }
                counts[baseIndex + lo]++;
            }
        }
    }

    // ------------------------------------------------------------------ trapezoid and gradient

    /// <summary>Trapezoidal integral of each lane with a uniform spacing: the terms <c>dx·(y[k] + y[k+1])/2</c> are formed first and then
    /// added as <paramref name="method"/> says (NumPy's <c>trapezoid</c> adds them with its pairwise sum). A lane shorter than two samples gives 0.
    /// NaN propagates. Allocates one temporary buffer of one lane.</summary>
    public static void Trapezoid<T>(ReadOnlySpan<T> y, Lane lane, T dx, Span<T> dst, SumMethod method = SumMethod.Sequential)
        where T : IFloatingPointIeee754<T> => TrapezoidCore(y, lane, dx, default, dst, method);

    /// <summary>Trapezoidal integral of each lane over the sample positions <paramref name="x"/> (one per element of the lane, same for every lane):
    /// terms <c>(x[k+1] − x[k])·(y[k] + y[k+1])/2</c>. Otherwise as <see cref="Trapezoid{T}(ReadOnlySpan{T}, Lane, T, Span{T}, SumMethod)"/>.</summary>
    public static void Trapezoid<T>(ReadOnlySpan<T> y, Lane lane, ReadOnlySpan<T> x, Span<T> dst, SumMethod method = SumMethod.Sequential)
        where T : IFloatingPointIeee754<T>
    {
        if (x.Length != lane.Length) throw new ArgumentException("x must have one position per element of the lane.", nameof(x));
        TrapezoidCore(y, lane, T.Zero, x, dst, method);
    }

    static void TrapezoidCore<T>(ReadOnlySpan<T> y, Lane lane, T dx, ReadOnlySpan<T> x, Span<T> dst, SumMethod method) where T : IFloatingPointIeee754<T>
    {
        lane.CheckSource(y.Length, nameof(y));
        if (dst.Length < lane.Count) throw new ArgumentException("Destination is shorter than the number of lanes.", nameof(dst));
        if (lane.Count > 0 && y.Overlaps(dst)) throw new ArgumentException("Destination must not overlap the source.", nameof(dst));
        int terms = Math.Max(lane.Length - 1, 0);
        var buf = new T[terms];
        var one = new T[1];
        T two = T.CreateChecked(2);
        for (int l = 0; l < lane.Count; l++)
        {
            int p = lane.Start(l);
            for (int k = 0; k < terms; k++, p += lane.Inner)
            {
                T d = x.IsEmpty ? dx : x[k + 1] - x[k];
                buf[k] = d * (y[p] + y[p + lane.Inner]) / two;
            }
            if (terms == 0) { dst[l] = T.Zero; continue; }
            Reductions.Sum<T>(buf, Lane.Flat(terms), one, NanPolicy.Propagate, method);
            dst[l] = one[0];
        }
    }

    /// <summary>Numerical derivative along each lane for a uniform spacing <paramref name="h"/>, written with the layout of the source (NumPy's
    /// <c>gradient</c> with <c>edge_order=1</c>): central differences <c>(y[k+1] − y[k−1])/(2h)</c> inside, one-sided <c>(y[1] − y[0])/h</c> and
    /// <c>(y[n−1] − y[n−2])/h</c> at the ends. A lane needs at least two samples. NaN propagates to the outputs that use it. The destination may
    /// not overlap the source.</summary>
    public static void Gradient<T>(ReadOnlySpan<T> src, Lane lane, T h, Span<T> dst) where T : IFloatingPointIeee754<T>
        => GradientCore(src, lane, h, default, dst);

    /// <summary>Numerical derivative along each lane over non-uniform sample positions <paramref name="x"/> (strictly monotonic, one per element of the lane).
    /// Inside, the second-order formula of NumPy's <c>gradient</c>: with <c>d1 = x[k] − x[k−1]</c> and <c>d2 = x[k+1] − x[k]</c>,
    /// <c>−d2/(d1(d1+d2))·y[k−1] + (d2−d1)/(d1 d2)·y[k] + d1/(d2(d1+d2))·y[k+1]</c>; one-sided differences at the ends.</summary>
    public static void Gradient<T>(ReadOnlySpan<T> src, Lane lane, ReadOnlySpan<T> x, Span<T> dst) where T : IFloatingPointIeee754<T>
    {
        if (x.Length != lane.Length) throw new ArgumentException("x must have one position per element of the lane.", nameof(x));
        GradientCore(src, lane, T.Zero, x, dst);
    }

    static void GradientCore<T>(ReadOnlySpan<T> src, Lane lane, T h, ReadOnlySpan<T> x, Span<T> dst) where T : IFloatingPointIeee754<T>
    {
        lane.CheckSource(src.Length, nameof(src));
        if (lane.Count > 0 && lane.Length < 2) throw new ArgumentException("A gradient needs at least two samples per lane.", nameof(lane));
        if (dst.Length < lane.Total) throw new ArgumentException("Destination is shorter than the lane layout.", nameof(dst));
        if (lane.Total > 0 && src.Overlaps(dst)) throw new ArgumentException("Destination must not overlap the source.", nameof(dst));
        bool uniform = x.IsEmpty;
        T two = T.CreateChecked(2);
        for (int l = 0; l < lane.Count; l++)
        {
            int p0 = lane.Start(l), s = lane.Inner, n = lane.Length;
            dst[p0] = (src[p0 + s] - src[p0]) / Step(uniform, h, x, 0);
            dst[p0 + (n - 1) * s] = (src[p0 + (n - 1) * s] - src[p0 + (n - 2) * s]) / Step(uniform, h, x, n - 2);
            for (int k = 1; k < n - 1; k++)
            {
                int p = p0 + k * s;
                if (uniform) dst[p] = (src[p + s] - src[p - s]) / (two * h);
                else
                {
                    T d1 = x[k] - x[k - 1], d2 = x[k + 1] - x[k];
                    T a = -d2 / (d1 * (d1 + d2)), b = (d2 - d1) / (d1 * d2), c = d1 / (d2 * (d1 + d2));
                    dst[p] = a * src[p - s] + b * src[p] + c * src[p + s];
                }
            }
        }
    }
}
