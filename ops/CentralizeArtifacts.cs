#:property TargetFramework=net11.0
using System.Xml.Linq;
foreach (var name in args.Length > 0 ? args : new[] { "MathemaSharp", "PlotSharp", "JupyterSharp", "NDSharp", "MatSharp" })
{
    var root = Path.Combine(@"D:\Dev\Science", name);
    if (!Directory.Exists(root)) { Console.WriteLine("NOT FOUND " + root); continue; }
    var output = Path.Combine(@"D:\Dev\BuildArtifacts", name);
    Directory.CreateDirectory(output);
    bool Active(string p) => !Path.GetRelativePath(root, p).Split(Path.DirectorySeparatorChar).Any(s => s is ".ralfai" or "imports" or "bin" or "obj" or ".git" or "node_modules" or ".venv-oracles" or "artifacts");
    var props = Directory.EnumerateFiles(root, "Directory.Build.props", SearchOption.AllDirectories).Where(Active).Append(Path.Combine(root, "Directory.Build.props")).Distinct();
    foreach (var file in props)
    {
        var doc = File.Exists(file) ? XDocument.Load(file) : new XDocument(new XElement("Project"));
        var p = doc.Root!; var ns = p.Name.Namespace;
        p.Elements(ns + "PropertyGroup").Where(e => (string?)e.Attribute("Label") == "CentralBuildArtifacts").Remove();
        p.Add(new XElement(ns + "PropertyGroup", new XAttribute("Label", "CentralBuildArtifacts"),
            new XElement(ns + "UseArtifactsOutput", "true"), new XElement(ns + "ArtifactsPath", output),
            new XElement(ns + "PackageOutputPath", @"D:\Dev\NuGetLocalFeed")));
        doc.Save(file); Console.WriteLine("CONFIGURED " + file);
    }
}
