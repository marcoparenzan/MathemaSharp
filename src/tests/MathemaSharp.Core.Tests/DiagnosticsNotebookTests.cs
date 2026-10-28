using System.Text.Json;

namespace MathemaSharp.Core.Tests;

public class DiagnosticsNotebookTests
{
    [Fact]
    public void SavedDiagnosticsShowConditionWarning()
    {
        var path=Path.Combine(NotebookTests.RepositoryRoot().FullName,"notebooks","LinearAlgebra","13-solve-diagnostics.ipynb");
        using var document=JsonDocument.Parse(File.ReadAllText(path));
        var outputs=document.RootElement.GetProperty("cells").EnumerateArray()
            .Where(cell=>cell.GetProperty("cell_type").GetString()=="code")
            .SelectMany(cell=>cell.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.NotEmpty(outputs);
        Assert.DoesNotContain(outputs,output=>output.GetProperty("output_type").GetString()=="error");
        var text=string.Join("\n",outputs.Select(output=>string.Concat(output.GetProperty("data").GetProperty("text/plain").EnumerateArray().Select(line=>line.GetString()))));
        Assert.Contains("simple_condition: 2",text);
        Assert.Contains("Double_H12_warning: True",text);
        Assert.Contains("Decimal128_H8_warning: False",text);
        Assert.Contains("diagnostics: OK",text);
    }
}
