using System.Numerics;
using MathemaSharp.Core;
namespace MathemaSharp.Core.Tests;
public class BluesteinTests
{
    [Fact] public void PrimeAndCompositeLengthsOnSixFormats()
    { Check<Half>(); Check<float>(); Check<double>(); Check<Decimal32>(); Check<Decimal64>(); Check<Decimal128>(); }
    private static void Check<T>() where T:IFloatingPointIeee754<T>,IMinMaxValue<T>
    {
        foreach(int n in new[]{3,5,7,9,10,15,17})
        {
            var input=Enumerable.Range(0,n).Select(i=>new Complex<T>(T.CreateChecked(i%3-1),T.CreateChecked(i%2))).ToArray();
            var original=input.ToArray();var actual=Signals.Fft<T>(input);var expected=Signals.Dft<T>(input);var back=Signals.Ifft<T>(actual);
            var tolerance=(T.BitIncrement(T.One)-T.One)*T.CreateChecked(n*64);
            for(int i=0;i<n;i++)
            {
                Assert.True(T.Hypot(actual[i].Real-expected[i].Real,actual[i].Imaginary-expected[i].Imaginary)<=tolerance,$"{typeof(T).Name}, n={n}, k={i}");
                Assert.True(T.Hypot(back[i].Real-input[i].Real,back[i].Imaginary-input[i].Imaginary)<=tolerance);
            }
            Assert.Equal(original,input);
        }
    }
    [Theory] [InlineData(3)] [InlineData(7)] [InlineData(11)]
    public void AnalyticImpulseAndConstant(int n)
    {
        var impulse=new Complex<double>[n];impulse[0]=Complex<double>.One;
        Assert.All(Signals.Fft<double>(impulse),v=>Assert.True(double.Hypot(v.Real-1,v.Imaginary)<1e-12));
        var constant=Signals.Fft<double>(Enumerable.Repeat(Complex<double>.One,n).ToArray());
        for(int i=0;i<n;i++)Assert.True(double.Hypot(constant[i].Real-(i==0?n:0),constant[i].Imaginary)<1e-11);
    }
    [Fact] public void Decimal128SmallSignalSurvives()
    {
        var delta=Decimal128.Parse("1e-25");
        var output=Signals.Fft<Decimal128>(new Complex<Decimal128>[] {new(delta,Decimal128.Zero),Complex<Decimal128>.Zero,Complex<Decimal128>.Zero});
        var bound=Decimal128.Parse("1e-54");
        Assert.All(output,v=>Assert.True(Decimal128.Hypot(v.Real-delta,v.Imaginary)<bound));
    }
}
