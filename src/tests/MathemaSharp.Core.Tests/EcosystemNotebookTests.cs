using System.Text.Json;

namespace MathemaSharp.Core.Tests;

public class EcosystemNotebookTests
{
    [Fact]
    public void NDSharpContourNotebookHasExecutedLineAndFilledEvidence()
    {
        var root = NotebookTests.RepositoryRoot().FullName;
        var path = Path.Combine(root, "notebooks", "NDSharp", "pysharp", "02-contours-kernel.ipynb");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var cells = doc.RootElement.GetProperty("cells").EnumerateArray().Where(c => c.GetProperty("cell_type").GetString() == "code").ToArray();
        Assert.All(cells, c => Assert.Equal("pysharp", c.GetProperty("metadata").GetProperty("vscode").GetProperty("languageId").GetString()));
        var outputs = cells.SelectMany(c => c.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.DoesNotContain(outputs, o => o.GetProperty("output_type").GetString() == "error");
        var serialized = string.Join("\n", outputs.Select(o => o.GetRawText()));
        Assert.Contains("NDSHARP_LINE_CONTOUR_KERNEL_OK", serialized);
        Assert.Contains("NDSHARP_FILLED_CONTOUR_KERNEL_OK", serialized);
        var images = outputs.Where(o => o.TryGetProperty("data", out var data) && data.TryGetProperty("image/png", out _)).ToArray();
        Assert.Equal(2, images.Length);
        foreach (var output in images)
        {
            var value = output.GetProperty("data").GetProperty("image/png");
            var base64 = value.ValueKind == JsonValueKind.Array ? string.Concat(value.EnumerateArray().Select(v => v.GetString())) : value.GetString();
            var bytes = Convert.FromBase64String(base64!);
            Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, bytes.Take(8).ToArray());
            Assert.Equal(320u, System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(16, 4)));
            Assert.Equal(240u, System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(20, 4)));
        }
        using var report = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "ops", "validation", "ndsharp-contour-notebook", "report.json")));
        Assert.True(report.RootElement.GetProperty("Success").GetBoolean());
        Assert.False(report.RootElement.GetProperty("ReviewImageFilesSaved").GetBoolean());
    }

    [Fact]
    public void SourcePlotBindingIsExplicitOptInNotAReplacementOfPreviewDefault()
    {
        var root = NotebookTests.RepositoryRoot().FullName;
        var project = System.Xml.Linq.XDocument.Load(Path.Combine(root, "..", "PySharp", "src", "PySharpLib.Matplotlib", "PySharpLib.Matplotlib.csproj"));
        var package = Assert.Single(project.Descendants("PackageReference"), e => e.Attribute("Include")?.Value == "NDSharp.Plot");
        Assert.Contains("!= 'true'", package.Attribute("Condition")!.Value);
        var source = Assert.Single(project.Descendants("ProjectReference"), e => e.Attribute("Include")?.Value.Contains("NDSharp.Plot.csproj") == true);
        Assert.Contains("== 'true'", source.Attribute("Condition")!.Value);
        Assert.Contains("ScienceUseSourcePlots", source.Attribute("Condition")!.Value);
    }

    [Fact]
    public void SavedBrowserEvidenceHasNoTitleCollisionsAndReachableTooltipTargets()
    {
        var root = NotebookTests.RepositoryRoot().FullName;
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "ops", "migrated-notebooks.json")));
        int expected = manifest.RootElement.EnumerateArray().Sum(e => e.GetProperty("expectedSvg").GetInt32());
        using var report = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "ops", "visual", "browser", "report.json")));
        Assert.Equal(expected, report.RootElement.GetProperty("Total").GetInt32());
        Assert.Equal(0, report.RootElement.GetProperty("Failed").GetInt32());
        var charts = report.RootElement.GetProperty("Charts").EnumerateArray().ToArray();
        Assert.Equal(expected, charts.Length);
        Assert.Equal(expected, charts.Select(c => c.GetProperty("Chart").GetString()).Distinct().Count());
        foreach (var chart in charts)
        {
            Assert.True(chart.GetProperty("Success").GetBoolean());
            var checks = chart.GetProperty("Checks");
            Assert.Empty(checks.GetProperty("outside").EnumerateArray());
            Assert.Empty(checks.GetProperty("collisions").EnumerateArray());
            var targets = checks.GetProperty("tooltipTargets").GetInt32();
            var reachable = checks.GetProperty("reachable").GetInt32();
            Assert.True(targets > 0);
            Assert.InRange(reachable, 1, targets);
        }
        var expectedDashboards = manifest.RootElement.EnumerateArray().Where(e => e.TryGetProperty("expectedKpi", out _)).ToArray();
        var dashboards = report.RootElement.GetProperty("Dashboards").EnumerateArray().ToArray();
        Assert.Equal(expectedDashboards.Length, dashboards.Length);
        foreach (var entry in expectedDashboards)
        {
            var dashboard = Assert.Single(dashboards, d => d.GetProperty("Id").GetString() == entry.GetProperty("id").GetString());
            Assert.True(dashboard.GetProperty("Success").GetBoolean());
            var checks = dashboard.GetProperty("Checks");
            Assert.Equal(entry.GetProperty("expectedKpi").GetInt32(), checks.GetProperty("kpi").GetInt32());
            Assert.Equal(entry.GetProperty("expectedQuotes").GetInt32(), checks.GetProperty("quotes").GetInt32());
            Assert.Equal(0, checks.GetProperty("emptyQuotes").GetInt32());
            Assert.False(checks.GetProperty("horizontalOverflow").GetBoolean());
            Assert.True(checks.GetProperty("tiles").GetInt32() > 0);
        }
        Assert.Contains("overlapping", report.RootElement.GetProperty("Notes").GetString());
    }

    [Theory]
    [InlineData("core", 2)]
    [InlineData("smoothing", 6)]
    [InlineData("autoregression", 6)]
    [InlineData("arima", 13)]
    [InlineData("stl", 6)]
    [InlineData("tour", 5)]
    [InlineData("cold-storage", 3)]
    [InlineData("solar", 2)]
    public void MigratedManifestMatchesExecutedSvgEvidence(string id, int expected)
    {
        var root = NotebookTests.RepositoryRoot().FullName;
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "ops", "migrated-notebooks.json")));
        var entry = Assert.Single(manifest.RootElement.EnumerateArray(), e => e.GetProperty("id").GetString() == id);
        Assert.Equal(expected, entry.GetProperty("expectedSvg").GetInt32());
        var relative = entry.GetProperty("path").GetString()!;
        Assert.StartsWith("notebooks/", relative);
        Assert.DoesNotContain("..", relative);
        using var notebook = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, relative)));
        var code = notebook.RootElement.GetProperty("cells").EnumerateArray().Where(c => c.GetProperty("cell_type").GetString() == "code").ToArray();
        Assert.Contains("MathemaSharp.PlotSharp.csx", string.Concat(code.SelectMany(c => c.GetProperty("source").EnumerateArray()).Select(s => s.GetString())));
        var outputs = code.SelectMany(c => c.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.DoesNotContain(outputs, o => o.GetProperty("output_type").GetString() == "error");
        int svgCount = 0, kpi = 0, quotes = 0; bool tables = false, gap = false;
        foreach (var output in outputs)
        {
            if (!output.TryGetProperty("data", out var data) || !data.TryGetProperty("text/html", out var html)) continue;
            var text = html.ValueKind == JsonValueKind.Array ? string.Concat(html.EnumerateArray().Select(v => v.GetString())) : html.GetString();
            var element = System.Xml.Linq.XElement.Parse(text!);
            tables |= element.Name.LocalName == "table";
            kpi += element.DescendantsAndSelf().Count(e => e.Attribute("class")?.Value == "mathema-kpi");
            quotes += element.DescendantsAndSelf().Count(e => e.Attribute("class")?.Value == "mathema-quote");
            gap |= element.Descendants().Any(e => e.Name.LocalName == "path" && (e.Attribute("d")?.Value.Count(c => c == 'M') ?? 0) > 1);
            svgCount += element.DescendantsAndSelf().Count(e => e.Name.LocalName == "svg");
        }
        Assert.Equal(expected, svgCount);
        if (entry.GetProperty("requireTable").GetBoolean()) Assert.True(tables);
        if (entry.GetProperty("requireGap").GetBoolean()) Assert.True(gap);
        if (entry.TryGetProperty("expectedKpi", out var ek)) Assert.Equal(ek.GetInt32(), kpi);
        if (entry.TryGetProperty("expectedQuotes", out var eq)) Assert.Equal(eq.GetInt32(), quotes);
    }

    [Fact]
    public void SmoothingNotebookUsesPlotSharpAndPreservesMissingObservationGap()
    {
        var root = NotebookTests.RepositoryRoot().FullName;
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "notebooks", "TimeSeries", "07-ts-smoothing-kalman.ipynb")));
        var cells = document.RootElement.GetProperty("cells").EnumerateArray().Where(c => c.GetProperty("cell_type").GetString() == "code").ToArray();
        var source = string.Concat(cells.SelectMany(c => c.GetProperty("source").EnumerateArray()).Select(s => s.GetString()));
        Assert.Contains("MathemaSharp.PlotSharp.csx", source);
        Assert.Contains("double.NaN", source);
        var outputs = cells.SelectMany(c => c.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.DoesNotContain(outputs, o => o.GetProperty("output_type").GetString() == "error");
        var html = outputs.Where(o => o.TryGetProperty("data", out var d) && d.TryGetProperty("text/html", out _))
            .Select(o => o.GetProperty("data").GetProperty("text/html"))
            .Select(h => h.ValueKind == JsonValueKind.Array ? string.Concat(h.EnumerateArray().Select(v => v.GetString())) : h.GetString()).ToArray();
        var roots = html.Select(h => System.Xml.Linq.XElement.Parse(h!)).ToArray();
        Assert.Equal(6, roots.SelectMany(r => r.DescendantsAndSelf()).Count(e => e.Name.LocalName == "svg"));
        Assert.Contains(roots.SelectMany(r => r.Descendants()), e => e.Name.LocalName == "path" && (e.Attribute("d")?.Value.Count(c => c == 'M') ?? 0) > 1);
        Assert.Contains(roots, r => r.Name.LocalName == "table");
        Assert.All(html, h => Assert.DoesNotContain("NaN", h!));
    }

    [Fact]
    public void PresentationIsSharedBetweenLegacyAndMigratedBootstraps()
    {
        var shared = Path.Combine(NotebookTests.RepositoryRoot().FullName, "notebooks", "shared");
        Assert.Contains("#load \"Presentation.csx\"", File.ReadAllText(Path.Combine(shared, "Charts.csx")));
        Assert.Contains("#load \"Presentation.csx\"", File.ReadAllText(Path.Combine(shared, "MathemaSharp.PlotSharp.csx")));
        Assert.DoesNotContain("public static class Table", File.ReadAllText(Path.Combine(shared, "Charts.csx")));
        Assert.Contains("public static class Table", File.ReadAllText(Path.Combine(shared, "Presentation.csx")));
        Assert.Contains("public static class Kpi", File.ReadAllText(Path.Combine(shared, "Presentation.csx")));
    }

    [Fact]
    public void CoreSignalsUsesSharedPlotSharpBootstrap()
    {
        var root = NotebookTests.RepositoryRoot().FullName;
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "notebooks", "Core", "03-complex-signals.ipynb")));
        var cells = document.RootElement.GetProperty("cells").EnumerateArray().Where(c => c.GetProperty("cell_type").GetString() == "code").ToArray();
        var source = string.Concat(cells.SelectMany(c => c.GetProperty("source").EnumerateArray()).Select(s => s.GetString()));
        Assert.Contains("MathemaSharp.PlotSharp.csx", source);
        Assert.DoesNotContain("../shared/MathemaSharp.csx", source);
        var bootstrap = File.ReadAllText(Path.Combine(root, "notebooks", "shared", "MathemaSharp.PlotSharp.csx"));
        Assert.Contains("#load \"PlotSharpCharts.csx\"", bootstrap);
        Assert.DoesNotContain("#load \"Charts.csx\"", bootstrap);
        var outputs = cells.SelectMany(c => c.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.DoesNotContain(outputs, o => o.GetProperty("output_type").GetString() == "error");
        Assert.Equal(2, outputs.Count(o => o.TryGetProperty("data", out var data) && data.TryGetProperty("text/html", out _)));
    }

    [Fact]
    public void IndexedFacadeHasFourSavedSvgDocuments()
    {
        var path = Path.Combine(NotebookTests.RepositoryRoot().FullName, "notebooks", "Ecosystem", "03-indexed-charts.ipynb");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var outputs = document.RootElement.GetProperty("cells").EnumerateArray()
            .Where(c => c.GetProperty("cell_type").GetString() == "code")
            .SelectMany(c => c.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.DoesNotContain(outputs, o => o.GetProperty("output_type").GetString() == "error");
        Assert.Contains("INDEXED_FACADE_EXECUTED", string.Join("\n", outputs.Select(o => o.GetRawText())));
        int count = 0;
        foreach (var output in outputs)
        {
            if (!output.TryGetProperty("data", out var data) || !data.TryGetProperty("text/html", out var html)) continue;
            var text = html.ValueKind == JsonValueKind.Array ? string.Concat(html.EnumerateArray().Select(v => v.GetString())) : html.GetString();
            count += System.Xml.Linq.XElement.Parse(text!).DescendantsAndSelf().Count(e => e.Name.LocalName == "svg");
        }
        Assert.Equal(4, count);
    }

    [Fact]
    public void TypedLanguagePilotHasSavedPrecisionEvidence()
    {
        var path = Path.Combine(NotebookTests.RepositoryRoot().FullName, "notebooks", "Ecosystem", "02-typed-language-interop.ipynb");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var outputs = document.RootElement.GetProperty("cells").EnumerateArray()
            .Where(c => c.GetProperty("cell_type").GetString() == "code")
            .SelectMany(c => c.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.NotEmpty(outputs);
        Assert.DoesNotContain(outputs, o => o.GetProperty("output_type").GetString() == "error");
        var text = string.Join("\n", outputs.Select(o => o.GetRawText()));
        Assert.Contains("PYSHARP_GENERIC_TYPE_PRECISION_OK", text);
        Assert.Contains("MATSHARP_TYPED_SOLVE_PRECISION_OK", text);
    }

    [Fact]
    public void PackagedPilotHasSavedFftAndSvgEvidence()
    {
        var path = Path.Combine(NotebookTests.RepositoryRoot().FullName, "notebooks", "Ecosystem", "01-signal-spectrum.ipynb");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var outputs = document.RootElement.GetProperty("cells").EnumerateArray()
            .Where(c => c.GetProperty("cell_type").GetString() == "code")
            .SelectMany(c => c.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.NotEmpty(outputs);
        Assert.DoesNotContain(outputs, o => o.GetProperty("output_type").GetString() == "error");
        var text = string.Join("\n", outputs.Select(o => o.GetRawText()));
        Assert.Contains("FFT_PEAK_5_VERIFIED", text);
        Assert.Contains("PLOTSHARP_TWO_SVG_VERIFIED", text);
        Assert.Equal(2, outputs.Count(o => o.TryGetProperty("data", out var data)
            && data.TryGetProperty("text/html", out var html)
            && (html.ValueKind == JsonValueKind.Array
                ? string.Concat(html.EnumerateArray().Select(v => v.GetString()))
                : html.GetString())?.Contains("<svg") == true));
    }
}
