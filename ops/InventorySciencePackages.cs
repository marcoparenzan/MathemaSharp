#:property TargetFramework=net11.0
using System.Xml.Linq;
foreach(var name in new[]{"MathemaSharp","NDSharp","PySharp","MatSharp","PlotSharp","JupyterSharp"}) {
var root=Path.Combine(@"D:\Dev\Science",name);
foreach(var p in Directory.EnumerateFiles(root,"*.csproj",SearchOption.AllDirectories).Where(p=>!p.Split('\\').Any(s=>new[]{"imports","obj","bin",".ralfai"}.Contains(s)))) {
var d=XDocument.Load(p);Console.WriteLine(name+" "+Path.GetRelativePath(root,p)+" : "+string.Join(" ",d.Descendants().Where(e=>new[]{"PackageId","Version","IsPackable","PackAsTool","IsTestProject"}.Contains(e.Name.LocalName)).Select(e=>e.Name+"="+e.Value)));
}}
