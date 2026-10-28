#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp\src\PySharpLib";
foreach(var p in Directory.GetFiles(root,"*.cs",SearchOption.AllDirectories).Where(p=>!p.Contains("\\obj\\"))) {
 var lines=File.ReadAllLines(p);
 for(int i=0;i<lines.Length;i++)if(lines[i].Contains("BinOp")||lines[i].Contains("EvalBinary")||lines[i].Contains("BinaryOp")||lines[i].Contains("UnaryOp"))Console.WriteLine($"{p}:{i+1}: {lines[i]}");
}
