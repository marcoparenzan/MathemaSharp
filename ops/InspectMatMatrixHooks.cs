#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\MatSharp\src\MatSharpLib";
foreach(var f in new[]{"Runtime/GenericScalar.cs","Runtime/Ops.cs","Runtime/Values.cs"}) {
 var lines=File.ReadAllLines(Path.Combine(root,f));
 for(int i=0;i<lines.Length;i++)if(lines[i].Contains("Concat(")||lines[i].Contains("Cat(")||lines[i].Contains("Transpose(")||lines[i].Contains("public static MValue Unary")||lines[i].Contains("public override MValue Gather")||lines[i].Contains("public override MValue Set"))Console.WriteLine($"{f}:{i+1}: {lines[i]}");
}
