using System.Globalization;
using System.Numerics;
using System.Text.Json;
using MathemaSharp.LinearAlgebra;

namespace MathemaSharp.TimeSeries.Tests;

public class LinearSystemDiagnosticsTests
{
    private static void Verify<T>() where T:IFloatingPointIeee754<T>
    {
        T N(int n)=>T.CreateChecked(n);
        T[,] a={{N(2),T.Zero},{T.Zero,N(4)}};
        T[] b=[N(4),N(12)];
        var report=LinearSystemDiagnostics.Solve(a,b);
        Assert.Equal(new T[] {N(2),N(3)},report.Solution);
        var epsilon=T.BitIncrement(T.One)-T.One;
        Assert.True(report.RelativeResidualInfinity<=N(8)*epsilon);
        Assert.True(report.NormwiseBackwardError<=N(8)*epsilon);
        Assert.Equal(N(2),report.ConditionInfinityEstimate);
        Assert.Equal(T.One/N(2),report.ReciprocalConditionInfinityEstimate);
        Assert.False(report.IsIllConditioned);
        Assert.Equal(N(2),a[0,0]);
        Assert.Equal(N(4),b[0]);
        var zero=LinearSystemDiagnostics.Solve(a,new T[2]);
        Assert.Equal(T.Zero,zero.RelativeResidualInfinity);
        Assert.Equal(T.Zero,zero.NormwiseBackwardError);
        var risky=LinearSystemDiagnostics.Solve(new T[,] {{T.One,T.Zero},{T.Zero,N(8)*epsilon}},new T[] {T.One,N(8)*epsilon});
        Assert.True(risky.IsIllConditioned);
        Assert.Equal(T.Zero,risky.NormwiseBackwardError);
        Assert.Equal(N(8)*epsilon,risky.ReciprocalConditionInfinityEstimate);
    }
    [Fact] public void HalfNative()=>Verify<Half>();
    [Fact] public void FloatNative()=>Verify<float>();
    [Fact] public void DoubleNative()=>Verify<double>();
    [Fact] public void Decimal32Native()=>Verify<Decimal32>();
    [Fact] public void Decimal64Native()=>Verify<Decimal64>();
    [Fact] public void Decimal128Native()=>Verify<Decimal128>();

    [Theory] [InlineData(1e-200)] [InlineData(1e200)]
    public void GlobalScaleDoesNotChangeCondition(double scale)
    {
        var report=LinearSystemDiagnostics.Solve(new double[,] {{scale,0},{0,2*scale}},new double[] {2*scale,6*scale});
        Assert.Equal(2d,report.ConditionInfinityEstimate,12);
        Assert.Equal(2d,report.Solution[0],12);
        Assert.Equal(3d,report.Solution[1],12);
        Assert.True(report.NormwiseBackwardError<1e-14);
    }

    [Theory] [InlineData(5,1e-8)] [InlineData(8,1e-4)] [InlineData(12,0.25)]
    public void HilbertConditionAgainstMpmath(int n,double tolerance)
    {
        var root=new DirectoryInfo(System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes<System.Reflection.AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!);
        while(root is not null && !File.Exists(Path.Combine(root.FullName,"src","MathemaSharp.slnx")))root=root.Parent;
        using var oracle=JsonDocument.Parse(File.ReadAllText(Path.Combine(root!.FullName,"ops","oracles","numerical-reference.json")));
        var row=oracle.RootElement.GetProperty("hilbert").EnumerateArray().Single(item=>item.GetProperty("n").GetInt32()==n);
        var rows=row.GetProperty("matrix").EnumerateArray().ToArray();
        var matrix=new double[n,n];
        for(var i=0;i<n;i++){var cells=rows[i].EnumerateArray().ToArray();for(var j=0;j<n;j++)matrix[i,j]=double.Parse(cells[j].GetString()!,CultureInfo.InvariantCulture);}
        var b=row.GetProperty("rhs").EnumerateArray().Select(value=>double.Parse(value.GetString()!,CultureInfo.InvariantCulture)).ToArray();
        var expected=double.Parse(row.GetProperty("condition_infinity").GetString()!,CultureInfo.InvariantCulture);
        var report=LinearSystemDiagnostics.Solve(matrix,b);
        Assert.True(Math.Abs(report.ConditionInfinityEstimate/expected-1)<=tolerance);
        Assert.True(report.NormwiseBackwardError<1e-12);
        if(n>=8) Assert.True(report.IsIllConditioned);
        if(n==12) Assert.True(report.Solution.Max(value=>Math.Abs(value-1))>1e-4);
    }

    [Fact]
    public void InvalidAndUnrepresentableInputsAreRejected()
    {
        Assert.Throws<ArgumentNullException>(()=>LinearSystemDiagnostics.Solve<double>(null!,[1]));
        Assert.Throws<ArgumentException>(()=>LinearSystemDiagnostics.Solve(new double[2,3],new double[2]));
        Assert.Throws<ArgumentException>(()=>LinearSystemDiagnostics.Solve(new double[,] {{0}},new double[] {0}));
        Assert.Throws<ArgumentException>(()=>LinearSystemDiagnostics.Solve(new double[,] {{double.NaN}},new double[] {1}));
        Assert.Throws<ArgumentException>(()=>LinearSystemDiagnostics.Solve(new double[,] {{1}},new double[] {double.PositiveInfinity}));
        Assert.Throws<ArithmeticException>(()=>LinearSystemDiagnostics.Solve(new double[,] {{1e-300}},new double[] {1e300}));
    }
}
