#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\MatSharp";
if(!Directory.Exists(root)){Console.WriteLine("NOT FOUND");return;}
bool Active(string p)=>!p.Split(Path.DirectorySeparatorChar).Any(s=>new[]{"bin","obj",".git",".ralfai","node_modules","artifacts"}.Contains(s));
foreach(var p in Directory.GetFiles(root,"*.csproj",SearchOption.AllDirectories).Where(Active)) {Console.WriteLine("PROJECT "+Path.GetRelativePath(root,p));Console.WriteLine(File.ReadAllText(p));}
foreach(var p in Directory.GetFiles(root,"*.cs",SearchOption.AllDirectories).Where(Active)){
 var lines=File.ReadAllLines(p);for(int i=0;i<lines.Length;i++)if(lines[i].Contains("class Clr")||lines[i].Contains("MathemaSharp")||lines[i].Contains("Numbers.")||lines[i].Contains("NDSharp"))Console.WriteLine($"{Path.GetRelativePath(root,p)}:{i+1}: {lines[i].Trim()}");
}
