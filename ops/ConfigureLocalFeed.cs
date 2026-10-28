#:property TargetFramework=net11.0
using System.Security.Cryptography;
using System.Xml.Linq;
var feed = @"D:\Dev\NuGetLocalFeed";
Directory.CreateDirectory(feed);
foreach (var name in args.Length > 0 ? args : new[] { "MathemaSharp", "PlotSharp", "JupyterSharp", "MatSharp" })
{
    var root = Path.Combine(@"D:\Dev\Science", name);
    if (!Directory.Exists(root)) continue;
    var targets = Path.Combine(root, "Directory.Build.targets");
    var doc = File.Exists(targets) ? XDocument.Load(targets) : new XDocument(new XElement("Project"));
    var project = doc.Root!;
    var ns = project.Name.Namespace;
    project.Elements(ns + "PropertyGroup").Where(e => (string?)e.Attribute("Label") == "SharedLocalNuGetFeed").Remove();
    project.Add(new XElement(ns + "PropertyGroup", new XAttribute("Label", "SharedLocalNuGetFeed"), new XElement(ns + "PackageOutputPath", feed)));
    doc.Save(targets);
    bool Safe(string p) => !Path.GetRelativePath(root, p).Split(Path.DirectorySeparatorChar).Any(s => s is ".git" or "node_modules" or ".venv-oracles");
    foreach (var p in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Where(Safe).Where(p => p.EndsWith(".nupkg") || p.EndsWith(".snupkg")).ToArray())
    {
        var destination = Path.Combine(feed, Path.GetFileName(p));
        if (File.Exists(destination) && !SHA256.HashData(File.ReadAllBytes(p)).SequenceEqual(SHA256.HashData(File.ReadAllBytes(destination))))
            throw new InvalidOperationException($"Package collision; source preserved: {p}");
        if (!File.Exists(destination)) File.Copy(p, destination);
        File.Delete(p);
        Console.WriteLine($"PACKAGE {p} -> {destination}");
    }
    var dirs = Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories).Where(Safe)
        .Where(p => new[] { "bin", "obj", "artifacts", "BenchmarkDotNet.Artifacts" }.Contains(Path.GetFileName(p), StringComparer.OrdinalIgnoreCase))
        .OrderByDescending(p => p.Length).ToArray();
    foreach (var dir in dirs)
    {
        try { Directory.Delete(dir, true); Console.WriteLine("REMOVED " + dir); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) { Console.WriteLine($"LOCKED {dir}: {e.Message}"); }
    }
    Console.WriteLine("CONFIGURED " + targets);
}
