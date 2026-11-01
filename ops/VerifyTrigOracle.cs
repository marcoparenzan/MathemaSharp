#:property TargetFramework=net11.0
#:project ../src/libs/MathemaSharp.Core/MathemaSharp.Core.csproj
// Copyright (c) 2026 Marco Parenzan. Licensed under the MIT License.
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using MathemaSharp;

const string fixture = "src/tests/MathemaSharp.Core.Tests/Fixtures/double-trig-oracle.json";
var output = @"D:\Dev\BuildArtifacts\validation\trig-oracle";
Directory.CreateDirectory(output);
int exit = 1;
try
{
    byte[]? first = null;
    for (int i = 0; i < 2; i++)
    {
        var start = new ProcessStartInfo("python") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        start.ArgumentList.Add("ops/GenerateTrigOracle.py");
        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(); Console.Write(await stdout + await stderr);
        if (process.ExitCode != 0) throw new Exception("Independent generation failed.");
        var bytes = File.ReadAllBytes(fixture);
        if (first is not null && !first.SequenceEqual(bytes)) throw new Exception("Generator is not deterministic.");
        first = bytes;
    }
    using var doc = JsonDocument.Parse(first!);
    if (doc.RootElement.GetProperty("acceptanceMaxUlp").GetInt32() != 1) throw new Exception("Acceptance policy changed.");
    var maxima = new Dictionary<string, ulong>();
    var samples = new List<(string Function, ulong[] Inputs, ulong Expected, ulong Actual, ulong Distance)>();
    foreach (var sample in doc.RootElement.GetProperty("cases").EnumerateArray())
    {
        var function = sample.GetProperty("function").GetString()!;
        var bits = sample.GetProperty("inputBits").EnumerateArray().Select(v => Convert.ToUInt64(v.GetString(), 16)).ToArray();
        double x = BitConverter.UInt64BitsToDouble(bits[0]);
        double actual = function switch
        {
            "Sin" => GenericMath.Sin(x), "Cos" => GenericMath.Cos(x), "Tan" => GenericMath.Tan(x),
            "Asin" => GenericMath.Asin(x), "Acos" => GenericMath.Acos(x), "Atan" => GenericMath.Atan(x),
            "Atan2" => GenericMath.Atan2(x, BitConverter.UInt64BitsToDouble(bits[1])),
            _ => throw new Exception("Unknown function")
        };
        var expected = Convert.ToUInt64(sample.GetProperty("expectedBits").GetString(), 16);
        var actualBits = BitConverter.DoubleToUInt64Bits(actual);
        ulong a = Ordered(expected), b = Ordered(actualBits);
        ulong distance = double.IsFinite(actual) ? (a > b ? a - b : b - a) : ulong.MaxValue;
        maxima[function] = Math.Max(maxima.GetValueOrDefault(function), distance);
        samples.Add((function, bits, expected, actualBits, distance));
    }
    using (var stream = File.Create(Path.Combine(output, "report.json")))
    using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
    {
        writer.WriteStartObject();
        writer.WriteString("GeneratedUtc", DateTime.UtcNow);
        writer.WriteString("Runtime", Environment.Version.ToString());
        writer.WriteString("OS", System.Runtime.InteropServices.RuntimeInformation.OSDescription);
        writer.WriteString("Reference", "mpmath 1.3.0, stable 160/200 digit references");
        writer.WriteNumber("AcceptanceMaxUlp", 1);
        writer.WriteString("FixtureSha256", Convert.ToHexString(SHA256.HashData(first!)));
        writer.WriteBoolean("DeterministicRegeneration", true);
        writer.WriteString("Scope", "Finite binary64 samples, not a universal or cross-platform bound.");
        writer.WriteStartObject("MaximumUlp");
        foreach (var pair in maxima) writer.WriteNumber(pair.Key, pair.Value);
        writer.WriteEndObject();
        writer.WriteStartArray("Cases");
        foreach (var sample in samples)
        {
            writer.WriteStartObject();
            writer.WriteString("Function", sample.Function);
            writer.WriteStartArray("InputBits");
            foreach (var bits in sample.Inputs) writer.WriteStringValue(bits.ToString("x16"));
            writer.WriteEndArray();
            writer.WriteString("ExpectedBits", sample.Expected.ToString("x16"));
            writer.WriteString("ActualBits", sample.Actual.ToString("x16"));
            writer.WriteNumber("UlpDistance", sample.Distance);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
    foreach (var pair in maxima) Console.WriteLine($"{pair.Key}: maximum {pair.Value} ULP");
    exit = maxima.Values.All(v => v <= 1) ? 0 : 1;
    Console.WriteLine("Report: " + output);
}
finally
{
    File.AppendAllText(@"D:\Dev\BuildArtifacts\logs\test-runs.log", $"{DateTime.Now:O} | MathemaSharp independent trig oracle | dotnet run --file ops/VerifyTrigOracle.cs | Exit {exit} | {output}\n");
}
return exit;
static ulong Ordered(ulong bits) => (bits & (1UL << 63)) != 0 ? ~bits : bits | (1UL << 63);
