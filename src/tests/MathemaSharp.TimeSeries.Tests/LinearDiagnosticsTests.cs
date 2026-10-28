using System.Globalization;
using System.Numerics;
using System.Text.Json;
using MathemaSharp.LinearAlgebra;

namespace MathemaSharp.TimeSeries.Tests;

public class LinearDiagnosticsTests
{
    private static void Verify<T>() where T:IFloatingPointIeee754<T>
    {
        T N(int n)=>T.CreateChecked(n);
        T[,] a={{N(2),T.Zero},{T.Zero,N(4)}};
        var report=LinearSystemDiagnostics.Solve<T>(a,[N(2),N(8)]);
        Assert.Equal(new T[] {T.One,N(2)},report.Solution);
        Assert.Equal(T.Zero,report.RelativeResidualInfinity);
        Assert.Equal(T.Zero,report.NormwiseBackwardErrorInfinity);
        Assert.Equal(N(2),report.ConditionInfinityEstimate);
        Assert.Equal(T.One/N(2),report.ReciprocalConditionInfinityEstimate);
        Assert.False(report.IsIllConditioned);
        var zero=LinearSystemDiagnostics.Solve<T>(a,[T.Zero,T.Zero]);
        Assert.Equal(T.Zero,zero.RelativeResidualInfinity);
        Assert.Equal(T.Zero,zero.NormwiseBackwardErrorInfinity);
        Assert.Equal(report.ConditionInfinityEstimate,zero.ConditionInfinityEstimate);
        a[0,0]=N(100);
        Assert.Equal(T.One,report.Solution[0]);
    }
    [Fact] public void HalfNative()=>Verify<Half>();
    [Fact] public void FloatNative()=>Verify<float>();
    [Fact] public void DoubleNative()=>Verify<double>();
    [Fact] public void Decimal32Native()=>Verify<Decimal32>();
    [Fact] public void Decimal64Native()=>Verify<Decimal64>();
    [Fact] public void Decimal128Native()=>Verify<Decimal128>();

    [Theory] [InlineData(1e-200)] [InlineData(1e200)]
    public void GlobalScalingDoesNotChangeCondition(double scale)
    {
        var report=LinearSystemDiagnostics.Solve<double>(new double[,] {{2*scale,0},{0,4*scale}},[2*scale,8*scale]);
        Assert.Equal(2d,report.ConditionInfinityEstimate,12);
        Assert.Equal(1d,report.Solution[0],12);
        Assert.Equal(2d,report.Solution[1],12);
        Assert.True(report.NormwiseBackwardErrorInfinity<1e-14);
    }

    private static JsonDocument Oracle()
    {
        var root=new DirectoryInfo(System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes<System.Reflection.AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!);
        while(root is not null&&!File.Exists(Path.Combine(root.FullName,"src","MathemaSharp.slnx")))root=root.Parent;
        return JsonDocument.Parse(File.ReadAllText(Path.Combine(root!.FullName,"ops","oracles","numerical-reference.json")));
    }
    private static LinearSolveReport<T> Hilbert<T>(int n,out T condition) where T:IFloatingPointIeee754<T>
    {
        T P(string value)=>T.Parse(value,CultureInfo.InvariantCulture);
        using var oracle=Oracle();
        var row=oracle.RootElement.GetProperty("hilbert").EnumerateArray().Single(r=>r.GetProperty("n").GetInt32()==n);
        var rows=row.GetProperty("matrix").EnumerateArray().ToArray();
        var a=new T[n,n];
        for(var i=0;i<n;i++){var values=rows[i].EnumerateArray().ToArray();for(var j=0;j<n;j++)a[i,j]=P(values[j].GetString()!);}
        var b=row.GetProperty("rhs").EnumerateArray().Select(v=>P(v.GetString()!)).ToArray();
        condition=P(row.GetProperty("condition_infinity").GetString()!);
        return LinearSystemDiagnostics.Solve(a,b);
    }
    [Theory] [InlineData(3)] [InlineData(5)] [InlineData(8)]
    public void DoubleConditionAgainstMpmath(int n)
    {
        var report=Hilbert<double>(n,out var reference);
        Assert.True(Math.Abs(report.ConditionInfinityEstimate/reference-1)<1e-4);
        Assert.True(report.NormwiseBackwardErrorInfinity<1e-13);
        if(n==8)Assert.True(report.IsIllConditioned);
    }
    [Fact]
    public void Decimal128ConditionAgainstMpmath()
    {
        var report=Hilbert<Decimal128>(8,out var reference);
        var tolerance=Decimal128.Parse("1e-20",CultureInfo.InvariantCulture);
        Assert.True(Decimal128.Abs(report.ConditionInfinityEstimate/reference-Decimal128.One)<tolerance);
        Assert.False(report.IsIllConditioned); // rcond≈3e-11 > sqrt(epsilon128)≈3e-17
    }
    [Fact]
    public void Hilbert12WarnsDespiteTinyResidual()
    {
        var report=Hilbert<double>(12,out _);
        Assert.True(report.IsIllConditioned);
        Assert.True(report.ConditionInfinityEstimate>1e14);
        Assert.True(report.RelativeResidualInfinity<1e-12);
        Assert.True(report.NormwiseBackwardErrorInfinity<1e-12);
        Assert.True(report.Solution.Max(x=>Math.Abs(x-1))>1e-4);
    }
    [Fact]
    public void InvalidInputsAndUnrepresentableSolutionsAreRejected()
    {
        Assert.Throws<ArgumentException>(()=>LinearSystemDiagnostics.Solve<double>(new double[,] {{1}},[]));
        Assert.Throws<ArgumentException>(()=>LinearSystemDiagnostics.Solve<double>(new double[,] {{double.NaN}},[1]));
        Assert.Throws<ArgumentException>(()=>LinearSystemDiagnostics.Solve<double>(new double[,] {{1}},[double.PositiveInfinity]));
        Assert.Throws<ArithmeticException>(()=>LinearSystemDiagnostics.Solve<double>(new double[,] {{1e-300}},[1e300]));
    }
}
