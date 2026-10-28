using System.Text.Json;

namespace MathemaSharp.Core.Tests;

public class OracleNotebookTests
{
    [Theory]
    [InlineData("TimeSeries")]
    [InlineData("Quantum")]
    public void IndependentValidationHasSavedEvidence(string module)
    {
        var path=Path.Combine(NotebookTests.RepositoryRoot().FullName,"notebooks",module,"13-independent-oracles.ipynb");
        using var document=JsonDocument.Parse(File.ReadAllText(path));
        var outputs=document.RootElement.GetProperty("cells").EnumerateArray()
            .Where(cell=>cell.GetProperty("cell_type").GetString()=="code")
            .SelectMany(cell=>cell.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.NotEmpty(outputs);
        Assert.DoesNotContain(outputs,output=>output.GetProperty("output_type").GetString()=="error");
        var text=string.Join("\n",outputs.Select(output=>string.Concat(output.GetProperty("data").GetProperty("text/plain").EnumerateArray().Select(line=>line.GetString()))));
        if(module=="TimeSeries")
        {
            Assert.Contains("oracle_precision: 200",text);
            Assert.Contains("Double_tail_max_relative_error:",text);
            Assert.Contains("Decimal128_tail_max_relative_error:",text);
            Assert.Contains("hilbert12_solution_error:",text);
            Assert.Contains("hilbert12_relative_residual:",text);
            Assert.Contains("small_residual_not_accuracy: OK",text);
        }
        else
        {
            Assert.Contains("Double_expm_max_scaled_error:",text);
            Assert.Contains("Decimal128_expm_max_scaled_error:",text);
            Assert.Contains("Double_diagonal_imbalance_lambda_minus: 1E-40",text);
            Assert.Contains("oracle_matrix_exp_and_small_eigenvalues: OK",text);
        }
    }
}
