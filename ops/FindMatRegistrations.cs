#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\MatSharp\src\MatSharpLib";
foreach(var p in Directory.GetFiles(root,"*.cs",SearchOption.AllDirectories).Where(p=>!p.Contains("\\obj\\"))){var lines=File.ReadAllLines(p);for(int i=0;i<lines.Length;i++)if(lines[i].Contains("public static MValue Unary")||lines[i].Contains("Register(this)")||lines[i].Contains("new Reg(")||lines[i].Contains("Builtins.Register"))Console.WriteLine($"{p}:{i+1}: {lines[i]}");}
