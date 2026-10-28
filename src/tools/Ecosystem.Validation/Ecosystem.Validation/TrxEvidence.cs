using System.Xml.Linq;

namespace Ecosystem.Validation;

public sealed record SkippedTest(string Name, string Reason);
public sealed record TrxEvidence(int Results, int Passed, int Failed, IReadOnlyList<SkippedTest> Skipped)
{
    public static TrxEvidence Parse(string xml)
    {
        var document = XDocument.Parse(xml);
        var root = document.Root ?? throw new FormatException("Missing TRX root.");
        if (root.Name.LocalName != "TestRun") throw new FormatException("Expected TRX TestRun.");
        var ns = root.Name.Namespace;
        var rows = root.Element(ns + "Results")?.Elements(ns + "UnitTestResult").ToArray() ?? [];
        if (rows.Length == 0) throw new FormatException("No results; an empty report is not success.");
        if (rows.Any(r => (string?)r.Attribute("outcome") is not ("Passed" or "Failed" or "NotExecuted")))
            throw new FormatException("Incomplete or unsupported outcome.");
        var skips = rows.Where(r => (string?)r.Attribute("outcome") == "NotExecuted")
            .Select(r => new SkippedTest((string?)r.Attribute("testName") ?? "Unnamed test", Reason(r)))
            .OrderBy(r => r.Name, StringComparer.Ordinal).ToArray();
        return new(rows.Length, rows.Count(r => (string?)r.Attribute("outcome") == "Passed"),
            rows.Count(r => (string?)r.Attribute("outcome") == "Failed"), skips);
    }

    private static string Reason(XElement result)
    {
        var text = string.Join(" ", result.Descendants().Where(e => e.Name.LocalName is "Message" or "StdOut")
            .Select(e => e.Value.Trim()).Where(s => s.Length > 0));
        return text.Length > 0 ? text : "Reason absent in TRX; inspect test and fixture (not automatically approved).";
    }
}
