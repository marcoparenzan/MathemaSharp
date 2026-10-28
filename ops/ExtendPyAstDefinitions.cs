#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp";
var p=Path.Combine(root,"src/PySharpLib/Modules/AstPublic.cs");var t=File.ReadAllText(p);
t=t.Replace("[\"Load\"]=[],", """
["Dict"]=["keys","values"], ["Set"]=["elts"], ["Slice"]=["lower","upper","step"], ["IfExp"]=["test","body","orelse"],
        ["FunctionDef"]=["name","args","body","decorator_list","returns"], ["AsyncFunctionDef"]=["name","args","body","decorator_list","returns"],
        ["ClassDef"]=["name","bases","keywords","body","decorator_list"],
        ["arguments"]=["posonlyargs","args","vararg","kwonlyargs","kw_defaults","kwarg","defaults"], ["arg"]=["arg","annotation"],
        ["Load"]=[],
""");
t=t.Replace("            IfStmt i=>", """
            DictExpr d=>Make("Dict",new PyList(d.Items.Select(p=>p.Key is null?PyNone.Instance:Export(p.Key))),new PyList(d.Items.Select(p=>Export(p.Value)))),
            SetExpr s=>Make("Set",new PyList(s.Items.Select(e=>Export(e)))),
            SliceExpr s=>Make("Slice",Optional(s.Start),Optional(s.Stop),Optional(s.Step)),
            IfExpExpr i=>Make("IfExp",Export(i.Cond),Export(i.Then),Export(i.Else)),
            FuncDef f=>Make(f.IsAsync?"AsyncFunctionDef":"FunctionDef",f.Name,ExportParameters(f.Params),ExportBody(f.Body),new PyList(f.Decorators.Select(e=>Export(e))),Optional(f.Returns)),
            ClassDef c when c.Bases.All(a=>!a.IsStar&&!a.IsDoubleStar)=>Make("ClassDef",c.Name,
                new PyList(c.Bases.Where(a=>a.Name is null).Select(a=>Export(a.Value))),
                new PyList(c.Bases.Where(a=>a.Name is not null).Select(a=>(object)Make("keyword",a.Name!,Export(a.Value)))),ExportBody(c.Body),new PyList(c.Decorators.Select(e=>Export(e)))),
            IfStmt i=>
""");
t=t.Replace("            \"If\"=>", """
            "Dict"=>ImportDict(n),
            "Set"=>new SetExpr(Expressions(n,"elts")),
            "Slice"=>new SliceExpr(OptionalExpression(n,"lower"),OptionalExpression(n,"upper"),OptionalExpression(n,"step")),
            "IfExp"=>new IfExpExpr(Expression(n,"test"),Expression(n,"body"),Expression(n,"orelse")),
            "FunctionDef" or "AsyncFunctionDef"=>new FuncDef((string)Get(n,"name"),ImportParameters((PyInstance)Get(n,"args")),Body(n,"body"),Expressions(n,"decorator_list"),OptionalExpression(n,"returns"),false) { IsAsync=n.Class.Name=="AsyncFunctionDef" },
            "ClassDef"=>new ClassDef((string)Get(n,"name"),ReadClassBases(n),Body(n,"body"),Expressions(n,"decorator_list")),
            "If"=>
""");
var anchor="    private static PyList ExportBody";
t=t.Replace(anchor,"""
    private static object Optional(Expr? e)=>e is null?PyNone.Instance:Export(e);
    private static Expr? OptionalExpression(PyInstance n,string key)=>Get(n,key) is PyNone?null:Expression(n,key);
    private static DictExpr ImportDict(PyInstance n)
    {
        var keys=((PyList)Get(n,"keys")).Items;
        var values=Expressions(n,"values");
        if(keys.Count!=values.Count)throw PyErr.TypeError("Dict keys and values length mismatch");
        return new DictExpr(keys.Select((k,i)=>(k is PyNone?null:(Expr?)Import((PyInstance)k),values[i])).ToList());
    }
    private static object ExportParameters(Parameters p)
    {
        object arg(Param a)=>Make("arg",a.Name,Optional(a.Annotation));
        return Make("arguments",new PyList(),new PyList(p.Positional.Select(arg)),
            string.IsNullOrEmpty(p.StarArgs)?PyNone.Instance:Make("arg",p.StarArgs,PyNone.Instance),
            new PyList(p.KwOnly.Select(arg)),new PyList(p.KwOnly.Select(a=>Optional(a.Default))),
            p.KwArgs is null?PyNone.Instance:Make("arg",p.KwArgs,PyNone.Instance),
            new PyList(p.Positional.Where(a=>a.Default is not null).Select(a=>Export(a.Default!))));
    }
    private static Parameters ImportParameters(PyInstance p)
    {
        if(((PyList)Get(p,"posonlyargs")).Items.Count!=0)throw PyErr.NotImplementedError("positional-only AST parameters");
        var args=((PyList)Get(p,"args")).Items.Cast<PyInstance>().ToList();
        var defaults=Expressions(p,"defaults");
        if(defaults.Count>args.Count)throw PyErr.TypeError("too many parameter defaults");
        Param param(PyInstance a,Expr? def)=>new((string)Get(a,"arg"),def,OptionalExpression(a,"annotation"));
        var pos=args.Select((a,i)=>param(a,i>=args.Count-defaults.Count?defaults[i-(args.Count-defaults.Count)]:null)).ToList();
        var kw=((PyList)Get(p,"kwonlyargs")).Items.Cast<PyInstance>().ToList();
        var kd=((PyList)Get(p,"kw_defaults")).Items;
        if(kw.Count!=kd.Count)throw PyErr.TypeError("keyword defaults length mismatch");
        var kwargs=kw.Select((a,i)=>param(a,kd[i] is PyNone?null:(Expr)Import((PyInstance)kd[i]))).ToList();
        string? name(string field)=>Get(p,field) is PyNone?null:(string)Get((PyInstance)Get(p,field),"arg");
        return new Parameters(pos,name("vararg")??(kw.Count>0?"":null),kwargs,name("kwarg"));
    }
    private static List<CallArg> ReadClassBases(PyInstance n)
    {
        var args=Expressions(n,"bases").Select(e=>new CallArg(null,e,false,false)).ToList();
        foreach(var item in ((PyList)Get(n,"keywords")).Items)
        {
            var k=(PyInstance)item;
            args.Add(new CallArg((string)Get(k,"arg"),Expression(k,"value"),false,false));
        }
        return args;
    }

"""+anchor);
// Reject generators rather than silently dropping their IsGenerator flag.
t=t.Replace("FuncDef f=>Make", "FuncDef f when !f.IsGenerator=>Make");
File.WriteAllText(p,t);
File.WriteAllText(Path.Combine(root,"src/PySharp.Tests/AstDefinitionsTests.cs"),""""
using PySharpLib;
namespace PySharp.Tests;
public class AstDefinitionsTests
{
    [Fact]
    public void Functions_classes_defaults_and_collections_roundtrip()
    {
        var output=new StringWriter();
        new PyEngine(output).Run("""
import ast
source = '''def compute(x, scale=2):
    return {'result': x * scale}
class Example:
    def method(self, value):
        return compute(value)
values = [1, 2, 3, 4]
part = values[1:3]
chosen = 1 if True else 0
'''
node = ast.parse(source)
node.body[0].args.defaults[0].value = 3
scope = {}
exec(compile(node, 'definitions.py', 'exec'), scope)
print(scope['compute'](4)['result'])
print(scope['Example']().method(5)['result'])
print(scope['part'], scope['chosen'])
expr = ast.parse("{'x': 2, 'y': 3}", mode='eval')
expr.body.values[0].value = 9
print(eval(compile(expr, 'dict.py', 'eval'))['x'])
""");
        Assert.Equal("12\n15\n[2, 3] 1\n9\n",output.ToString());
    }
}
"""");
