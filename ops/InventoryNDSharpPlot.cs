#:property TargetFramework=net10.0
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using System.Text.RegularExpressions;
var project="../NDSharp/src/NDSharp.Plot/NDSharp.Plot.csproj";
var xml=XDocument.Load(project);var packages=new JsonArray();var references=new JsonArray();var files=new JsonArray();
foreach(var p in xml.Descendants().Where(e=>e.Name.LocalName=="PackageReference"))
 packages.Add((JsonNode)new JsonObject { ["Name"]=(string?)p.Attribute("Include"),["Version"]=(string?)p.Attribute("Version") });
foreach(var p in xml.Descendants().Where(e=>e.Name.LocalName=="ProjectReference"))
{
 var relative=(string)p.Attribute("Include")!;
 var resolved=Path.GetFullPath(Path.Combine(Path.GetDirectoryName(project)!,relative.Replace('\\',Path.DirectorySeparatorChar)));
 if(!File.Exists(resolved))throw new Exception("Missing declared dependency: "+relative);
 references.Add((JsonNode)new JsonObject { ["Path"]=relative,["Exists"]=true });
}
foreach(var path in new[]{project,"../NDSharp/src/NDSharp.Plot/Contour.cs","../NDSharp/src/NDSharp.Plot/Ticks.cs","../PlotSharp/src/PlotSharp/PlotSharp/ContourKernel.cs","../PlotSharp/src/PlotSharp/PlotSharp/RectilinearContours.cs","../PlotSharp/src/PlotSharp/PlotSharp/RectilinearFieldSampler.cs","../PlotSharp/src/PlotSharp/PlotSharp/ContourBands.cs","../NDSharp/src/NDSharp.Plot.Tests/ContourBaselineTests.cs","../NDSharp/src/NDSharp.Plot/Fonts/LICENSE_DEJAVU"})
 files.Add((JsonNode)new JsonObject { ["Path"]=path,["Sha256"]=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) });
var psi=new ProcessStartInfo("dotnet") { UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true };
foreach(var a in new[]{"test","../NDSharp/src/NDSharp.Plot.Tests/NDSharp.Plot.Tests.csproj","--no-build","--verbosity","quiet"})psi.ArgumentList.Add(a);
using var process=Process.Start(psi)!;var stdout=process.StandardOutput.ReadToEndAsync();var stderr=process.StandardError.ReadToEndAsync();process.WaitForExit();var log=await stdout+await stderr;
Directory.CreateDirectory("ops/inventory");File.WriteAllText("ops/inventory/ndsharp-plot-tests.log",log);
var match=Regex.Match(log,@"Passed:\s*(\d+)");
int verifiedExit=process.ExitCode;
if(verifiedExit==0 && (!match.Success||int.Parse(match.Groups[1].Value)==0))verifiedExit=1;
var notes=new JsonArray();
foreach(var note in new[]{
 "Active source NDSharp/src/NDSharp.Plot, not imported snapshot. PlotSharp dependency added for line-contour geometry only.",
 "Line-contour geometry delegates to RectilinearContours/ContourKernel. Xs/Ys remain physical and nonuniform, not globally scaled sample indices.",
 "NDSharp retains endpoint-level suppression; shared kernel preserves invalid corner-sum skip, edge direction/order and zero-length segments. Malformed axes/data or non-finite coordinates/levels are rejected.",
 "AutoLevels uses matplotlib-shaped Ticks fineSteps; do not replace with PlotSharp NiceTicks.",
 "Filled-contour sampling and inclusive band/midpoint selection delegate to RectilinearFieldSampler/ContourBands; alpha/colors, Skia paint/styles, Figure/Axes and registry remain NDSharp.",
 "New baseline tests inspect PNG pixels in memory: physical grid, non-finite cell, endpoint levels and filled bands. No saved images.",
 "Licenses/fonts metadata recorded, not complete release/legal audit; preserve DejaVu notice."}) notes.Add((JsonNode)JsonValue.Create(note)!);
var report=new JsonObject { ["GeneratedUtc"]=DateTime.UtcNow.ToString("O"),["TargetFramework"]=xml.Descendants().First(e=>e.Name.LocalName=="TargetFramework").Value,
 ["ProjectReferences"]=references,["Packages"]=packages,["Files"]=files,["Notes"]=notes,["LineContourGeometryDelegated"]=true,["FilledBilinearSamplingDelegated"]=true,["FilledBandSelectionDelegated"]=true,["FamilyMigrationComplete"]=false,["TestExitCode"]=verifiedExit,
 ["PassedTests"]=match.Success?int.Parse(match.Groups[1].Value):0,["Scope"]="Line-contour geometry migration and targeted baseline, not full NDSharp solution, notebook integration, visual review or publishing." };
File.WriteAllText("ops/inventory/ndsharp-plot-contract.json",report.ToJsonString(new JsonSerializerOptions { WriteIndented=true }));
Console.WriteLine($"NDSharp.Plot: line-contour geometry delegated, family incomplete, targeted tests exit {verifiedExit}, passed {(match.Success?match.Groups[1].Value:"unknown")}. No images.");
return verifiedExit;
