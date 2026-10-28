#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\MatSharp";
var p=Path.Combine(root,"src","MatSharpLib","Runtime","GenericScalar.cs");var t=File.ReadAllText(p);
t=t.Replace("    private T[] data;", """
    private T[] data;
    private static Func<GenericScalar<T>, GenericScalar<T>, MValue>? leftDivide;
    /// <summary>Opt-in shared-kernel adapter. Arithmetic storage remains independent of the math library.</summary>
    public static void RegisterLeftDivide(Func<GenericScalar<T>, GenericScalar<T>, MValue> solver)
    {
        ArgumentNullException.ThrowIfNull(solver);
        Interlocked.CompareExchange(ref leftDivide, solver, null);
    }
""");
t=t.Replace("        if(op==\"*\" && !a.IsScalar && !b.IsScalar)", """
        if(op=="\\" && !a.IsScalar)
        {
            var solver = Volatile.Read(ref leftDivide) ?? throw new MatException("MathemaSharp:typed:solve", "Register the shared mathematics adapter before typed matrix left division.");
            return solver(a,b);
        }
        if(op=="*" && !a.IsScalar && !b.IsScalar)
""");
t=t.Replace("if(op==\"/\" && !b.IsScalar || op==\"\\\\\" && (!a.IsScalar || !b.IsScalar))", "if(op==\"/\" && !b.IsScalar)");File.WriteAllText(p,t);
p=Path.Combine(root,"src","MatSharpLib.NDSharp","MathemaFunctions.cs");t=File.ReadAllText(p).Replace("public static class MathemaFunctions", "public static partial class MathemaFunctions");
t=t.Replace("        var reg=new Reg(interp);", """
        EnableTypedSolver<Half>(); EnableTypedSolver<float>(); EnableTypedSolver<double>();
        EnableTypedSolver<System.Numerics.Decimal32>(); EnableTypedSolver<System.Numerics.Decimal64>(); EnableTypedSolver<System.Numerics.Decimal128>();
        var reg=new Reg(interp);
""");File.WriteAllText(p,t);
File.WriteAllText(Path.Combine(root,"src","MatSharpLib.NDSharp","TypedSolve.cs"),"""
using System.Numerics;
using MathemaSharp.LinearAlgebra;
using MatSharpLib.Runtime;
namespace MatSharpLib.NDSharpBridge;
public static partial class MathemaFunctions
{
    public static void EnableTypedSolver<T>() where T:IFloatingPointIeee754<T>
        => GenericScalar<T>.RegisterLeftDivide(SolveTyped<T>);
    private static MValue SolveTyped<T>(GenericScalar<T> matrix, GenericScalar<T> rhs) where T:IFloatingPointIeee754<T>
    {
        if(matrix.Rows!=rhs.Rows || matrix.Cols==0 || rhs.Cols==0)
            throw new MatException("MathemaSharp:typed:solveShape","Expected matching rows and nonempty coefficient and RHS columns.");
        if(matrix.Rows<matrix.Cols)
            throw new MatException("MathemaSharp:typed:underdetermined","Underdetermined typed systems need a minimum-norm solver, which is not implemented.");
        var source=matrix.ToArray(); var values=rhs.ToArray();
        var dense=new T[matrix.Rows,matrix.Cols];
        for(int j=0;j<matrix.Cols;j++)for(int i=0;i<matrix.Rows;i++)dense[i,j]=source[j*matrix.Rows+i];
        var result=new T[checked(matrix.Cols*rhs.Cols)];
        try
        {
            var factor=matrix.Rows==matrix.Cols?LuFactorization<T>.Create(dense):null;
            for(int j=0;j<rhs.Cols;j++)
            {
                var b=values.AsSpan(j*rhs.Rows,rhs.Rows).ToArray();
                var x=factor is null?MatrixAlgebra.LeastSquares<T>(dense,b):factor.Solve(b);
                Array.Copy(x,0,result,j*matrix.Cols,x.Length);
            }
            return new GenericScalar<T>(result,[matrix.Cols,rhs.Cols]);
        }
        catch(Exception e) when(e is ArgumentException or ArithmeticException or InvalidOperationException)
        { throw new MatException("MathemaSharp:typed:solveInvalid",e.Message); }
    }
}
""");
File.WriteAllText(Path.Combine(root,"src","MatSharpLib.Tests","TypedSolveTests.cs"),""""
using System.Numerics;
using MatSharpLib;
using MatSharpLib.Runtime;
using MatSharpLib.NDSharpBridge;
public class TypedSolveTests
{
    [Fact] public void SixFormatsSolveColumnMajorMultipleRhsAndTallSystems()
    { Verify<Half>(); Verify<float>(); Verify<double>(); Verify<Decimal32>(); Verify<Decimal64>(); Verify<Decimal128>(); }
    private static void Verify<T>() where T:IFloatingPointIeee754<T>
    {
        MathemaFunctions.EnableTypedSolver<T>();
        T N(int x)=>T.CreateChecked(x);
        var a=new GenericScalar<T>([N(2),T.Zero,T.Zero,N(4)],[2,2]);
        var b=new GenericScalar<T>([N(2),N(8),N(4),N(12)],[2,2]);
        var x=Assert.IsType<GenericScalar<T>>(Ops.Binary("\\",a,b));
        Assert.Equal(new[]{N(1),N(2),N(2),N(3)},x.ToArray());
        Assert.Equal(new[]{N(2),T.Zero,T.Zero,N(4)},a.ToArray());
        var tall=new GenericScalar<T>([N(1),N(0),N(1),N(0),N(1),N(1)],[3,2]);
        var result=Assert.IsType<GenericScalar<T>>(Ops.Binary("\\",tall,new GenericScalar<T>([N(1),N(2),N(3)],[3,1]))).ToArray();
        Assert.True(T.Abs(result[0]-N(1))<T.CreateChecked(0.02));
        Assert.True(T.Abs(result[1]-N(2))<T.CreateChecked(0.02));
    }
    [Fact] public void InterpreterPreservesDecimal128IncrementInBackslash()
    {
        var engine=new MatEngine();NDSharpFunctions.Register(engine);
        engine.Run("a=[decimal128(1) decimal128(0);decimal128(0) decimal128(1)]; b=[decimal128('1.0000000000000000000000001');decimal128(2)]; x=a\\b;");
        var x=Assert.IsType<GenericScalar<Decimal128>>(engine.Eval("x(1)-decimal128(1)"));
        Assert.Equal(Decimal128.Parse("1e-25"),x.Value);
    }
    [Fact] public void UnsupportedAndSingularSystemsAreExplicitErrors()
    {
        MathemaFunctions.EnableTypedSolver<double>();
        var wide=new GenericScalar<double>([1,2],[1,2]);
        Assert.Throws<MatException>(()=>Ops.Binary("\\",wide,new GenericScalar<double>(1)));
        var singular=new GenericScalar<double>([1,2,2,4],[2,2]);
        Assert.Throws<MatException>(()=>Ops.Binary("\\",singular,new GenericScalar<double>([1,2],[2,1])));
    }
}
"""");
