#:property TargetFramework=net11.0
using System.Xml.Linq;
var root=@"D:\Dev\Science\MatSharp";
var project=Path.Combine(root,"src","MatSharpLib.NDSharp","MatSharpLib.NDSharp.csproj");
var doc=XDocument.Load(project);
doc.Descendants("PackageReference").Single(e=>(string?)e.Attribute("Include")=="NDSharp").SetAttributeValue("Version","2.8.0");
var refs=doc.Descendants("ItemGroup").Last();
refs.Add(new XElement("ProjectReference",new XAttribute("Include","../../../MathemaSharp/src/libs/MathemaSharp.LinearAlgebra/MathemaSharp.LinearAlgebra.csproj")));
doc.Save(project);
File.WriteAllText(Path.Combine(root,"src","MatSharpLib.NDSharp","MathemaFunctions.cs"),"""
using MathemaSharp.LinearAlgebra;
using MatSharpLib.Runtime;
using MatSharpLib.Interpretation;
using static MatSharpLib.Interpretation.A;
namespace MatSharpLib.NDSharpBridge;

/// <summary>Real double adapters to MathemaSharp. Column-major language storage is copied explicitly; no Decimal language support is implied.</summary>
public static class MathemaFunctions
{
    public static LinearSolveReport<double> Solve(Mat matrix, Mat rhs)
    {
        if(matrix.Dims.Length!=2 || rhs.Dims.Length!=2 || rhs.Dims[1]!=1)
            throw new MatException("MathemaSharp:solve:shape","Expected a rank-two matrix and column-vector RHS.");
        if(matrix.Im is not null || rhs.Im is not null)
            throw new MatException("MathemaSharp:solve:complex","This adapter supports real inputs only.");
        int rows=matrix.Dims[0],columns=matrix.Dims[1];
        var dense=new double[rows,columns];
        for(int i=0;i<rows;i++)for(int j=0;j<columns;j++)dense[i,j]=matrix.Re[j*rows+i];
        try { return LinearSystemDiagnostics.Solve<double>(dense,rhs.Re); }
        catch(Exception e) when(e is ArgumentException or ArithmeticException or InvalidOperationException)
        { throw new MatException("MathemaSharp:solve:invalid",e.Message); }
    }
    public static void Register(Interp interp)
    {
        var reg=new Reg(interp);
        reg.F("mathema_solve",args=>
        {
            if(args.Length!=2)throw new MatException("MathemaSharp:solve:arity","Expected A and b.");
            var report=Solve(M(args[0],"mathema_solve"),M(args[1],"mathema_solve"));
            return Mat.Matrix(report.Solution.Length,1,report.Solution);
        });
        reg.F("mathema_rcond",args=>
        {
            if(args.Length!=2)throw new MatException("MathemaSharp:rcond:arity","Expected A and b.");
            return Mat.Scalar(Solve(M(args[0],"mathema_rcond"),M(args[1],"mathema_rcond")).ReciprocalConditionInfinityEstimate);
        });
    }
}
""");
var register=Path.Combine(root,"src","MatSharpLib.NDSharp","FftFunctions.cs");var text=File.ReadAllText(register);text=text.Replace("EigFunctions.Register(it);","EigFunctions.Register(it);\n        MathemaFunctions.Register(it);");File.WriteAllText(register,text);
File.WriteAllText(Path.Combine(root,"src","MatSharpLib.Tests","MathemaBridgeTests.cs"),"""
using MatSharpLib;
using MatSharpLib.Runtime;
using MatSharpLib.NDSharpBridge;
public class MathemaBridgeTests
{
    private static MatEngine Engine(){var engine=new MatEngine();NDSharpFunctions.Register(engine);return engine;}
    [Fact] public void InterpreterCallsSharedSolver()
    {
        var engine=Engine();var x=Assert.IsType<Mat>(engine.Eval("mathema_solve([2 1; 1 3],[4;7])"));
        Assert.Equal(2,x.Dims[0]);Assert.Equal(1,x.Dims[1]);
        Assert.True(Math.Abs(x.Re[0]-1)<1e-12);Assert.True(Math.Abs(x.Re[1]-2)<1e-12);
    }
    [Fact] public void ConditionDiagnosticIsExposed()
    {
        var result=Assert.IsType<Mat>(Engine().Eval("mathema_rcond([2 0; 0 4],[2;8])"));
        Assert.Equal(0.5,result.Re[0]);
    }
    [Fact] public void ExistingMatlabIndexingAndBackslashRemain()
    {
        var engine=Engine();engine.Run("A=[2 0;0 4]; b=[2;8]; x=A\\b;");
        Assert.Equal(2,Assert.IsType<Mat>(engine.Eval("x(2)")).Re[0]);
        Assert.Equal(new double[]{2,0,0,4},Assert.IsType<Mat>(engine.Eval("A(:)")).Re);
    }
}
""");
Console.WriteLine("MatSharp linked to NDSharp 2.8 and MathemaSharp LinearAlgebra.");
