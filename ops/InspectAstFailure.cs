#:property TargetFramework=net11.0
var path=Directory.GetFiles(@"D:\Dev\BuildArtifacts\net11-migration","PySharp*test.log").First();
var lines=File.ReadAllLines(path);var i=Array.FindIndex(lines,l=>l.Contains("Failed PySharp.Tests.M16_FastApi.FastApiSmokeTests.Import_fastapi"));
Console.WriteLine(string.Join("\n",lines.Skip(i).Take(95)));
