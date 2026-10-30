#:property TargetFramework=net11.0
using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;
string id = args.Length == 0 ? "core" : args.Length == 1 && args[0] == "--smoothing" ? "smoothing" : args.Length == 2 && args[0] == "--id" ? args[1] : throw new ArgumentException("Use --id <manifest-id> (or legacy --smoothing).");
using var manifest=JsonDocument.Parse(File.ReadAllText("ops/migrated-notebooks.json"));
var matches=manifest.RootElement.EnumerateArray().Where(e=>e.GetProperty("id").GetString()==id).ToArray();
if(matches.Length!=1)throw new ArgumentException("Unknown or duplicate notebook id: "+id);
var entry=matches[0];int expected=entry.GetProperty("expectedSvg").GetInt32();bool smoothing=entry.GetProperty("requireGap").GetBoolean();
var relative=entry.GetProperty("path").GetString()!;
var repo=Path.GetFullPath(Directory.GetCurrentDirectory())+Path.DirectorySeparatorChar;
var notebook=Path.GetFullPath(relative);
if(!relative.StartsWith("notebooks/",StringComparison.Ordinal)||!notebook.StartsWith(repo,StringComparison.OrdinalIgnoreCase)||!notebook.EndsWith(".ipynb",StringComparison.Ordinal)||expected<=0)throw new ArgumentException("Invalid notebook manifest entry.");
var clean=System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(notebook))!;
foreach(var cell in clean["cells"]!.AsArray().Where(c=>c!["cell_type"]!.GetValue<string>()=="code")) { cell!["outputs"]=new System.Text.Json.Nodes.JsonArray(); cell["execution_count"]=null; }
File.WriteAllText(notebook,clean.ToJsonString());
var psi=new ProcessStartInfo("dotnet"){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true};
foreach(var arg in new[]{"run","--project",Path.GetFullPath("../JupyterSharp/src/JupyterSharp.Cli"),"-c","Release","--","run",notebook,"--in-place","--fail-fast"})psi.ArgumentList.Add(arg);
psi.Environment["DOTNET_ROLL_FORWARD"]="LatestMajor";
psi.Environment["DOTNET_ROLL_FORWARD_TO_PRERELEASE"]="1";
using var process=Process.Start(psi)!;
var stdout=process.StandardOutput.ReadToEndAsync();var stderr=process.StandardError.ReadToEndAsync();process.WaitForExit();
var log=await stdout+await stderr;
var logDir=Path.Combine(@"D:\Dev\BuildArtifacts\migrated-notebooks",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));Directory.CreateDirectory(logDir);
File.WriteAllText(Path.Combine(logDir,"execution.log"),log);Console.WriteLine("Execution log: "+logDir);
if(process.ExitCode!=0){Console.WriteLine(string.Join(Environment.NewLine,log.Split('\n').Where(l=>!l.StartsWith("<svg") && !l.StartsWith("<section")).TakeLast(25)));return process.ExitCode;}
using var doc=JsonDocument.Parse(File.ReadAllText(notebook));int svg=0,kpi=0,quotes=0;bool observedGap=false,observedTable=false;
foreach(var cell in doc.RootElement.GetProperty("cells").EnumerateArray())
if(cell.TryGetProperty("outputs",out var outputs))foreach(var output in outputs.EnumerateArray())
{
 if(output.GetProperty("output_type").GetString()=="error")throw new Exception("Notebook error output");
 if(!output.TryGetProperty("data",out var data)||!data.TryGetProperty("text/html",out var html))continue;
 var text=html.ValueKind==JsonValueKind.Array?string.Concat(html.EnumerateArray().Select(v=>v.GetString())):html.GetString();
 var root=XElement.Parse(text!);svg+=root.DescendantsAndSelf().Count(e=>e.Name.LocalName=="svg");
 observedTable|=root.DescendantsAndSelf().Any(e=>e.Name.LocalName=="table");
 kpi+=root.DescendantsAndSelf().Count(e=>e.Attribute("class")?.Value=="mathema-kpi");
 quotes+=root.DescendantsAndSelf().Count(e=>e.Attribute("class")?.Value=="mathema-quote");
 if(smoothing && root.Descendants().Any(e=>e.Name.LocalName=="path" && (e.Attribute("d")?.Value.Count(c=>c=='M')??0)>1))observedGap=true;
}
if(entry.TryGetProperty("expectedKpi",out var ek)&&kpi!=ek.GetInt32())throw new Exception("Missing or unexpected KPI grids.");
if(entry.TryGetProperty("expectedQuotes",out var eq)&&quotes!=eq.GetInt32())throw new Exception("Missing or unexpected response quotes.");
if(entry.GetProperty("requireTable").GetBoolean() && !observedTable)throw new Exception("No required HTML table found in migrated notebook.");
if(smoothing && !observedGap)throw new Exception("No split line path found in missing-observation notebook.");
if(svg!=expected)throw new Exception($"Expected {expected} migrated SVGs, got {svg}");
Console.WriteLine($"Migrated notebook {notebook}: real execution, {svg} PlotSharp SVGs XML validated. JupyterSharp host; visual review pending.");return 0;
