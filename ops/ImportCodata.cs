#:property TargetFramework=net11.0
#:project ../src/libs/MathemaSharp.Core/MathemaSharp.Core.csproj
// Copyright (c) 2026 Marco Parenzan. Licensed under the MIT license.
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MathemaSharp;

const string source = "https://physics.nist.gov/cuu/Constants/Table/allascii.txt";
const string expectedHash = "77fb90e66c40db3e6eb16630bc9c88e4c7c8beddbe5e71be406f2f26e3f67e67";
var artifacts = @"D:\Dev\BuildArtifacts";
var snapshot = Path.Combine(artifacts, "sources", "nist-codata", expectedHash + ".txt");
int exit = 1;
try
{
    byte[] bytes;
    if (File.Exists(snapshot)) bytes = File.ReadAllBytes(snapshot);
    else
    {
        using var http = new HttpClient();
        bytes = await http.GetByteArrayAsync(source);
    }
    var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    if (hash != expectedHash) throw new InvalidOperationException("NIST source checksum changed; review before repinning.");
    Directory.CreateDirectory(Path.GetDirectoryName(snapshot)!);
    if (!File.Exists(snapshot)) File.WriteAllBytes(snapshot, bytes);
    var records = CodataParser.Parse(Encoding.UTF8.GetString(bytes), 2022);
    Directory.CreateDirectory("ops/data");
    using (var stream = File.Create("ops/data/codata2022.json"))
    using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
    {
        writer.WriteStartObject();
        writer.WriteString("sourceUrl", source); writer.WriteString("sha256", hash); writer.WriteNumber("edition", 2022);
        writer.WriteStartArray("records");
        foreach (var row in records)
        {
            writer.WriteStartObject();
            writer.WriteString("name", row.Name); writer.WriteString("valueText", row.ValueText);
            writer.WriteString("uncertaintyText", row.UncertaintyText); writer.WriteString("unit", row.Unit);
            writer.WriteString("decimalValue", row.DecimalValue); writer.WriteString("decimalUncertainty", row.DecimalUncertainty);
            writer.WriteBoolean("isExact", row.IsExact); writer.WriteBoolean("valueIsTruncated", row.ValueIsTruncated);
            writer.WriteEndObject();
        }
        writer.WriteEndArray(); writer.WriteEndObject();
    }
    Console.WriteLine($"CODATA2022: {records.Count} records, {records.Count(x => x.IsExact)} exact markers, {records.Count(x => x.ValueIsTruncated)} truncated values.");
    Console.WriteLine("SHA256: " + hash);
    exit = 0;
}
finally
{
    Directory.CreateDirectory(Path.Combine(artifacts, "logs"));
    File.AppendAllText(Path.Combine(artifacts, "logs", "test-runs.log"), $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Repo: MathemaSharp | What: CODATA source import | Command: dotnet run --file ops/ImportCodata.cs | Result: exit{exit}\n");
}
return exit;
