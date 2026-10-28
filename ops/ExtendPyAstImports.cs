#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp";
var p=Path.Combine(root,"src/PySharpLib/Modules/AstPublic.cs");
var t=File.ReadAllText(p);
t=t.Replace("[\"Load\"]=[],", "[\"Import\"]=[\"names\"], [\"ImportFrom\"]=[\"module\",\"names\",\"level\"], [\"alias\"]=[\"name\",\"asname\"],\n        [\"Load\"]=[],");
t=t.Replace("            AssignStmt a=>", """
            ImportStmt i=>Make("Import",new PyList(i.Names.Select(a=>(object)Make("alias",a.DottedName,(object?)a.AsName??PyNone.Instance)))),
            FromImportStmt i=>Make("ImportFrom",string.IsNullOrEmpty(i.Module)?PyNone.Instance:(object)i.Module,
                new PyList((i.Star?new List<ImportAlias>{new("*",null)}:i.Names).Select(a=>(object)Make("alias",a.DottedName,(object?)a.AsName??PyNone.Instance))),new BigInteger(i.Level)),
            AssignStmt a=>
""");
t=t.Replace("            \"Assign\"=>", """
            "Import"=>new ImportStmt(ReadAliases(n)),
            "ImportFrom"=>new FromImportStmt(Get(n,"module") is PyNone?"":(string)Get(n,"module"),(int)(BigInteger)Get(n,"level"),ReadAliases(n),ReadAliases(n).Any(a=>a.DottedName=="*")),
            "Assign"=>
""");
var anchor="    public static PyCompiledCode CompilePublic";
t=t.Replace(anchor,"""
    private static List<ImportAlias> ReadAliases(PyInstance n)
        => ((PyList)Get(n,"names")).Items.Select(v =>
        {
            if(v is not PyInstance a || a.Class.Name != "alias") throw PyErr.TypeError("invalid import alias");
            return new ImportAlias((string)Get(a,"name"),Get(a,"asname") is PyNone?null:(string)Get(a,"asname"));
        }).ToList();

"""+anchor);
File.WriteAllText(p,t);
File.WriteAllText(Path.Combine(root,"src/PySharp.Tests/AstImportTests.cs"),""""
using PySharpLib;
namespace PySharp.Tests;
public class AstImportTests
{
    [Fact]
    public void Import_alias_mutations_are_used_by_compilation()
    {
        var output=new StringWriter();
        new PyEngine(output).Run("""
import ast
node = ast.parse('import math as old\nfrom math import sqrt as root')
print(isinstance(node.body[0], ast.Import), isinstance(node.body[1], ast.ImportFrom))
node.body[0].names[0].asname = 'renamed'
node.body[1].names[0].asname = 'square_root'
scope = {}
exec(compile(node, 'imports.py', 'exec'), scope)
print(scope['renamed'].__name__, scope['square_root'](9))
print('old' in scope, 'root' in scope)
""");
        Assert.Equal("True True\nmath 3.0\nFalse False\n",output.ToString());
    }
}
"""");
