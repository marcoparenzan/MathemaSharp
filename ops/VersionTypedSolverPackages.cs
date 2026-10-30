#:property TargetFramework=net11.0
using System.Xml.Linq;
foreach(var p in Directory.EnumerateFiles(@"D:\Dev\Science\MatSharp\src","*.csproj",SearchOption.AllDirectories))
{
var d=XDocument.Load(p);if(d.Descendants("PackageId").Any()) {d.Descendants("Version").Single().Value="0.1.1-net11-preview.3";d.Save(p);}
}
var kernel=@"D:\Dev\Science\PySharp\src\JupyterSharp.Kernels.PySharp\JupyterSharp.Kernels.PySharp.csproj";
var doc=XDocument.Load(kernel);doc.Descendants("Version").Single().Value="0.1.1-net11-preview.2";doc.Save(kernel);
