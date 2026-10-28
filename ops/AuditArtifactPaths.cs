#:property TargetFramework=net11.0
foreach(var name in new[]{"MathemaSharp","PlotSharp","JupyterSharp"}) {
 var root=Path.Combine(@"D:\Dev\Science",name);
 bool Active(string p)=>!Path.GetRelativePath(root,p).Split(Path.DirectorySeparatorChar).Any(s=>new[]{"imports",".git",".venv-oracles","node_modules"}.Contains(s));
 Console.WriteLine("=== "+name);
 foreach(var dir in Directory.EnumerateDirectories(root,"*",SearchOption.AllDirectories).Where(Active).Where(p=>new[]{"bin","obj","artifacts","BenchmarkDotNet.Artifacts","TestResults"}.Contains(Path.GetFileName(p),StringComparer.OrdinalIgnoreCase)))Console.WriteLine("LOCAL OUTPUT "+dir);
 foreach(var f in Directory.EnumerateFiles(root,"*",SearchOption.AllDirectories).Where(Active).Where(p=>new[]{".ps1",".csx",".proj",".targets"}.Contains(Path.GetExtension(p)))) {
 var i=0;foreach(var line in File.ReadLines(f)){i++;if(line.Contains("artifacts",StringComparison.OrdinalIgnoreCase)&&!line.Contains("D:/Dev/BuildArtifacts",StringComparison.OrdinalIgnoreCase)&&!line.Contains(@"D:\Dev\BuildArtifacts",StringComparison.OrdinalIgnoreCase))Console.WriteLine($"REVIEW {Path.GetRelativePath(root,f)}:{i}: {line.Trim()}");}
 }
}
