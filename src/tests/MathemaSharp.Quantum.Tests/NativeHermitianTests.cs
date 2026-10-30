using System.Numerics;

namespace MathemaSharp.Quantum.Tests;

public class NativeHermitianTests
{
    private static void Verify<T>() where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        T N(int n) => T.CreateChecked(n);
        var tolerance=N(64)*(T.BitIncrement(T.One)-T.One);
        foreach(var scale in new T[] {T.One/N(100),T.One,T.MaxValue/N(4),T.Epsilon})
        {
            // H = scale * Y: composantes complexes, spectre ±scale.
            var h=new ComplexMatrix<T>(new Complex<T>[,] {{Complex<T>.Zero,new(T.Zero,-scale)},{new(T.Zero,scale),Complex<T>.Zero}});
            var (lp,vp,lm,vm)=QuantumAlgorithms.DiagonalizeHermitian2(h);
            Assert.True(T.Abs(lp/scale-T.One)<=tolerance);
            Assert.True(T.Abs(lm/scale+T.One)<=tolerance);
            Assert.True(T.Abs(QuantumAlgorithms.Norm<T>(vp)-T.One)<=tolerance);
            Assert.True(T.Abs(QuantumAlgorithms.Norm<T>(vm)-T.One)<=tolerance);
            Assert.True(QuantumAlgorithms.InnerProduct<T>(vp,vm).GetMagnitude()<=tolerance);
            // Residuals on the unitary matrix: no overflow in the check.
            var ap=QuantumAlgorithms.Apply(Pauli<T>.Y,vp);
            var am=QuantumAlgorithms.Apply(Pauli<T>.Y,vm);
            for(var i=0;i<2;i++)
            {
                Assert.True((ap[i]-vp[i]).GetMagnitude()<=tolerance);
                Assert.True((am[i]+vm[i]).GetMagnitude()<=tolerance);
            }
        }
        var identity=QuantumAlgorithms.DiagonalizeHermitian2(Pauli<T>.I);
        Assert.Equal(T.One,identity.LambdaPlus);
        Assert.Equal(T.One,identity.LambdaMinus);
        Assert.Equal(Complex<T>.Zero,QuantumAlgorithms.InnerProduct<T>(identity.VectorPlus,identity.VectorMinus));
        var zero=QuantumAlgorithms.DiagonalizeHermitian2(ComplexMatrix<T>.Zero(2,2));
        Assert.Equal(T.Zero,zero.LambdaPlus);
        Assert.Equal(T.Zero,zero.LambdaMinus);
        Assert.Equal(Complex<T>.Zero,QuantumAlgorithms.InnerProduct<T>(zero.VectorPlus,zero.VectorMinus));
        var diagonal=QuantumAlgorithms.DiagonalizeHermitian2(new ComplexMatrix<T>(new Complex<T>[,] {{new(-T.One,T.Zero),Complex<T>.Zero},{Complex<T>.Zero,new(T.One,T.Zero)}}));
        Assert.Equal(Complex<T>.One,diagonal.VectorPlus[1]);
        Assert.Equal(Complex<T>.Zero,diagonal.VectorPlus[0]);
    }
    [Fact] public void HalfNative()=>Verify<Half>();
    [Fact] public void FloatNative()=>Verify<float>();
    [Fact] public void DoubleNative()=>Verify<double>();
    [Fact] public void Decimal32Native()=>Verify<Decimal32>();
    [Fact] public void Decimal64Native()=>Verify<Decimal64>();
    [Fact] public void Decimal128Native()=>Verify<Decimal128>();

    [Fact]
    public void Decimal128TinyCouplingIsNotDiscarded()
    {
        var tiny=Decimal128.One;
        for(var i=0;i<25;i++) tiny/=Decimal128.CreateChecked(10);
        var h=new ComplexMatrix<Decimal128>(new Complex<Decimal128>[,] {{new(Decimal128.One,0),new(tiny,0)},{new(tiny,0),new(Decimal128.One,0)}});
        var (lp,vp,lm,vm)=QuantumAlgorithms.DiagonalizeHermitian2(h);
        Assert.Equal(tiny,lp-Decimal128.One);
        Assert.Equal(tiny,Decimal128.One-lm);
        Assert.NotEqual(Decimal128.Zero,vp[0].Real);
        Assert.NotEqual(Decimal128.Zero,vp[1].Real);
        var residual=QuantumAlgorithms.Apply(h,vp);
        var tolerance=Decimal128.CreateChecked(64)*(Decimal128.BitIncrement(Decimal128.One)-Decimal128.One);
        for(var i=0;i<2;i++) Assert.True((residual[i]-lp*vp[i]).GetMagnitude()<=tolerance);
        Assert.True(QuantumAlgorithms.InnerProduct<Decimal128>(vp,vm).GetMagnitude()<=tolerance);
    }

    [Fact]
    public void InvalidAndUnrepresentableInputsAreReported()
    {
        Assert.Throws<ArgumentException>(()=>QuantumAlgorithms.DiagonalizeHermitian2(ComplexMatrix<double>.Identity(3)));
        Assert.Throws<ArgumentException>(()=>QuantumAlgorithms.DiagonalizeHermitian2(new ComplexMatrix<double>(new Complex<double>[,] {{new(double.NaN,0),Complex<double>.Zero},{Complex<double>.Zero,Complex<double>.One}})));
        Assert.Throws<ArgumentException>(()=>QuantumAlgorithms.DiagonalizeHermitian2(new ComplexMatrix<double>(new Complex<double>[,] {{Complex<double>.One,Complex<double>.One},{Complex<double>.Zero,Complex<double>.One}})));
        var huge=new Complex<double>(double.MaxValue,0);
        Assert.Throws<ArithmeticException>(()=>QuantumAlgorithms.DiagonalizeHermitian2(new ComplexMatrix<double>(new Complex<double>[,] {{huge,huge},{huge,huge}})));
    }
}
