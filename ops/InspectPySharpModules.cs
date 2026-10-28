#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp\src\PySharpLib";
foreach(var f in Directory.GetFiles(root,"*.cs",SearchOption.AllDirectories).Where(p=>!p.Contains("\\obj\\"))) {
 var lines=File.ReadAllLines(f);
 for(int i=0;i<lines.Length;i++)if(lines[i].Contains("DecimalModule")||lines[i].Contains("class ModuleRegistry")||lines[i].Contains("RegisterModule"))Console.WriteLine($"{f}:{i+1}: {lines[i]}");
}
Console.WriteLine(string.Join("\n",File.ReadLines(Path.Combine(root,"Modules","DecimalModule.cs")).Take(60)));
