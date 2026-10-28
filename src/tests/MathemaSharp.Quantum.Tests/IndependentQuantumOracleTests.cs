using System.Globalization;
using System.Numerics;
using System.Text.Json;

namespace MathemaSharp.Quantum.Tests;

public class IndependentQuantumOracleTests
{
    private static JsonDocument Oracle()
    {
        var root=new DirectoryInfo(System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes<System.Reflection.AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!);
        while(root is not null && !File.Exists(Path.Combine(root.FullName,"src","MathemaSharp.slnx"))) root=root.Parent;
        return JsonDocument.Parse(File.ReadAllText(Path.Combine(root!.FullName,"ops","oracles","numerical-reference.json")));
    }
    private static T P<T>(string text) where T:IFloatingPointIeee754<T> => T.Parse(text,CultureInfo.InvariantCulture);
    private static ComplexMatrix<T> Matrix<T>(JsonElement data) where T:IFloatingPointIeee754<T>,IMinMaxValue<T>
    {
        var rows=data.EnumerateArray().ToArray();
        var result=new ComplexMatrix<T>(rows.Length,rows[0].GetArrayLength());
        for(var i=0;i<result.Rows;i++)
        {
            var cells=rows[i].EnumerateArray().ToArray();
            for(var j=0;j<result.Cols;j++)
            {
                var pair=cells[j].EnumerateArray().ToArray();
                result[i,j]=new(P<T>(pair[0].GetString()!),P<T>(pair[1].GetString()!));
            }
        }
        return result;
    }
    private static void Exp<T>(string toleranceText) where T:IFloatingPointIeee754<T>,IMinMaxValue<T>
    {
        using var oracle=Oracle();
        var tolerance=P<T>(toleranceText);
        foreach(var row in oracle.RootElement.GetProperty("matrix_exp").EnumerateArray())
        {
            var matrix=Matrix<T>(row.GetProperty("matrix"));
            var expected=Matrix<T>(row.GetProperty("expected"));
            var actual=QuantumAlgorithms.MatrixExp(matrix);
            for(var i=0;i<actual.Rows;i++) for(var j=0;j<actual.Cols;j++)
            {
                var error=(actual[i,j]-expected[i,j]).GetMagnitude()/T.Max(T.One,expected[i,j].GetMagnitude());
                Assert.True(error<=tolerance,$"{typeof(T).Name} {row.GetProperty("name")} [{i},{j}] error={error}");
            }
        }
    }
    [Fact] public void FloatMatrixExp()=>Exp<float>("0.0001");
    [Fact] public void DoubleMatrixExp()=>Exp<double>("1e-12");
    [Fact] public void Decimal64MatrixExp()=>Exp<Decimal64>("1e-12");
    [Fact] public void Decimal128MatrixExp()=>Exp<Decimal128>("1e-26");

    private static void Eigen<T>(string toleranceText) where T:IFloatingPointIeee754<T>,IMinMaxValue<T>
    {
        using var oracle=Oracle();
        var tolerance=P<T>(toleranceText);
        foreach(var row in oracle.RootElement.GetProperty("hermitian").EnumerateArray())
        {
            var matrix=Matrix<T>(row.GetProperty("matrix"));
            var expected=row.GetProperty("eigenvalues").EnumerateArray().Select(v=>P<T>(v.GetString()!)).ToArray();
            var result=QuantumAlgorithms.DiagonalizeHermitian2(matrix);
            var actual=new[] {result.LambdaPlus,result.LambdaMinus};
            var vectors=new[] {result.VectorPlus,result.VectorMinus};
            for(var i=0;i<2;i++)
            {
                var error=T.Abs(actual[i]/expected[i]-T.One);
                Assert.True(error<=tolerance,$"{typeof(T).Name} {row.GetProperty("name")} eigenvalue {i}: actual={actual[i]} expected={expected[i]}");
                var applied=QuantumAlgorithms.Apply(matrix,vectors[i]);
                for(var j=0;j<2;j++) Assert.True((applied[j]-actual[i]*vectors[i][j]).GetMagnitude()<=tolerance*T.CreateChecked(10));
            }
            Assert.True(QuantumAlgorithms.InnerProduct<T>(vectors[0],vectors[1]).GetMagnitude()<=tolerance);
        }
    }
    [Fact] public void DoubleImbalancedEigenvalues()=>Eigen<double>("1e-10");
    [Fact] public void Decimal128ImbalancedEigenvalues()=>Eigen<Decimal128>("1e-28");
}
