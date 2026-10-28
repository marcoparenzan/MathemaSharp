#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\NDSharp";
File.WriteAllText(Path.Combine(root,"src","NDSharp","NDArray.Generic.cs"), """
using System.Numerics;
namespace NDSharp;

/// <summary>A typed, fixed-shape array. Arithmetic and storage preserve T; transpose creates a shared-storage view.</summary>
public sealed class NDArray<T> where T : INumber<T>
{
    private readonly T[] buffer;
    private readonly int[] shape;
    private readonly int[] strides;
    public int Rank => shape.Length;
    public int Length => buffer.Length;
    public int[] Shape => (int[])shape.Clone();
    public int[] Strides => (int[])strides.Clone();

    /// <summary>Copies the values. Empty dimensions are supported; rank zero is a single scalar.</summary>
    public NDArray(ReadOnlySpan<T> values, params int[] dimensions)
    {
        ArgumentNullException.ThrowIfNull(dimensions);
        shape = (int[])dimensions.Clone();
        strides = new int[shape.Length];
        int size = 1;
        for (int i = shape.Length - 1; i >= 0; i--)
        {
            if (shape[i] < 0) throw new ArgumentOutOfRangeException(nameof(dimensions));
            strides[i] = size;
            size = checked(size * shape[i]);
        }
        if (values.Length != size) throw new ArgumentException("Values do not match shape.", nameof(values));
        buffer = values.ToArray();
    }
    private NDArray(T[] values, int[] dimensions, int[] steps)
    { buffer = values; shape = dimensions; strides = steps; }

    public T this[params int[] indices]
    {
        get => buffer[Offset(indices)];
        set => buffer[Offset(indices)] = value;
    }
    private int Offset(int[] indices)
    {
        ArgumentNullException.ThrowIfNull(indices);
        if (indices.Length != Rank) throw new ArgumentException("Index rank mismatch.", nameof(indices));
        int offset = 0;
        for (int i = 0; i < Rank; i++)
        {
            if ((uint)indices[i] >= (uint)shape[i]) throw new ArgumentOutOfRangeException(nameof(indices));
            offset += indices[i] * strides[i];
        }
        return offset;
    }
    /// <summary>Permutes the axes; omitted axes reverse their order. Writes are shared with the source.</summary>
    public NDArray<T> Transpose(params int[] axes)
    {
        ArgumentNullException.ThrowIfNull(axes);
        if (axes.Length == 0) axes = Enumerable.Range(0, Rank).Reverse().ToArray();
        if (axes.Length != Rank || axes.Distinct().Count() != Rank || axes.Any(a => (uint)a >= (uint)Rank))
            throw new ArgumentException("Expected an axis permutation.", nameof(axes));
        return new NDArray<T>(buffer, axes.Select(a => shape[a]).ToArray(), axes.Select(a => strides[a]).ToArray());
    }
    /// <summary>Copies in logical C order, honoring view strides.</summary>
    public T[] ToArray()
    {
        var result = new T[Length];
        for (int linear = 0; linear < Length; linear++)
        {
            int remainder = linear, offset = 0;
            for (int axis = Rank - 1; axis >= 0; axis--)
            { offset += (remainder % shape[axis]) * strides[axis]; remainder /= shape[axis]; }
            result[linear] = buffer[offset];
        }
        return result;
    }
    /// <summary>Sequential sum in logical order. Uses the overflow and rounding semantics of T.</summary>
    public T Sum()
    {
        var result = T.Zero;
        foreach (var value in ToArray()) result += value;
        return result;
    }
    /// <summary>Elementwise sum of equal shapes; broadcasting is not supported.</summary>
    public NDArray<T> Add(NDArray<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (!shape.SequenceEqual(other.shape)) throw new ArgumentException("Shape mismatch.", nameof(other));
        var left = ToArray(); var right = other.ToArray();
        for (int i = 0; i < left.Length; i++) left[i] += right[i];
        return new NDArray<T>(left.AsSpan(), shape);
    }
}
""");
File.WriteAllText(Path.Combine(root,"src","NDSharp.Tests","TypedNDArrayTests.cs"), """
using NDSharp;
using Xunit;
public class TypedNDArrayTests
{
    [Fact] public void DecimalStorageRetainsSmallIncrement()
    {
        var x = new NDArray<decimal>(new decimal[] { 1m, 0.0000000000000000000000001m }, 2);
        Assert.Equal(1.0000000000000000000000001m, x.Sum());
    }
    [Fact] public void TransposeHonorsStridesAndSharesWrites()
    {
        var x = new NDArray<long>(new long[] { 9007199254740993, 2, 3, 4, 5, 6 }, 2, 3);
        var t = x.Transpose();
        Assert.Equal(new long[] {9007199254740993,4,2,5,3,6}, t.ToArray());
        t[2,1] = 99; Assert.Equal(99L, x[1,2]);
    }
    [Fact] public void AddUsesLogicalOrder()
    {
        var x = new NDArray<int>(new int[] {1,2,3,4},2,2);
        Assert.Equal(new int[] {2,5,5,8},x.Add(x.Transpose()).ToArray());
    }
    [Fact] public void ShapeAndInputAreCopied()
    {
        int[] data=[1,2]; int[] shape=[2];
        var x=new NDArray<int>(data,shape); data[0]=9; shape[0]=9;
        var returned=x.Shape; returned[0]=9;
        Assert.Equal(1,x[0]); Assert.Equal(2,x.Shape[0]);
    }
    [Fact] public void ScalarAndEmptyArrays()
    {
        Assert.Equal(7,new NDArray<int>(new int[]{7}).Sum());
        Assert.Empty(new NDArray<int>(Array.Empty<int>(),0,3).Transpose().ToArray());
    }
    [Fact] public void InvalidShapeAxesAndIndices()
    {
        Assert.Throws<ArgumentException>(()=>new NDArray<int>(new int[]{1},2));
        var x=new NDArray<int>(new int[]{1,2},1,2);
        Assert.Throws<ArgumentException>(()=>x.Transpose(0,0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>x[0,2]);
    }
}
""");
Console.WriteLine("Typed NDArray<T> and six tests written.");
