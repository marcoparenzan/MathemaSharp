#:property TargetFramework=net11.0
#:project "../../articoli/2026.10 NavierStokes/src/NavierStokes.Core/NavierStokes.Core.csproj"
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NC=NavierStokes.Core.Charts;
var output="ops/compatibility/colorbar-baseline.json";
if(File.Exists(output)){Console.WriteLine("Colorbar baseline exists; no regeneration.");return 0;}
(string Name,double Min,double Max,bool Diverging,int Width,string Label,string Subtitle)[] cases=
[
 ("sequential",0,1,false,40,"Scala <&'", ""),
 ("diverging",-3,7,true,60,"pressione [Pa]", "sub"),
 ("constant",5,5,false,37,"costante", "")
];
var samples=new JsonArray();XNamespace ns="http://www.w3.org/2000/svg";
foreach(var item in cases)
{
 var field=new double[2,2];var limits=PlotSharp.FieldColorLimits.Resolve(field,item.Min,item.Max);
 var root=XElement.Parse(NC.FieldPlot(field,new() { Min=item.Min,Max=item.Max,Map=item.Diverging?NC.ColorMap.Diverging:NC.ColorMap.Sequential,PlotWidth=item.Width,ColorbarLabel=item.Label,Subtitle=item.Subtitle }));
 var elements=root.Elements().SkipWhile(e=>e.Name!=ns+"defs").ToArray();
 if(elements.Length!=6)throw new Exception("Unexpected colorbar structure.");
 var markup=Regex.Replace(string.Concat(elements.Select(e=>e.ToString(SaveOptions.DisableFormatting))),"cb[0-9a-f]{8}","cb-fixed");
 samples.Add((JsonNode)new JsonObject { ["Name"]=item.Name,["Min"]=item.Min,["Max"]=item.Max,["ResolvedMin"]=limits.Min,["ResolvedMax"]=limits.Max,
 ["Diverging"]=item.Diverging,["PlotWidth"]=item.Width,["Label"]=item.Label,["Subtitle"]=item.Subtitle,["Markup"]=markup });
}
var report=new JsonObject { ["GeneratedUtc"]=DateTime.UtcNow.ToString("O"),["SourceSha256"]=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes("../articoli/2026.10 NavierStokes/src/NavierStokes.Core/Charts.cs"))),
 ["Notes"]="Production colorbar markup before renderer migration; only random gradient ids normalized. No image files.",["Samples"]=samples };
Directory.CreateDirectory("ops/compatibility");File.WriteAllText(output,report.ToJsonString(new JsonSerializerOptions { WriteIndented=true }));
Console.WriteLine($"Captured {samples.Count} legacy gradient colorbars. No images.");return 0;
