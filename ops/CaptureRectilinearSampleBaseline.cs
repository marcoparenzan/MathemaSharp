#:property TargetFramework=net11.0
#:property PublishAot=false
#:property JsonSerializerIsReflectionEnabledByDefault=true
#:project ../../NDSharp/src/NDSharp.Plot/NDSharp.Plot.csproj
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using NDSharp.Plot;
var output="ops/compatibility/rectilinear-sample-baseline.json";
if(File.Exists(output)){Console.WriteLine("Sample baseline exists; no regeneration.");return 0;}
double[] xs=[-2,0.25,10],ys=[-1,2,9],values=[-1.3,0.2,2.1,1.7,-0.4,0.9,0.3,1.8,-0.8];
var artist=new ContourArtist { Xs=xs,Ys=ys,Z=values };
var method=typeof(ContourArtist).GetMethod("Sample",BindingFlags.NonPublic|BindingFlags.Instance)??throw new Exception("Legacy sampler missing.");
var samples=new JsonArray();
var xqueries=new[]{-2d,-0.875,0.25,5.125,10,double.BitDecrement(0.25),double.BitIncrement(0.25),-3,11};
var yqueries=new[]{-1d,0.5,2,5.5,9,double.BitDecrement(2),double.BitIncrement(2),-2,10};
foreach(double x in xqueries)foreach(double y in yqueries)
 samples.Add((JsonNode)new JsonObject { ["X"]=x,["Y"]=y,["Value"]=(double)method.Invoke(artist,new object[]{x,y})! });
static JsonArray Array(double[] data){var array=new JsonArray();foreach(double value in data)array.Add((JsonNode)JsonValue.Create(value)!);return array;}
var report=new JsonObject { ["GeneratedUtc"]=DateTime.UtcNow.ToString("O"),["SourceSha256"]=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes("../NDSharp/src/NDSharp.Plot/Contour.cs"))),
 ["Notes"]="Pre-filled-sampler migration, numeric outputs from legacy private sampler; nonuniform axes, boundaries/adjacent doubles and extrapolation. No images.",
 ["Xs"]=Array(xs),["Ys"]=Array(ys),["Values"]=Array(values),["Samples"]=samples };
Directory.CreateDirectory("ops/compatibility");File.WriteAllText(output,report.ToJsonString(new JsonSerializerOptions { WriteIndented=true }));
Console.WriteLine($"Captured {samples.Count} legacy sample queries. No images.");return 0;
