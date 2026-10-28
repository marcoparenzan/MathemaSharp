#:property TargetFramework=net11.0
using System.Text.Json;
var path=@"D:\Dev\BuildArtifacts\net11-pack\consumer\obj\project.assets.json";
using var doc=JsonDocument.Parse(File.ReadAllText(path));
foreach(var target in doc.RootElement.GetProperty("targets").EnumerateObject())
foreach(var lib in target.Value.EnumerateObject())
if(lib.Value.TryGetProperty("dependencies",out var deps))
foreach(var dep in deps.EnumerateObject())if(dep.Name.Contains("Cryptography"))Console.WriteLine($"{lib.Name} -> {dep.Name} {dep.Value}");
