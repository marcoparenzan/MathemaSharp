#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\MatSharp\src\MatSharpLib\Runtime";
Console.WriteLine(string.Join("\n",File.ReadLines(Path.Combine(root,"Values.cs")).Skip(45).Take(60)));
Console.WriteLine(string.Join("\n",File.ReadLines(Path.Combine(root,"Ops.cs")).Take(160)));
