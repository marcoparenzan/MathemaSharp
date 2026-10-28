#:property TargetFramework=net11.0
using System.IO.Compression;
foreach(var path in Directory.GetFiles(@"D:\Dev\NuGetLocalFeed","*.nupkg").Where(p=>p.EndsWith(".0.2.0.nupkg")||p.EndsWith("NDSharp.2.8.0.nupkg"))) {
 Console.WriteLine(Path.GetFileName(path));
 using var zip=ZipFile.OpenRead(path);var spec=zip.Entries.Single(e=>e.FullName.EndsWith(".nuspec"));using var r=new StreamReader(spec.Open());Console.WriteLine(r.ReadToEnd());
}
