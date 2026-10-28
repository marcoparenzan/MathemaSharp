#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\NDSharp\src\NDSharp";
foreach(var f in Directory.EnumerateFiles(root,"*.cs",SearchOption.AllDirectories).Where(p=>!p.Contains("\\obj\\")&&!p.Contains("\\bin\\")))Console.WriteLine(Path.GetRelativePath(root,f));
Console.WriteLine(string.Join("\n", File.ReadLines(Path.Combine(root,"Cast.cs")).Take(100)));
