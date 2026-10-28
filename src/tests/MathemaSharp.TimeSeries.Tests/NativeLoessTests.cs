using System.Numerics;
using MathemaSharp.TimeSeries;

namespace MathemaSharp.TimeSeries.Tests;

public class NativeLoessTests
{
    private static void Verify<T>() where T : IFloatingPointIeee754<T>
    {
        T N(int n) => T.CreateChecked(n);
        T[] line = [N(1), N(3), N(5), N(7), N(9), N(11), N(13)];
        var epsilon = T.BitIncrement(T.One) - T.One;
        foreach (var x in new T[] { N(2), N(3), N(4), N(3) / N(2) })
        {
            var actual = Stl.Loess<T>(line, [], 5, x);
            var expected = T.One + N(2) * x;
            Assert.True(T.Abs(actual - expected) <= N(64) * epsilon * T.Abs(expected));
        }
        Assert.Equal(N(7), Stl.Loess<T>([N(7)], [], 5, T.Zero));
        Assert.Equal(N(5), Stl.Loess<T>(line, new T[line.Length], 5, N(2)));
    }

    [Fact] public void HalfNative() => Verify<Half>();
    [Fact] public void FloatNative() => Verify<float>();
    [Fact] public void DoubleNative() => Verify<double>();
    [Fact] public void Decimal32Native() => Verify<Decimal32>();
    [Fact] public void Decimal64Native() => Verify<Decimal64>();
    [Fact] public void Decimal128Native() => Verify<Decimal128>();

    [Fact]
    public void Decimal128KeepsTinyTrend()
    {
        var tiny = Decimal128.One;
        for (var i = 0; i < 25; i++) tiny /= Decimal128.CreateChecked(10);
        var line = Enumerable.Range(0, 7).Select(i => Decimal128.One + Decimal128.CreateChecked(i) * tiny).ToArray();
        var x = Decimal128.CreateChecked(3) / Decimal128.CreateChecked(2);
        var actual = Stl.Loess<Decimal128>(line, [], 5, x);
        var expected = Decimal128.One + x * tiny;
        Assert.NotEqual(Decimal128.One, actual);
        var epsilon = Decimal128.BitIncrement(Decimal128.One) - Decimal128.One;
        Assert.True(Decimal128.Abs(actual - expected) <= Decimal128.CreateChecked(64) * epsilon);
    }

    [Fact]
    public void InvalidInputsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => Stl.Loess<double>([1, 2], [1], 3, 0));
        Assert.Throws<ArgumentException>(() => Stl.Loess<double>([1, double.NaN], [], 3, 0));
        Assert.Throws<ArgumentException>(() => Stl.Loess<double>([1, 2], [1, -1], 3, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Stl.Loess<double>([1, 2], [], 3, double.PositiveInfinity));
    }

    [Fact]
    public void LargeIndexDoesNotDestroyLocalLine()
    {
        var values = Enumerable.Range(0, 100001).Select(i => 1.0 + 2 * (i - 100000)).ToArray();
        var actual = Stl.Loess<double>(values, [], 7, 99999.5);
        Assert.True(Math.Abs(actual) < 1e-12);
    }
}
