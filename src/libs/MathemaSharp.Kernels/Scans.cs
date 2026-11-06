// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp.Kernels;

/// <summary>Cumulative scans and differences along a lane.</summary>
public static class Scans
{
    /// <summary>Running sum, product, maximum or minimum along each lane, written to <paramref name="dst"/> with the layout of
    /// <paramref name="lane"/> (same length as the source). The destination may be the source itself (exact in-place) but must not
    /// partially overlap it. With <see cref="NanPolicy.Propagate"/> a NaN makes every later output of the lane NaN. With Omit NaN elements are
    /// skipped: their output position repeats the running value (the identity 0 or 1 for sum/product; NaN for max/min until a first non-NaN
    /// element). The Omit behaviour for scans is not verified against MATLAB's <c>'omitnan'</c>.</summary>
    public static void Scan<T>(ReadOnlySpan<T> src, Lane lane, Span<T> dst, ScanOp op, ScanDirection direction = ScanDirection.Forward,
        NanPolicy nan = NanPolicy.Propagate) where T : INumber<T>
    {
        lane.CheckSource(src.Length, nameof(src));
        if (dst.Length < lane.Total) throw new ArgumentException("Destination is shorter than the lane layout.", nameof(dst));
        if (lane.Total > 0 && src.Overlaps(dst, out int offset) && offset != 0)
            throw new ArgumentException("Destination may only overlap the source exactly.", nameof(dst));
        bool rev = direction == ScanDirection.Reverse;
        for (int l = 0; l < lane.Count; l++)
        {
            int p0 = lane.Start(l);
            T acc = default!;
            bool have = false, sticky = false;
            for (int j = 0; j < lane.Length; j++)
            {
                int p = p0 + (rev ? lane.Length - 1 - j : j) * lane.Inner;
                T x = src[p];
                if (T.IsNaN(x))
                {
                    if (nan == NanPolicy.Omit) { dst[p] = have ? acc : Identity<T>(op); continue; }
                    acc = x; have = sticky = true;
                }
                else if (!sticky)
                {
                    acc = !have ? x : op switch
                    {
                        ScanOp.Sum => acc + x,
                        ScanOp.Product => acc * x,
                        ScanOp.Max => x > acc ? x : acc,
                        _ => x < acc ? x : acc,
                    };
                    have = true;
                }
                dst[p] = acc;
            }
        }
    }

    static T Identity<T>(ScanOp op) where T : INumber<T> => op switch
    {
        ScanOp.Sum => T.Zero,
        ScanOp.Product => T.One,
        _ => Reductions.Nan<T>(),
    };

    /// <summary>Order-<paramref name="order"/> forward difference along each lane: <c>x[k+1] − x[k]</c> applied <paramref name="order"/> times.
    /// The destination uses the lane layout <c>(Length − order, Inner, Outer)</c>; an order not smaller than the length gives no output.
    /// Allocates one temporary buffer of one lane. The destination must not overlap the source.</summary>
    public static void Difference<T>(ReadOnlySpan<T> src, Lane lane, Span<T> dst, int order = 1) where T : INumber<T>
    {
        if (order < 0) throw new ArgumentOutOfRangeException(nameof(order));
        lane.CheckSource(src.Length, nameof(src));
        int outLen = Math.Max(lane.Length - order, 0);
        var outLane = new Lane(outLen, lane.Inner, lane.Outer);
        if (dst.Length < outLane.Total) throw new ArgumentException("Destination is shorter than the difference layout.", nameof(dst));
        if (outLane.Total > 0 && src.Overlaps(dst)) throw new ArgumentException("Destination must not overlap the source.", nameof(dst));
        if (outLen == 0) return;
        var tmp = new T[lane.Length];
        for (int l = 0; l < lane.Count; l++)
        {
            int p = lane.Start(l);
            for (int k = 0; k < lane.Length; k++, p += lane.Inner) tmp[k] = src[p];
            for (int pass = 0, n = lane.Length; pass < order; pass++, n--)
                for (int k = 0; k + 1 < n; k++) tmp[k] = tmp[k + 1] - tmp[k];
            int q = outLane.Start(l);
            for (int k = 0; k < outLen; k++, q += lane.Inner) dst[q] = tmp[k];
        }
    }
}
