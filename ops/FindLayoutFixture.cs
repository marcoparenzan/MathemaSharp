#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp";
foreach(var p in Directory.EnumerateFiles(root,"*",SearchOption.AllDirectories).Where(p=>!p.Split('\\').Any(s=>new[]{"obj","bin",".git",".ralfai"}.Contains(s)))) {
if(Path.GetFileName(p).Contains("P5_layout")) {Console.WriteLine(p);Console.WriteLine(File.ReadAllText(p));}
}
