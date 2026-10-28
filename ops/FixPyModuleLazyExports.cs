#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp";
var p=Path.Combine(root,"src/PySharpLib/Interpretation/Interp.cs");var t=File.ReadAllText(p);
t=t.Replace("if (resolved.Dict.TryGet(alias.DottedName, out var v))", "if (TryGetAttr(resolved, alias.DottedName, out var v))");
var old="""
                if (name == "__dict__")
                {
                    value = module.Dict;
                    return true;
                }
                value = PyNone.Instance;
                return false;
""";
var replacement="""
                if (name == "__dict__")
                {
                    value = module.Dict;
                    return true;
                }
                // PEP 562: module-level __getattr__ resolves lazy exports after normal lookup.
                if (module.Dict.TryGet("__getattr__", out var hook))
                {
                    try
                    {
                        value = Call(hook, new object[] { name });
                        return true;
                    }
                    catch (PyRaise ex) when (PyErr.Matches(ex.Value, PyErr.AttributeErrorClass))
                    {
                        // AttributeError means absence; other errors from the lazy import propagate.
                    }
                }
                value = PyNone.Instance;
                return false;
""";
if(!t.Contains(old))throw new Exception("Module lookup anchor not found");
File.WriteAllText(p,t.Replace(old,replacement));
File.WriteAllText(Path.Combine(root,"src/PySharp.Tests/ModuleLazyExportsTests.cs"),""""
using PySharpLib;
namespace PySharp.Tests;
public class ModuleLazyExportsTests
{
    [Fact]
    public void Lazy_module_exports_support_attributes_from_import_and_missing_names()
    {
        var dir=Path.Combine(Path.GetTempPath(),"pysharp-lazy-"+Guid.NewGuid());
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir,"lazy_probe.py"),"""
existing = 7
def __getattr__(name):
    if name == 'exported':
        return 42
    if name == 'broken':
        raise RuntimeError('loader failed')
    raise AttributeError(name)
""");
            var output=new StringWriter();
            var engine=new PyEngine(output);
            engine.Importer.SearchPaths.Add(dir);
            engine.Run("""
import lazy_probe
from lazy_probe import exported
print(lazy_probe.existing, lazy_probe.exported, exported)
print(hasattr(lazy_probe, 'missing'))
try:
    from lazy_probe import broken
except RuntimeError:
    print('propagated')
""");
            Assert.Equal("7 42 42\nFalse\npropagated\n",output.ToString());
        }
        finally {Directory.Delete(dir,true);}
    }
}
"""");
