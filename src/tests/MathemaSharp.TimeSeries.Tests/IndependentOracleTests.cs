using System.Globalization;
using System.Numerics;
using System.Text.Json;
using MathemaSharp.LinearAlgebra;
using MathemaSharp.TimeSeries;

namespace MathemaSharp.TimeSeries.Tests;

public class IndependentOracleTests
{
    private static JsonDocument Oracle()
    {
        var root=new DirectoryInfo(System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes<System.Reflection.AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!);
        while(root is not null && !File.Exists(Path.Combine(root.FullName,"src","MathemaSharp.slnx"))) root=root.Parent;
        return JsonDocument.Parse(File.ReadAllText(Path.Combine(root!.FullName,"ops","oracles","numerical-reference.json")));
    }
    private static T P<T>(string text) where T:IFloatingPointIeee754<T> => T.Parse(text,CultureInfo.InvariantCulture);

    private static void VerifySpecial<T>(string toleranceText) where T:IFloatingPointIeee754<T>
    {
        using var oracle=Oracle();
        var tolerance=P<T>(toleranceText);
        Assert.Equal(200,oracle.RootElement.GetProperty("precision_decimal_digits").GetInt32());
        foreach(var row in oracle.RootElement.GetProperty("gamma_q").EnumerateArray())
        {
            var a=P<T>(row.GetProperty("a").GetString()!);
            var x=P<T>(row.GetProperty("x").GetString()!);
            var expected=P<T>(row.GetProperty("q").GetString()!);
            var actual=SpecialFunctions.RegularizedGammaQ(a,x);
            Assert.True(T.IsFinite(actual) && actual>=T.Zero && actual<=T.One);
            if(expected==T.Zero) Assert.Equal(T.Zero,actual); // underflow del riferimento arrotondato in T
            else Assert.True(T.Abs(actual/expected-T.One)<=tolerance,
                $"{typeof(T).Name} Q({a},{x}): actual={actual}, expected={expected}");
            var degrees=T.CreateChecked(2)*a;
            if(degrees==T.Truncate(degrees) && degrees>T.Zero)
            {
                var survival=SpecialFunctions.ChiSquareSurvival(T.CreateChecked(2)*x,int.CreateChecked(degrees));
                if(expected==T.Zero) Assert.Equal(T.Zero,survival);
                else Assert.True(T.Abs(survival/expected-T.One)<=tolerance);
            }
        }
        foreach(var row in oracle.RootElement.GetProperty("log_gamma").EnumerateArray())
        {
            var x=P<T>(row.GetProperty("x").GetString()!);
            var expected=P<T>(row.GetProperty("value").GetString()!);
            var actual=SpecialFunctions.LogGamma(x);
            Assert.True(T.Abs(actual-expected)/T.Max(T.One,T.Abs(expected))<=tolerance);
        }
    }
    [Fact] public void FloatSpecialFunctionsAgainstMpmath()=>VerifySpecial<float>("0.002");
    [Fact] public void DoubleSpecialFunctionsAgainstMpmath()=>VerifySpecial<double>("1e-10");
    [Fact] public void Decimal64SpecialFunctionsAgainstMpmath()=>VerifySpecial<Decimal64>("1e-10");
    [Fact] public void Decimal128SpecialFunctionsAgainstMpmath()=>VerifySpecial<Decimal128>("1e-10");

    private static void VerifyHilbert<T>(int n,string solutionTolerance,string determinantTolerance) where T:IFloatingPointIeee754<T>
    {
        using var oracle=Oracle();
        var row=oracle.RootElement.GetProperty("hilbert").EnumerateArray().Single(item=>item.GetProperty("n").GetInt32()==n);
        var a=new T[n,n];
        var matrix=row.GetProperty("matrix").EnumerateArray().ToArray();
        for(var i=0;i<n;i++)
        {
            var values=matrix[i].EnumerateArray().ToArray();
            for(var j=0;j<n;j++) a[i,j]=P<T>(values[j].GetString()!);
        }
        var b=row.GetProperty("rhs").EnumerateArray().Select(item=>P<T>(item.GetString()!)).ToArray();
        var expected=row.GetProperty("solution").EnumerateArray().Select(item=>P<T>(item.GetString()!)).ToArray();
        var lu=LuFactorization<T>.Create(a);
        foreach(var x in new[] {lu.Solve(b),MatrixAlgebra.LeastSquares(a,b)})
        {
            var error=x.Zip(expected,(actual,truth)=>T.Abs(actual-truth)).Aggregate(T.Zero,T.Max);
            Assert.True(error<=P<T>(solutionTolerance),$"{typeof(T).Name} H{n}: solution error={error}");
            var ax=MatrixAlgebra.Multiply(a,x);
            var residual=ax.Zip(b,(actual,truth)=>actual-truth).ToArray();
            var relativeResidual=MatrixAlgebra.NormL2<T>(residual)/MatrixAlgebra.NormL2<T>(b);
            Assert.True(relativeResidual<=T.CreateChecked(256)*(T.BitIncrement(T.One)-T.One));
        }
        var det=P<T>(row.GetProperty("determinant").GetString()!);
        Assert.True(T.Abs(lu.Determinant()/det-T.One)<=P<T>(determinantTolerance));
    }
    [Theory] [InlineData(3,"1e-12")] [InlineData(5,"1e-9")] [InlineData(8,"1e-5")]
    public void DoubleHilbert(int n,string tolerance)=>VerifyHilbert<double>(n,tolerance,"1e-5");
    [Theory] [InlineData(3,"1e-29")] [InlineData(5,"1e-26")] [InlineData(8,"1e-20")]
    public void Decimal128Hilbert(int n,string tolerance)=>VerifyHilbert<Decimal128>(n,tolerance,"1e-20");

    [Fact]
    public void DoubleHilbert12ExposesSmallResidualButInaccurateSolution()
    {
        using var oracle=Oracle();
        var row=oracle.RootElement.GetProperty("hilbert").EnumerateArray().Single(item=>item.GetProperty("n").GetInt32()==12);
        var matrix=row.GetProperty("matrix").EnumerateArray().ToArray();
        var a=new double[12,12];
        for(var i=0;i<12;i++)
        {
            var values=matrix[i].EnumerateArray().ToArray();
            for(var j=0;j<12;j++) a[i,j]=P<double>(values[j].GetString()!);
        }
        var b=row.GetProperty("rhs").EnumerateArray().Select(item=>P<double>(item.GetString()!)).ToArray();
        var x=LuFactorization<double>.Create(a).Solve(b);
        var error=x.Max(value=>Math.Abs(value-1));
        var residual=MatrixAlgebra.Multiply(a,x).Zip(b,(actual,truth)=>actual-truth).ToArray();
        Assert.True(error>1e-4,"Hilbert12 must not be presented as an accurate double solve.");
        Assert.True(MatrixAlgebra.NormL2<double>(residual)/MatrixAlgebra.NormL2<double>(b)<1e-12);
        Assert.Throws<ArgumentException>(()=>MatrixAlgebra.LeastSquares(a,b));
    }
}
