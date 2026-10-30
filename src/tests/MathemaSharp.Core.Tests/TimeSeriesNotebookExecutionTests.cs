using System.Text.Json;
using System.Text.RegularExpressions;

namespace MathemaSharp.Core.Tests;

/// <summary>Validates the saved outputs of selected C# notebooks, without running a kernel or PySharp comparisons.</summary>
public class TimeSeriesNotebookExecutionTests
{
    [Theory]
    [InlineData("06-ts-reference-autoregression")]
    [InlineData("07-ts-smoothing-kalman")]
    [InlineData("08-ts-arima-var-garch")]
    [InlineData("10-ts-libreria-tour")]
    public void SelectedCSharpExamplesHaveOutputs(string name)
    {
        var path=Path.Combine(NotebookTests.RepositoryRoot().FullName,"notebooks","TimeSeries",name+".ipynb");
        using var document=JsonDocument.Parse(File.ReadAllText(path));
        var cells=document.RootElement.GetProperty("cells").EnumerateArray().ToArray();
        var source=string.Concat(cells.SelectMany(cell=>cell.GetProperty("source").EnumerateArray()).Select(line=>line.GetString()));
        Assert.Contains("## Usage",source);
        Assert.Contains("12-api-reference.ipynb",source);
        Assert.DoesNotContain("#load \"../shared/NDSharp.csx\"",source);
        var outputs=cells.Where(cell=>cell.GetProperty("cell_type").GetString()=="code")
            .SelectMany(cell=>cell.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.NotEmpty(outputs);
        Assert.DoesNotContain(outputs,output=>output.GetProperty("output_type").GetString()=="error");
        // Some notebooks produce only HTML/SVG; separating the cells preserves the labels at the start of a line.
        var text=string.Join("\n",outputs.Select(output=>
        {
            var data=output.GetProperty("data");
            return data.TryGetProperty("text/plain",out var plain)
                ? string.Concat(plain.EnumerateArray().Select(line=>line.GetString()))
                : data.GetRawText();
        }));
        Assert.False(string.IsNullOrWhiteSpace(text));
        if(name=="10-ts-libreria-tour")
        {
            Assert.Contains("mean:",text);
            Assert.Contains("ses_forecast:",text);
            var labels=Regex.Matches(text,@"(?m)^([a-z0-9_]+): ").Select(match=>match.Groups[1].Value).Distinct().ToArray();
            Assert.True(labels.Length>=24);
        }
    }
}
