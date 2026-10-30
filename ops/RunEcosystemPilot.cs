#:property TargetFramework=net11.0
using System.Diagnostics;
using System.Text.Json;
var repo=Directory.GetCurrentDirectory();
var artifacts=@"D:\Dev\BuildArtifacts\ecosystem-pilot";Directory.CreateDirectory(artifacts);
int Run(string name,string command,params string[] args)
{
 var psi=new ProcessStartInfo(command){WorkingDirectory=repo,UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true};
 foreach(var arg in args)psi.ArgumentList.Add(arg);
 using var p=Process.Start(psi)!;var o=p.StandardOutput.ReadToEndAsync();var e=p.StandardError.ReadToEndAsync();p.WaitForExit();var text=o.GetAwaiter().GetResult()+e.GetAwaiter().GetResult();
 File.WriteAllText(Path.Combine(artifacts,name+".log"),text);Console.WriteLine($"{name}: {p.ExitCode}");if(p.ExitCode!=0)Console.WriteLine(text);return p.ExitCode;
}
if(Run("packages","dotnet","run","--file","ops/VerifyNet11Packages.cs")!=0)return 1;
var tools=Path.Combine(artifacts,"tools");
var executable=Path.Combine(tools,"jupytersharp.exe");
// install the pinned CLI, or move an existing pilot copy to it (an old copy would otherwise stay for ever and keep writing notebooks its old way)
var cliVerb=File.Exists(executable)?"update":"install";
if(Run("install-cli","dotnet","tool",cliVerb,"JupyterSharp.Cli","--version","0.3.1-net11-preview.5","--tool-path",tools,"--configfile",@"D:\Dev\BuildArtifacts\net11-pack\consumer\NuGet.Config")!=0)return 1;
var notebook=Path.Combine(repo,"notebooks","Ecosystem","01-signal-spectrum.ipynb");
if(Run("execute",executable,"run",notebook,"--in-place","--fail-fast")!=0)return 1;
using var doc=JsonDocument.Parse(File.ReadAllText(notebook));
int html=0,errors=0;var textOutput="";
foreach(var cell in doc.RootElement.GetProperty("cells").EnumerateArray())
if(cell.TryGetProperty("outputs",out var outputs))foreach(var output in outputs.EnumerateArray())
{
 if(output.TryGetProperty("output_type",out var type)&&type.GetString()=="error")errors++;
 if(output.TryGetProperty("data",out var data)&&data.TryGetProperty("text/html",out var h)) {var content=h.ValueKind==JsonValueKind.Array?string.Concat(h.EnumerateArray().Select(v=>v.GetString())):h.GetString();if(content?.Contains("<svg")==true)html++;}
 if(output.TryGetProperty("data",out var plainData)&&plainData.TryGetProperty("text/plain",out var plain))textOutput+=plain.ValueKind==JsonValueKind.Array?string.Concat(plain.EnumerateArray().Select(v=>v.GetString())):plain.GetString();
 if(output.TryGetProperty("text",out var t))textOutput+=t.ValueKind==JsonValueKind.Array?string.Concat(t.EnumerateArray().Select(v=>v.GetString())):t.GetString();
}
if(errors>0||html!=2||!textOutput.Contains("FFT_PEAK_5_VERIFIED")||!textOutput.Contains("PLOTSHARP_TWO_SVG_VERIFIED"))throw new Exception($"Notebook assertions failed: html={html}, errors={errors}, text={textOutput}");
File.WriteAllText(Path.Combine(artifacts,"summary.txt"),"SUCCESS: packaged JupyterSharp executed MathemaSharp FFT and emitted two PlotSharp SVG HTML outputs. No ProjectReference, no JupyterNet execution. Not a visual review.\n");
Console.WriteLine("SUCCESS: real JupyterSharp CLI, packaged mathematics and PlotSharp, two saved SVG outputs.");
return 0;
