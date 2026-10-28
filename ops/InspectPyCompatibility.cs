#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp";
foreach(var p in Directory.EnumerateFiles(root,"*.cs",SearchOption.AllDirectories).Where(p=>!p.Contains("\\obj\\")&&!p.Contains("\\bin\\"))) {
var lines=File.ReadAllLines(p);
for(int i=0;i<lines.Length;i++) if(lines[i].Contains("_getframe")||lines[i].Contains("P5_layout")||lines[i].Contains("set_position")||lines[i].Contains("class SysModule")) Console.WriteLine($"{Path.GetRelativePath(root,p)}:{i+1}: {lines[i]}");
}
