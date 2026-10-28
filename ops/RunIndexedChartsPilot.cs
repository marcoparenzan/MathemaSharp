#:property TargetFramework=net11.0
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Xml.Linq;
var notebook=Path.GetFullPath("notebooks/Ecosystem/03-indexed-charts.ipynb");
var document=JsonNode.Parse(File.ReadAllText(notebook))!;
foreach(var cell in document["cells"]!.AsArray().Where(c=>c!["cell_type"]!.GetValue<string>()=="code")) { cell!["outputs"]=new JsonArray(); cell["execution_count"]=null; }
File.WriteAllText(notebook,document.ToJsonString());
var psi=new ProcessStartInfo(@"D:\Dev\BuildArtifacts\ecosystem-pilot\tools\jupytersharp.exe"){UseShellExecute=false};
foreach(var arg in new[]{"run",notebook,"--in-place","--fail-fast"})psi.ArgumentList.Add(arg);
using var process=Process.Start(psi)!;process.WaitForExit();if(process.ExitCode!=0)return process.ExitCode;
document=JsonNode.Parse(File.ReadAllText(notebook))!;
int count=0;bool marker=false;
foreach(var cell in document["cells"]!.AsArray())
foreach(var output in cell?["outputs"]?.AsArray()??new JsonArray())
{
 if(output!["output_type"]!.GetValue<string>()=="error")throw new Exception("Notebook error");
 marker|=output.ToJsonString().Contains("INDEXED_FACADE_EXECUTED");
 var html=output["data"]?["text/html"];
 if(html is null)continue;
 var text=html is JsonArray a?string.Concat(a.Select(n=>n!.GetValue<string>())):html.GetValue<string>();
 var root=XElement.Parse(text);
 count+=root.DescendantsAndSelf().Count(e=>e.Name.LocalName=="svg");
}
if(count!=4||!marker)throw new Exception($"Expected four SVGs and execution marker, got {count}, {marker}");
Console.WriteLine("Indexed facade: four SVGs executed and XML validated; visual review still required.");return 0;
