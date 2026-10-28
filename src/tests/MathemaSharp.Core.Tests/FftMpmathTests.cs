using System.Globalization;
using System.Numerics;
using System.Text.Json;
using MathemaSharp.Core;
namespace MathemaSharp.Core.Tests;

public class FftMpmathTests
{
    [Fact] public void FloatAgainstIndependentDft() => Check<float>("2e-4");
    [Fact] public void DoubleAgainstIndependentDft() => Check<double>("2e-12");
    [Fact] public void Decimal64AgainstIndependentDft() => Check<Decimal64>("2e-12");
    [Fact] public void Decimal128AgainstIndependentDft() => Check<Decimal128>("2e-29");
    private static void Check<T>(string bound) where T:IFloatingPointIeee754<T>,IMinMaxValue<T>
    {
        var path=Path.Combine(NotebookTests.RepositoryRoot().FullName,"ops","oracles","fft-mpmath.json");
        using var doc=JsonDocument.Parse(File.ReadAllText(path));
        Assert.Equal(200,doc.RootElement.GetProperty("precisionDigits").GetInt32());
        T Parse(string text)=>T.Parse(text,CultureInfo.InvariantCulture);
        var tolerance=Parse(bound);
        foreach(var fixture in doc.RootElement.GetProperty("cases").EnumerateArray())
        {
            Complex<T>[] Read(string name)=>fixture.GetProperty(name).EnumerateArray().Select(z=>new Complex<T>(Parse(z[0].GetString()!),Parse(z[1].GetString()!))).ToArray();
            var input=Read("input");var expected=Read("output");var actual=Signals.Fft<T>(input);
            for(int i=0;i<input.Length;i++)
            {
                var error=T.Hypot(actual[i].Real-expected[i].Real,actual[i].Imaginary-expected[i].Imaginary);
                var scale=T.Max(T.One,T.Hypot(expected[i].Real,expected[i].Imaginary));
                Assert.True(error/scale<=tolerance,$"{typeof(T).Name} n={input.Length}, k={i}, error={error/scale}, bound={bound}");
            }
        }
    }
    [Theory] [InlineData(1024)] [InlineData(1009)]
    public void LargerDoubleRoundTrips(int n)
    {
        var input=Enumerable.Range(0,n).Select(i=>new Complex<double>((i%17)/16.0,(i%11)/8.0)).ToArray();
        var back=Signals.Ifft<double>(Signals.Fft<double>(input));
        for(int i=0;i<n;i++) Assert.True(double.Hypot(back[i].Real-input[i].Real,back[i].Imaginary-input[i].Imaginary)<1e-11);
    }
}
