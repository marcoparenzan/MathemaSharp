#:property TargetFramework=net10.0
using System.Buffers.Binary;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
var kernel=@"D:\Dev\BuildArtifacts\PySharp\kernel-source-plots";
var notebook=Path.GetFullPath("notebooks/NDSharp/pysharp/02-contours-kernel.ipynb");
var output="ops/validation/ndsharp-contour-notebook";Directory.CreateDirectory(output);
var plotDll=Path.Combine(kernel,"NDSharp.Plot.dll");var sharedDll=Path.Combine(kernel,"PlotSharp.dll");
if(!File.Exists(plotDll)||!File.Exists(sharedDll))throw new Exception("BuildPySharpKernel.cs -- --source-plots must run first; source kernel dependency missing.");
using(var stream=File.OpenRead(plotDll))using(var pe=new PEReader(stream))
{
 var metadata=pe.GetMetadataReader();
 if(!metadata.AssemblyReferences.Any(h=>metadata.GetString(metadata.GetAssemblyReference(h).Name)=="PlotSharp"))
  throw new Exception("Kernel contains stale NDSharp.Plot without PlotSharp reference.");
}
using(var stream=File.OpenRead(sharedDll))using(var pe=new PEReader(stream))
{
 var metadata=pe.GetMetadataReader();
 foreach(var name in new[]{"RectilinearContours","RectilinearFieldSampler","ContourBands"})
  if(!metadata.TypeDefinitions.Any(h=>metadata.GetString(metadata.GetTypeDefinition(h).Name)==name))
   throw new Exception("Kernel contains stale PlotSharp without "+name+".");
}
var document=JsonNode.Parse(File.ReadAllText(notebook))!;
foreach(var cell in document["cells"]!.AsArray().Where(c=>c!["cell_type"]!.GetValue<string>()=="code")){cell!["outputs"]=new JsonArray();cell["execution_count"]=null;}
File.WriteAllText(notebook,document.ToJsonString());
var psi=new ProcessStartInfo("dotnet") { UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true };
foreach(var a in new[]{"run","--project",Path.GetFullPath("../JupyterSharp/src/JupyterSharp.Cli"),"-c","Release","--","run",notebook,"--in-place","--fail-fast","--kernel-paths",kernel})psi.ArgumentList.Add(a);
psi.Environment["DOTNET_ROLL_FORWARD"]="LatestMajor";psi.Environment["DOTNET_ROLL_FORWARD_TO_PRERELEASE"]="1";
using var process=Process.Start(psi)!;var stdout=process.StandardOutput.ReadToEndAsync();var stderr=process.StandardError.ReadToEndAsync();process.WaitForExit();
var log=await stdout+await stderr;File.WriteAllText(Path.Combine(output,"execution.log"),log);
if(process.ExitCode!=0){Console.WriteLine(log);return process.ExitCode;}
using var saved=JsonDocument.Parse(File.ReadAllText(notebook));var pngs=new JsonArray();var text="";
static string MimeText(JsonElement value)=>value.ValueKind==JsonValueKind.Array?string.Concat(value.EnumerateArray().Select(v=>v.GetString())):value.GetString()!;
foreach(var cell in saved.RootElement.GetProperty("cells").EnumerateArray())
if(cell.TryGetProperty("outputs",out var outputs))foreach(var o in outputs.EnumerateArray())
{
 if(o.GetProperty("output_type").GetString()=="error")throw new Exception("Notebook error output.");
 if(o.TryGetProperty("text",out var stream))text+=MimeText(stream);
 if(!o.TryGetProperty("data",out var data))continue;
 if(data.TryGetProperty("text/plain",out var plain))text+=MimeText(plain);
 if(!data.TryGetProperty("image/png",out var image))continue;
 var bytes=Convert.FromBase64String(MimeText(image));
 if(bytes.Length<24||!bytes.Take(8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10}))throw new Exception("Invalid notebook PNG.");
 int width=checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(16,4))),height=checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(20,4)));
 if(width!=320||height!=240)throw new Exception($"Unexpected PNG dimensions {width}x{height}.");
 pngs.Add((JsonNode)new JsonObject { ["Width"]=width,["Height"]=height,["Bytes"]=bytes.Length,["Sha256"]=Convert.ToHexString(SHA256.HashData(bytes)) });
}
foreach(var marker in new[]{"NONUNIFORM_CONTOUR_INPUT_READY","NDSHARP_LINE_CONTOUR_KERNEL_OK","NDSHARP_FILLED_CONTOUR_KERNEL_OK"})
 if(!text.Contains(marker))throw new Exception("Missing execution marker: "+marker);
if(pngs.Count!=2)throw new Exception("Expected one line and one filled notebook image output.");
var report=new JsonObject { ["GeneratedUtc"]=DateTime.UtcNow.ToString("O"),["Success"]=true,["Kernel"]="Real JupyterNet pysharp plugin",
 ["PlotSharpSha256"]=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(sharedDll))),["NDSharpPlotSha256"]=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(plotDll))),
 ["PngOutputs"]=pngs,["ReviewImageFilesSaved"]=false,["Scope"]="PNG outputs embedded in executed notebook only; no screenshot/review PNG files. Source-build JupyterNet, not native JupyterSharp or numerical/pixel oracle." };
File.WriteAllText(Path.Combine(output,"report.json"),report.ToJsonString(new JsonSerializerOptions { WriteIndented=true }));
Console.WriteLine("Real PySharp kernel: line + filled contour outputs verified, PlotSharp dependency present, no review image files.");return 0;
