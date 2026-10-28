using System.Numerics;
using MathemaSharp.Core;
namespace MathemaSharp.Core.Tests;

public class FftAnalyticOracleTests
{
    [Fact] public void FourPointClosedFormOnSixFormats()
    { Check<Half>(); Check<float>(); Check<double>(); Check<Decimal32>(); Check<Decimal64>(); Check<Decimal128>(); }
    private static void Check<T>() where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        // Exact independent DFT for [1,2,3,4]: [10,-2+2i,-2,-2-2i].
        var input = new Complex<T>[] { new(T.One,T.Zero), new(T.CreateChecked(2),T.Zero), new(T.CreateChecked(3),T.Zero), new(T.CreateChecked(4),T.Zero) };
        var expected = new Complex<T>[] { new(T.CreateChecked(10),T.Zero),new(T.CreateChecked(-2),T.CreateChecked(2)),new(T.CreateChecked(-2),T.Zero),new(T.CreateChecked(-2),T.CreateChecked(-2)) };
        var tolerance = (T.BitIncrement(T.One)-T.One)*T.CreateChecked(128);
        var actual=Signals.Fft<T>(input);
        for(int i=0;i<4;i++) Assert.True(T.Hypot(actual[i].Real-expected[i].Real,actual[i].Imaginary-expected[i].Imaginary)<=tolerance);
        var inverse=Signals.Ifft<T>(expected);
        for(int i=0;i<4;i++) Assert.True(T.Hypot(inverse[i].Real-input[i].Real,inverse[i].Imaginary-input[i].Imaginary)<=tolerance);
    }
    [Fact] public void ComplexToneUsesNegativeForwardSign()
    {
        var x=new Complex<double>[] {new(1,0),new(0,1),new(-1,0),new(0,-1)};
        var result=Signals.Fft<double>(x);
        for(int i=0;i<4;i++) Assert.True(double.Hypot(result[i].Real-(i==1?4:0),result[i].Imaginary)<1e-13);
    }
    [Fact] public void ParsevalOnComplexEightPointSignal()
    {
        var x=Enumerable.Range(0,8).Select(i=>new Complex<double>(i-3,(i%3)-1)).ToArray();
        var y=Signals.Fft<double>(x);
        var time=x.Sum(v=>v.Real*v.Real+v.Imaginary*v.Imaginary);
        var frequency=y.Sum(v=>v.Real*v.Real+v.Imaginary*v.Imaginary)/x.Length;
        Assert.True(Math.Abs(time-frequency)<=1e-12*time);
    }
}
