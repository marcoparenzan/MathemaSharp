#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\MatSharp\src\MatSharpLib";
File.WriteAllText(Path.Combine(root,"Runtime","GenericScalar.cs"),"""
using System.Globalization;
using System.Numerics;
namespace MatSharpLib.Runtime;

public interface IGenericScalar
{
    MValue Binary(string op,MValue other,bool reversed=false);
    MValue Unary(string op);
}

/// <summary>Native generic scalar, separate from the existing double matrix storage. Matrix concatenation and resizing are deliberately unsupported.</summary>
public sealed class GenericScalar<T> : MValue, IGenericScalar where T:INumber<T>
{
    public T Value {get;}
    public GenericScalar(T value){Value=value;Dims=new[]{1,1};}
    public override string ClassName=>typeof(T).Name.ToLowerInvariant();
    public override string ToString()=>Value.ToString(null,CultureInfo.InvariantCulture);
    public static MValue Parse(string value)=>new GenericScalar<T>(T.Parse(value,CultureInfo.InvariantCulture));
    public MValue Unary(string op)=>op switch
    {
        "+"=>Clone(),"-"=>new GenericScalar<T>(-Value),
        _=>throw new MatException("MathemaSharp:scalar:operator","Unsupported generic scalar unary operator "+op)
    };
    private static T Read(MValue other)
    {
        if(other is GenericScalar<T> typed)return typed.Value;
        if(other is Mat {Numel:1,Im:null} m && m.Class!=MClass.Char && double.IsFinite(m.Re[0]) && Math.Truncate(m.Re[0])==m.Re[0] && Math.Abs(m.Re[0])<=9007199254740991d)
            return T.CreateChecked((long)m.Re[0]);
        throw new MatException("MathemaSharp:scalar:mixedType","Use the same decimal type or an exactly representable integer scalar; float and mixed decimal coercion are not implicit.");
    }
    public MValue Binary(string op,MValue other,bool reversed=false)
    {
        var operand=Read(other);var a=reversed?operand:Value;var b=reversed?Value:operand;
        bool? comparison=op switch {"=="=>a==b,"~="=>a!=b,"<"=>a<b,">"=>a>b,"<="=>a<=b,">="=>a>=b,_=>null};
        if(comparison is bool truth)return new Mat(MClass.Logical,new[]{1,1},new double[]{truth?1:0});
        return new GenericScalar<T>(op switch
        {
            "+"=>a+b,"-"=>a-b,"*" or ".*"=>a*b,"/" or "./"=>a/b,"\\" or ".\\"=>b/a,
            _=>throw new MatException("MathemaSharp:scalar:operator","Unsupported generic scalar binary operator "+op)
        });
    }
    public override MValue Clone()=>new GenericScalar<T>(Value);
    public override MValue Gather(int[] offsets,int[] dimensions)
    {
        if(offsets.Length!=1 || offsets[0]!=0 || Dm.Numel(dimensions)!=1)throw new MatException("MathemaSharp:scalar:index","Only scalar indexing is supported.");
        return Clone();
    }
    public override MValue Reshape(int[] dimensions)
    {
        if(Dm.Numel(dimensions)!=1)throw new MatException("MathemaSharp:scalar:shape","Generic matrices are not implemented.");
        return Clone();
    }
    public override MValue Resize(int[] dimensions)=>throw new MatException("MathemaSharp:scalar:resize","Generic scalar growth is not supported.");
    public override void SetFrom(int[] offsets,MValue rhs)=>throw new MatException("MathemaSharp:scalar:assignment","Generic scalars are immutable; replace the variable.");
}
""");
File.WriteAllText(Path.Combine(root,"Builtins","GenericNumbers.cs"),"""
using System.Globalization;
using System.Reflection;
using MatSharpLib.Interpretation;
using MatSharpLib.Runtime;
using static MatSharpLib.Interpretation.A;
namespace MatSharpLib.Builtins;
public static class GenericNumbers
{
    public static void Register(Interp interp)
    {
        var reg=new Reg(interp);
        foreach(var bits in new[]{32,64,128})
        {
            var name="decimal"+bits;
            reg.F(name,args=>
            {
                if(args.Length!=1)throw new MatException("MathemaSharp:decimal:arity","Expected one string or integer scalar.");
                var input=M(args[0],name);string literal;
                if(input.Class==MClass.Char)literal=new string(input.Re.Select(v=>(char)v).ToArray());
                else if(input.IsScalar && input.Im is null && double.IsFinite(input.Re[0]) && Math.Truncate(input.Re[0])==input.Re[0] && Math.Abs(input.Re[0])<=9007199254740991d)literal=((long)input.Re[0]).ToString(CultureInfo.InvariantCulture);
                else throw new MatException("MathemaSharp:decimal:input","Use a string for decimal literals; fractional double inputs are not implicit.");
                var type=typeof(object).Assembly.GetType("System.Numerics.Decimal"+bits);
                if(type is null)throw new MatException("MathemaSharp:decimal:runtime","IEEE decimals require runtime .NET 11.");
                var scalar=typeof(GenericScalar<>).MakeGenericType(type);
                try { return (MValue)scalar.GetMethod("Parse",BindingFlags.Public|BindingFlags.Static)!.Invoke(null,new object[]{literal})!; }
                catch(TargetInvocationException e){throw new MatException("MathemaSharp:decimal:literal",e.InnerException?.Message ?? e.Message);}
            });
        }
    }
}
""");
var file=Path.Combine(root,"Runtime","Ops.cs");var text=File.ReadAllText(file).Replace("\r\n","\n");
text=text.Replace("public static MValue Binary(string op, MValue av, MValue bv)\n    {","public static MValue Binary(string op, MValue av, MValue bv)\n    {\n        if(av is IGenericScalar left)return left.Binary(op,bv);\n        if(bv is IGenericScalar right)return right.Binary(op,av,true);");
text=text.Replace("public static MValue Unary(string op, MValue v)\n    {","public static MValue Unary(string op, MValue v)\n    {\n        if(v is IGenericScalar scalar)return scalar.Unary(op);");
File.WriteAllText(file,text);
file=Path.Combine(root,"Builtins","BuiltinRegistry.cs");text=File.ReadAllText(file).Replace("var r = new Reg(it);","var r = new Reg(it);\n        GenericNumbers.Register(it);");File.WriteAllText(file,text);
Console.WriteLine("Generic scalar storage and IEEE decimal constructors registered.");
