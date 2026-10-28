#:property TargetFramework=net11.0
#:project "../../articoli/2026.10 NavierStokes/src/NavierStokes.Core/NavierStokes.Core.csproj"
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using NC=NavierStokes.Core.Charts;
var output="ops/compatibility/vector-baseline.json";
if(File.Exists(output)){Console.WriteLine("Vector baseline exists; no regeneration.");return 0;}
var directionsU=new double[5,5];var directionsV=new double[5,5];
directionsU[1,1]=1;directionsV[1,3]=1;directionsU[3,1]=-1;directionsV[3,3]=-1;
var thresholdU=new double[3,3];thresholdU[0,0]=1;thresholdU[1,0]=0.019;thresholdU[1,1]=0.02;
(string Name,double[,] U,double[,] V,int Stride,int Width)[] fixtures=
[
 ("horizontal-rectangular",new double[,]{{1,1,1,1},{1,1,1,1}},new double[2,4],1,60),
 ("four-directions",directionsU,directionsV,2,64),
 ("zero",new double[3,3],new double[3,3],1,40),
 ("threshold",thresholdU,new double[3,3],1,40)
];
var samples=new JsonArray();XNamespace ns="http://www.w3.org/2000/svg";
foreach(var fixture in fixtures)
{
 var field=new double[fixture.U.GetLength(0),fixture.U.GetLength(1)];
 var svg=XElement.Parse(NC.FieldPlot(field,new() { Arrows=(fixture.U,fixture.V),ArrowStride=fixture.Stride,PlotWidth=fixture.Width }));
 var path=svg.Elements(ns+"path").Single(p=>p.Attribute("stroke-opacity")?.Value=="0.75").Attribute("d")!.Value;
 var u=new JsonArray();foreach(double value in fixture.U)u.Add((JsonNode)JsonValue.Create(value)!);
 var v=new JsonArray();foreach(double value in fixture.V)v.Add((JsonNode)JsonValue.Create(value)!);
 samples.Add((JsonNode)new JsonObject { ["Name"]=fixture.Name,["Rows"]=field.GetLength(0),["Columns"]=field.GetLength(1),["U"]=u,["V"]=v,
 ["Stride"]=fixture.Stride,["PlotWidth"]=fixture.Width,["PlotHeight"]=(int)Math.Round(fixture.Width*(double)(field.GetLength(0)-1)/(field.GetLength(1)-1)),["Left"]=16,["Top"]=44,["Path"]=path });
}
var report=new JsonObject { ["GeneratedUtc"]=DateTime.UtcNow.ToString("O"),["SourceSha256"]=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes("../articoli/2026.10 NavierStokes/src/NavierStokes.Core/Charts.cs"))),
 ["Notes"]="Pre-vector migration: SVG paths from production FieldPlot, invariant coordinate formatting, no images saved.",["Samples"]=samples };
Directory.CreateDirectory("ops/compatibility");File.WriteAllText(output,report.ToJsonString(new JsonSerializerOptions { WriteIndented=true }));
Console.WriteLine($"Captured {samples.Count} legacy vector paths. No image files.");return 0;
