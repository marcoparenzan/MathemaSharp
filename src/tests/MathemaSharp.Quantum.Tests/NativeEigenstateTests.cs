using System.Numerics;

namespace MathemaSharp.Quantum.Tests;

public class NativeEigenstateTests
{
    private static void Verify<T>() where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        T N(int n)=>T.CreateChecked(n);
        var tolerance=N(32)*(T.BitIncrement(T.One)-T.One);
        foreach(var amplitude in new T[] {T.One,T.MaxValue,T.Epsilon})
        {
            var result=QuantumAlgorithms.CheckEigenstate<T>(Pauli<T>.X,[new(amplitude,T.Zero),new(amplitude,T.Zero)],tolerance);
            Assert.True(result.IsEigenstate);
            Assert.True((result.Eigenvalue-Complex<T>.One).GetMagnitude()<=tolerance);
        }
        foreach(var scale in new T[] {T.One,T.MaxValue/N(4),T.Epsilon})
        {
            var h=new ComplexMatrix<T>(new Complex<T>[,] {{Complex<T>.Zero,new(scale,T.Zero)},{new(scale,T.Zero),Complex<T>.Zero}});
            var yes=QuantumAlgorithms.CheckEigenstate<T>(h,[Complex<T>.One,Complex<T>.One],tolerance);
            Assert.True(yes.IsEigenstate);
            Assert.True(T.Abs(yes.Eigenvalue.Real/scale-T.One)<=tolerance);
            var no=QuantumAlgorithms.CheckEigenstate<T>(h,[Complex<T>.One,Complex<T>.Zero],tolerance);
            Assert.False(no.IsEigenstate);
        }
        Assert.True(QuantumAlgorithms.CheckEigenstate<T>(ComplexMatrix<T>.Zero(2,2),[Complex<T>.One,Complex<T>.Zero],T.Zero).IsEigenstate);
        var imaginary=new Complex<T>(T.Zero,T.One);
        var nonHermitian=new ComplexMatrix<T>(new Complex<T>[,] {{imaginary,Complex<T>.Zero},{Complex<T>.Zero,Complex<T>.One}});
        var complexEigen=QuantumAlgorithms.CheckEigenstate<T>(nonHermitian,[Complex<T>.One,Complex<T>.Zero],tolerance);
        Assert.True(complexEigen.IsEigenstate);
        Assert.Equal(imaginary,complexEigen.Eigenvalue);
        Assert.Throws<ArgumentOutOfRangeException>(()=>QuantumAlgorithms.CheckEigenstate<T>(Pauli<T>.X,[Complex<T>.One,Complex<T>.Zero],-T.One));
        Assert.Throws<ArgumentOutOfRangeException>(()=>QuantumAlgorithms.CheckEigenstate<T>(Pauli<T>.X,[Complex<T>.One,Complex<T>.Zero],T.NaN));
        Assert.Throws<ArgumentException>(()=>QuantumAlgorithms.CheckEigenstate<T>(Pauli<T>.X,[Complex<T>.Zero,Complex<T>.Zero],tolerance));
        Assert.Throws<ArgumentException>(()=>QuantumAlgorithms.CheckEigenstate<T>(Pauli<T>.X,[Complex<T>.One],tolerance));
    }
    [Fact] public void HalfNative()=>Verify<Half>();
    [Fact] public void FloatNative()=>Verify<float>();
    [Fact] public void DoubleNative()=>Verify<double>();
    [Fact] public void Decimal32Native()=>Verify<Decimal32>();
    [Fact] public void Decimal64Native()=>Verify<Decimal64>();
    [Fact] public void Decimal128Native()=>Verify<Decimal128>();

    [Fact]
    public void Decimal128DetectsSubDoublePerturbation()
    {
        var tiny=Decimal128.One;
        for(var i=0;i<25;i++) tiny/=Decimal128.CreateChecked(10);
        var tolerance=tiny/Decimal128.CreateChecked(100);
        var result=QuantumAlgorithms.CheckEigenstate<Decimal128>(Pauli<Decimal128>.Z,[Complex<Decimal128>.One,new(tiny,Decimal128.Zero)],tolerance);
        Assert.False(result.IsEigenstate);
    }
}
