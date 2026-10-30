#:property TargetFramework=net11.0
// Runs the NDSharp notebooks (NDSharp/src/notebooks) in JupyterSharp, built from source. They reference the NDSharp packages of the local feed (versions set in NDSharp.Notebooks/Program.cs) with `#r "nuget: ..."`,
// so this also proves that the pinned packages restore from the local feed and that the C# kernel resolves them.
// A notebook passes when the CLI exits 0, no cell produced an error output, and at least one cell produced output.
// The notebooks write files (PNGs) into their working directory: that is a scratch folder under BuildArtifacts, never the repository.
using System.Diagnostics;
using System.Text.Json;

var notebooks = Path.GetFullPath("../NDSharp/src/notebooks");
var cli = Path.GetFullPath("../JupyterSharp/src/JupyterSharp.Cli");
var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
var outDir = Path.Combine(@"D:\Dev\BuildArtifacts", "ndsharp-notebooks", stamp);
var scratch = Path.Combine(@"D:\Dev\BuildArtifacts", "tmp", "ndsharp-notebooks");
Directory.CreateDirectory(outDir);
Directory.CreateDirectory(scratch);

// Notebooks that are known not to run, with the reason. They are reported, never silently skipped. None at the moment.
var excluded = new Dictionary<string, string>();
const int MinimumNotebooks = 14;

static (int Code, string Log) Run(string workDir, params string[] args)
{
    var psi = new ProcessStartInfo("dotnet") { WorkingDirectory = workDir, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
    foreach (var a in args) psi.ArgumentList.Add(a);
    using var p = Process.Start(psi)!;
    var stdout = p.StandardOutput.ReadToEndAsync();
    var stderr = p.StandardError.ReadToEndAsync();
    p.WaitForExit();
    return (p.ExitCode, stdout.Result + stderr.Result);
}

var build = Run(scratch, "build", cli, "-c", "Release");
File.WriteAllText(Path.Combine(outDir, "build.log"), build.Log);
if (build.Code != 0) { Console.WriteLine("JupyterSharp CLI build failed:\n" + build.Log[^Math.Min(build.Log.Length, 2000)..]); return 1; }

var files = Directory.GetFiles(notebooks, "*.ipynb").OrderBy(f => f, StringComparer.Ordinal).ToArray();
var passed = 0; var failed = new List<string>();
foreach (var file in files)
{
    var name = Path.GetFileNameWithoutExtension(file);
    if (excluded.TryGetValue(name, out var reason)) { Console.WriteLine($"{name}: EXCLUDED - {reason}"); continue; }

    var output = Path.Combine(outDir, name + ".ipynb");
    var result = Run(scratch, "run", "--no-build", "-c", "Release", "--project", cli, "--", "run", file, "--fail-fast", "--output", output);
    File.WriteAllText(Path.Combine(outDir, name + ".log"), result.Log);

    int errors = 0, outputs = 0; string first = "";
    if (File.Exists(output))
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(output));
        foreach (var cell in doc.RootElement.GetProperty("cells").EnumerateArray())
        {
            if (cell.GetProperty("cell_type").GetString() != "code" || !cell.TryGetProperty("outputs", out var outs)) continue;
            foreach (var o in outs.EnumerateArray())
            {
                outputs++;
                if (o.GetProperty("output_type").GetString() == "error") { errors++; if (first.Length == 0 && o.TryGetProperty("evalue", out var v)) first = v.GetString() ?? ""; }
            }
        }
    }
    var ok = result.Code == 0 && errors == 0 && outputs > 0;
    Console.WriteLine($"{name}: {(ok ? "ok" : "FAILED")}  exit={result.Code} outputs={outputs} errors={errors}" + (first.Length > 0 ? " - " + first.Split('\n')[0][..Math.Min(first.Split('\n')[0].Length, 160)] : ""));
    if (ok) passed++; else failed.Add(name);
}

Console.WriteLine($"{passed} notebooks passed, {failed.Count} failed, {excluded.Count} excluded. Output: {outDir}");
if (passed + failed.Count < MinimumNotebooks) { Console.WriteLine($"Expected at least {MinimumNotebooks} notebooks to run, found {passed + failed.Count}."); return 1; }
return failed.Count == 0 ? 0 : 1;
