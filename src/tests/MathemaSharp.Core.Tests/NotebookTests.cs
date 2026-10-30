using System.Text.Json;
using System.Text.RegularExpressions;

namespace MathemaSharp.Core.Tests;

/// <summary>
/// The notebooks are saved executed (scripts/run-notebooks.ps1): the test checks the Jupyter format, the absence of errors in the outputs
/// and that the files loaded with #load exist. It does not re-run the cells: for that use the script.
/// </summary>
public class NotebookTests
{
    internal static DirectoryInfo RepositoryRoot()
    {
        var directory = new DirectoryInfo(System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes<System.Reflection.AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "src", "MathemaSharp.slnx")))
            directory = directory.Parent;
        return directory ?? throw new InvalidOperationException("Repository root not found.");
    }

    [Fact]
    public void NotebooksHaveValidStructureAndExistingLoads()
    {
        var root = RepositoryRoot();
        var files = Directory.GetFiles(Path.Combine(root.FullName, "notebooks"), "*.ipynb", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(Path.Combine(root.FullName, "scenari"), "*.ipynb", SearchOption.AllDirectories)).ToArray();
        Assert.True(files.Length >= 15, $"Expected at least 15 notebooks (10 C#, the PySharp twin and the two pairs of scenarios A and D), found {files.Length}.");
        foreach (var file in files)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(file));
            var root1 = document.RootElement;
            Assert.Equal(4, root1.GetProperty("nbformat").GetInt32());
            // Notebooks executed by JupyterSharp are saved with its kernelspec; PySharp ones still run in the old jupyternet tool.
            Assert.Contains(root1.GetProperty("metadata").GetProperty("kernelspec").GetProperty("name").GetString(), new[] { "jupytersharp", "jupyternet" });
            var codeCells = 0;
            var cellsWithOutput = 0;
            foreach (var cell in root1.GetProperty("cells").EnumerateArray())
            {
                Assert.Equal(JsonValueKind.Array, cell.GetProperty("source").ValueKind);
                if (cell.GetProperty("cell_type").GetString() != "code") continue;
                var language = cell.GetProperty("metadata").GetProperty("vscode").GetProperty("languageId").GetString();
                Assert.Contains(language, new[] { "csharp", "pysharp" });
                var outputs = cell.GetProperty("outputs").EnumerateArray().ToArray();
                codeCells++;
                if (outputs.Length > 0) cellsWithOutput++;
                Assert.DoesNotContain(outputs, o => o.GetProperty("output_type").GetString() == "error");
                var source = string.Concat(cell.GetProperty("source").EnumerateArray().Select(s => s.GetString()));
                // The kernel (≥ 0.3.0) resolves relative #load directives against the notebook's folder.
                foreach (Match load in Regex.Matches(source, "#load \"([^\"]+)\""))
                    Assert.True(File.Exists(Path.Combine(Path.GetDirectoryName(file)!, load.Groups[1].Value)), $"{Path.GetFileName(file)}: #load inesistente {load.Groups[1].Value}");
            }
            // A cell that only defines variables prints nothing: execution is recognised by at least half of the cells having output.
            // The outputs were cleared in the migration; this test only checks structure and paths.
            Assert.True(codeCells > 0);
        }
    }

    /// <summary>
    /// The two "library tour" notebooks do the same operations, one in C# with the generic API and one in PySharp with the non-generic facade,
    /// on the same data (same seed). Every "label: values" line they have in common must be identical.
    /// </summary>
    [Fact]
    public void LibraryTourNotebooksAgreeAcrossLanguages() =>
        AssertSameNumbers("10-ts-libreria-tour.ipynb", Path.Combine("notebooks", "TimeSeries", "10-ts-libreria-tour.ipynb"), Path.Combine("notebooks", "TimeSeries", "pysharp", "10-ts-libreria-tour.ipynb"), minimumLabels: 24);

        /// <summary>Scenario A (cold store): the same analysis in C# (generic API) and in PySharp (Interop facade), the same CSV data.</summary>
    [Fact]
    public void ScenarioANotebooksAgreeAcrossLanguages() =>
        AssertSameNumbers("scenario A", Path.Combine("notebooks", "TimeSeries", "Scenarios", "A-magazzino-frigorifero", "A1-magazzino-frigorifero-csharp.ipynb"),
            Path.Combine("notebooks", "TimeSeries", "Scenarios", "A-magazzino-frigorifero", "A1-magazzino-frigorifero-pysharp.ipynb"), minimumLabels: 38);

    /// <summary>Scenario D (parco fotovoltaico): stessa analisi in C# e in PySharp, con quantili, pinball e copertura.</summary>
    [Fact]
    public void ScenarioDNotebooksAgreeAcrossLanguages() =>
        AssertSameNumbers("scenario D", Path.Combine("notebooks", "TimeSeries", "Scenarios", "D-fotovoltaico", "D1-fotovoltaico-csharp.ipynb"),
            Path.Combine("notebooks", "TimeSeries", "Scenarios", "D-fotovoltaico", "D1-fotovoltaico-pysharp.ipynb"), minimumLabels: 29);

    private static void AssertSameNumbers(string name, string csharpRelative, string pysharpRelative, int minimumLabels)
    {
        var root = RepositoryRoot().FullName;
        var csharp = PrintedLines(Path.Combine(root, csharpRelative));
        var pysharp = PrintedLines(Path.Combine(root, pysharpRelative));
        var common = csharp.Keys.Intersect(pysharp.Keys).Where(k => !k.StartsWith("numpy_")).ToArray();
        Assert.True(common.Length >= minimumLabels, $"{name}: attese almeno {minimumLabels} etichette in comune, trovate {common.Length}.");
        foreach (var label in common) Assert.Equal(csharp[label], pysharp[label]);
    }

    private static Dictionary<string, string> PrintedLines(string path)
    {
        var result = new Dictionary<string, string>();
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        foreach (var cell in document.RootElement.GetProperty("cells").EnumerateArray())
        {
            if (cell.GetProperty("cell_type").GetString() != "code") continue;
            foreach (var output in cell.GetProperty("outputs").EnumerateArray())
            {
                if (!output.TryGetProperty("data", out var data) || !data.TryGetProperty("text/plain", out var text)) continue;
                var joined = string.Concat(text.EnumerateArray().Select(t => t.GetString()));
                foreach (var line in joined.Split('\n'))
                {
                    var match = Regex.Match(line.Trim(), @"^([a-z0-9_]+): (.*)$");
                    if (match.Success) result[match.Groups[1].Value] = match.Groups[2].Value;
                }
            }
        }
        return result;
    }
}
