#:property TargetFramework=net11.0
var root=@"D:\Dev\BuildArtifacts\PySharp\dependency-evidence\anyio";
Console.WriteLine(string.Join("\n",File.ReadLines(Path.Combine(root,"METADATA")).Take(8)));
Console.WriteLine(File.ReadAllText(Path.Combine(root,"__init__.py")));
