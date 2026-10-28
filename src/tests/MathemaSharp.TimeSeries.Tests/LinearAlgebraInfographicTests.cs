using MathemaSharp.TimeSeries;

namespace MathemaSharp.TimeSeries.Tests;

public class LinearAlgebraInfographicTests
{
    [Fact] public void Norms()
    {
        Assert.Equal(7d, MatrixAlgebra.NormL1<double>([3, -4]));
        Assert.Equal(5d, MatrixAlgebra.NormL2<double>([3, -4]), 12);
        Assert.Equal(4d, MatrixAlgebra.NormInfinity<double>([3, -4]));
        Assert.Equal(5d, MatrixAlgebra.FrobeniusNorm(new double[,] {{3, 0}, {0, -4}}), 12);
        Assert.Equal(0d, MatrixAlgebra.NormL2<double>([]));
        Assert.Equal(0d, MatrixAlgebra.NormL1<double>([]));
        Assert.Equal(0d, MatrixAlgebra.NormInfinity<double>([]));
    }
    [Fact] public void L2AvoidsPrematureOverflow()
    {
        var norm = MatrixAlgebra.NormL2<double>([1e200, 1e200]);
        Assert.True(double.IsFinite(norm));
        Assert.InRange(norm / 1e200, 1.41421356237, 1.41421356238);
    }
    [Fact] public void InverseReconstructsIdentity()
    {
        double[,] a = {{4, 7}, {2, 6}};
        Assert.Equal(10d, MatrixAlgebra.Determinant2x2(a));
        var result = MatrixAlgebra.Multiply(a, MatrixAlgebra.Inverse2x2(a));
        for (var i = 0; i < 2; i++)
            for (var j = 0; j < 2; j++) Assert.Equal(i == j ? 1d : 0d, result[i,j], 12);
    }
    [Fact] public void InvalidInverseRejected()
    {
        Assert.Throws<ArgumentException>(() => MatrixAlgebra.Inverse2x2(new double[,] {{1,2},{2,4}}));
        Assert.Throws<ArgumentException>(() => MatrixAlgebra.Inverse2x2(new double[,] {{double.NaN,0},{0,1}}));
        Assert.Throws<ArgumentException>(() => MatrixAlgebra.Inverse2x2(new double[2,3]));
        Assert.Throws<ArgumentException>(() => MatrixAlgebra.Determinant2x2(new double[3,3]));
    }
    [Fact] public void InverseUsesRelativeScale()
    {
        var inverse = MatrixAlgebra.Inverse2x2(new double[,] {{1e-200,0},{0,1e-200}});
        Assert.InRange(inverse[0,0] / 1e200, 0.999999999999, 1.000000000001);
    }
    [Fact] public void FloatNorm()
        => Assert.Equal(5f, MatrixAlgebra.NormL2<float>([3,4]));
}
