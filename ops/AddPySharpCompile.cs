#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp";
void Write(string p,string t)=>File.WriteAllText(Path.Combine(root,p),t);
Write("src/PySharpLib/Runtime/PyCompiledCode.cs","""
using PySharpLib.Parsing;
namespace PySharpLib.Runtime;
/// <summary>Parsed executable source, distinct from function signature metadata PyCode.</summary>
public sealed record PyCompiledCode(string FileName, string Mode, Module? Statements, Expr? Expression);
""");
var p=Path.Combine(root,"src/PySharpLib/Builtins/Builtins.cs");
var t=File.ReadAllText(p);
var anchor="        Add(\"eval\", (interp, args, kwargs) =>";
t=t.Replace(anchor,"""
        Add("compile", (_, args, kwargs) =>
        {
            if (args.Length < 3 || args.Length > 6 || args[0] is not string source || args[1] is not string filename || args[2] is not string mode)
                throw PyErr.TypeError("compile() requires source, filename and mode strings");
            if (mode is not ("exec" or "eval")) throw PyErr.NotImplementedError("compile supports exec and eval modes");
            object flags = args.Length > 3 ? args[3] : kwargs is not null && kwargs.TryGetValue("flags", out var f) ? f : BigInteger.Zero;
            if (flags is not BigInteger flagValue) throw PyErr.TypeError("flags must be an integer");
            if (flagValue != 0) throw PyErr.NotImplementedError("compile flags are not supported");
            if (args.Length > 5 || kwargs is not null && kwargs.ContainsKey("optimize"))
                throw PyErr.NotImplementedError("compile optimization is not supported");
            if (kwargs is not null && kwargs.Keys.Any(k => k is not ("flags" or "dont_inherit" or "optimize")))
                throw PyErr.TypeError("unexpected compile keyword argument");
            try
            {
                return mode == "exec"
                    ? new PyCompiledCode(filename, mode, Parsing.Parser.Parse(source, filename), null)
                    : new PyCompiledCode(filename, mode, null, Parsing.Parser.ParseExpression(source));
            }
            catch (PySyntaxError ex) { throw PyErr.SyntaxLike(ex.Message); }
        });

"""+anchor);
t=t.Replace("""
            if (args.Length == 0 || args[0] is not string source)
                throw PyErr.TypeError("eval() arg 1 must be a string");
            var expr = Parsing.Parser.ParseExpression(source);
""","""
            if (args.Length == 0) throw PyErr.TypeError("eval() requires a source or code object");
            var expr = args[0] switch
            {
                string source => Parsing.Parser.ParseExpression(source),
                PyCompiledCode { Expression: not null } code => code.Expression,
                _ => throw PyErr.TypeError("eval() requires a string or eval code object")
            };
""");
t=t.Replace("""
            if (args.Length == 0 || args[0] is not string source)
                throw PyErr.TypeError("exec() arg 1 must be a string");
            var moduleAst = Parsing.Parser.Parse(source, "<string>");
""","""
            if (args.Length == 0) throw PyErr.TypeError("exec() requires source or code");
            var filename = args[0] is PyCompiledCode compiled ? compiled.FileName : "<string>";
            var moduleAst = args[0] switch
            {
                string source => Parsing.Parser.Parse(source, filename),
                PyCompiledCode { Statements: not null } code => code.Statements,
                _ => throw PyErr.TypeError("exec() requires a string or exec code object")
            };
""");
t=t.Replace("new PyModule(\"<string>\", globalsDict) { Builtins = interp.BuiltinsModule };\n\n            object? localsArg = args.Length > 2 ? args[2]", "new PyModule(\"<string>\", globalsDict) { Builtins = interp.BuiltinsModule };\n\n            object? localsArg = args.Length > 2 ? args[2]");
t=t.Replace("var execModule = new PyModule(\"<string>\", globalsDict) { Builtins = interp.BuiltinsModule };", "var execModule = new PyModule(\"<string>\", globalsDict) { Builtins = interp.BuiltinsModule, FileName = filename };");
File.WriteAllText(p,t);
p=Path.Combine(root,"src/PySharpLib/Modules/InspectModule.cs");t=File.ReadAllText(p).Replace("a[0] is PyCode)","a[0] is PyCode or PyCompiledCode)");File.WriteAllText(p,t);
Write("src/PySharp.Tests/CompileTests.cs",""""
using PySharpLib;
namespace PySharp.Tests;
public class CompileTests
{
    [Fact]
    public void Code_is_parsed_once_and_reused_with_distinct_namespaces()
    {
        var output=new StringWriter();
        new PyEngine(output).Run("""
import inspect
code = compile('result = value * 2', 'generated.py', 'exec')
a = {'value': 3}
b = {'value': 7}
exec(code, a)
exec(code, b)
print(a['result'], b['result'], inspect.iscode(code))
expr = compile('value + 1', 'expression.py', 'eval')
print(eval(expr, a), eval(expr, b))
locals_dict = {}
exec(code, a, locals_dict)
print(locals_dict['result'])
""");
        Assert.Equal("6 14 True\n4 8\n6\n",output.ToString());
    }
    [Fact]
    public void Compile_reports_syntax_errors_and_unsupported_flags()
    {
        var output=new StringWriter();
        new PyEngine(output).Run("""
try:
    compile('def broken(', 'broken.py', 'exec')
except SyntaxError:
    print('syntax')
try:
    compile('x = 1', 'flags.py', 'exec', 1)
except NotImplementedError:
    print('flags')
""");
        Assert.Equal("syntax\nflags\n",output.ToString());
    }
}
"""");
