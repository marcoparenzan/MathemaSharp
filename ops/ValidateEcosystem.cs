#:property TargetFramework=net11.0
#:project ../src/tools/Ecosystem.Validation/Ecosystem.Validation/Ecosystem.Validation.csproj
using System.Diagnostics;
var options=Ecosystem.Validation.EvidenceOptions.Parse(args);
string[] imageArguments=options.SaveImages?[]:["--","--no-images"];
var artifacts=Path.Combine(@"D:\Dev\BuildArtifacts","ecosystem-validation",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
Directory.CreateDirectory(artifacts);
using var migratedManifest=System.Text.Json.JsonDocument.Parse(File.ReadAllText("ops/migrated-notebooks.json"));
(string Name,string[] Args)[] steps = [
    ("build-test",["run","--file","ops/UpgradeNet11.cs","--","--verify"]),
    ("security",["run","--file","ops/AuditSciencePackages.cs"]),
    ("baseline",["msbuild","ops/validate.proj","-t:Baseline"]),
    ("pysharp-kernel",["run","--file","ops/BuildPySharpKernel.cs"]),
    ("pysharp-source-plot-kernel",["run","--file","ops/BuildPySharpKernel.cs","--","--source-plots"]),
    ("ndsharp-contour-notebook",["run","--file","ops/RunNDSharpContourNotebook.cs"]),
    ("pysharp-notebooks",["msbuild","ops/pysharp-notebooks.proj","-t:Run"]),
    ("matsharp-notebook",["msbuild","ops/matsharp-notebook.proj","-t:Run"]),
    ("packaged-pilot",["run","--file","ops/RunEcosystemPilot.cs"]),
    ("language-interop-pilot",["run","--file","ops/RunLanguageInteropPilot.cs"]),
    ("indexed-charts-pilot",["run","--file","ops/RunIndexedChartsPilot.cs"]),
    .. migratedManifest.RootElement.EnumerateArray().Select(e => ("migrated-"+e.GetProperty("id").GetString(), new[] {"run","--file","ops/RunMigratedCoreNotebook.cs","--","--id",e.GetProperty("id").GetString()!})),
    ("navier-cli-build",["build","../articoli/2026.10 NavierStokes/src/NavierStokes.Cli/NavierStokes.Cli.csproj","-c","Release"]),
    ("navier-web-build",["build","../articoli/2026.10 NavierStokes/src/NavierStokes.Web/NavierStokes.Web.csproj","-c","Release"]),
    ("navier-web-ui",["run","--file","ops/VerifyNavierWeb.cs"]),
    ("navier-mobile-layout",["run","--file","ops/VerifyNavierWeb.cs","--","--mobile"]),
    ("navier-quadro-cancellation",["run","--file","ops/VerifyNavierWeb.cs","--","--quadro"]),
    ("navier-fischio-cancellation",["run","--file","ops/VerifyNavierWeb.cs","--","--fischio"]),
    ("navier-numeric-selftest",["run","--project","../articoli/2026.10 NavierStokes/src/NavierStokes.Cli/NavierStokes.Cli.csproj","-c","Release","--no-build","--","selftest"]),
    ("navier-adoption",["run","--file","ops/VerifyNavierAdoption.cs",..imageArguments]),
    ("visual-raster-evidence",["run","--file","ops/RasterizeNotebookEvidence.cs",..imageArguments]),
    ("browser-charts",["run","--file","ops/VerifyBrowserCharts.cs",..imageArguments]),
    ("pysharp-trx",["test","../PySharp/src/PySharp.Tests/PySharp.Tests.csproj","-c","Release","--logger","trx;LogFileName=pysharp.trx","--results-directory",Path.Combine(artifacts,"test-results"),"--verbosity","quiet"]),
    ("skip-report",["run","--file","ops/ReportSkippedTests.cs","--",Path.Combine(artifacts,"test-results","pysharp.trx")])
];
var reportPaths=new Dictionary<string,string>
{
 ["ndsharp-contour-notebook"]="ops/validation/ndsharp-contour-notebook/report.json",
 ["navier-web-ui"]="ops/validation/navier-web-smoke/report.json",
 ["navier-mobile-layout"]="ops/validation/navier-mobile-smoke/report.json",
 ["navier-quadro-cancellation"]="ops/validation/navier-quadro-smoke/report.json",
 ["navier-fischio-cancellation"]="ops/validation/navier-fischio-smoke/report.json",
 ["navier-adoption"]=options.SaveImages?"ops/visual/navier/report.json":"ops/validation/navier-adoption-no-images/report.json",
 ["visual-raster-evidence"]=options.SaveImages?"ops/visual/manifest.json":"ops/validation/raster-no-images/manifest.json",
 ["browser-charts"]=options.SaveImages?"ops/visual/browser/report.json":"ops/validation/browser-no-images/report.json"
};
var summary=new List<string>();int failed=0;
summary.Add("Save new visual image files: "+options.SaveImages);
Console.WriteLine(summary[0]);
foreach(var step in steps)
{
    var psi=new ProcessStartInfo("dotnet"){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true};
    foreach(var arg in step.Args)psi.ArgumentList.Add(arg);
    var started=DateTime.UtcNow;
    using var p=Process.Start(psi)!;var stdout=p.StandardOutput.ReadToEndAsync();var stderr=p.StandardError.ReadToEndAsync();p.WaitForExit();
    var finished=DateTime.UtcNow;
    File.WriteAllText(Path.Combine(artifacts,step.Name+".log"),await stdout+await stderr);
    int exit=p.ExitCode;
    if(exit==0 && reportPaths.TryGetValue(step.Name,out var report))
    {
        try
        {
            if(!File.Exists(report))throw new Exception("Expected JSON evidence missing.");
            bool? imagePolicy=step.Name is "navier-adoption" or "visual-raster-evidence" or "browser-charts"?options.SaveImages:null;
            Ecosystem.Validation.EvidenceReport.Validate(File.ReadAllText(report),started,finished,imagePolicy);
            File.Copy(report,Path.Combine(artifacts,step.Name+".json"),overwrite:true);
        }
        catch(Exception ex){exit=1;File.AppendAllText(Path.Combine(artifacts,step.Name+".log"),"\nEvidence validation: "+ex.Message);}
    }
    var line=$"{step.Name}: exit {exit}";Console.WriteLine(line);summary.Add(line);if(exit!=0)failed++;
}
summary.Add("Failed steps: "+failed);
summary.Add("No failures are waived. Full gate success requires every step to exit zero; skipped tests remain explicitly reported by their suites.");
File.WriteAllLines(Path.Combine(artifacts,"summary.txt"),summary);
Console.WriteLine("Report: "+artifacts);
return failed==0?0:1;
