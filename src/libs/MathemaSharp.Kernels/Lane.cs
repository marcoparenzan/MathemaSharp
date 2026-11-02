// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
namespace MathemaSharp.Kernels;

/// <summary>How a reduction, scan or difference visits a contiguous buffer: <paramref name="Outer"/> blocks of
/// <paramref name="Length"/> × <paramref name="Inner"/> elements; the element k of lane (o, i) sits at
/// <c>o·Length·Inner + k·Inner + i</c>. The kernel never knows whether the language said <c>dim=1</c> (MATLAB, column-major)
/// or <c>axis=0</c> (NumPy, row-major): the adapter turns either into a lane.</summary>
/// <param name="Length">Elements along the reduced axis.</param>
/// <param name="Inner">Product of the extents that vary faster than the axis (the distance between two lane elements).</param>
/// <param name="Outer">Product of the extents that vary slower than the axis.</param>
public readonly record struct Lane(int Length, int Inner, int Outer)
{
    /// <summary>Number of lanes, which is also the number of results of a reduction.</summary>
    public int Count => checked(Inner * Outer);

    /// <summary>Number of elements the source buffer must hold.</summary>
    public int Total => checked(Length * Inner * Outer);

    /// <summary>Index in the source of the first element of lane <paramref name="lane"/> (0 ≤ lane &lt; Count).</summary>
    public int Start(int lane) => checked((lane / Inner) * Length * Inner + lane % Inner);

    /// <summary>One lane over the whole buffer (MATLAB <c>'all'</c>, NumPy <c>axis=None</c>).</summary>
    public static Lane Flat(int length) => new(length, 1, 1);

    /// <summary>Lane for dimension <paramref name="dim"/> (0-based) of a column-major array: the first dimension varies fastest.
    /// A dimension past the last one has length 1.</summary>
    public static Lane ForColumnMajorDim(ReadOnlySpan<int> dims, int dim)
    {
        if (dim < 0) throw new ArgumentOutOfRangeException(nameof(dim));
        int inner = 1, outer = 1;
        for (int i = 0; i < Math.Min(dim, dims.Length); i++) inner = checked(inner * dims[i]);
        for (int i = dim + 1; i < dims.Length; i++) outer = checked(outer * dims[i]);
        return new Lane(dim < dims.Length ? dims[dim] : 1, inner, outer);
    }

    /// <summary>Lane for <paramref name="axis"/> (0-based, already normalised to 0..rank-1) of a row-major array: the last axis varies fastest.</summary>
    public static Lane ForRowMajorAxis(ReadOnlySpan<int> shape, int axis)
    {
        if ((uint)axis >= (uint)shape.Length) throw new ArgumentOutOfRangeException(nameof(axis));
        int inner = 1, outer = 1;
        for (int i = axis + 1; i < shape.Length; i++) inner = checked(inner * shape[i]);
        for (int i = 0; i < axis; i++) outer = checked(outer * shape[i]);
        return new Lane(shape[axis], inner, outer);
    }

    internal void CheckSource(int sourceLength, string name)
    {
        if (Length < 0 || Inner < 0 || Outer < 0) throw new ArgumentException("Lane extents must not be negative.", name);
        if (sourceLength < Total) throw new ArgumentException("Source is shorter than the lane layout.", name);
    }
}

/// <summary>What a reduction or scan does with NaN. There is no language default here: MATLAB's <c>sum</c> propagates,
/// its <c>min</c>/<c>max</c> omit; NumPy's propagate everywhere. The adapter chooses.</summary>
public enum NanPolicy
{
    /// <summary>A NaN in a lane makes the result of that lane NaN (scans: NaN from that position on).</summary>
    Propagate,
    /// <summary>NaN elements are ignored, as if absent. A lane with no other element gives the empty result of the function (sum 0, product 1, others NaN).</summary>
    Omit,
}

/// <summary>Direction in which a scan accumulates.</summary>
public enum ScanDirection { Forward, Reverse }

/// <summary>The operation of a scan.</summary>
public enum ScanOp { Sum, Product, Max, Min }
