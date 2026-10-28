#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\MatSharp\src\MatSharpLib";
foreach(var f in Directory.GetFiles(root,"*.cs",SearchOption.AllDirectories).Where(p=>!p.Contains("\\obj\\"))) {
 var lines=File.ReadAllLines(f);for(int i=0;i<lines.Length;i++)if(lines[i].Contains("abstract class MValue")||lines[i].Contains("abstract record MValue")||lines[i].Contains("class Mat :")||lines[i].Contains("UnaryOp(")||lines[i].Contains("BinaryOp(")||lines[i].Contains("class Ops"))Console.WriteLine($"{f}:{i+1}: {lines[i]}");
}
