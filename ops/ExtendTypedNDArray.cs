#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\NDSharp";
var path=Path.Combine(root,"src","NDSharp","NDArray.Generic.cs");
var text=File.ReadAllText(path);
void Replace(string old,string value) { if(!text.Contains(old)) throw new Exception("Missing text: "+old); text=text.Replace(old,value); }
Replace("private readonly T[] buffer;", "private readonly T[] buffer;\n    private readonly int baseOffset;");
Replace("public int Length => buffer.Length;", "public int Length => shape.Aggregate(1, (size, dim) => checked(size * dim));");
Replace("private NDArray(T[] values, int[] dimensions, int[] steps)\n    { buffer = values; shape = dimensions; strides = steps; }", "private NDArray(T[] values, int[] dimensions, int[] steps, int offset = 0)\n    { buffer = values; shape = dimensions; strides = steps; baseOffset = offset; }");
Replace("int offset = 0;", "int offset = baseOffset;");
Replace("int remainder = linear, offset = 0;", "int remainder = linear, offset = baseOffset;");
Replace("axes.Select(a => strides[a]).ToArray());", "axes.Select(a => strides[a]).ToArray(), baseOffset);");
Replace("    /// <summary>Copies in logical C order, honoring view strides.</summary>", """
    /// <summary>Creates a shared-storage slice along one axis. Start is explicit, length is a count, step may be negative.</summary>
    public NDArray<T> Slice(int axis, int start, int length, int step = 1)
    {
        if ((uint)axis >= (uint)Rank) throw new ArgumentOutOfRangeException(nameof(axis));
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        if (step == 0) throw new ArgumentOutOfRangeException(nameof(step));
        if (length == 0)
        {
            if (start < 0 || start > shape[axis]) throw new ArgumentOutOfRangeException(nameof(start));
        }
        else
        {
            long last = start + (long)(length - 1) * step;
            if (start < 0 || start >= shape[axis] || last < 0 || last >= shape[axis])
                throw new ArgumentOutOfRangeException(nameof(start));
        }
        var dimensions = Shape; var steps = Strides;
        dimensions[axis] = length;
        steps[axis] = checked(steps[axis] * step);
        var offset = length == 0 ? baseOffset : checked(baseOffset + start * strides[axis]);
        return new NDArray<T>(buffer, dimensions, steps, offset);
    }

    /// <summary>Copies in logical C order, honoring view strides.</summary>
""");
var begin=text.IndexOf("    /// <summary>Elementwise sum of equal shapes;");
if(begin<0)throw new Exception("Add method not found");
text=text[..begin]+"""
    /// <summary>Elementwise addition using trailing-axis broadcasting. Returns independent storage.</summary>
    public NDArray<T> Add(NDArray<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        int rank = Math.Max(Rank, other.Rank);
        var dimensions = new int[rank];
        var leftSteps = new int[rank]; var rightSteps = new int[rank];
        for (int axis = 0; axis < rank; axis++)
        {
            int l = axis - (rank - Rank), r = axis - (rank - other.Rank);
            int ld = l < 0 ? 1 : shape[l], rd = r < 0 ? 1 : other.shape[r];
            if (ld != rd && ld != 1 && rd != 1) throw new ArgumentException("Shapes cannot broadcast.", nameof(other));
            dimensions[axis] = ld == 1 ? rd : ld;
            leftSteps[axis] = l < 0 || ld == 1 ? 0 : strides[l];
            rightSteps[axis] = r < 0 || rd == 1 ? 0 : other.strides[r];
        }
        int count = dimensions.Aggregate(1, (n, d) => checked(n * d));
        var values = new T[count];
        for (int linear = 0; linear < count; linear++)
        {
            int remainder = linear, left = baseOffset, right = other.baseOffset;
            for (int axis = rank - 1; axis >= 0; axis--)
            {
                int coordinate = remainder % dimensions[axis]; remainder /= dimensions[axis];
                left += coordinate * leftSteps[axis]; right += coordinate * rightSteps[axis];
            }
            values[linear] = buffer[left] + other.buffer[right];
        }
        return new NDArray<T>(values.AsSpan(), dimensions);
    }
}
""";
File.WriteAllText(path,text);
File.WriteAllText(Path.Combine(root,"src","NDSharp.Tests","TypedViewTests.cs"),"""
using NDSharp;
using Xunit;
public class TypedViewTests
{
    [Fact] public void ReverseSliceSharesStorage()
    {
        var x=new NDArray<long>(new long[]{1,2,3,4,5},5);
        var reverse=x.Slice(0,4,3,-2);
        Assert.Equal(new long[]{5,3,1},reverse.ToArray());
        reverse[1]=99; Assert.Equal(99L,x[2]); Assert.Equal(3,reverse.Length);
    }
    [Fact] public void SliceTransposeComposition()
    {
        var x=new NDArray<int>(new int[]{1,2,3,4,5,6},2,3);
        Assert.Equal(new int[]{5,6,2,3},x.Slice(1,1,2).Transpose().Slice(1,1,2,-1).Transpose().ToArray());
    }
    [Fact] public void RowBroadcast()
    {
        var x=new NDArray<int>(new int[]{1,2,3,4,5,6},2,3);
        var row=new NDArray<int>(new int[]{10,20,30},3);
        var result=x.Add(row);
        Assert.Equal(new int[]{11,22,33,14,25,36},result.ToArray());
        result[0,0]=99; Assert.Equal(1,x[0,0]);
    }
    [Fact] public void ColumnAndScalarBroadcast()
    {
        var row=new NDArray<decimal>(new decimal[]{1,2,3},1,3);
        var column=new NDArray<decimal>(new decimal[]{10,20},2,1);
        Assert.Equal(new decimal[]{11,12,13,21,22,23},row.Add(column).ToArray());
        Assert.Equal(new decimal[]{3,4,5},row.Add(new NDArray<decimal>(new decimal[]{2})).ToArray());
    }
    [Fact] public void BroadcastReversedView()
    {
        var x=new NDArray<int>(new int[]{1,2,3},3).Slice(0,2,3,-1);
        Assert.Equal(new int[]{13,12,11},x.Add(new NDArray<int>(new int[]{10})).ToArray());
    }
    [Fact] public void EmptyBroadcastAndSlice()
    {
        var x=new NDArray<int>(Array.Empty<int>(),0,3);
        Assert.Equal(new int[]{0,3},x.Add(new NDArray<int>(new int[]{1,2,3},1,3)).Shape);
        Assert.Empty(new NDArray<int>(new int[]{1,2},2).Slice(0,2,0).ToArray());
    }
    [Fact] public void InvalidArguments()
    {
        var x=new NDArray<int>(new int[]{1,2,3},3);
        Assert.Throws<ArgumentOutOfRangeException>(()=>x.Slice(0,0,2,-1));
        Assert.Throws<ArgumentOutOfRangeException>(()=>x.Slice(0,0,1,0));
        Assert.Throws<ArgumentException>(()=>x.Add(new NDArray<int>(new int[]{1,2},2)));
    }
}
""");
Console.WriteLine("Slicing and broadcasting implemented; seven tests written.");
