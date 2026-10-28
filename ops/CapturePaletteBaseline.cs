#:property TargetFramework=net11.0
#:project "../../articoli/2026.10 NavierStokes/src/NavierStokes.Core/NavierStokes.Core.csproj"
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
var output="ops/compatibility/palette-baseline.json";
if(args.Any(a=>a!="--extend-before-migration"))throw new ArgumentException("Only --extend-before-migration is supported.");
if(File.Exists(output))
{
 if(!args.Contains("--extend-before-migration")){Console.WriteLine("Baseline exists; no regeneration.");return 0;}
 using var previous=JsonDocument.Parse(File.ReadAllText(output));
 foreach(var hash in previous.RootElement.GetProperty("SourceHashes").EnumerateObject())
  if(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(hash.Name)))!=hash.Value.GetString())
   throw new Exception("Legacy source changed: refusing baseline extension.");
}
var points=Enumerable.Range(0,257).Select(i=>i/256.0).ToList();
foreach(int denominator in new[]{3,7,8,10})for(int k=1;k<denominator;k++)
{
 double t=(double)k/denominator;
 points.AddRange([double.BitDecrement(t),t,double.BitIncrement(t)]);
}
points.AddRange([-0.1,1.1,double.BitIncrement(0),double.BitDecrement(1)]);
var samples=new JsonArray();
foreach(double t in points.Distinct().Order())
 samples.Add((JsonNode)new JsonObject { ["T"]=t,["Sequential8"]=NavierStokes.Core.Charts.ColorMap.Sequential.Hex(t),
 ["Diverging9"]=NavierStokes.Core.Charts.ColorMap.Diverging.Hex(t),["Compact4"]=PlotSharp.Charts.SequentialColor(t) });
var hashes=new JsonObject();
foreach(var file in new[]{"../articoli/2026.10 NavierStokes/src/NavierStokes.Core/Charts.cs","../PlotSharp/src/PlotSharp/PlotSharp/Heatmap.cs"})
 hashes[file]=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file)));
var report=new JsonObject { ["GeneratedUtc"]=DateTime.UtcNow.ToString("O"),["Notes"]="Pre-palette-migration outputs, including stop boundaries and adjacent doubles; do not regenerate for new implementation.",["SourceHashes"]=hashes,["Samples"]=samples };
Directory.CreateDirectory("ops/compatibility");File.WriteAllText(output,report.ToJsonString(new JsonSerializerOptions { WriteIndented=true }));
Console.WriteLine($"Captured {samples.Count} inputs across three legacy palettes. No images.");return 0;
