#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\MatSharp\src\MatSharpLib.Tests";
var path=Path.Combine(root,"GenericScalarTests.cs");var text=File.ReadAllText(path).Replace("Assert.Throws<MatException>(()=>new GenericScalar<decimal>(1m).Resize(new[]{2,2}));","var grown=Assert.IsType<GenericScalar<decimal>>(new GenericScalar<decimal>(1m).Resize(new[]{2,2}));\n        Assert.Equal(new decimal[]{1,0,0,0},grown.ToArray());");File.WriteAllText(path,text);
File.WriteAllText(Path.Combine(root,"GenericMatrixTests.cs"),"""
using MatSharpLib.Runtime;
public class GenericMatrixTests
{
    [Fact] public void MatrixProductAndTranspose()
    {
        var a=new GenericScalar<decimal>(new decimal[]{1,3,2,4},new[]{2,2});
        var t=Assert.IsType<GenericScalar<decimal>>(Ops.Transpose(a,false));
        Assert.Equal(new decimal[]{1,2,3,4},t.ToArray());
        Assert.Equal(new decimal[]{5,11,11,25},Assert.IsType<GenericScalar<decimal>>(Ops.Binary("*",a,t)).ToArray());
    }
    [Fact] public void ConcatenationKeepsColumnMajor()
    {
        MValue[] rows=[new GenericScalar<decimal>(1),new GenericScalar<decimal>(2)];
        var row=Assert.IsType<GenericScalar<decimal>>(Ops.Cat(1,rows));
        var matrix=Assert.IsType<GenericScalar<decimal>>(Ops.Cat(0,new MValue[]{row,row}));
        Assert.Equal(new[]{2,2},matrix.Dims);Assert.Equal(new decimal[]{1,1,2,2},matrix.ToArray());
    }
    [Fact] public void GatheringAndAssignmentRetainPrecision()
    {
        var delta=0.0000000000000000000000001m;
        var x=new GenericScalar<decimal>(new decimal[]{1,2,3,4},new[]{2,2});
        x.SetFrom(new[]{1},new GenericScalar<decimal>(delta));
        Assert.Equal(delta,Assert.IsType<GenericScalar<decimal>>(x.Gather(new[]{1},new[]{1,1})).Value);
        var clone=(GenericScalar<decimal>)x.Clone();clone.SetFrom(new[]{1},new GenericScalar<decimal>(9));
        Assert.Equal(delta,x.ToArray()[1]);
    }
    [Fact] public void LeadingAxisBroadcastMatchesMatlab()
    {
        var column=new GenericScalar<decimal>(new decimal[]{1,2},new[]{2,1});
        var row=new GenericScalar<decimal>(new decimal[]{10,20,30},new[]{1,3});
        Assert.Equal(new decimal[]{11,12,21,22,31,32},Assert.IsType<GenericScalar<decimal>>(Ops.Binary("+",column,row)).ToArray());
    }
    [Fact] public void MatrixDivisionIsNotMistakenForElementwise()
    {
        var a=new GenericScalar<decimal>(new decimal[]{1,2,3,4},new[]{2,2});
        Assert.Throws<MatException>(()=>Ops.Binary("/",a,a));
        Assert.Equal(new decimal[]{1,1,1,1},Assert.IsType<GenericScalar<decimal>>(Ops.Binary("./",a,a)).ToArray());
    }
}
""");
