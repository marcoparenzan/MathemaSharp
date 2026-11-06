// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp.Kernels;

/// <summary>Matrix products and the statistics built on them (covariance, correlation) over contiguous row-major buffers.</summary>
public static class Products
{
    /// <summary>The matrix product <c>c = a · b</c> of an <paramref name="m"/> × <paramref name="k"/> matrix and a <paramref name="k"/> × <paramref name="n"/> matrix,
    /// all row-major and contiguous. Each element is accumulated left to right over <c>k</c> starting from zero (<c>c[i,j] += a[i,p]·b[p,j]</c>, <c>p</c> ascending),
    /// in the element type's own arithmetic: no blocking, no pairwise grouping, no wider accumulator. A BLAS routine groups the sums differently, so results can differ
    /// in the last bits from a BLAS-based <c>matmul</c>. <paramref name="c"/> is overwritten and must not overlap the inputs.</summary>
    public static void MatMul<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, Span<T> c, int m, int k, int n) where T : INumberBase<T>
    {
        if (m < 0 || k < 0 || n < 0) throw new ArgumentOutOfRangeException(nameof(m), "Dimensions must not be negative.");
        long am = (long)m * k, bm = (long)k * n, cm = (long)m * n;
        if (a.Length < am) throw new ArgumentException("a is shorter than m × k.", nameof(a));
        if (b.Length < bm) throw new ArgumentException("b is shorter than k × n.", nameof(b));
        if (c.Length < cm) throw new ArgumentException("c is shorter than m × n.", nameof(c));
        if (cm > 0 && (c.Overlaps(a) || c.Overlaps(b))) throw new ArgumentException("c must not overlap the inputs.", nameof(c));
        c.Slice(0, (int)cm).Clear();
        for (int i = 0; i < m; i++)
        {
            int crow = i * n;
            for (int p = 0; p < k; p++)
            {
                T av = a[i * k + p];
                int brow = p * n;
                for (int j = 0; j < n; j++) c[crow + j] += av * b[brow + j];
            }
        }
    }

    /// <summary>Covariance matrix of <paramref name="variables"/> variables observed <paramref name="observations"/> times; <paramref name="x"/> holds one variable per row
    /// (row-major, <c>variables × observations</c>) and <paramref name="cov"/> receives <c>variables × variables</c>. As NumPy's <c>cov</c>: each row's mean (summed pairwise),
    /// the centred rows, their product with the transpose (see <see cref="MatMul{T}"/>), then multiplied by the reciprocal of <c>max(observations − ddof, 0)</c> computed in
    /// double (an empty denominator gives infinity, and NaN where the sum is zero). <paramref name="meanSum"/> says how the means are summed: pairwise when the observations of a
    /// variable are adjacent in memory in the array the caller means (the default), <see cref="SumMethod.SequentialFromFirst"/> when they are not, as NumPy does for the transpose of a C array.
    /// Allocates the centred copy and its transpose.</summary>
    public static void Covariance<T>(ReadOnlySpan<T> x, int variables, int observations, Span<T> cov, int ddof, SumMethod meanSum = SumMethod.Pairwise) where T : IFloatingPointIeee754<T>
    {
        if (variables < 0 || observations < 0) throw new ArgumentOutOfRangeException(nameof(variables));
        if (x.Length < (long)variables * observations) throw new ArgumentException("x is shorter than variables × observations.", nameof(x));
        if (cov.Length < (long)variables * variables) throw new ArgumentException("cov is shorter than variables × variables.", nameof(cov));
        var means = new T[variables];
        Reductions.Mean<T>(x, new Lane(observations, 1, variables), means, NanPolicy.Propagate, meanSum);
        var centred = new T[variables * observations];
        var transposed = new T[variables * observations];
        for (int v = 0; v < variables; v++)
            for (int i = 0; i < observations; i++)
            {
                T d = x[v * observations + i] - means[v];
                centred[v * observations + i] = d;
                transposed[i * variables + v] = d;
            }
        MatMul<T>(centred, transposed, cov, variables, observations, variables);
        T reciprocal = T.CreateChecked(1.0 / Math.Max(observations - ddof, 0.0));
        for (int i = 0; i < variables * variables; i++) cov[i] *= reciprocal;
    }

    /// <summary>Correlation matrix from a covariance matrix (<paramref name="variables"/> × <paramref name="variables"/>, row-major): each element divided by the square roots of the
    /// two diagonal entries (first by the row's, then by the column's, as NumPy's <c>corrcoef</c> does), then clipped to [−1, 1]; NaN stays NaN.</summary>
    public static void Correlation<T>(ReadOnlySpan<T> cov, int variables, Span<T> corr) where T : IFloatingPointIeee754<T>
    {
        if (cov.Length < (long)variables * variables) throw new ArgumentException("cov is shorter than variables × variables.", nameof(cov));
        if (corr.Length < (long)variables * variables) throw new ArgumentException("corr is shorter than variables × variables.", nameof(corr));
        var std = new T[variables];
        for (int i = 0; i < variables; i++) std[i] = T.Sqrt(cov[i * variables + i]);
        T minusOne = -T.One;
        for (int i = 0; i < variables; i++)
            for (int j = 0; j < variables; j++)
            {
                T v = cov[i * variables + j] / std[i] / std[j];
                corr[i * variables + j] = v < minusOne ? minusOne : v > T.One ? T.One : v;
            }
    }
}
