#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp";
void Write(string p,string text) => File.WriteAllText(Path.Combine(root,p),text);
Write("src/PySharpLib/Interpretation/InterpFrames.cs", """
using System.Numerics;
using PySharpLib.Runtime;
namespace PySharpLib.Interpretation;
public sealed partial class Interp
{
    /// <summary>Expose the interpreter's actual thread-local stack, innermost first.</summary>
    public static object GetPythonFrame(int depth)
    {
        var frames = _frames?.ToArray() ?? [];
        if (depth < 0) depth = 0;
        if (depth >= frames.Length) throw PyErr.ValueError("call stack is not deep enough");
        object back = PyNone.Instance;
        for (int i = frames.Length - 1; i >= depth; i--)
        {
            var frame = frames[i];
            var value = new PyInstance(new PyClass("frame", new List<PyClass>()));
            var locals = frame.Env.Module.Dict;
            if (!frame.Env.IsGlobalScope)
            {
                locals = new PyDict();
                foreach (var pair in frame.Env.Locals) locals[pair.Key] = pair.Value;
            }
            var code = new PyInstance(new PyClass("code", new List<PyClass>()));
            code.Dict["co_name"] = frame.Name;
            code.Dict["co_filename"] = frame.File;
            value.Dict["f_globals"] = frame.Env.Module.Dict;
            value.Dict["f_locals"] = locals;
            value.Dict["f_lineno"] = new BigInteger(frame.Line);
            value.Dict["f_code"] = code;
            value.Dict["f_back"] = back;
            back = value;
        }
        return back;
    }
}
""");
var p=Path.Combine(root,"src/PySharpLib/Modules/SysModule.cs");
var text=File.ReadAllText(p);
var anchor="        d[\"maxunicode\"] = new BigInteger(0x10FFFF);";
if(!text.Contains("d[\"_getframe\"]")) text=text.Replace(anchor,anchor+"""

        d["_getframe"] = new PyBuiltinFunction("_getframe", (_, args, kwargs) =>
        {
            if (args.Length > 1 || kwargs is { Count: > 0 })
                throw PyErr.TypeError("_getframe() accepts at most one positional argument");
            if (args.Length == 0) return Interp.GetPythonFrame(0);
            var depth = args[0] switch
            {
                BigInteger n => n,
                bool b => new BigInteger(b ? 1 : 0),
                _ => throw PyErr.TypeError("depth must be an integer")
            };
            if (depth > int.MaxValue) throw PyErr.ValueError("call stack is not deep enough");
            return Interp.GetPythonFrame(depth < 0 ? 0 : (int)depth);
        });
""");
File.WriteAllText(p,text);
Write("src/PySharp.Tests/SysFrameTests.cs", """"
using PySharpLib;
namespace PySharp.Tests;
public class SysFrameTests
{
    [Fact]
    public void Frames_expose_real_caller_locals_and_globals()
    {
        var output = new StringWriter();
        new PyEngine(output).Run("""
import sys
marker = 17
def inner():
    local = 23
    f = sys._getframe()
    print(f.f_locals['local'], f.f_globals['marker'], f.f_code.co_name)
    print(sys._getframe(1).f_code.co_name)
    print(f.f_back.f_code.co_name)
def outer():
    inner()
outer()
""");
        Assert.Equal("23 17 inner\nouter\nouter\n", output.ToString());
    }
    [Fact]
    public void Invalid_depth_and_argument_are_python_errors()
    {
        var output = new StringWriter();
        new PyEngine(output).Run("""
import sys
try:
    sys._getframe(100000)
except ValueError:
    print('depth')
try:
    sys._getframe(1.5)
except TypeError:
    print('type')
print(sys._getframe().f_code.co_name)
""");
        Assert.Equal("depth\ntype\n<module>\n", output.ToString());
    }
}
"""");
