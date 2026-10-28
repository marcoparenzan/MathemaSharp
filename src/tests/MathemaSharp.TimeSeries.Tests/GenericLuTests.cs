using System.Numerics;
using MathemaSharp.LinearAlgebra;

namespace MathemaSharp.TimeSeries.Tests;

public class GenericLuTests
{
    private static void Verify<T>() where T : IFloatingPointIeee754<T>
    {
        T N(int value) => T.CreateChecked(value);
        var a = new T[,] {{T.Zero,N(2),N(1)}, {N(1),N(1),T.Zero}, {N(2),T.Zero,N(1)}};
        var original = (T[,])a.Clone();
        var factor = LuFactorization<T>.Create(a);
        var x = factor.Solve(new T[] {N(7),N(3),N(5)}); // x=[1,2,3]
        var tolerance = N(100) * (T.BitIncrement(T.One) - T.One);
        for (var i = 0; i < 3; i++) Assert.True(T.Abs(x[i] - N(i+1)) <= tolerance);
        Assert.True(T.Abs(factor.Determinant() - N(-4)) <= tolerance);
        var identity = MatrixAlgebra.Multiply(a, factor.Inverse());
        for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
            {
                Assert.Equal(original[i,j], a[i,j]);
                Assert.True(T.Abs(identity[i,j] - (i == j ? T.One : T.Zero)) <= tolerance);
            }
    }
    [Fact] public void HalfFormat() => Verify<Half>();
    [Fact] public void FloatFormat() => Verify<float>();
    [Fact] public void DoubleFormat() => Verify<double>();
    [Fact] public void Decimal32Format() => Verify<Decimal32>();
    [Fact] public void Decimal64Format() => Verify<Decimal64>();
    [Fact] public void Decimal128Format() => Verify<Decimal128>();
    [Fact] public void InvalidInputs()
    {
        Assert.Throws<ArgumentException>(() => LuFactorization<double>.Create(new double[0,0]));
        Assert.Throws<ArgumentException>(() => LuFactorization<double>.Create(new double[2,3]));
        Assert.Throws<ArgumentException>(() => LuFactorization<double>.Create(new double[,] {{1,2},{2,4}}));
        Assert.Throws<ArgumentException>(() => LuFactorization<double>.Create(new double[,] {{1,double.NaN},{0,1}}));
        var lu = LuFactorization<double>.Create(new double[,] {{1}});
        Assert.Throws<ArgumentException>(() => lu.Solve(new double[2]));
        Assert.Throws<ArgumentException>(() => lu.Solve(new double[] {double.PositiveInfinity}));
    }
    [Fact] public void FactorizationIsIndependentOfSource()
    {
        double[,] a = {{2,0},{0,4}};
        var lu = LuFactorization<double>.Create(a);
        a[0,0] = 100;
        Assert.Equal(new double[] {1,2}, lu.Solve(new double[] {2,8}));
        Assert.Equal(new double[] {1,2}, lu.Solve(new double[] {2,8}));
    }
    [Fact] public void RelativeScale()
    {
        var lu = LuFactorization<double>.Create(new double[,] {{1e-200,0},{0,2e-200}});
        var x = lu.Solve(new double[] {1e-200,4e-200});
        Assert.Equal(1d,x[0],12);
        Assert.Equal(2d,x[1],12);
    }
    [Fact] public void ProductsAlsoSupportIntegersAndDecimal()
    {
        var a = MatrixAlgebra.Multiply(new int[,] {{1,2}},new int[,] {{3},{4}});
        Assert.Equal(11,a[0,0]);
        var b = MatrixAlgebra.Multiply(new decimal[,] {{0.1m,0.2m}},new decimal[,] {{1m},{1m}});
        Assert.Equal(0.3m,b[0,0]);
    }
}
