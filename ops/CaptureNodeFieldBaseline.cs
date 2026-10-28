#:property TargetFramework=net11.0
#:project "../../articoli/2026.10 NavierStokes/src/NavierStokes.Core/NavierStokes.Core.csproj"
using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NC=NavierStokes.Core.Charts;
var output="ops/compatibility/node-field-baseline.json";
if(File.Exists(output)){Console.WriteLine("Node-field baseline exists; no regeneration.");return 0;}
(string Name,double[,] Data,int Width,string Subtitle,bool Diverging,bool Contours,bool Vectors,bool Obstacle)[] cases=
[
 ("gradient",new double[,]{{0,1,2},{2,1,0}},12,"",false,false,false,false),
 ("contours",new double[,]{{0,1,2},{0,1,2},{0,1,2}},14,"sub",false,true,false,false),
 ("vectors",new double[,]{{-1,0,1},{1,-1,0},{2,-2,0}},17,"sub",true,false,true,false),
 ("obstacle",new double[,]{{0,1,2},{0,1,2},{0,1,2}},12,"",false,false,false,true)
];
var samples=new JsonArray();XNamespace ns="http://www.w3.org/2000/svg";
foreach(var item in cases)
{
 int rows=item.Data.GetLength(0),cols=item.Data.GetLength(1);var u=new double[rows,cols];var v=new double[rows,cols];
 foreach(int row in Enumerable.Range(0,rows))foreach(int col in Enumerable.Range(0,cols))u[row,col]=1;
 var options=new NC.FieldPlotOptions { Title="Field <&'",Subtitle=item.Subtitle,ColorbarLabel="Scale <&'",PlotWidth=item.Width,
  Min=item.Diverging?-2:0,Max=2,Map=item.Diverging?NC.ColorMap.Diverging:NC.ColorMap.Sequential,Symmetric=item.Diverging,
  Contours=item.Contours?item.Data:null,ContourLevels=1,Arrows=item.Vectors?(u,v):null,ArrowStride=1,Obstacle=item.Obstacle?(1,1,0.4):null };
 var root=XElement.Parse(NC.FieldPlot(item.Data,options));var image=root.Element(ns+"image")!;
 var png=Convert.FromBase64String(image.Attribute("href")!.Value["data:image/png;base64,".Length..]);
 using var compressed=new MemoryStream();int offset=8;
 while(offset<png.Length){int length=checked((int)BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(offset,4)));if(Encoding.ASCII.GetString(png,offset+4,4)=="IDAT")compressed.Write(png,offset+8,length);offset+=length+12;}
 compressed.Position=0;using var zlib=new ZLibStream(compressed,CompressionMode.Decompress);using var raw=new MemoryStream();zlib.CopyTo(raw);
 image.SetAttributeValue("href","RGB-PIXELS-CHECKED-SEPARATELY");
 var markup=Regex.Replace(root.ToString(SaveOptions.DisableFormatting),"cb[0-9a-f]{8}","cb-fixed");
 var values=new JsonArray();foreach(double value in item.Data)values.Add((JsonNode)JsonValue.Create(value)!);
 samples.Add((JsonNode)new JsonObject { ["Name"]=item.Name,["Rows"]=rows,["Columns"]=cols,["Values"]=values,["PlotWidth"]=item.Width,["Subtitle"]=item.Subtitle,
 ["Diverging"]=item.Diverging,["Contours"]=item.Contours,["Vectors"]=item.Vectors,["Obstacle"]=item.Obstacle,["Markup"]=markup,
 ["RawPixelSha256"]=Convert.ToHexString(SHA256.HashData(raw.ToArray())) });
}
var report=new JsonObject { ["GeneratedUtc"]=DateTime.UtcNow.ToString("O"),["SourceSha256"]=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes("../articoli/2026.10 NavierStokes/src/NavierStokes.Core/Charts.cs"))),
 ["Notes"]="Before FieldPlot assembly migration; only random ids and encoded PNG payload replaced in structural markup, decoded RGB checked independently. No images saved.",["Samples"]=samples };
Directory.CreateDirectory("ops/compatibility");File.WriteAllText(output,report.ToJsonString(new JsonSerializerOptions { WriteIndented=true }));
Console.WriteLine($"Captured {samples.Count} production node fields, markup and raw pixel hashes. No images.");return 0;
