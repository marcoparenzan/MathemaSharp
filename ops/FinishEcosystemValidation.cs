#:property TargetFramework=net11.0
var log=@"D:\Dev\BuildArtifacts\ecosystem-validation\20261027-223603\build-test.log";
foreach(var line in File.ReadAllLines(log).Where(l=>l.Contains("RESULT")||l.Contains("Passed!")||l.Contains("Skipped:")))Console.WriteLine(line);
var root=@"D:\Dev\Science\PySharp";
// Only disposable local build outputs; references and cached packages outside the repository are retained.
foreach(var name in new[]{"bin","obj"})
foreach(var path in Directory.EnumerateDirectories(Path.Combine(root,"src"),name,SearchOption.AllDirectories).Where(p=>!p.Split('\\').Contains("imports")).OrderByDescending(p=>p.Length).ToArray())
{
 if(Directory.Exists(path)){Directory.Delete(path,true);Console.WriteLine("REMOVED "+path);}
}
