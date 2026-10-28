#:property TargetFramework=net11.0
using System.Diagnostics;
using System.Text.RegularExpressions;
var root = @"D:\Dev\Science";
string[] names = ["MathemaSharp", "NDSharp", "PlotSharp", "JupyterSharp", "PySharp", "MatSharp"];
var apply = args.Contains("--apply");
var verify = args.Contains("--verify");
var output = @"D:\Dev\BuildArtifacts\net11-migration";
Directory.CreateDirectory(output);
bool Active(string p) => !p.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(s => new[] { "bin", "obj", ".git", ".ralfai", "imports", "node_modules", ".venv", ".venv-oracles", "artifacts" }.Contains(s));
int failures = 0;
foreach (var name in names.Where(n => !args.Any(a => names.Contains(a)) || args.Contains(n)))
{
    var dir = Path.Combine(root, name);
    if (!Directory.Exists(dir)) { Console.WriteLine($"MISSING {dir}"); failures++; continue; }
    var files = Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Where(Active).Where(p => new[] { ".csproj", ".props", ".targets", ".proj", ".cs", ".ps1", ".sh" }.Contains(Path.GetExtension(p)) || Path.GetFileName(p) == "global.json").ToArray();
    foreach (var path in files)
    {
        var text = File.ReadAllText(path);
        // Only framework identifiers, not arbitrary version numbers or historical prose.
        var updated = Regex.Replace(text, @"\bnet(?:[6-9]|10)\.0\b", "net11.0");
        if (Path.GetFileName(path) == "global.json")
            updated = Regex.Replace(updated, "(\"version\"\\s*:\\s*\")[^\"]+(\")", m => m.Groups[1].Value + "11.0.100-rc.3.26522.148" + m.Groups[2].Value);
        if (updated == text) continue;
        Console.WriteLine($"{(apply ? "UPDATE" : "PLAN")} {name}/{Path.GetRelativePath(dir, path)}");
        if (apply)
        {
            var backup = Path.Combine(output, "backup", name, Path.GetRelativePath(dir, path));
            Directory.CreateDirectory(Path.GetDirectoryName(backup)!);
            if (!File.Exists(backup)) File.Copy(path, backup);
            File.WriteAllText(path, updated);
        }
    }
    if (!verify) continue;
    var solutions = Directory.EnumerateFiles(dir, "*.sln*", SearchOption.AllDirectories).Where(Active).Where(p => p.EndsWith(".sln") || p.EndsWith(".slnx")).OrderBy(p => p.Length).ToArray();
    var targets = solutions.Length > 0 ? new[] { solutions[0] } : Directory.EnumerateFiles(dir, "*.csproj", SearchOption.AllDirectories).Where(Active).ToArray();
    foreach (var target in targets)
    foreach (var command in new[] { "build", "test" })
    {
        var psi = new ProcessStartInfo("dotnet") { WorkingDirectory = dir, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var a in new[] { command, target, "-c", "Release", "--nologo" }) psi.ArgumentList.Add(a);
        using var process = Process.Start(psi)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        var log = await stdout + await stderr;
        File.WriteAllText(Path.Combine(output, $"{name}-{Path.GetFileNameWithoutExtension(target)}-{command}.log"), log);
        Console.WriteLine($"RESULT {name} {command}: {process.ExitCode}");
        foreach (var line in log.Split('\n').Where(l => l.Contains("error ") || l.Contains("Failed ") || l.Contains("Error Message:") || l.Contains("Expected:") || l.Contains("Actual:") || l.Contains("Passed!") || l.Contains("Failed:") || l.Contains("Warning(s)") || l.Contains("Error(s)"))) Console.WriteLine(line.Trim());
        if (process.ExitCode != 0) failures++;
    }
}
return failures == 0 ? 0 : 1;
