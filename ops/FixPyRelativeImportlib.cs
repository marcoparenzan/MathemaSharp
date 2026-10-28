#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp";
var p=Path.Combine(root,"src/PySharpLib/Modules/ImportlibModule.cs");var t=File.ReadAllText(p);
var old="""
        m.Dict["import_module"] = new PyBuiltinFunction("import_module", (interp, a, _) =>
        {
            string name = (string)a[0];
            if (name.StartsWith('.'))
                throw PyErr.NotImplementedError("importlib.import_module: relative names are not supported yet");
            return importer.Import(interp, name, 0, null!);
        });
""";
var replacement="""
        m.Dict["import_module"] = new PyBuiltinFunction("import_module", (interp, a, kw) =>
        {
            if(a.Length is < 1 or > 2 || a[0] is not string name) throw PyErr.TypeError("import_module requires a name string");
            if (name.StartsWith('.'))
            {
                var package = a.Length>1?a[1]:kw is not null && kw.TryGetValue("package",out var v)?v:PyNone.Instance;
                if(package is not string pkg || pkg.Length==0) throw PyErr.TypeError("package is required for relative import");
                int level=name.TakeWhile(c=>c=='.').Count();
                var parts=pkg.Split('.');
                if(level>parts.Length)throw PyErr.ImportError("attempted relative import beyond top-level package");
                var prefix=string.Join(".",parts.Take(parts.Length-level+1));
                var suffix=name[level..];
                name=prefix+(suffix.Length>0?"."+suffix:"");
            }
            return importer.Import(interp, name, 0, null!);
        });
""";
if(!t.Contains(old))throw new Exception("Anchor missing");File.WriteAllText(p,t.Replace(old,replacement));
File.WriteAllText(Path.Combine(root,"src/PySharp.Tests/RelativeImportlibTests.cs"),""""
using PySharpLib;
namespace PySharp.Tests;
public class RelativeImportlibTests
{
    [Fact]
    public void Relative_imports_resolve_package_and_preserve_module_identity()
    {
        var dir=Path.Combine(Path.GetTempPath(),"pysharp-relative-"+Guid.NewGuid());
        Directory.CreateDirectory(Path.Combine(dir,"probe_pkg","nested"));
        try
        {
            File.WriteAllText(Path.Combine(dir,"probe_pkg","__init__.py"),"");
            File.WriteAllText(Path.Combine(dir,"probe_pkg","nested","__init__.py"),"");
            File.WriteAllText(Path.Combine(dir,"probe_pkg","value.py"),"value = 42\n");
            var output=new StringWriter();var engine=new PyEngine(output);
            engine.Importer.SearchPaths.Add(dir);
            engine.Run("""
from importlib import import_module
import probe_pkg.value
one = import_module('.value', package='probe_pkg')
two = import_module('..value', 'probe_pkg.nested')
print(one.value, one is two, one is probe_pkg.value)
try:
    import_module('..value', 'probe_pkg')
except ImportError:
    print('beyond')
try:
    import_module('.value')
except TypeError:
    print('package')
""");
            Assert.Equal("42 True True\nbeyond\npackage\n",output.ToString());
        }
        finally {Directory.Delete(dir,true);}
    }
}
"""");
