#:property TargetFramework=net11.0
#:project "../../articoli/2026.10 NavierStokes/src/NavierStokes.Core/NavierStokes.Core.csproj"
using System.Buffers.Binary;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using NC=NavierStokes.Core.Charts;
var output="ops/compatibility/png-baseline.json";
if(File.Exists(output)){Console.WriteLine("PNG baseline exists; no regeneration.");return 0;}
(string Name,double[,] Field,bool Diverging,double Min,double Max,int Width,int Height,bool Mask)[] fixtures=
[
 ("gradient",new double[,]{{0,1},{2,3}},false,0,3,6,4,false),
 ("signed",new double[,]{{-1,0,1},{1,0.25,-0.5},{0,0.5,-1}},true,-1,1,7,5,false),
 ("masked",new double[,]{{0,1,2},{2,1,0}},false,0,2,8,6,true)
];
var samples=new JsonArray();
foreach(var fixture in fixtures)
{
 Func<double,double,bool>? mask=fixture.Mask?(x,y)=>(x-1)*(x-1)+(y-0.5)*(y-0.5)<=0.16:null;
 var png=NC.FieldToPng(fixture.Field,fixture.Diverging?NC.ColorMap.Diverging:NC.ColorMap.Sequential,fixture.Min,fixture.Max,fixture.Width,fixture.Height,mask);
 using var compressed=new MemoryStream();int offset=8;
 while(offset<png.Length)
 {
  int length=checked((int)BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(offset,4)));
  var type=Encoding.ASCII.GetString(png,offset+4,4);
  if(type=="IDAT")compressed.Write(png,offset+8,length);
  offset+=length+12;
 }
 compressed.Position=0;using var zlib=new ZLibStream(compressed,CompressionMode.Decompress);using var raw=new MemoryStream();zlib.CopyTo(raw);
 var values=new JsonArray();foreach(double value in fixture.Field)values.Add((JsonNode)JsonValue.Create(value)!);
 samples.Add((JsonNode)new JsonObject { ["Name"]=fixture.Name,["Rows"]=fixture.Field.GetLength(0),["Columns"]=fixture.Field.GetLength(1),["Values"]=values,
 ["Diverging"]=fixture.Diverging,["Min"]=fixture.Min,["Max"]=fixture.Max,["Width"]=fixture.Width,["Height"]=fixture.Height,["MaskCircle"]=fixture.Mask,
 ["ScanlinesHex"]=Convert.ToHexString(raw.ToArray()),["EncodedSha256"]=Convert.ToHexString(SHA256.HashData(png)) });
}
var report=new JsonObject { ["GeneratedUtc"]=DateTime.UtcNow.ToString("O"),["Framework"]=RuntimeInformation.FrameworkDescription,["OS"]=RuntimeInformation.OSDescription,
 ["SourceSha256"]=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes("../articoli/2026.10 NavierStokes/src/NavierStokes.Core/Charts.cs"))),
 ["Notes"]="Pre-PNG-encoder migration: raw scanline bytes always comparable; compressed hash only comparable on the recorded runtime/OS.",["Samples"]=samples };
Directory.CreateDirectory("ops/compatibility");File.WriteAllText(output,report.ToJsonString(new JsonSerializerOptions { WriteIndented=true }));
Console.WriteLine($"Captured {samples.Count} legacy PNG fixtures as raw hex and hashes. No image files.");return 0;
