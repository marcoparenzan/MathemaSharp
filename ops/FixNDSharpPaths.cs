#:property TargetFramework=net11.0
using System.Xml.Linq;
var root=@"D:\Dev\Science\NDSharp";
var file=Path.Combine(root,"src","NDSharp.Notebooks.Tests","NotebookTests.cs");
var text=File.ReadAllText(file);
text=text.Replace("var root = new DirectoryInfo(AppContext.BaseDirectory);", "static string SourceDirectory([System.Runtime.CompilerServices.CallerFilePath] string source = \"\") => Path.GetDirectoryName(source)!;\n        var root = new DirectoryInfo(SourceDirectory());");
File.WriteAllText(file,text);
var target=Path.Combine(root,"Directory.Build.targets");
var doc=File.Exists(target)?XDocument.Load(target):new XDocument(new XElement("Project"));
var ns=doc.Root!.Name.Namespace;
doc.Root.Elements(ns+"PropertyGroup").Where(e=>(string?)e.Attribute("Label")=="SharedLocalNuGetFeed").Remove();
doc.Root.Add(new XElement(ns+"PropertyGroup",new XAttribute("Label","SharedLocalNuGetFeed"),new XElement(ns+"PackageOutputPath",@"D:\Dev\NuGetLocalFeed")));
doc.Save(target);
Console.WriteLine("Updated NDSharp test source discovery and package destination.");
