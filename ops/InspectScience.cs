#:property TargetFramework=net11.0
using System.Diagnostics;
var root = @"D:\Dev\Science";
foreach (var name in args.Length > 0 ? args : new[] { "MathemaSharp", "PlotSharp", "JupyterSharp", "NDSharp" })
{
    var dir = Path.Combine(root, name);
    Console.WriteLine($"\n=== {name}: {dir} ===");
    if (!Directory.Exists(dir)) { Console.WriteLine("NOT FOUND"); continue; }
    bool Active(string p) => !p.Split(Path.DirectorySeparatorChar).Any(s => new[] { "bin", "obj", ".git", "imports", "node_modules", ".venv-oracles", "artifacts" }.Contains(s));
    var projects = Directory.EnumerateFiles(dir, "*.csproj", SearchOption.AllDirectories).Where(Active).ToArray();
    foreach (var p in projects) Console.WriteLine("PROJECT " + Path.GetRelativePath(dir, p));
    var solutions = Directory.EnumerateFiles(dir, "*.sln*", SearchOption.AllDirectories).Where(Active).Where(p => p.EndsWith(".sln") || p.EndsWith(".slnx")).ToArray();
    foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Where(Active).Where(p => new[] { ".csproj", ".props", ".targets", ".proj", ".json" }.Contains(Path.GetExtension(p))))
    {
        var lines = File.ReadAllLines(file);
        for (int i = 0; i < lines.Length; i++)
            if (lines[i].Contains(@"D:\Dev\2026", StringComparison.OrdinalIgnoreCase) || lines[i].Contains(@"D:\Dev\2027", StringComparison.OrdinalIgnoreCase) || lines[i].Contains("D:/Dev/2026", StringComparison.OrdinalIgnoreCase) || lines[i].Contains("D:/Dev/2027", StringComparison.OrdinalIgnoreCase))
                Console.WriteLine($"OLD PATH {Path.GetRelativePath(dir, file)}:{i + 1}: {lines[i].Trim()}");
    }
    var target = solutions.OrderBy(p => p.Length).FirstOrDefault();
    if (target is null) { Console.WriteLine("NO SOLUTION; build deferred"); continue; }
    foreach (var command in new[] { "build", "test" })
    {
        var psi = new ProcessStartInfo("dotnet") { WorkingDirectory = dir, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var arg in new[] { command, target, "-c", "Release", "--nologo" }) psi.ArgumentList.Add(arg);
        using var process = Process.Start(psi)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        var log = stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult();
        Directory.CreateDirectory(Path.Combine(@"D:\Dev\BuildArtifacts", "science-check"));
        File.WriteAllText(Path.Combine(@"D:\Dev\BuildArtifacts", "science-check", $"{name}-{command}.log"), log);
        Console.WriteLine($"RESULT {name} {command}: {process.ExitCode}");
        foreach (var line in log.Split('\n').Where(l => l.Contains("error ") || l.Contains("Failed") || l.Contains("Passed!") || l.Contains("Test summary") || l.Contains("Warning(s)") || l.Contains("Error(s)"))) Console.WriteLine(line.Trim());
    }
}
