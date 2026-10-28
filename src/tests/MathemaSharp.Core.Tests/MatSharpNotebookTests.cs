using System.Text.Json;
namespace MathemaSharp.Core.Tests;
public class MatSharpNotebookTests
{
    [Fact] public void TypedMatricesHaveExecutedEvidence()
    {
        var path=Path.Combine(NotebookTests.RepositoryRoot().FullName,"notebooks","MatSharp","01-decimal-scalars.ipynb");
        using var doc=JsonDocument.Parse(File.ReadAllText(path));
        var outputs=doc.RootElement.GetProperty("cells").EnumerateArray().Where(c=>c.GetProperty("cell_type").GetString()=="code").SelectMany(c=>c.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.NotEmpty(outputs);
        Assert.DoesNotContain(outputs,o=>o.GetProperty("output_type").GetString()=="error");
        var text=string.Join("\n",outputs.Select(o=>o.ToString()));
        Assert.Contains("typed_matrix_product: OK",text);
        Assert.Contains("typed_index_assignment_and_copy: OK",text);
        Assert.Contains("matsharp_decimal128_increment: OK",text);
        Assert.Contains("typed_left_divide_precision: OK",text);
    }
}
