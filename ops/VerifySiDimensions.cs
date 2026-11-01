#:property TargetFramework=net11.0
#:project ../src/libs/MathemaSharp.Core/MathemaSharp.Core.csproj
// Copyright (c) 2026 Marco Parenzan. Licensed under the MIT license.
using System.Text.Json;
using MathemaSharp;

var artifacts = @"D:\Dev\BuildArtifacts";
var output = Path.Combine(artifacts, "validation", "si-dimensions");
Directory.CreateDirectory(output);
using var document = JsonDocument.Parse(File.ReadAllText("ops/data/codata2022.json"));
var groups = document.RootElement.GetProperty("records").EnumerateArray()
    .GroupBy(row => row.GetProperty("unit").GetString()!).OrderBy(group => group.Key, StringComparer.Ordinal).ToArray();
int supported = 0, unsupported = 0;
using (var stream = File.Create(Path.Combine(output, "report.json")))
using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
{
    writer.WriteStartObject(); writer.WriteString("GeneratedUtc", DateTime.UtcNow);
    writer.WriteString("SourceSha256", document.RootElement.GetProperty("sha256").GetString());
    writer.WriteString("Scope", "Coherent SI dimensions only; no scale conversion, prefixes, non-SI units or slash/parenthesis inference.");
    writer.WriteStartArray("Units");
    foreach (var group in groups)
    {
        bool parsed = SiUnitDimensions.TryParse(group.Key, out var d);
        if (parsed) supported += group.Count(); else unsupported += group.Count();
        writer.WriteStartObject(); writer.WriteString("Text", group.Key); writer.WriteNumber("Rows", group.Count());
        writer.WriteBoolean("Supported", parsed);
        if (parsed)
        {
            writer.WriteStartArray("Exponents");
            foreach (var exponent in new[] { d.Metres, d.Kilograms, d.Seconds, d.Amperes, d.Kelvins, d.Moles, d.Candelas }) writer.WriteNumberValue(exponent);
            writer.WriteEndArray();
        }
        else writer.WriteNull("Exponents");
        writer.WriteEndObject();
    }
    writer.WriteEndArray(); writer.WriteNumber("SupportedRows", supported); writer.WriteNumber("UnsupportedRows", unsupported); writer.WriteEndObject();
}
Console.WriteLine($"SI dimensions: {supported} supported rows, {unsupported} unsupported rows.");
Console.WriteLine("Report: " + output);
Directory.CreateDirectory(Path.Combine(artifacts, "logs"));
File.AppendAllText(Path.Combine(artifacts, "logs", "test-runs.log"), $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Repo: MathemaSharp | What: SI dimension coverage | Command: dotnet run --file ops/VerifySiDimensions.cs | Result: {supported} supported/{unsupported} explicitly unsupported\n");
return supported + unsupported == 355 ? 0 : 1;
