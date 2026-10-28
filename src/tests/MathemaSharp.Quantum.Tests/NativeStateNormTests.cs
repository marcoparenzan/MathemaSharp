using System.Numerics;

namespace MathemaSharp.Quantum.Tests;

public class NativeStateNormTests
{
    private static void Verify<T>() where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        T N(int n) => T.CreateChecked(n);
        var tolerance = N(32) * (T.BitIncrement(T.One) - T.One);
        Assert.Equal(N(5), QuantumAlgorithms.Norm<T>([new(N(3),N(4))]));
        Assert.Equal(T.Zero, QuantumAlgorithms.Norm<T>([]));
        foreach (var scale in new T[] {T.One/N(100),N(100),T.MaxValue/N(4),T.Epsilon})
        {
            Complex<T>[] input = [new(scale,T.Zero),new(T.Zero,scale)];
            var normalized = QuantumAlgorithms.Normalize<T>(input);
            var expected = T.One/T.Sqrt(N(2));
            Assert.True(T.Abs(normalized[0].Real-expected)<=tolerance);
            Assert.True(T.Abs(normalized[1].Imaginary-expected)<=tolerance);
            Assert.Equal(T.Zero,normalized[0].Imaginary);
            Assert.Equal(T.Zero,normalized[1].Real);
            Assert.True(T.Abs(QuantumAlgorithms.Norm<T>(normalized)-T.One)<=tolerance);
            Assert.Equal(scale,input[0].Real);
            Assert.Equal(scale,input[1].Imaginary);
        }
        // La norma non è rappresentabile, ma lo stato finito rimane normalizzabile.
        Complex<T>[] enormous = [new(T.MaxValue,T.MaxValue),new(T.MaxValue,T.MaxValue)];
        Assert.True(T.IsPositiveInfinity(QuantumAlgorithms.Norm<T>(enormous)));
        var unit = QuantumAlgorithms.Normalize<T>(enormous);
        foreach(var value in unit)
        {
            Assert.True(T.Abs(value.Real-T.One/N(2))<=tolerance);
            Assert.True(T.Abs(value.Imaginary-T.One/N(2))<=tolerance);
        }
        Assert.Throws<ArgumentException>(()=>QuantumAlgorithms.Normalize<T>([]));
        Assert.Throws<ArgumentException>(()=>QuantumAlgorithms.Normalize<T>([Complex<T>.Zero]));
        Assert.Throws<ArgumentException>(()=>QuantumAlgorithms.Normalize<T>([new(T.NaN,T.Zero)]));
        Assert.Throws<ArgumentException>(()=>QuantumAlgorithms.Normalize<T>([new(T.Zero,T.PositiveInfinity)]));
    }
    [Fact] public void HalfNative()=>Verify<Half>();
    [Fact] public void FloatNative()=>Verify<float>();
    [Fact] public void DoubleNative()=>Verify<double>();
    [Fact] public void Decimal32Native()=>Verify<Decimal32>();
    [Fact] public void Decimal64Native()=>Verify<Decimal64>();
    [Fact] public void Decimal128Native()=>Verify<Decimal128>();

    [Fact]
    public void Decimal128RetainsTinyImaginaryAmplitude()
    {
        var tiny=Decimal128.One;
        for(var i=0;i<25;i++) tiny/=Decimal128.CreateChecked(10);
        var normalized=QuantumAlgorithms.Normalize<Decimal128>([new(Decimal128.One,tiny)]);
        Assert.Equal(tiny,normalized[0].Imaginary);
        Assert.Equal(Decimal128.One,normalized[0].Real);
    }
}
