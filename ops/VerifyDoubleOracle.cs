#:property TargetFramework=net11.0
#:project ../src/libs/MathemaSharp.Core/MathemaSharp.Core.csproj
// Copyright (c) 2026 Marco Parenzan. Licensed under the MIT license.
using System.Diagnostics;
using System.Text.Json;
using MathemaSharp;

var logs = @"D:\Dev\BuildArtifacts\logs";
Directory.CreateDirectory(logs);
var output = Path.Combine(@"D:\Dev\BuildArtifacts", "validation", "double-oracle");
Directory.CreateDirectory(output);
int exit = 1;
try
{
    var start = new ProcessStartInfo("python") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
    start.ArgumentList.Add("ops/GenerateDoubleOracle.py");
    using var process = Process.Start(start)!;
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    Console.Write(await stdout + await stderr);
    if (process.ExitCode != 0) throw new Exception("Reference generation failed.");
    using var document = JsonDocument.Parse(File.ReadAllText("src/tests/MathemaSharp.Core.Tests/Fixtures/double-elementary-oracle.json"));
    var cases = new List<(string Function, ulong Input, ulong Expected, ulong Actual, ulong Distance)>();
    var maximum = new Dictionary<string, ulong>();
    foreach (var sample in document.RootElement.GetProperty("cases").EnumerateArray())
    {
        string function = sample.GetProperty("function").GetString()!;
        ulong inputBits = Convert.ToUInt64(sample.GetProperty("inputBits").GetString(), 16);
        ulong expectedBits = Convert.ToUInt64(sample.GetProperty("expectedBits").GetString(), 16);
        double input = BitConverter.UInt64BitsToDouble(inputBits);
        double actual = function switch { "Sqrt" => GenericMath.Sqrt(input), "Exp" => GenericMath.Exp(input), "Log" => GenericMath.Log(input), _ => throw new Exception("Unknown function") };
        ulong actualBits = BitConverter.DoubleToUInt64Bits(actual);
        ulong a = Ordered(actualBits), b = Ordered(expectedBits);
        ulong distance = a > b ? a - b : b - a;
        maximum[function] = Math.Max(maximum.GetValueOrDefault(function), distance);
        cases.Add((function, inputBits, expectedBits, actualBits, distance));
    }
    using (var stream = File.Create(Path.Combine(output, "report.json")))
    using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
    {
        writer.WriteStartObject();
        writer.WriteString("GeneratedUtc", DateTime.UtcNow);
        writer.WriteString("Reference", document.RootElement.GetProperty("reference").GetString());
        writer.WriteString("Runtime", Environment.Version.ToString());
        writer.WriteString("OS", System.Runtime.InteropServices.RuntimeInformation.OSDescription);
        writer.WriteString("Scope", "Finite binary64 corpus; 100/200-digit decimal agreement, not a universal correct-rounding proof.");
        writer.WriteStartObject("MaximumUlp");
        foreach (var pair in maximum) writer.WriteNumber(pair.Key, pair.Value);
        writer.WriteEndObject();
        writer.WriteStartArray("Cases");
        foreach (var sample in cases)
        {
            writer.WriteStartObject();
            writer.WriteString("Function", sample.Function);
            writer.WriteString("InputBits", $"{sample.Input:x16}");
            writer.WriteString("ExpectedBits", $"{sample.Expected:x16}");
            writer.WriteString("ActualBits", $"{sample.Actual:x16}");
            writer.WriteNumber("UlpDistance", sample.Distance);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
    foreach (var pair in maximum) Console.WriteLine($"{pair.Key}: maximum {pair.Value} ULP");
    exit = maximum.Values.All(value => value <= 1) ? 0 : 1;
    Console.WriteLine("Report: " + output);
}
finally
{
    File.AppendAllText(Path.Combine(logs, "test-runs.log"), $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Repo: MathemaSharp | What: independent double reference generator/verification | Command: dotnet run --file ops/VerifyDoubleOracle.cs | Result: exit {exit} | Report: {output}\n");
}
return exit;
static ulong Ordered(ulong bits) => (bits & (1UL << 63)) != 0 ? ~bits : bits | (1UL << 63);
