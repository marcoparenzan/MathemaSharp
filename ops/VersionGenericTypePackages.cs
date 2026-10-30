#:property TargetFramework=net11.0
using System.Xml.Linq;
foreach(var item in new[]{("PySharpLib","2.6.1-net11-preview.2"),("PySharp","2.5.1-net11-preview.4"),("JupyterSharp.Kernels.PySharp","0.1.1-net11-preview.3")})
{
var p=Path.Combine(@"D:\Dev\Science\PySharp\src",item.Item1,item.Item1+".csproj");var d=XDocument.Load(p);d.Descendants("Version").Single().Value=item.Item2;d.Save(p);
}
