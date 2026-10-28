#:property TargetFramework=net11.0
using System.IO.Compression;
using System.Security.Cryptography;
foreach(var p in new[]{@"D:\Dev\BuildArtifacts\NDSharp\bin\NDSharp.Plot\release\NDSharp.Plot.dll",@"D:\Dev\BuildArtifacts\PySharp\bin\PySharp.Tests\release\NDSharp.Plot.dll"})Console.WriteLine(p+" "+Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)))+" "+File.GetLastWriteTime(p));
using var zip=ZipFile.OpenRead(@"D:\Dev\NuGetLocalFeed\NDSharp.Plot.2.6.3-net11-preview.2.nupkg");
foreach(var e in zip.Entries.Where(e=>e.FullName.EndsWith("NDSharp.Plot.dll"))){using var s=e.Open();Console.WriteLine(e.FullName+" "+Convert.ToHexString(SHA256.HashData(s)));}
foreach(var p in Directory.EnumerateFiles(@"D:\Dev\BuildArtifacts\PySharp\obj","project.assets.json",SearchOption.AllDirectories).Where(p=>p.Contains("PySharp.Tests")||p.Contains("Matplotlib"))){Console.WriteLine(p);foreach(var line in File.ReadLines(p).Where(l=>l.Contains("NDSharp.Plot/")))Console.WriteLine(line);}
