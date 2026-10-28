#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\NDSharp";
var path=Path.Combine(root,"src","NDSharp","NDArray.Generic.cs");
var text=File.ReadAllText(path);
var marker="    /// <summary>Elementwise addition using trailing-axis broadcasting. Returns independent storage.</summary>";
if(!text.Contains(marker))throw new Exception("Insertion point missing");
text=text.Replace(marker,"""
    /// <summary>Non-conjugating dot product of two rank-one arrays in logical order, accumulated in T.</summary>
    public T Dot(NDArray<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Rank != 1 || other.Rank != 1) throw new ArgumentException("Dot requires rank-one arrays.", nameof(other));
        if (shape[0] != other.shape[0]) throw new ArgumentException("Vector lengths differ.", nameof(other));
        var result = T.Zero;
        for (int i = 0; i < shape[0]; i++)
            result += buffer[baseOffset + i * strides[0]] * other.buffer[other.baseOffset + i * other.strides[0]];
        return result;
    }

    /// <summary>Dense rank-two matrix product, honoring strides and returning independent storage.
    /// Accumulation uses T's ordinary rounding and overflow semantics; no batched broadcasting.</summary>
    public NDArray<T> MatMul(NDArray<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Rank != 2 || other.Rank != 2) throw new ArgumentException("MatMul requires rank-two arrays.", nameof(other));
        if (shape[1] != other.shape[0]) throw new ArgumentException("Inner dimensions differ.", nameof(other));
        int rows = shape[0], inner = shape[1], columns = other.shape[1];
        var values = new T[checked(rows * columns)];
        for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
            {
                var sum = T.Zero;
                int left = baseOffset + row * strides[0];
                int right = other.baseOffset + column * other.strides[1];
                for (int k = 0; k < inner; k++)
                    sum += buffer[left + k * strides[1]] * other.buffer[right + k * other.strides[0]];
                values[row * columns + column] = sum;
            }
        return new NDArray<T>(values.AsSpan(), rows, columns);
    }

"""+marker);
File.WriteAllText(path,text);
File.WriteAllText(Path.Combine(root,"src","NDSharp.Tests","TypedProductTests.cs"),"""
using NDSharp;
using Xunit;
public class TypedProductTests
{
    [Fact] public void DotHandlesReverseSlice()
    {
        var x=new NDArray<long>(new long[]{1,2,3},3);
        Assert.Equal(10L,x.Dot(x.Slice(0,2,3,-1)));
    }
    [Fact] public void MatMulRectangular()
    {
        var a=new NDArray<int>(new int[]{1,2,3,4,5,6},2,3);
        var b=new NDArray<int>(new int[]{7,8,9,10,11,12},3,2);
        var c=a.MatMul(b);
        Assert.Equal(new int[]{58,64,139,154},c.ToArray());
        c[0,0]=0; Assert.Equal(1,a[0,0]); Assert.Equal(7,b[0,0]);
    }
    [Fact] public void MatMulViews()
    {
        var a=new NDArray<int>(new int[]{1,2,3,4,5,6},2,3);
        Assert.Equal(new int[]{14,32,32,77},a.MatMul(a.Transpose()).ToArray());
        var reversed=a.Slice(1,2,3,-1);
        Assert.Equal(new int[]{14,32,32,77},reversed.MatMul(reversed.Transpose()).ToArray());
    }
    [Fact] public void EmptyInnerDimensionProducesZero()
    {
        var a=new NDArray<int>(Array.Empty<int>(),2,0);
        var b=new NDArray<int>(Array.Empty<int>(),0,3);
        Assert.Equal(new int[]{0,0,0,0,0,0},a.MatMul(b).ToArray());
        Assert.Equal(0,new NDArray<int>(Array.Empty<int>(),0).Dot(new NDArray<int>(Array.Empty<int>(),0)));
    }
    [Fact] public void RankAndDimensionErrors()
    {
        var a=new NDArray<int>(new int[]{1,2},2);
        var b=new NDArray<int>(new int[]{1},1);
        Assert.Throws<ArgumentException>(()=>a.Dot(b));
        Assert.Throws<ArgumentException>(()=>a.MatMul(b));
        Assert.Throws<ArgumentException>(()=>new NDArray<int>(new int[]{1,2},1,2).MatMul(new NDArray<int>(new int[]{1},1,1)));
    }
}
""");
Console.WriteLine("Dot and MatMul added with five tests.");
