#:property TargetFramework=net11.0
using System.Xml.Linq;
var root=@"D:\Dev\Science\JupyterSharp";
var p=Path.Combine(root,"src","JupyterSharp.Kernels.PowerShell","JupyterSharp.Kernels.PowerShell.csproj");
var d=XDocument.Load(p);
foreach(var id in new[]{"System.Security.Cryptography.Pkcs","System.Security.Cryptography.Xml"}) {
var e=d.Descendants("PackageReference").FirstOrDefault(e=>e.Attribute("Include")?.Value==id);
if(e is null)d.Root!.Add(new XElement("ItemGroup",new XElement("PackageReference",new XAttribute("Include",id),new XAttribute("Version","10.0.13"))));else e.SetAttributeValue("Version","10.0.13");
}
d.Save(p);
foreach(var path in Directory.EnumerateFiles(Path.Combine(root,"src"),"*.csproj",SearchOption.AllDirectories)) {
var doc=XDocument.Load(path);var version=doc.Descendants("Version").FirstOrDefault();
if(version?.Value=="0.3.1-net11-preview.1"){version.Value="0.3.1-net11-preview.2";doc.Save(path);}
}
