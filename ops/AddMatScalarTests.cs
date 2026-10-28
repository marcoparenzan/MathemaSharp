#:property TargetFramework=net11.0
File.WriteAllText(@"D:\Dev\Science\MatSharp\src\MatSharpLib.Tests\GenericScalarTests.cs","""
using MatSharpLib.Runtime;
public class GenericScalarTests
{
    [Fact] public void DecimalArithmeticPreservesDigits()
    {
        var one=new GenericScalar<decimal>(1m);var delta=new GenericScalar<decimal>(0.0000000000000000000000001m);
        var sum=Assert.IsType<GenericScalar<decimal>>(Ops.Binary("+",one,delta));
        var diff=Assert.IsType<GenericScalar<decimal>>(Ops.Binary("-",sum,Mat.Scalar(1)));
        Assert.Equal(delta.Value,diff.Value);
    }
    [Fact] public void ComparisonProducesLogical()
    {
        var result=Assert.IsType<Mat>(Ops.Binary("<",new GenericScalar<decimal>(2m),new GenericScalar<decimal>(3m)));
        Assert.Equal(MClass.Logical,result.Class);Assert.Equal(1d,result.Re[0]);
    }
    [Fact] public void FractionalDoubleAndDifferentDtypeAreRejected()
    {
        var x=new GenericScalar<decimal>(1m);
        Assert.Throws<MatException>(()=>Ops.Binary("+",x,Mat.Scalar(0.1)));
        Assert.Throws<MatException>(()=>Ops.Binary("+",x,new GenericScalar<double>(1d)));
    }
    [Fact] public void GenericScalarDoesNotGrowSilently()
    {
        Assert.Throws<MatException>(()=>new GenericScalar<decimal>(1m).Resize(new[]{2,2}));
    }
}
""");
