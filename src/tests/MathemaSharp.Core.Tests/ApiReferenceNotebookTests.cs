using System.Text.Json;

namespace MathemaSharp.Core.Tests;

/// <summary>Checks the saved catalogue, does not run the kernel and does not certify the listed methods.</summary>
public class ApiReferenceNotebookTests
{
    [Theory]
    [InlineData("Core", "Sum", "INumberBase")]
    [InlineData("LinearAlgebra", "LeastSquares", "IFloatingPointIeee754")]
    [InlineData("TimeSeries", "Evaluate", "IFloatingPointIeee754")]
    [InlineData("Quantum", "MatrixExp", "IMinMaxValue")]
    public void CatalogContainsPublicApiAndRuntime(string module, string method, string constraint)
    {
        var path=Path.Combine(NotebookTests.RepositoryRoot().FullName,"notebooks",module,"12-api-reference.ipynb");
        using var document=JsonDocument.Parse(File.ReadAllText(path));
        var code=document.RootElement.GetProperty("cells").EnumerateArray()
            .Where(cell=>cell.GetProperty("cell_type").GetString()=="code").ToArray();
        var outputs=code.SelectMany(cell=>cell.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.NotEmpty(outputs);
        Assert.DoesNotContain(outputs,output=>output.GetProperty("output_type").GetString()=="error");
        var text=string.Concat(outputs.SelectMany(output=>output.GetProperty("data").GetProperty("text/plain").EnumerateArray()).Select(line=>line.GetString()));
        Assert.Contains("Assembly: MathemaSharp."+module,text);
        Assert.Contains("Runtime: 11.",text);
        Assert.Contains("TYPE MathemaSharp."+module,text);
        Assert.Contains(method,text);
        Assert.Contains(constraint,text);
        Assert.Contains("catalog: OK",text);
        Assert.DoesNotContain("TYPE MathemaSharp.TimeSeries.Num ",text);
    }
}
