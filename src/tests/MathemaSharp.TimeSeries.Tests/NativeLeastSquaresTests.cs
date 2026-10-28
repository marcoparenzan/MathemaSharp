using System.Numerics;
using MathemaSharp.LinearAlgebra;

namespace MathemaSharp.TimeSeries.Tests;

public class NativeLeastSquaresTests
{
    private static void Verify<T>() where T : IFloatingPointIeee754<T>
    {
        T N(int n) => T.CreateChecked(n);
        T[,] design = {{T.One,T.Zero},{T.Zero,T.One},{T.One,T.One}};
        T[] observations = [N(2), N(3), N(5)];
        var original = (T[,])design.Clone();
        var result = MatrixAlgebra.LeastSquares(design, observations);
        var tolerance = N(64) * (T.BitIncrement(T.One) - T.One);
        Assert.True(T.Abs(result[0] - N(2)) <= tolerance);
        Assert.True(T.Abs(result[1] - N(3)) <= tolerance);
        for (var i=0; i<3; i++) for(var j=0; j<2; j++) Assert.Equal(original[i,j],design[i,j]);
        Assert.Equal(new T[] {N(2),N(3),N(5)}, observations);
        foreach (var scale in new T[] {T.One / N(100), N(100)})
        {
            var scaled = new T[3,2];
            for(var i=0; i<3; i++) for(var j=0; j<2; j++) scaled[i,j]=design[i,j]*scale;
            var y = observations.Select(v=>v*scale).ToArray();
            var beta = MatrixAlgebra.LeastSquares(scaled,y);
            Assert.True(T.Abs(beta[0]-N(2)) <= tolerance);
            Assert.True(T.Abs(beta[1]-N(3)) <= tolerance);
        }
        Assert.Throws<ArgumentException>(() => MatrixAlgebra.LeastSquares(new T[,] {{T.One,T.One},{T.One,T.One}}, new T[] {T.One,T.One}));
    }
    [Fact] public void HalfNative() => Verify<Half>();
    [Fact] public void FloatNative() => Verify<float>();
    [Fact] public void DoubleNative() => Verify<double>();
    [Fact] public void Decimal32Native() => Verify<Decimal32>();
    [Fact] public void Decimal64Native() => Verify<Decimal64>();
    [Fact] public void Decimal128Native() => Verify<Decimal128>();

    [Theory] [InlineData(1e-200)] [InlineData(1e200)]
    public void ExtremeGlobalScales(double scale)
    {
        var beta = MatrixAlgebra.LeastSquares(new double[,] {{scale,0},{0,scale},{scale,scale}},new double[] {2*scale,3*scale,5*scale});
        Assert.True(Math.Abs(beta[0]-2)<1e-12);
        Assert.True(Math.Abs(beta[1]-3)<1e-12);
    }

    [Fact]
    public void Decimal128RetainsTinyCoefficientDifference()
    {
        var tiny = Decimal128.One;
        for(var i=0;i<25;i++) tiny/=Decimal128.CreateChecked(10);
        var one = Decimal128.One;
        var beta = MatrixAlgebra.LeastSquares(new Decimal128[,] {{one,0},{0,one},{one,one}},new Decimal128[] {one+tiny,one,one+one+tiny});
        Assert.NotEqual(one,beta[0]);
        var epsilon = Decimal128.BitIncrement(one)-one;
        Assert.True(Decimal128.Abs(beta[0]-(one+tiny))<=Decimal128.CreateChecked(64)*epsilon);
    }

    [Fact]
    public void InvalidInputsAreRejected()
    {
        Assert.Throws<ArgumentNullException>(() => MatrixAlgebra.LeastSquares<double>(null!,[1]));
        Assert.Throws<ArgumentException>(() => MatrixAlgebra.LeastSquares(new double[,] {{double.NaN}},new double[] {1}));
        Assert.Throws<ArgumentException>(() => MatrixAlgebra.LeastSquares(new double[,] {{1}},new double[] {double.PositiveInfinity}));
        Assert.Throws<ArgumentException>(() => MatrixAlgebra.LeastSquares(new double[,] {{0}},new double[] {1}));
        Assert.Throws<ArithmeticException>(() => MatrixAlgebra.LeastSquares(new double[,] {{1e-300}},new double[] {1e300}));
        Assert.Empty(MatrixAlgebra.LeastSquares(new double[2,0],new double[] {1,2}));
    }
}
