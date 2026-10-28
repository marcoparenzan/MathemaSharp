#:property TargetFramework=net10.0
#:project ../src/tools/Ecosystem.Validation/Ecosystem.Validation/Ecosystem.Validation.csproj
#:package Svg.Skia@5.2.3
using System.Text.Json;
using System.Xml.Linq;
using Svg.Skia;
using SkiaSharp;
// Review-only rasterizer. Does not add a PNG backend to PlotSharp or certify browser output.
var options=Ecosystem.Validation.EvidenceOptions.Parse(args);
var output=Path.GetFullPath(options.SaveImages?"ops/visual":"ops/validation/raster-no-images");Directory.CreateDirectory(output);
var manifest=new System.Text.Json.Nodes.JsonArray();
using var notebookManifest=JsonDocument.Parse(File.ReadAllText("ops/migrated-notebooks.json"));
foreach(var entry in notebookManifest.RootElement.EnumerateArray())
{
 var relative=entry.GetProperty("path").GetString()!;
 using var doc=JsonDocument.Parse(File.ReadAllText(relative));int index=0;
 foreach(var cell in doc.RootElement.GetProperty("cells").EnumerateArray())
 if(cell.TryGetProperty("outputs",out var outputs))foreach(var o in outputs.EnumerateArray())
 {
  if(!o.TryGetProperty("data",out var data)||!data.TryGetProperty("text/html",out var html))continue;
  var text=html.ValueKind==JsonValueKind.Array?string.Concat(html.EnumerateArray().Select(v=>v.GetString())):html.GetString();
  var root=XElement.Parse(text!);
  foreach(var element in root.DescendantsAndSelf().Where(e=>e.Name.LocalName=="svg"))
  {
   using var svg=new SKSvg();
   using var input=new MemoryStream(System.Text.Encoding.UTF8.GetBytes(element.ToString()));
   svg.Load(input);
   var picture=svg.Picture??throw new Exception("No rasterizable SVG picture");
   var bounds=picture.CullRect;
   using var bitmap=new SKBitmap((int)Math.Ceiling(bounds.Width*2),(int)Math.Ceiling(bounds.Height*2));
   using(var canvas=new SKCanvas(bitmap)){canvas.Clear(SKColors.White);canvas.Scale(2);canvas.Translate(-bounds.Left,-bounds.Top);canvas.DrawPicture(picture);}
   using var image=SKImage.FromBitmap(bitmap);using var encoded=image.Encode(SKEncodedImageFormat.Png,100)??throw new Exception("PNG encoding failed.");
   var bytes=encoded.ToArray();
   if(bytes.Length<8||!bytes.Take(8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10}))throw new Exception("Invalid encoded PNG.");
   var filename=Path.GetFileNameWithoutExtension(relative)+"-"+(++index).ToString("D2")+".png";
   if(options.SaveImages)File.WriteAllBytes(Path.Combine(output,filename),bytes);
   manifest.Add((System.Text.Json.Nodes.JsonNode)new System.Text.Json.Nodes.JsonObject { ["Notebook"]=relative,["EvidenceName"]=filename,
    ["Image"]=options.SaveImages?filename:null,["ImageWritten"]=options.SaveImages,["PngBytes"]=bytes.Length,["Sha256"]=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)) });
  }
 }
 if(index!=entry.GetProperty("expectedSvg").GetInt32())throw new Exception("Missing rasterized charts: "+relative);
}
var report=new System.Text.Json.Nodes.JsonObject { ["GeneratedUtc"]=DateTime.UtcNow.ToString("O"), ["Renderer"]="Svg.Skia 5.2.3 (review tool only)",["SaveImages"]=options.SaveImages,["PngEncodingValidated"]=true,["Images"]=manifest };
File.WriteAllText(Path.Combine(output,"manifest.json"),report.ToJsonString(new JsonSerializerOptions { WriteIndented=true }));
Console.WriteLine($"Raster validation: {manifest.Count} PNGs encoded, saved images={options.SaveImages}.");
return 0;
