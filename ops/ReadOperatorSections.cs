#:property TargetFramework=net11.0
var lines=File.ReadAllLines(@"D:\Dev\Science\PySharp\src\PySharpLib\Interpretation\Interp.cs");
Console.WriteLine(string.Join("\n",lines.Skip(2252).Take(125)));
for(int i=0;i<lines.Length;i++)if(lines[i].Contains("bool Compare")||lines[i].Contains("object Compare")||lines[i].Contains("CompareOp("))Console.WriteLine($"{i+1}: {lines[i]}");
