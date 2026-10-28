#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp\src\PySharpLib";
Console.WriteLine(string.Join("\n",File.ReadLines(Path.Combine(root,"Runtime","Clr.cs")).Take(220)));
