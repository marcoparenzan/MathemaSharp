using System.Text.Json;
namespace MathemaSharp.Core.Tests;

public class TypedNDSharpNotebookTests
{
    [Fact]
    public void SavedGenericExampleContainsVerifiedOutputs()
    {
        var path = Path.Combine(NotebookTests.RepositoryRoot().FullName, "notebooks", "NDSharp", "01-generic-math.ipynb");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var outputs = doc.RootElement.GetProperty("cells").EnumerateArray()
            .Where(c => c.GetProperty("cell_type").GetString() == "code")
            .SelectMany(c => c.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.NotEmpty(outputs);
        Assert.DoesNotContain(outputs, o => o.GetProperty("output_type").GetString() == "error");
        var text = string.Join("\n", outputs.Select(o => o.ToString()));
        foreach (var marker in new[] { "decimal128_storage: OK", "broadcast_reverse: OK", "shared_view_write: OK", "lu_solution: OK", "qr_solution: OK", "diagnostics: OK" })
            Assert.Contains(marker, text);
        foreach (var type in new[] { "Half", "Single", "Double", "Decimal32", "Decimal64", "Decimal128" })
            Assert.Contains(type + "_lu: OK", text);
    }
}
