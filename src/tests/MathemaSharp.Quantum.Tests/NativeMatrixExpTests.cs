using System.Numerics;

namespace MathemaSharp.Quantum.Tests;

public class NativeMatrixExpTests
{
    private static void Verify<T>() where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        T N(int n)=>T.CreateChecked(n);
        var tolerance=N(64)*(T.BitIncrement(T.One)-T.One);
        var zero=QuantumAlgorithms.MatrixExp(ComplexMatrix<T>.Zero(2,2));
        Assert.Equal(Complex<T>.One,zero[0,0]);
        Assert.Equal(Complex<T>.One,zero[1,1]);
        Assert.Equal(Complex<T>.Zero,zero[0,1]);
        var diagonal=new ComplexMatrix<T>(new Complex<T>[,] {{Complex<T>.One,Complex<T>.Zero},{Complex<T>.Zero,new(-T.One,T.Zero)}});
        var exp=QuantumAlgorithms.MatrixExp(diagonal);
        Assert.True(T.Abs(exp[0,0].Real-T.Exp(T.One))<=tolerance*T.Exp(T.One));
        Assert.True(T.Abs(exp[1,1].Real-T.Exp(-T.One))<=tolerance);
        // Nilpotent: exp(A)=I+A, even when the old computation of the factor 2^s overflowed on Half.
        var large=N(32768);
        var nilpotent=new ComplexMatrix<T>(new Complex<T>[,] {{Complex<T>.Zero,new(large,T.Zero)},{Complex<T>.Zero,Complex<T>.Zero}});
        var result=QuantumAlgorithms.MatrixExp(nilpotent);
        Assert.Equal(Complex<T>.One,result[0,0]);
        Assert.Equal(Complex<T>.One,result[1,1]);
        Assert.True(T.Abs(result[0,1].Real/large-T.One)<=tolerance);
        Assert.Equal(large,nilpotent[0,1].Real);
        var u=QuantumAlgorithms.TimeEvolution(Pauli<T>.X,T.Pi/N(2));
        Assert.True(u.IsUnitary(tolerance));
        Assert.True(u[0,0].GetMagnitude()<=tolerance);
        Assert.True((u[0,1]+Complex<T>.ImaginaryOne).GetMagnitude()<=tolerance);
        Assert.Throws<ArgumentOutOfRangeException>(()=>QuantumAlgorithms.MatrixExp(diagonal,0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>QuantumAlgorithms.TimeEvolution(Pauli<T>.X,T.One,T.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(()=>QuantumAlgorithms.TimeEvolution(Pauli<T>.X,T.NaN));
    }
    [Fact] public void HalfNative()=>Verify<Half>();
    [Fact] public void FloatNative()=>Verify<float>();
    [Fact] public void DoubleNative()=>Verify<double>();
    [Fact] public void Decimal32Native()=>Verify<Decimal32>();
    [Fact] public void Decimal64Native()=>Verify<Decimal64>();
    [Fact] public void Decimal128Native()=>Verify<Decimal128>();

    [Fact]
    public void Decimal128RetainsTinyMatrixEntry()
    {
        var tiny=Decimal128.One;
        for(var i=0;i<25;i++) tiny/=Decimal128.CreateChecked(10);
        var zero=Complex<Decimal128>.Zero;
        var m=new ComplexMatrix<Decimal128>(new Complex<Decimal128>[,] {{zero,new(tiny,0)},{zero,zero}});
        var exp=QuantumAlgorithms.MatrixExp(m);
        Assert.Equal(tiny,exp[0,1].Real);
    }

    [Fact]
    public void NonFiniteInputAndOverflowAreReported()
    {
        Assert.Throws<ArgumentException>(()=>QuantumAlgorithms.MatrixExp(new ComplexMatrix<double>(new Complex<double>[,] {{new(double.NaN,0)}})));
        Assert.Throws<ArgumentException>(()=>QuantumAlgorithms.MatrixExp(new ComplexMatrix<double>(2,3)));
        Assert.Throws<ArithmeticException>(()=>QuantumAlgorithms.MatrixExp(new ComplexMatrix<double>(new Complex<double>[,] {{new(1000,0)}})));
    }
}
