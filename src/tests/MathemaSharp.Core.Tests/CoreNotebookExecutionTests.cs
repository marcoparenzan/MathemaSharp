using System.Text.Json;

namespace MathemaSharp.Core.Tests;

/// <summary>Checks the outputs saved by the re-run; it does not start the kernel.</summary>
public class CoreNotebookExecutionTests
{
    [Theory]
    [InlineData("01-generic-math")]
    [InlineData("02-precision-decimal")]
    [InlineData("03-complex-signals")]
    [InlineData("04-performance")]
    [InlineData("05-dotnet11-decimals")]
    public void CoreExamplesHaveSavedOutputsWithoutErrors(string name)
    {
        var path=Path.Combine(NotebookTests.RepositoryRoot().FullName,"notebooks","Core",name+".ipynb");
        using var document=JsonDocument.Parse(File.ReadAllText(path));
        var cells=document.RootElement.GetProperty("cells").EnumerateArray().ToArray();
        var outputs=cells.Where(cell=>cell.GetProperty("cell_type").GetString()=="code")
            .SelectMany(cell=>cell.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.NotEmpty(outputs);
        Assert.DoesNotContain(outputs,output=>output.GetProperty("output_type").GetString()=="error");
        var text=string.Concat(outputs
            .Where(output=>output.TryGetProperty("data",out var data) && data.TryGetProperty("text/plain",out _))
            .SelectMany(output=>output.GetProperty("data").GetProperty("text/plain").EnumerateArray()).Select(line=>line.GetString()));
        Assert.False(string.IsNullOrWhiteSpace(text));
        if(name=="05-dotnet11-decimals")
        {
            Assert.Contains(".NET 11.",text);
            Assert.Contains("0.3333333333333333333333333333333333",text);
            Assert.Contains("Infinity",text);
            Assert.Contains("consumed=4",text);
        }
        if(name=="04-performance") Assert.Contains(".NET 11.",text);
    }
}
