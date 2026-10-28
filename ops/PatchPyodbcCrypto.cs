#:property TargetFramework=net11.0
using System.Xml.Linq;
var root=@"D:\Dev\Science\PySharp\src";
var p=Path.Combine(root,"PySharpLib.Pyodbc","PySharpLib.Pyodbc.csproj");var d=XDocument.Load(p);
var reference=d.Descendants("PackageReference").FirstOrDefault(e=>e.Attribute("Include")?.Value=="System.Security.Cryptography.Pkcs");
if(reference is null)d.Root!.Add(new XElement("ItemGroup",new XElement("PackageReference",new XAttribute("Include","System.Security.Cryptography.Pkcs"),new XAttribute("Version","10.0.13"))));else reference.SetAttributeValue("Version","10.0.13");
d.Descendants("Version").Single().Value="2.5.1-net11-preview.2";d.Save(p);
p=Path.Combine(root,"PySharp","PySharp.csproj");d=XDocument.Load(p);d.Descendants("Version").Single().Value="2.5.1-net11-preview.2";d.Save(p);
