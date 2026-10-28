#:property TargetFramework=net11.0
using System.Diagnostics;
using System.Text.Json;
var notebook=Path.Combine(Directory.GetCurrentDirectory(),"notebooks","Ecosystem","02-typed-language-interop.ipynb");
var cli=@"D:\Dev\BuildArtifacts\ecosystem-pilot\tools\jupytersharp.exe";
var output=@"D:\Dev\BuildArtifacts\ecosystem-pilot";
var psi=new ProcessStartInfo(cli){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true};
foreach(var a in new[]{"run",notebook,"--in-place","--fail-fast"})psi.ArgumentList.Add(a);
using var p=Process.Start(psi)!;var stdout=p.StandardOutput.ReadToEndAsync();var stderr=p.StandardError.ReadToEndAsync();p.WaitForExit();var log=await stdout+await stderr;
File.WriteAllText(Path.Combine(output,"language-interop.log"),log);Console.WriteLine(log);if(p.ExitCode!=0)return p.ExitCode;
using var doc=JsonDocument.Parse(File.ReadAllText(notebook));var text="";
foreach(var cell in doc.RootElement.GetProperty("cells").EnumerateArray())
if(cell.TryGetProperty("outputs",out var outputs))foreach(var o in outputs.EnumerateArray())
{
 if(o.GetProperty("output_type").GetString()=="error")throw new Exception("Notebook error output");
 if(o.TryGetProperty("data",out var data)&&data.TryGetProperty("text/plain",out var value))text+=value.ValueKind==JsonValueKind.Array?string.Concat(value.EnumerateArray().Select(v=>v.GetString())):value.GetString();
}
foreach(var marker in new[]{"PYSHARP_GENERIC_TYPE_PRECISION_OK","MATSHARP_TYPED_SOLVE_PRECISION_OK"})if(!text.Contains(marker))throw new Exception("Missing assertion "+marker);
Console.WriteLine("Packaged language interop notebook: OK");return 0;
