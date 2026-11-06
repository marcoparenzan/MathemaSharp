// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp.Kernels;

/// <summary>Small numerical-analysis kernels on arrays, for every IEEE Generic Math format: cumulative trapezoid integration of grid columns, one-dimensional linear
/// interpolation, and the upward zero crossings of a series with their oscillation period. Used by the fluid solvers (stream function, comparison with a benchmark
/// profile, vortex-shedding frequency) and available to any caller.</summary>
public static class Calculus
{
    /// <summary>Cumulative trapezoid integral along the rows (down the <c>j</c> axis) of a grid <c>f[j, i]</c> with spacing <paramref name="h"/>: row 0 is zero and
    /// <c>out[j, i] = out[j−1, i] + ½ (f[j, i] + f[j−1, i]) h</c>, the integral of each column from the first row. For a velocity field this is the stream function.</summary>
    public static T[,] CumulativeTrapezoidAlongRows<T>(T[,] f, T h) where T : IFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNull(f);
        int rows = f.GetLength(0), cols = f.GetLength(1);
        T half = T.CreateChecked(0.5);
        var result = new T[rows, cols];
        for (int i = 0; i < cols; i++)
            for (int j = 1; j < rows; j++)
                result[j, i] = result[j - 1, i] + half * (f[j, i] + f[j - 1, i]) * h;
        return result;
    }

    /// <summary>Linear interpolation of the table (<paramref name="xs"/>, <paramref name="ys"/>) at <paramref name="x"/>, with <paramref name="xs"/> increasing:
    /// the first value to the left of the table, the last to its right (as <c>numpy.interp</c>). The interval is found by a linear scan from the left.</summary>
    public static T InterpolateLinear<T>(ReadOnlySpan<T> xs, ReadOnlySpan<T> ys, T x) where T : IFloatingPointIeee754<T>
    {
        if (xs.Length == 0 || xs.Length != ys.Length) throw new ArgumentException("xs and ys must be non-empty and of the same length.");
        if (x <= xs[0]) return ys[0];
        for (int k = 1; k < xs.Length; k++)
            if (x <= xs[k])
            {
                T t = (x - xs[k - 1]) / (xs[k] - xs[k - 1]);
                return ys[k - 1] + t * (ys[k] - ys[k - 1]);
            }
        return ys[^1];
    }

    /// <summary>Root mean square of the difference between a function sampled at <paramref name="xs"/> / <paramref name="ys"/> and reference values
    /// <paramref name="referenceY"/> given at the abscissas <paramref name="referenceX"/>: the table is interpolated linearly at each reference abscissa.</summary>
    public static T RmsAgainstReference<T>(ReadOnlySpan<T> xs, ReadOnlySpan<T> ys, ReadOnlySpan<T> referenceX, ReadOnlySpan<T> referenceY) where T : IFloatingPointIeee754<T>
    {
        if (referenceX.Length == 0 || referenceX.Length != referenceY.Length) throw new ArgumentException("The reference must be non-empty, one value per abscissa.");
        T sum = T.Zero;
        for (int k = 0; k < referenceX.Length; k++)
        {
            T v = InterpolateLinear(xs, ys, referenceX[k]);
            sum += (v - referenceY[k]) * (v - referenceY[k]);
        }
        return T.Sqrt(sum / T.CreateChecked(referenceX.Length));
    }

    /// <summary>The positions (fractional indices) where the series, minus <paramref name="level"/>, goes from negative to non-negative: for each such pair
    /// (k−1, k) the position is <c>startIndex + k − 1 + a / (a − b)</c> with a, b the two shifted values. <paramref name="startIndex"/> is the index of the first sample in a
    /// longer series (the positions then refer to the longer series, with the same rounding as if it had been scanned whole).</summary>
    public static List<T> UpwardCrossings<T>(ReadOnlySpan<T> series, T level, int startIndex = 0) where T : IFloatingPointIeee754<T>
    {
        var crossings = new List<T>();
        for (int k = 1; k < series.Length; k++)
        {
            T a = series[k - 1] - level, b = series[k] - level;
            if (a < T.Zero && b >= T.Zero) crossings.Add(T.CreateChecked(startIndex + k - 1) + a / (a - b));
        }
        return crossings;
    }

    /// <summary>Mean spacing between the first and the last of at least three crossings, or null when there are fewer: (last − first) / (count − 1).</summary>
    public static T? MeanPeriod<T>(IReadOnlyList<T> crossings) where T : struct, IFloatingPointIeee754<T>
    {
        if (crossings.Count < 3) return null;
        return (crossings[^1] - crossings[0]) / T.CreateChecked(crossings.Count - 1);
    }
}
