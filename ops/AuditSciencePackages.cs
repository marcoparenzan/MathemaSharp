#:property TargetFramework=net11.0
using System.Diagnostics;
using System.Text.Json;
var root=@"D:\Dev\Science";
var output=Path.Combine(@"D:\Dev\BuildArtifacts","security-audit",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
Directory.CreateDirectory(output);
int failures=0;
foreach(var name in new[]{"MathemaSharp","NDSharp","PlotSharp","JupyterSharp","PySharp","MatSharp"})
{
var dir=Path.Combine(root,name);
var solution=Directory.EnumerateFiles(dir,"*.sln*",SearchOption.AllDirectories).Where(p=>!p.Split('\\').Any(s=>new[]{"imports","obj","bin",".git",".ralfai"}.Contains(s))).Where(p=>p.EndsWith(".sln")||p.EndsWith(".slnx")).OrderBy(p=>p.Length).First();
var psi=new ProcessStartInfo("dotnet"){WorkingDirectory=dir,UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true};
foreach(var a in new[]{"list",solution,"package","--vulnerable","--include-transitive","--format","json"})psi.ArgumentList.Add(a);
using var process=Process.Start(psi)!;var stdout=process.StandardOutput.ReadToEndAsync();var stderr=process.StandardError.ReadToEndAsync();process.WaitForExit();var text=await stdout;var err=await stderr;
File.WriteAllText(Path.Combine(output,name+".log"),text+err);
if(process.ExitCode!=0){Console.WriteLine($"{name}: audit command failed ({process.ExitCode})");failures++;continue;}
try
{
// CLI may print restore progress before its JSON payload.
int start=text.IndexOf('{');if(start<0)throw new Exception("No JSON report");
var json=text[start..];File.WriteAllText(Path.Combine(output,name+".json"),json);
using var report=JsonDocument.Parse(json);
int vulnerable=0;bool auditErrors=false;
void Walk(JsonElement node)
{
 if(node.ValueKind==JsonValueKind.Object)
 {
  if(node.TryGetProperty("vulnerabilities",out var issues)&&issues.ValueKind==JsonValueKind.Array&&issues.GetArrayLength()>0)
  {vulnerable++;Console.WriteLine(name+": "+node.GetRawText());}
  if(node.TryGetProperty("problemType",out _))auditErrors=true;
  foreach(var p in node.EnumerateObject())Walk(p.Value);
 }
 else if(node.ValueKind==JsonValueKind.Array)foreach(var item in node.EnumerateArray())Walk(item);
}
Walk(report.RootElement);
Console.WriteLine($"{name}: vulnerable package entries = {vulnerable}; report errors = {auditErrors}");
if(vulnerable>0||auditErrors)failures++;
}
catch(Exception e){Console.WriteLine(name+": invalid audit report: "+e.Message);failures++;}
}
File.WriteAllText(Path.Combine(output,"summary.txt"),$"Repositories with findings/errors: {failures}\nThis audit covers solution package graphs using current advisory sources, not all security risks or installed runtimes.\n");
Console.WriteLine("Reports: "+output);
return failures==0?0:1;
