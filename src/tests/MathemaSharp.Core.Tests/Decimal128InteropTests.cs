using System.Numerics;
using System.Text.Json;
using MathemaSharp.Core.Interop;
namespace MathemaSharp.Core.Tests;
public class Decimal128InteropTests
{
    [Fact] public void IncrementAndTypedArrayArePreserved()
    {
        var one=Decimal128Interop.Parse("1");var delta=Decimal128Interop.Parse("1e-25");
        Decimal128[] values=[one,delta];var copy=Decimal128Interop.Copy(values);
        Assert.NotSame(values,copy);
        Assert.Equal(delta,Decimal128Interop.Subtract(Decimal128Interop.Sum(copy),one));
        Assert.True(Decimal128Interop.Equal(one,Decimal128Interop.Parse("1.00")));
    }
    [Fact] public void ComplexHermitianProductRetainsComponent()
    {
        var delta=Decimal128Interop.Parse("1e-25");
        var z=Decimal128Interop.Complex(Decimal128.One,delta);
        var result=Decimal128Interop.HermitianDot([z],[Complex<Decimal128>.One]);
        Assert.Equal(Decimal128.One,Decimal128Interop.Real(result));
        Assert.Equal(-delta,Decimal128Interop.Imaginary(result));
    }
    [Fact] public void GenericBindingNotebookHasExecutedEvidence()
    {
        var path=Path.Combine(NotebookTests.RepositoryRoot().FullName,"notebooks","TimeSeries","pysharp","13-generic-binding.ipynb");
        using var doc=JsonDocument.Parse(File.ReadAllText(path));
        var outputs=doc.RootElement.GetProperty("cells").EnumerateArray().Where(c=>c.GetProperty("cell_type").GetString()=="code").SelectMany(c=>c.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.DoesNotContain(outputs,o=>o.GetProperty("output_type").GetString()=="error");
        var text=string.Join("\n",outputs.Select(o=>o.ToString()));
        Assert.Contains("generic_decimal128_determinant: OK",text);
        Assert.Contains("generic_matrix_copy: OK",text);
    }
    [Fact] public void NativeOperatorNotebookHasExecutedEvidence()
    {
        var path=Path.Combine(NotebookTests.RepositoryRoot().FullName,"notebooks","TimeSeries","pysharp","12-clr-operators.ipynb");
        using var doc=JsonDocument.Parse(File.ReadAllText(path));
        var outputs=doc.RootElement.GetProperty("cells").EnumerateArray().Where(c=>c.GetProperty("cell_type").GetString()=="code").SelectMany(c=>c.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.DoesNotContain(outputs,o=>o.GetProperty("output_type").GetString()=="error");
        var text=string.Join("\n",outputs.Select(o=>o.ToString()));
        Assert.Contains("decimal128_native_increment: OK",text);
        Assert.Equal(3,text.Split("decimal_operators: OK").Length-1);
    }
    [Fact] public void PySharpNotebookContainsExecutedEvidence()
    {
        var path=Path.Combine(NotebookTests.RepositoryRoot().FullName,"notebooks","TimeSeries","pysharp","11-precision-interop.ipynb");
        using var doc=JsonDocument.Parse(File.ReadAllText(path));
        var outputs=doc.RootElement.GetProperty("cells").EnumerateArray().Where(c=>c.GetProperty("cell_type").GetString()=="code").SelectMany(c=>c.GetProperty("outputs").EnumerateArray()).ToArray();
        Assert.DoesNotContain(outputs,o=>o.GetProperty("output_type").GetString()=="error");
        var text=string.Join("\n",outputs.Select(o=>o.ToString()));
        foreach(var marker in new[]{"decimal128_roundtrip: OK","typed_array_and_list: OK","complex128_hermitian: OK"})Assert.Contains(marker,text);
    }
}
