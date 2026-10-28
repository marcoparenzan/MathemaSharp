#:property TargetFramework=net11.0
#:project ../src/tools/Ecosystem.Validation/Ecosystem.Validation/Ecosystem.Validation.csproj
using Ecosystem.Validation;
using System.Text;
var path=args.Length>0?args[0]:@"D:\Dev\BuildArtifacts\ecosystem-skips\pysharp.trx";
var evidence=TrxEvidence.Parse(File.ReadAllText(path));
var report=new StringBuilder("# PySharp skipped-test evidence\n\nGenerated from a real TRX test execution, not source discovery. No skips are waived.\n\n");
report.AppendLine($"Source: `{path}`");
report.AppendLine($"Generated UTC: {DateTime.UtcNow:O}");
report.AppendLine($"Results: {evidence.Results}; passed: {evidence.Passed}; skipped: {evidence.Skipped.Count}; failed: {evidence.Failed}\n");
foreach(var skip in evidence.Skipped)report.AppendLine($"- **{skip.Name}**: {skip.Reason}");
report.AppendLine("\n## Re-run prerequisites\n\nPostgreSQL: configure PGHOST/PGPORT/PGUSER/PGPASSWORD/PGDATABASE for a reachable test database (TLS required by PostgresLiveFixture). Do not commit credentials. Pretrained torchvision: set PYSHARP_ORACLE_PRETRAINED=1; large model downloads are opt-in. LocalDB tests may skip on other hosts. An unmet prerequisite is not a passing integration test.\n\nCommand: `dotnet test src/PySharp.Tests/PySharp.Tests.csproj -c Release --logger \"trx;LogFileName=pysharp.trx\" --results-directory D:/Dev/BuildArtifacts/ecosystem-skips` (from PySharp). Then run this script from MathemaSharp. The ecosystem gate generates a fresh TRX in its timestamped report directory.");
Directory.CreateDirectory("ops/validation");
File.WriteAllText("ops/validation/pysharp-skips.md",report.ToString());
File.WriteAllText(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!,"pysharp-skips.md"),report.ToString());
Console.WriteLine(report);
return evidence.Failed==0?0:1;
