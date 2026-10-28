#:property TargetFramework=net11.0
#:project "../../articoli/2026.10 NavierStokes/src/NavierStokes.Core/NavierStokes.Core.csproj"
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using NC=NavierStokes.Core.Charts;
var output="ops/compatibility/field-limits-baseline.json";
if(File.Exists(output)){Console.WriteLine("Field limits baseline exists; no regeneration.");return 0;}
(string Name,double[,] Field,double? Min,double? Max,bool Symmetric,double Clip)[] fixtures=
[
 ("plain",new double[,]{{0,1,2,3,4},{5,6,7,8,9}},null,null,false,0),
 ("clipped",new double[,]{{0,1,2,3,4},{5,6,7,8,9}},null,null,false,0.2),
 ("symmetric",new double[,]{{-6,-3,0},{2,4,9}},null,null,true,0),
 ("constant",new double[,]{{5,5},{5,5}},null,null,false,0),
 ("zero-symmetric",new double[2,2],null,null,true,0),
 ("explicit",new double[,]{{0,1},{2,3}},-2,8,false,0.2),
 ("nan-ignored",new double[,]{{double.NaN,-3,0},{2,4,9}},null,null,false,0),
 ("nan-fixed",new double[,]{{double.NaN,double.NaN},{double.NaN,double.NaN}},0,1,false,0)
];
var samples=new JsonArray();XNamespace ns="http://www.w3.org/2000/svg";
foreach(var fixture in fixtures)
{
 const int plotWidth=40;var svg=XElement.Parse(NC.FieldPlot(fixture.Field,new() { PlotWidth=plotWidth,Min=fixture.Min,Max=fixture.Max,Symmetric=fixture.Symmetric,ClipPercentile=fixture.Clip }));
 var ticks=svg.Elements(ns+"text").Where(t=>t.Attribute("x")?.Value==(16+plotWidth+20+14+6).ToString(CultureInfo.InvariantCulture)).Select(t=>t.Value).ToArray();
 if(ticks.Length!=3)throw new Exception("Expected three production colorbar labels.");
 var values=new JsonArray();foreach(double value in fixture.Field)values.Add(double.IsNaN(value)?null:(JsonNode)JsonValue.Create(value)!);
 samples.Add((JsonNode)new JsonObject { ["Name"]=fixture.Name,["Rows"]=fixture.Field.GetLength(0),["Columns"]=fixture.Field.GetLength(1),["Values"]=values,
 ["Min"]=fixture.Min,["Max"]=fixture.Max,["Symmetric"]=fixture.Symmetric,["Clip"]=fixture.Clip,
 ["ExpectedMin"]=double.Parse(ticks[0],CultureInfo.InvariantCulture),["ExpectedMax"]=double.Parse(ticks[2],CultureInfo.InvariantCulture) });
}
var report=new JsonObject { ["GeneratedUtc"]=DateTime.UtcNow.ToString("O"),["SourceSha256"]=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes("../articoli/2026.10 NavierStokes/src/NavierStokes.Core/Charts.cs"))),
 ["Notes"]="Before field-limits migration, endpoints read from production FieldPlot colorbar; chosen cases have exactly representable displayed endpoints. Null samples encode NaN.",["Samples"]=samples };
Directory.CreateDirectory("ops/compatibility");File.WriteAllText(output,report.ToJsonString(new JsonSerializerOptions { WriteIndented=true }));
Console.WriteLine($"Captured {samples.Count} production field-limit fixtures. No image files.");return 0;
