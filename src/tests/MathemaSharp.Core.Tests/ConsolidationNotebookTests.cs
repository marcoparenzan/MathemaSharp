using System.Text.Json;

namespace MathemaSharp.Core.Tests;

/// <summary>Verifica l'evidenza salvata; non sostituisce la riesecuzione con JupyterNet.</summary>
public class ConsolidationNotebookTests
{
    [Theory]
    [InlineData("Core")]
    [InlineData("LinearAlgebra")]
    [InlineData("TimeSeries")]
    [InlineData("Quantum")]
    public void SavedOutputsContainAllSixFormats(string module)
    {
        var path = Path.Combine(NotebookTests.RepositoryRoot().FullName,"notebooks",module,"11-consolidation.ipynb");
        using var document=JsonDocument.Parse(File.ReadAllText(path));
        var cells=document.RootElement.GetProperty("cells").EnumerateArray().ToArray();
        var source=string.Concat(cells.SelectMany(cell=>cell.GetProperty("source").EnumerateArray()).Select(line=>line.GetString()));
        Assert.Contains("## Usage",source);
        Assert.Contains("## Reference",source);
        Assert.Contains("MathemaSharp.Numerics.csx",source);
        var outputs=cells.Where(cell=>cell.GetProperty("cell_type").GetString()=="code")
            .SelectMany(cell=>cell.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.NotEmpty(outputs);
        Assert.DoesNotContain(outputs,output=>output.GetProperty("output_type").GetString()=="error");
        var text=string.Concat(outputs.SelectMany(output=>output.GetProperty("data").GetProperty("text/plain").EnumerateArray()).Select(line=>line.GetString()));
        foreach(var format in new[] {"Half","Single","Double","Decimal32","Decimal64","Decimal128"})
            Assert.Contains(format+":",text);
        Assert.Contains("OK",text);
    }
}
