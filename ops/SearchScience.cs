#:property TargetFramework=net11.0
var root=Path.Combine(@"D:\Dev\Science",args[0]);
int count=0;
foreach(var p in Directory.EnumerateFiles(root,"*",SearchOption.AllDirectories).Where(p=>!p.Split('\\').Any(s=>new[]{"obj","bin",".git",".ralfai","node_modules"}.Contains(s))).Where(p=>new[]{".cs",".py"}.Contains(Path.GetExtension(p)))) {
var lines=File.ReadAllLines(p);for(int i=0;i<lines.Length;i++) if(lines[i].Contains(args[1])) {Console.WriteLine($"{Path.GetRelativePath(root,p)}:{i+1}: {lines[i]}"); if(++count==50)return;}}
