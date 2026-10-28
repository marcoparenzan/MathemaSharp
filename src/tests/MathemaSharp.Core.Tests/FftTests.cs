using System.Numerics;
using MathemaSharp.Core;
namespace MathemaSharp.Core.Tests;
public class FftTests
{
    [Fact] public void AllSixFormatsMatchDftAndRoundTrip()
    { Check<Half>(); Check<float>(); Check<double>(); Check<Decimal32>(); Check<Decimal64>(); Check<Decimal128>(); }
    private static void Check<T>() where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        foreach (var n in new[] {1,2,4,8,16})
        {
            var x=Enumerable.Range(0,n).Select(i=>new Complex<T>(T.CreateChecked(i%3-1),T.CreateChecked(i%2))).ToArray();
            var original=x.ToArray(); var fft=Signals.Fft<T>(x); var dft=Signals.Dft<T>(x); var back=Signals.Ifft<T>(fft);
            var tolerance=T.Sqrt(T.BitIncrement(T.One)-T.One)*T.CreateChecked(n*8);
            for(int i=0;i<n;i++)
            {
                Assert.True(T.Hypot(fft[i].Real-dft[i].Real,fft[i].Imaginary-dft[i].Imaginary)<=tolerance);
                Assert.True(T.Hypot(back[i].Real-x[i].Real,back[i].Imaginary-x[i].Imaginary)<=tolerance);
            }
            Assert.Equal(original,x);
        }
    }
    [Fact] public void AnalyticImpulseAndConstant()
    {
        var impulse=new Complex<double>[] {Complex<double>.One,Complex<double>.Zero,Complex<double>.Zero,Complex<double>.Zero};
        Assert.All(Signals.Fft<double>(impulse),v=>Assert.Equal(Complex<double>.One,v));
        var output=Signals.Fft<double>(Enumerable.Repeat(Complex<double>.One,4).ToArray());
        Assert.Equal(new Complex<double>(4,0),output[0]);
        Assert.All(output.Skip(1),v=>Assert.Equal(Complex<double>.Zero,v));
    }
    [Fact] public void Decimal128IncrementSurvives()
    {
        var delta=Decimal128.Parse("1e-25");
        var output=Signals.Fft<Decimal128>(new Complex<Decimal128>[] {new(Decimal128.One,Decimal128.Zero),new(delta,Decimal128.Zero)});
        Assert.Equal(delta,output[0].Real-Decimal128.One);
    }
    [Fact] public void EmptyAndInvalidInputs()
    {
        Assert.Empty(Signals.Fft<double>(Array.Empty<Complex<double>>()));
        Assert.Equal(3, Signals.Fft<double>(new Complex<double>[3]).Length);
        Assert.Throws<ArgumentException>(()=>Signals.Ifft<double>(new Complex<double>[] {new(double.NaN,0)}));
        Assert.Throws<ArithmeticException>(()=>Signals.Fft<double>(new Complex<double>[] {new(double.MaxValue,0),new(double.MaxValue,0)}));
    }
}
