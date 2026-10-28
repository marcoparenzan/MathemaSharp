#:property TargetFramework=net11.0
foreach(var path in Directory.GetFiles(@"D:\Dev\BuildArtifacts\net11-migration", "PySharp*test.log")) {
var lines=File.ReadAllLines(path);
for(int i=0;i<lines.Length;i++) if(lines[i].TrimStart().StartsWith("Failed PySharp")) {
Console.WriteLine(lines[i]);
Console.WriteLine(string.Join("\n",lines.Skip(i+1).Take(6)).Substring(0,Math.Min(1400,string.Join("\n",lines.Skip(i+1).Take(6)).Length)));
}
}
