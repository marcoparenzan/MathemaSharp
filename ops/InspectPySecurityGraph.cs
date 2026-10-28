#:property TargetFramework=net11.0
using System.Text.Json;
var audit=Directory.GetDirectories(@"D:\Dev\BuildArtifacts\security-audit").OrderDescending().First();
using var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(audit,"PySharp.json")));
Console.WriteLine(doc.RootElement.GetRawText());
