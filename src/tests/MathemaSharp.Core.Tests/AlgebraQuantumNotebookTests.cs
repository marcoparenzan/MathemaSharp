using System.Text.Json;

namespace MathemaSharp.Core.Tests;

public class AlgebraQuantumNotebookTests
{
    [Theory]
    [InlineData("LinearAlgebra", "01-lu", "lu_residual: OK", "qr_decimal128: OK")]
    [InlineData("Quantum", "01-operators", "eigenstate_and_rejection: OK", "evolution_decimal128_1e28: OK")]
    public void SavedExamplesContainVerifiedResults(string module,string name,string first,string second)
    {
        var path=Path.Combine(NotebookTests.RepositoryRoot().FullName,"notebooks",module,name+".ipynb");
        using var document=JsonDocument.Parse(File.ReadAllText(path));
        var cells=document.RootElement.GetProperty("cells").EnumerateArray().ToArray();
        var sources=string.Concat(cells.SelectMany(cell=>cell.GetProperty("source").EnumerateArray()).Select(line=>line.GetString()));
        Assert.Contains("## Contratti",sources);
        Assert.Contains("12-api-reference.ipynb",sources);
        var outputs=cells.Where(cell=>cell.GetProperty("cell_type").GetString()=="code")
            .SelectMany(cell=>cell.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.NotEmpty(outputs);
        Assert.DoesNotContain(outputs,output=>output.GetProperty("output_type").GetString()=="error");
        var text=string.Concat(outputs.Where(output=>output.TryGetProperty("data",out var data)&&data.TryGetProperty("text/plain",out _))
            .SelectMany(output=>output.GetProperty("data").GetProperty("text/plain").EnumerateArray()).Select(line=>line.GetString()));
        Assert.Contains(".NET 11.",text);
        Assert.Contains(first,text);
        Assert.Contains(second,text);
    }
}
