// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using MathemaSharp.Kernels;
using Xunit;

namespace MathemaSharp.Kernels.Tests;

public class FourierTests
{
    /// <summary>The definition, with twiddles from exact reduction: X[k] = Σ x[j] exp(−2πijk/n) in long double-free arithmetic (the sums are small).</summary>
    static Complex[] NaiveDft(Complex[] x, bool inverse)
    {
        int n = x.Length;
        var r = new Complex[n];
        for (int k = 0; k < n; k++)
        {
            Complex s = Complex.Zero;
            for (int j = 0; j < n; j++)
            {
                var w = Fourier.Twiddle((long)j * k, n);
                s += x[j] * (inverse ? w : Complex.Conjugate(w));
            }
            r[k] = s;
        }
        return r;
    }

    static Complex[] Signal(int n) => Enumerable.Range(0, n).Select(i => new Complex(Math.Sin(i * 0.7) + (i % 3), Math.Cos(i * 1.3) - 0.25 * (i % 5))).ToArray();

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(12)]
    [InlineData(16)] [InlineData(17)] [InlineData(30)] [InlineData(64)] [InlineData(97)] [InlineData(100)] [InlineData(127)] [InlineData(128)] [InlineData(250)]
    public void MatchesTheDefinitionForPowersOfTwoPrimesAndComposites(int n)
    {
        var x = Signal(n);
        foreach (var inverse in new[] { false, true })
        {
            var expected = NaiveDft(x, inverse);
            var got = (Complex[])x.Clone();
            Fourier.Transform(got, inverse);
            double scale = expected.Max(z => z.Magnitude) + 1;
            for (int i = 0; i < n; i++) Assert.True((expected[i] - got[i]).Magnitude <= 1e-12 * scale, $"n={n} inverse={inverse} i={i}: {expected[i]} vs {got[i]}");
        }
    }

    [Theory]
    [InlineData(8)] [InlineData(13)] [InlineData(100)]
    public void InverseUndoesForwardAfterScaling(int n)
    {
        var x = Signal(n);
        var y = (Complex[])x.Clone();
        Fourier.Transform(y, false);
        Fourier.Transform(y, true);
        for (int i = 0; i < n; i++) Assert.True((y[i] * Fourier.Scale(FourierNorm.Backward, n, true) - x[i]).Magnitude < 1e-12);
    }

    [Fact]
    public void QuarterTurnsAreExact()
    {
        Assert.Equal(new Complex(0, 1), Fourier.Twiddle(1, 4));
        Assert.Equal(new Complex(-1, 0), Fourier.Twiddle(2, 4));
        Assert.Equal(new Complex(0, -1), Fourier.Twiddle(3, 4));
        Assert.Equal(new Complex(Math.Sqrt(0.5), Math.Sqrt(0.5)), Fourier.Twiddle(1, 8));
        var x = new[] { new Complex(0, 0), new(1, 0), new(0, 0), new(-1, 0) };
        Fourier.Transform(x, false);
        Assert.Equal(new Complex(0, 0), x[0]);
        Assert.Equal(new Complex(0, -2), x[1]);   // exactly, not 1e-16 away
        Assert.Equal(new Complex(0, 0), x[2]);
        Assert.Equal(new Complex(0, 2), x[3]);
    }

    [Fact]
    public void HermitianSymmetryFollowsFromRealData()
    {
        var real = new double[] { 1, 4, 2, 8, 5, 7 };
        var x = real.Select(v => new Complex(v, 0)).ToArray();
        Fourier.Transform(x, false);
        Fourier.MakeHermitian(x);
        Assert.Equal(0.0, x[0].Imaginary); Assert.Equal(0.0, x[3].Imaginary);
        for (int i = 1; i < 3; i++) Assert.Equal(Complex.Conjugate(x[i]), x[6 - i]);
    }

    [Theory]
    [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(1)]
    public void InverseRealRecoversTheSignal(int n)
    {
        var real = Enumerable.Range(0, n).Select(i => Math.Sin(i * 0.9) * 3 + i).ToArray();
        var x = real.Select(v => new Complex(v, 0)).ToArray();
        Fourier.Transform(x, false);
        var back = new double[n];
        Fourier.InverseReal(x.AsSpan(0, n / 2 + 1), back, Fourier.Scale(FourierNorm.Backward, n, true));
        for (int i = 0; i < n; i++) Assert.Equal(real[i], back[i], 11);
    }

    [Fact]
    public void ScalesAndFrequencies()
    {
        Assert.Equal(1.0, Fourier.Scale(FourierNorm.Backward, 8, false));
        Assert.Equal(0.125, Fourier.Scale(FourierNorm.Backward, 8, true));
        Assert.Equal(1.0 / Math.Sqrt(8), Fourier.Scale(FourierNorm.Ortho, 8, false));
        Assert.Equal(0.125, Fourier.Scale(FourierNorm.Forward, 8, false));
        Assert.Equal(1.0, Fourier.Scale(FourierNorm.Forward, 8, true));
        var f = new double[5];
        Fourier.Frequencies(f, 1.0);
        Assert.Equal(new[] { 0.0, 0.2, 0.4, -0.4, -0.2 }, f);
        var g = new double[4];
        Fourier.Frequencies(g, 0.5);
        Assert.Equal(new[] { 0.0, 0.5, -1.0, -0.5 }, g);
        var rf = new double[3];
        Fourier.RealFrequencies(rf, 4, 1.0);
        Assert.Equal(new[] { 0.0, 0.25, 0.5 }, rf);
        Assert.Throws<ArgumentException>(() => Fourier.Frequencies(Span<double>.Empty, 1.0));
    }

    [Fact]
    public void EmptyAndSingleSamplesAreUnchanged()
    {
        var one = new[] { new Complex(3, -2) };
        Fourier.Transform(one, false);
        Assert.Equal(new Complex(3, -2), one[0]);
        Fourier.Transform(Span<Complex>.Empty, true);
    }
}
