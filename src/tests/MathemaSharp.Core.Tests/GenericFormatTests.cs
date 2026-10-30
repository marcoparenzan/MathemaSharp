using System.Numerics;

namespace MathemaSharp.Core.Tests;

/// <summary>Precision and Signals are generic over the .NET 11 Generic Math IEEE formats: float, double, Half, Decimal32/64/128.</summary>
public class GenericFormatTests
{
    private static T V<T>(double x) where T : IFloatingPointIeee754<T> => T.CreateChecked(x);

    [Fact] public void NearlyEqualScalesItsDefaultToleranceWithTheFormat()
    {
        // 0.1 + 0.2 ≠ 0.3 in binary but it is "nearly equal": the default is 4096 ε of the type, so it holds in every format.
        Assert.True(Precision.NearlyEqual(0.1f + 0.2f, 0.3f));
        Assert.True(Precision.NearlyEqual(0.1 + 0.2, 0.3));
        Assert.True(Precision.NearlyEqual(V<Decimal64>(0.1) + V<Decimal64>(0.2), V<Decimal64>(0.3)));
        Assert.True(Precision.NearlyEqual(Decimal128.One / V<Decimal128>(3) * V<Decimal128>(3), Decimal128.One));
        // The same difference (1e-9) is significant for double and Decimal128 but not for float.
        Assert.False(Precision.NearlyEqual(1.0, 1.0 + 1e-9));
        Assert.False(Precision.NearlyEqual(Decimal128.One, Decimal128.One + V<Decimal128>(1e-9)));
        Assert.True(Precision.NearlyEqual(1.0f, 1.0f + 1e-9f));
        Assert.False(Precision.NearlyEqual(Decimal64.NaN, Decimal64.NaN));
    }

    [Fact] public void DftOfACosineHasTwoPeaksInEveryFormat()
    {
        Check<float>(1e-4);
        Check<double>(1e-12);
        Check<Decimal64>(1e-12);
        Check<Decimal128>(1e-12);

        static void Check<T>(double tolerance) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
        {
            const int n = 16;
            var samples = new Complex<T>[n];
            for (var i = 0; i < n; i++) samples[i] = new Complex<T>(V<T>(Math.Cos(2 * Math.PI * 3 * i / n)), T.Zero);
            var spectrum = Signals.Dft<T>(samples);
            for (var k = 0; k < n; k++)
            {
                var expected = k is 3 or 13 ? 8.0 : 0.0;
                Assert.True(Math.Abs(double.CreateChecked(spectrum[k].GetMagnitude()) - expected) < tolerance * 10, $"{typeof(T).Name} bin {k}");
            }
        }
    }

    [Fact] public void VectorDotFallsBackToScalarWhereSimdIsNotSupported()
    {
        Assert.False(Vector<Decimal64>.IsSupported);
        var random = new Random(7);
        var left = Enumerable.Range(0, 100).Select(_ => V<Decimal64>(random.NextDouble() - 0.5)).ToArray();
        var right = Enumerable.Range(0, 100).Select(_ => V<Decimal64>(random.NextDouble() - 0.5)).ToArray();
        Assert.Equal(NumericAlgorithms.Dot<Decimal64>(left, right), Signals.VectorDot<Decimal64>(left, right));

        float[] a = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        Assert.Equal(285f, Signals.VectorDot<float>(a, a));
    }

    [Fact] public void KahanSumWorksOnDecimalIeeeFormats()
    {
        Decimal64[] values = [V<Decimal64>(1e15), V<Decimal64>(1), V<Decimal64>(1), V<Decimal64>(-1e15)];
        Assert.Equal(V<Decimal64>(2), NumericAlgorithms.KahanSum<Decimal64>(values));
    }
}
