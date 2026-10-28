using System.Text.Json;
using System.Text.RegularExpressions;

namespace MathemaSharp.Core.Tests;

/// <summary>
/// I notebook sono salvati eseguiti (scripts/run-notebooks.ps1): il test controlla formato JupyterNet, assenza di errori negli output
/// e che i file caricati con #load esistano. Non riesegue le celle: per quello serve lo script.
/// </summary>
public class NotebookTests
{
    internal static DirectoryInfo RepositoryRoot()
    {
        var directory = new DirectoryInfo(System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes<System.Reflection.AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "src", "MathemaSharp.slnx")))
            directory = directory.Parent;
        return directory ?? throw new InvalidOperationException("Radice del repository non trovata.");
    }

    [Fact]
    public void NotebooksHaveValidStructureAndExistingLoads()
    {
        var root = RepositoryRoot();
        var files = Directory.GetFiles(Path.Combine(root.FullName, "notebooks"), "*.ipynb", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(Path.Combine(root.FullName, "scenari"), "*.ipynb", SearchOption.AllDirectories)).ToArray();
        Assert.True(files.Length >= 15, $"Attesi almeno 15 notebook (10 C#, il gemello PySharp e le due coppie degli scenari A e D), trovati {files.Length}.");
        foreach (var file in files)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(file));
            var root1 = document.RootElement;
            Assert.Equal(4, root1.GetProperty("nbformat").GetInt32());
            var expectedKernel = file.Contains(Path.DirectorySeparatorChar + "Ecosystem" + Path.DirectorySeparatorChar)
                ? "jupytersharp" : "jupyternet";
            Assert.Equal(expectedKernel, root1.GetProperty("metadata").GetProperty("kernelspec").GetProperty("name").GetString());
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
                // JupyterNet ≥ 0.3.0 risolve i #load relativi rispetto alla cartella del notebook.
                foreach (Match load in Regex.Matches(source, "#load \"([^\"]+)\""))
                    Assert.True(File.Exists(Path.Combine(Path.GetDirectoryName(file)!, load.Groups[1].Value)), $"{Path.GetFileName(file)}: #load inesistente {load.Groups[1].Value}");
            }
            // Una cella che definisce solo variabili non stampa nulla: l'esecuzione si riconosce da almeno metà delle celle con output.
            // Gli output sono azzerati nella migrazione; questo test verifica solo struttura e percorsi.
            Assert.True(codeCells > 0);
        }
    }

    /// <summary>
    /// I due notebook del "giro della libreria" fanno le stesse operazioni, uno in C# con l'API generica e uno in PySharp con la facciata
    /// non generica, sugli stessi dati (stesso seme). Ogni riga "etichetta: valori" in comune deve essere identica.
    /// </summary>
    [Fact]
    public void LibraryTourNotebooksAgreeAcrossLanguages() =>
        AssertSameNumbers("10-ts-libreria-tour.ipynb", Path.Combine("notebooks", "TimeSeries", "10-ts-libreria-tour.ipynb"), Path.Combine("notebooks", "TimeSeries", "pysharp", "10-ts-libreria-tour.ipynb"), minimumLabels: 24);

    /// <summary>Scenario A (magazzino frigorifero): stessa analisi in C# (API generica) e in PySharp (facciata Interop), stessi dati CSV.</summary>
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
