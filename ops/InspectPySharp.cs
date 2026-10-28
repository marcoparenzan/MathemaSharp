#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp";
Console.WriteLine("Exists: "+Directory.Exists(root));
if(!Directory.Exists(root)) { foreach(var d in Directory.GetDirectories(@"D:\Dev\Science"))Console.WriteLine(d); return; }
bool Active(string p)=>!p.Split(Path.DirectorySeparatorChar).Any(s=>new[]{"bin","obj",".git",".ralfai","node_modules","artifacts"}.Contains(s));
foreach(var p in Directory.EnumerateFiles(root,"*.csproj",SearchOption.AllDirectories).Where(Active))Console.WriteLine("PROJECT "+Path.GetRelativePath(root,p));
foreach(var p in Directory.EnumerateFiles(root,"*",SearchOption.AllDirectories).Where(Active).Where(p=>new[]{".cs",".csproj",".props",".json"}.Contains(Path.GetExtension(p)))) {
 int line=0;foreach(var text in File.ReadLines(p)){line++;if(text.Contains("Mathema",StringComparison.OrdinalIgnoreCase)||text.Contains("Numbers.TimeSeries")||text.Contains("class Clr")||text.Contains("KernelPath"))Console.WriteLine($"{Path.GetRelativePath(root,p)}:{line}: {text.Trim()}");}
}
