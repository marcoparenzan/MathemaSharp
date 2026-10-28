#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp";
var p=Path.Combine(root,"src/PySharpLib/Modules/AstPublic.cs");var t=File.ReadAllText(p);
t=t.Replace("[\"Load\"]=[],", """
["If"]=["test","body","orelse"], ["While"]=["test","body","orelse"], ["For"]=["target","iter","body","orelse"],
        ["Return"]=["value"], ["Pass"]=[], ["Break"]=[], ["Continue"]=[],
        ["Attribute"]=["value","attr","ctx"], ["Call"]=["func","args","keywords"], ["keyword"]=["arg","value"],
        ["Compare"]=["left","ops","comparators"], ["BoolOp"]=["op","values"], ["UnaryOp"]=["op","operand"],
        ["List"]=["elts","ctx"], ["Tuple"]=["elts","ctx"], ["Subscript"]=["value","slice","ctx"],
        ["And"]=[], ["Or"]=[], ["Not"]=[], ["UAdd"]=[], ["USub"]=[], ["Invert"]=[],
        ["Eq"]=[], ["NotEq"]=[], ["Lt"]=[], ["LtE"]=[], ["Gt"]=[], ["GtE"]=[], ["Is"]=[], ["IsNot"]=[], ["In"]=[], ["NotIn"]=[],
        ["Load"]=[],
""");
t=t.Replace("            ImportStmt i=>", """
            IfStmt i=>Make("If",Export(i.Cond),ExportBody(i.Body),ExportBody(i.OrElse)),
            WhileStmt i=>Make("While",Export(i.Cond),ExportBody(i.Body),ExportBody(i.OrElse)),
            ForStmt f when !f.IsAsync=>Make("For",Export(f.Target,true),Export(f.Iter),ExportBody(f.Body),ExportBody(f.OrElse)),
            ReturnStmt r=>Make("Return",r.Value is null?PyNone.Instance:Export(r.Value)),
            PassStmt=>Make("Pass"), BreakStmt=>Make("Break"), ContinueStmt=>Make("Continue"),
            AttributeExpr a=>Make("Attribute",Export(a.Obj),a.Name,Make(store?"Store":"Load")),
            CallExpr c when c.Args.All(a=>!a.IsStar&&!a.IsDoubleStar)=>Make("Call",Export(c.Func),
                new PyList(c.Args.Where(a=>a.Name is null).Select(a=>Export(a.Value))),
                new PyList(c.Args.Where(a=>a.Name is not null).Select(a=>(object)Make("keyword",a.Name!,Export(a.Value))))),
            CompareExpr c=>Make("Compare",Export(c.Left),new PyList(c.Ops.Select(op=>(object)Make(OperatorName(op)))),new PyList(c.Comparators.Select(e=>Export(e)))),
            BoolOpExpr b=>Make("BoolOp",Make(OperatorName(b.Op)),new PyList(b.Values.Select(e=>Export(e)))),
            UnaryExpr u=>Make("UnaryOp",Make(u.Op switch {"+"=>"UAdd","-"=>"USub","~"=>"Invert","not"=>"Not",_=>throw PyErr.NotImplementedError("unary AST")}),Export(u.Operand)),
            ListExpr l=>Make("List",new PyList(l.Items.Select(e=>Export(e,store))),Make(store?"Store":"Load")),
            TupleExpr l=>Make("Tuple",new PyList(l.Items.Select(e=>Export(e,store))),Make(store?"Store":"Load")),
            IndexExpr i=>Make("Subscript",Export(i.Obj),Export(i.Index),Make(store?"Store":"Load")),
            ImportStmt i=>
""");
t=t.Replace("            \"Import\"=>", """
            "If"=>new IfStmt(Expression(n,"test"),Body(n,"body"),Body(n,"orelse")),
            "While"=>new WhileStmt(Expression(n,"test"),Body(n,"body"),Body(n,"orelse")),
            "For"=>new ForStmt(Expression(n,"target"),Expression(n,"iter"),Body(n,"body"),Body(n,"orelse")),
            "Return"=>new ReturnStmt(Get(n,"value") is PyNone?null:Expression(n,"value")),
            "Pass"=>new PassStmt(), "Break"=>new BreakStmt(), "Continue"=>new ContinueStmt(),
            "Attribute"=>new AttributeExpr(Expression(n,"value"),(string)Get(n,"attr")),
            "Call"=>new CallExpr(Expression(n,"func"),ReadCallArgs(n)),
            "Compare"=>new CompareExpr(Expression(n,"left"),((PyList)Get(n,"ops")).Items.Select(o=>OperatorText(((PyInstance)o).Class.Name)).ToList(),Expressions(n,"comparators")),
            "BoolOp"=>new BoolOpExpr(OperatorText(((PyInstance)Get(n,"op")).Class.Name),Expressions(n,"values")),
            "UnaryOp"=>new UnaryExpr(OperatorText(((PyInstance)Get(n,"op")).Class.Name),Expression(n,"operand")),
            "List"=>new ListExpr(Expressions(n,"elts")), "Tuple"=>new TupleExpr(Expressions(n,"elts")),
            "Subscript"=>new IndexExpr(Expression(n,"value"),Expression(n,"slice")),
            "Import"=>
""");
var anchor="    private static List<ImportAlias> ReadAliases";
t=t.Replace(anchor,"""
    private static PyList ExportBody(List<Stmt> body)=>new(body.Select(s=>Export(s)));
    private static List<Stmt> Body(PyInstance n,string key)=>((PyList)Get(n,key)).Items.Select(v=>(Stmt)Import((PyInstance)v)).ToList();
    private static Expr Expression(PyInstance n,string key)=>(Expr)Import((PyInstance)Get(n,key));
    private static List<Expr> Expressions(PyInstance n,string key)=>((PyList)Get(n,key)).Items.Select(v=>(Expr)Import((PyInstance)v)).ToList();
    private static readonly Dictionary<string,string> Operators=new()
    {
        ["=="]="Eq",["!="]="NotEq",["<"]="Lt",["<="]="LtE",[">"]="Gt",[">="]="GtE",
        ["is"]="Is",["is not"]="IsNot",["in"]="In",["not in"]="NotIn",["and"]="And",["or"]="Or"
    };
    private static string OperatorName(string op)=>Operators.TryGetValue(op,out var name)?name:throw PyErr.NotImplementedError("AST operator "+op);
    private static string OperatorText(string name)=>name switch
    {
        "UAdd"=>"+","USub"=>"-","Invert"=>"~","Not"=>"not",
        _=>Operators.FirstOrDefault(p=>p.Value==name).Key??throw PyErr.NotImplementedError("AST operator "+name)
    };
    private static List<CallArg> ReadCallArgs(PyInstance n)
    {
        var args=Expressions(n,"args").Select(e=>new CallArg(null,e,false,false)).ToList();
        foreach(var item in ((PyList)Get(n,"keywords")).Items)
        {
            var k=(PyInstance)item;
            args.Add(new CallArg((string)Get(k,"arg"),Expression(k,"value"),false,false));
        }
        return args;
    }

"""+anchor);
File.WriteAllText(p,t);
File.WriteAllText(Path.Combine(root,"src/PySharp.Tests/AstControlFlowTests.cs"),""""
using PySharpLib;
namespace PySharp.Tests;
public class AstControlFlowTests
{
    [Fact]
    public void Control_flow_and_expression_roundtrip_uses_mutated_condition()
    {
        var output=new StringWriter();
        new PyEngine(output).Run("""
import ast
source = '''total = 0
for value in [1, 2, 3, 4]:
    if value > 2:
        total = total + value
count = 0
while count < 3:
    count = count + 1
result = (total, count)
'''
node = ast.parse(source)
node.body[1].body[0].test.comparators[0].value = 3
scope = {}
exec(compile(node, 'control.py', 'exec'), scope)
print(scope['result'])
expr = ast.parse('not (2 > 3) and len([1, 2]) == 2', mode='eval')
print(eval(compile(expr, 'control.py', 'eval')))
""");
        Assert.Equal("(4, 3)\nTrue\n",output.ToString());
    }
}
"""");
