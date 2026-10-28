#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp";
var p=Path.Combine(root,"src/PySharpLib/Modules/InspectModule.cs");
var text=File.ReadAllText(p).Replace("public static class InspectModule", "public static partial class InspectModule");
var anchor="        d[\"Parameter\"] = ParameterClass;";
if(!text.Contains("d[\"getsource\"]")) text=text.Replace(anchor,"""
        d["getsource"] = new PyBuiltinFunction("getsource", (_, a, kw) =>
        {
            if (a.Length != 1 || kw is { Count: > 0 }) throw PyErr.TypeError("getsource() requires one positional argument");
            return ReadSource(a[0]);
        });

"""+anchor);
File.WriteAllText(p,text);
File.WriteAllText(Path.Combine(root,"src/PySharpLib/Modules/InspectSource.cs"),"""
using System.Text.RegularExpressions;
using PySharpLib.Runtime;
namespace PySharpLib.Modules;
public static partial class InspectModule
{
    private static string ReadSource(object target)
    {
        target = UnwrapBoundMethod(target);
        var module = target switch
        {
            PyModule m => m,
            PyFunction f => f.Module,
            _ => throw PyErr.TypeError("source inspection supports Python modules, functions and methods")
        };
        string source;
        try { source = File.ReadAllText(module.FileName).Replace("\r\n", "\n"); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException)
        { throw PyErr.OSError("could not get source code"); }
        if (target is PyModule) return source;
        var fn = (PyFunction)target;
        if (fn.LambdaBody is not null) throw PyErr.OSError("lambda source extraction is not supported");
        var lines = source.Split('\n');
        int bodyLine = fn.Body is { Count: > 0 } ? fn.Body[0].Line : lines.Length;
        var declaration = new Regex(@"^(\s*)(?:async\s+)?def\s+" + Regex.Escape(fn.Name) + @"\s*\(");
        int start = -1;
        for (int i = Math.Min(bodyLine - 1, lines.Length - 1); i >= 0; i--)
            if (declaration.IsMatch(lines[i])) { start = i; break; }
        if (start < 0) throw PyErr.OSError("could not locate function source");
        int indent = lines[start].TakeWhile(char.IsWhiteSpace).Count();
        int definition = start;
        while (start > 0 && lines[start - 1].TrimStart().StartsWith('@') && lines[start - 1].TakeWhile(char.IsWhiteSpace).Count() == indent) start--;
        int end = definition + 1;
        // File-backed indentation blocks; multiline string/continuation edge cases remain outside this first extraction path.
        while (end < lines.Length)
        {
            var line = lines[end];
            if (!string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith('#') && line.TakeWhile(char.IsWhiteSpace).Count() <= indent) break;
            end++;
        }
        while (end > definition + 1 && string.IsNullOrWhiteSpace(lines[end - 1])) end--;
        return string.Join("\n", lines[start..end]) + "\n";
    }
}
""");
File.WriteAllText(Path.Combine(root,"src/PySharp.Tests/InspectSourceTests.cs"),""""
using PySharpLib;
namespace PySharp.Tests;
public class InspectSourceTests
{
    [Fact]
    public void Source_is_read_from_real_module_and_function_file()
    {
        var dir = Path.Combine(Path.GetTempPath(), "pysharp-source-" + Guid.NewGuid());
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "source_probe.py"), "def sample():\n    return 42\n\nclass Example:\n    def method(self):\n        return 7\n");
            var output = new StringWriter();
            var engine = new PyEngine(output);
            engine.Importer.SearchPaths.Add(dir);
            engine.Run("""
import inspect
import source_probe
print(inspect.getsource(source_probe.sample) == 'def sample():\n    return 42\n')
print(inspect.getsource(source_probe.Example().method) == '    def method(self):\n        return 7\n')
print('class Example:' in inspect.getsource(source_probe))
try:
    inspect.getsource(len)
except TypeError:
    print('builtin')
""");
            Assert.Equal("True\nTrue\nTrue\nbuiltin\n", output.ToString());
        }
        finally { Directory.Delete(dir, true); }
    }
    [Fact]
    public void Unavailable_function_source_raises_oserror()
    {
        var output = new StringWriter();
        new PyEngine(output).Run("""
import inspect
def f():
    return 1
try:
    inspect.getsource(f)
except OSError:
    print('unavailable')
""");
        Assert.Equal("unavailable\n", output.ToString());
    }
}
"""");
