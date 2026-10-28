#:property TargetFramework=net11.0
using System.Reflection;
using System.Collections;
var assembly=Assembly.LoadFrom(@"D:\Dev\BuildArtifacts\PySharp\bin\PySharpLib\release\PySharpLib.dll");
var parse=assembly.GetType("PySharpLib.Parsing.Parser")!.GetMethod("Parse",BindingFlags.Public|BindingFlags.Static)!;
var node=assembly.GetType("PySharpLib.Parsing.Node")!;
var root=@"D:\Dev\Science\PySharp\site-packages";
var counts=new SortedDictionary<string,int>();var errors=new List<string>();int files=0;
void Walk(object? value) {
 if(value is null || value is string || value.GetType().IsPrimitive)return;
 if(node.IsInstanceOfType(value)) { var name=value.GetType().Name;counts[name]=counts.GetValueOrDefault(name)+1; }
 if(value is IEnumerable seq) {foreach(var v in seq)Walk(v);return;}
 if(value.GetType().Namespace!="PySharpLib.Parsing")return;
 foreach(var p in value.GetType().GetProperties().Where(p=>p.Name is not ("Line" or "Col" or "EqualityContract")))Walk(p.GetValue(value));
}
foreach(var name in new[]{"fastapi","starlette","anyio","pydantic"}) {
var dir=Path.Combine(root,name);if(!Directory.Exists(dir))continue;
foreach(var path in Directory.EnumerateFiles(dir,"*.py",SearchOption.AllDirectories)) {
try {Walk(parse.Invoke(null,new object[]{File.ReadAllText(path),path}));files++;}catch(Exception e){errors.Add(Path.GetRelativePath(root,path)+": "+e.Message);}
}}
var text=$"Parsed files: {files}\n"+string.Join("\n",counts.Select(p=>$"{p.Key}: {p.Value}"))+"\nParse gaps:\n"+string.Join("\n",errors);
Directory.CreateDirectory(@"D:\Dev\BuildArtifacts\net11-migration");File.WriteAllText(@"D:\Dev\BuildArtifacts\net11-migration\ast-inventory.txt",text);Console.WriteLine(text);
