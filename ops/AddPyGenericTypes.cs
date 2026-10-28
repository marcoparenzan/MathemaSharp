#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp";
var p=Path.Combine(root,"src","PySharpLib","Interpretation","Interp.cs");var t=File.ReadAllText(p);
var anchor="        if (obj is ClrMethod method) return ClrBinder.SelectGeneric(method, index);";
if(!t.Contains(anchor))throw new Exception("Generic binding anchor missing");
t=t.Replace(anchor,anchor+"\n        if (obj is ClrType type) return ClrBinder.SelectGenericType(type, index);");File.WriteAllText(p,t);
p=Path.Combine(root,"src","PySharpLib","Runtime","Clr.cs");t=File.ReadAllText(p);
var marker="    /// <summary>Construct a CLR instance (ClrType called like a function).</summary>";
t=t.Replace(marker,"""
    /// <summary>Close a CLR generic type explicitly; no type inference or numeric conversions.</summary>
    public static ClrType SelectGenericType(ClrType type, object index)
    {
        if(!type.Type.IsGenericTypeDefinition) throw PyErr.TypeError("CLR type is not an open generic definition");
        var values=index is PyTuple tuple?tuple.Items.ToArray():new[]{index};
        if(values.Length!=type.Type.GetGenericArguments().Length || values.Any(v=>v is not ClrType))
            throw PyErr.TypeError("Wrong generic type arity or non-CLR type argument");
        try { return new ClrType(type.Type.MakeGenericType(values.Cast<ClrType>().Select(v=>v.Type).ToArray())); }
        catch(ArgumentException e) { throw PyErr.TypeError("CLR generic constraints failed: "+e.Message); }
    }

"""+marker);File.WriteAllText(p,t);
File.WriteAllText(Path.Combine(root,"src","PySharp.Tests","M11_Interop","GenericTypeTests.cs"),""""
using System.Numerics;
using PySharpLib;
using PySharpLib.Runtime;
namespace PySharp.Tests.M11_Interop;
public class GenericTypeTests
{
    [Fact] public void ClosedClrListPreservesDecimal128InTheInterpreter()
    {
        var output=new StringWriter();var engine=new PyEngine(output);
        engine.SetVariable("ClrList",typeof(List<>));engine.SetVariable("D128",typeof(Decimal128));
        engine.Run("""
from mathema import decimal128
List128 = ClrList[D128]
values = List128()
values.Add(decimal128('1.0000000000000000000000001'))
values.Add(decimal128('1e-25'))
print(values.Count)
print(values[0] - 1 == values[1])
""");
        Assert.Equal("2\nTrue\n",output.ToString());
    }
    [Fact] public void GenericTypeConstraintsAndArityAreChecked()
    {
        Assert.ThrowsAny<Exception>(()=>ClrBinder.SelectGenericType(new ClrType(typeof(Nullable<>)),new ClrType(typeof(string))));
        Assert.ThrowsAny<Exception>(()=>ClrBinder.SelectGenericType(new ClrType(typeof(Dictionary<,>)),new ClrType(typeof(int))));
        Assert.ThrowsAny<Exception>(()=>ClrBinder.SelectGenericType(new ClrType(typeof(int)),new ClrType(typeof(int))));
    }
}
"""");
