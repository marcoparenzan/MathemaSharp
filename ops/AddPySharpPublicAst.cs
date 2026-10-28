#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp";
var path=Path.Combine(root,"src/PySharpLib/Modules/AstModule.cs");
var t=File.ReadAllText(path).Replace("public static class AstModule","public static partial class AstModule");
t=t.Replace("        var m = new PyModule(\"ast\");","        var m = new PyModule(\"ast\");\n        RegisterPublicAst(m);");File.WriteAllText(path,t);
File.WriteAllText(Path.Combine(root,"src/PySharpLib/Modules/AstPublic.cs"),"""
using System.Numerics;
using PySharpLib.Parsing;
using PySharpLib.Runtime;
namespace PySharpLib.Modules;
public static partial class AstModule
{
    private static readonly PyClass AstBase = new("AST", new List<PyClass>());
    private static readonly Dictionary<string, PyClass> NodeTypes = new();
    private static readonly object Gate = new();
    private static readonly Dictionary<string,string[]> Fields = new()
    {
        ["Module"]=["body", "type_ignores"], ["Expression"]=["body"], ["Assign"]=["targets","value"],
        ["Expr"]=["value"], ["Name"]=["id","ctx"], ["Constant"]=["value"], ["BinOp"]=["left","op","right"],
        ["Load"]=[], ["Store"]=[], ["Add"]=[], ["Sub"]=[], ["Mult"]=[], ["Div"]=[]
    };
    private static void RegisterPublicAst(PyModule module)
    {
        lock (Gate)
        {
            if (NodeTypes.Count == 0)
            foreach (var (name, fields) in Fields)
            {
                var cls = new PyClass(name, new List<PyClass> { AstBase });
                cls.Dict["_fields"] = new PyTuple(fields.Cast<object>().ToArray());
                cls.Dict["__init__"] = new PyBuiltinFunction(name+".__init__", (_, args, kw) =>
                {
                    var self = (PyInstance)args[0];
                    if (args.Length - 1 > fields.Length) throw PyErr.TypeError("too many AST arguments");
                    for(int i=1;i<args.Length;i++) self.Dict[fields[i-1]]=args[i];
                    if(kw is not null) foreach(var pair in kw) self.Dict[pair.Key]=pair.Value;
                    return PyNone.Instance;
                });
                NodeTypes[name] = cls;
            }
            module.Dict["AST"] = AstBase;
            foreach(var pair in NodeTypes) module.Dict[pair.Key]=pair.Value;
        }
        module.Dict["PyCF_ONLY_AST"]=new BigInteger(1024);
        module.Dict["parse"]=new PyBuiltinFunction("parse", (_, args, kw) =>
        {
            if(args.Length==0 || args[0] is not string source) throw PyErr.TypeError("parse requires source string");
            var file=args.Length>1?(string)args[1]:"<unknown>";
            var mode=args.Length>2?(string)args[2]:kw is not null && kw.TryGetValue("mode",out var m)?(string)m:"exec";
            return ParsePublic(source,file,mode);
        });
    }
    private static PyInstance Make(string name, params object[] values)
    {
        var value=new PyInstance(NodeTypes[name]);
        for(int i=0;i<values.Length;i++) value.Dict[Fields[name][i]]=values[i];
        return value;
    }
    public static object ParsePublic(string source,string filename,string mode)
    {
        // Ensure constructors are available even when compile is used before importing ast.
        RegisterPublicAst(new PyModule("ast"));
        try
        {
            return mode switch
            {
                "exec" => Make("Module",new PyList(Parser.Parse(source,filename).Body.Select(s=>Export(s))),new PyList(Array.Empty<object>())),
                "eval" => Make("Expression",Export(Parser.ParseExpression(source))),
                _ => throw PyErr.NotImplementedError("AST mode is not supported")
            };
        }
        catch(PySyntaxError e) { throw PyErr.SyntaxLike(e.Message); }
    }
    private static object Export(Node node, bool store=false)
    {
        var result = node switch
        {
            AssignStmt a=>Make("Assign",new PyList(a.Targets.Select(x=>Export(x,true))),Export(a.Value)),
            ExprStmt e=>Make("Expr",Export(e.Value)),
            NameExpr n=>Make("Name",n.Id,Make(store?"Store":"Load")),
            IntLit n=>Make("Constant",n.Value), FloatLit n=>Make("Constant",n.Value),
            StrLit s=>Make("Constant",s.Value), BoolLit b=>Make("Constant",b.Value), NoneLit=>Make("Constant",PyNone.Instance),
            BinaryExpr b=>Make("BinOp",Export(b.Left),Make(b.Op switch {"+"=>"Add","-"=>"Sub","*"=>"Mult","/"=>"Div",_=>throw PyErr.NotImplementedError("AST operator not supported")}),Export(b.Right)),
            _=>throw PyErr.NotImplementedError($"Public AST node {node.GetType().Name} is not supported")
        };
        result.Dict["lineno"]=new BigInteger(node.Line);
        result.Dict["col_offset"]=new BigInteger(Math.Max(0,node.Col-1));
        return result;
    }
    private static object Get(PyInstance n,string key) => n.Dict.TryGet(key,out var value)?value:throw PyErr.TypeError("missing AST field "+key);
    private static Node Import(PyInstance n)
    {
        Node result = n.Class.Name switch
        {
            "Assign"=>new AssignStmt(((PyList)Get(n,"targets")).Select(v=>(Expr)Import((PyInstance)v)).ToList(),(Expr)Import((PyInstance)Get(n,"value"))),
            "Expr"=>new ExprStmt((Expr)Import((PyInstance)Get(n,"value"))),
            "Name"=>new NameExpr((string)Get(n,"id")),
            "Constant"=>Get(n,"value") switch { BigInteger i=>new IntLit(i), double d=>new FloatLit(d), string s=>new StrLit(s),bool b=>new BoolLit(b),PyNone=>new NoneLit(),_=>throw PyErr.TypeError("invalid AST constant") },
            "BinOp"=>new BinaryExpr((Expr)Import((PyInstance)Get(n,"left")),((PyInstance)Get(n,"op")).Class.Name switch {"Add"=>"+","Sub"=>"-","Mult"=>"*","Div"=>"/",_=>throw PyErr.NotImplementedError("AST operator")},(Expr)Import((PyInstance)Get(n,"right"))),
            _=>throw PyErr.NotImplementedError("AST node compilation not supported: "+n.Class.Name)
        };
        return result;
    }
    public static PyCompiledCode CompilePublic(PyInstance n,string filename,string mode)
    {
        if(mode=="exec" && n.Class.Name=="Module")
            return new(filename,mode,new Module(((PyList)Get(n,"body")).Select(v=>(Stmt)Import((PyInstance)v)).ToList()),null);
        if(mode=="eval" && n.Class.Name=="Expression") return new(filename,mode,null,(Expr)Import((PyInstance)Get(n,"body")));
        throw PyErr.TypeError("AST root does not match compile mode");
    }
}
""");
path=Path.Combine(root,"src/PySharpLib/Builtins/Builtins.cs");t=File.ReadAllText(path);
t=t.Replace("args[0] is not string source || args[1] is not string filename", "args[1] is not string filename");
t=t.Replace("if (flagValue != 0) throw PyErr.NotImplementedError(\"compile flags are not supported\");", "if (flagValue != 0 && flagValue != 1024) throw PyErr.NotImplementedError(\"compile flags are not supported\");");
t=t.Replace("            try\n            {\n                return mode == \"exec\"", "            if (flagValue == 1024)\n                return args[0] is string astSource ? AstModule.ParsePublic(astSource,filename,mode) : throw PyErr.TypeError(\"AST parsing requires a source string\");\n            if (args[0] is PyInstance astNode) return AstModule.CompilePublic(astNode,filename,mode);\n            if (args[0] is not string source) throw PyErr.TypeError(\"compile requires string or supported AST\");\n            try\n            {\n                return mode == \"exec\"");File.WriteAllText(path,t);
File.WriteAllText(Path.Combine(root,"src/PySharp.Tests/PublicAstTests.cs"),""""
using PySharpLib;
namespace PySharp.Tests;
public class PublicAstTests
{
    [Fact]
    public void Parsed_ast_mutation_changes_compiled_result()
    {
        var output=new StringWriter();
        new PyEngine(output).Run("""
import ast
node = compile('result = 2 + 3', 'ast.py', 'exec', ast.PyCF_ONLY_AST)
print(isinstance(node, ast.Module), isinstance(node.body[0], ast.Assign))
node.body[0].value.right.value = 40
scope = {}
exec(compile(node, 'ast.py', 'exec'), scope)
print(scope['result'])
expr = ast.parse('3 * 4', mode='eval')
expr.body.left.value = 5
print(eval(compile(expr, 'ast.py', 'eval')))
""");
        Assert.Equal("True True\n42\n20\n",output.ToString());
    }
}
"""");
