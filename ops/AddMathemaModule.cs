#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp";
File.WriteAllText(Path.Combine(root,"src","PySharpLib","Modules","MathemaModule.cs"),"""
using System.Globalization;
using System.Numerics;
using System.Reflection;
using PySharpLib.Runtime;
namespace PySharpLib.Modules;

/// <summary>Native IEEE decimal constructors, resolved on the active runtime without a compile-time dependency on .NET 11.</summary>
public static class MathemaModule
{
    public static PyModule Create()
    {
        var module=new PyModule("mathema");
        foreach(var bits in new[]{32,64,128})
        {
            string name="decimal"+bits;
            module.Dict[name]=new PyBuiltinFunction(name,(_,args,kwargs)=>
            {
                if(kwargs is {Count:>0} || args.Length!=1) throw PyErr.TypeError(name+" expects one positional string or integer");
                var type=typeof(object).Assembly.GetType("System.Numerics.Decimal"+bits)
                    ?? AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("System.Numerics.Decimal"+bits)).FirstOrDefault(t=>t is not null);
                if(type is null)throw PyErr.RuntimeError(name+" requires a runtime providing .NET 11 IEEE Decimal types");
                if(args[0] is ClrObject wrapped && wrapped.Type==type)return wrapped;
                string value=args[0] switch
                {
                    string s=>s,
                    BigInteger n=>n.ToString(CultureInfo.InvariantCulture),
                    _=>throw PyErr.TypeError(name+" accepts a string, integer or same-type CLR value; float conversion must be explicit")
                };
                var parse=type.GetMethod("Parse",BindingFlags.Public|BindingFlags.Static,null,[typeof(string),typeof(IFormatProvider)],null)
                    ?? throw PyErr.RuntimeError("Runtime decimal parser unavailable");
                try { return ClrMarshal.ToPython(parse.Invoke(null,[value,CultureInfo.InvariantCulture])); }
                catch(TargetInvocationException e) when(e.InnerException is FormatException)
                { throw PyErr.ValueError("Invalid "+name+" literal: "+value); }
                catch(TargetInvocationException e) when(e.InnerException is OverflowException)
                { throw PyErr.OverflowError("Value outside "+name+" range"); }
            });
        }
        return module;
    }
}
""");
var path=Path.Combine(root,"src","PySharpLib","Modules","StdlibModules.cs");var text=File.ReadAllText(path);
var marker="importer.RegisterBuiltin(\"decimal\", _ => DecimalModule.Create());";
if(!text.Contains(marker))throw new Exception("Registration missing");
if(!text.Contains("importer.RegisterBuiltin(\"mathema\"")) File.WriteAllText(path,text.Replace(marker,marker+"\n        importer.RegisterBuiltin(\"mathema\", _ => MathemaModule.Create());"));
File.WriteAllText(Path.Combine(root,"src","PySharp.Tests","M11_Interop","MathemaModuleTests.cs"),""""
using PySharpLib;
using Xunit;
public class MathemaModuleTests
{
    private static string Run(string source){var output=new StringWriter();new PyEngine(output).Run(source);return output.ToString().Trim();}
    [Fact] public void ConstructorsAndOperators()=>Assert.Equal("OK",Run("""
from mathema import decimal32, decimal64, decimal128
for make in [decimal32, decimal64, decimal128]:
    assert make("1.5") * make(2) == make(3)
    assert make(make(2)) == make(2)
x = decimal128("1.0000000000000000000000001")
assert x - 1 == decimal128("1e-25")
print("OK")
"""));
    [Fact] public void FloatInputRejected()=>Assert.Equal("rejected",Run("""
from mathema import decimal128
try:
    decimal128(0.1)
except TypeError:
    print("rejected")
"""));
    [Fact] public void InvalidLiteralRejected()=>Assert.Equal("invalid",Run("""
from mathema import decimal128
try:
    decimal128("not-a-number")
except ValueError:
    print("invalid")
"""));
    [Fact] public void ArityRejected()=>Assert.Equal("arity",Run("""
from mathema import decimal128
try:
    decimal128()
except TypeError:
    print("arity")
"""));
}
"""");
Console.WriteLine("mathema module and four tests added.");
