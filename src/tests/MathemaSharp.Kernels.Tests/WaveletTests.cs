// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using MathemaSharp.Kernels;
using Xunit;

namespace MathemaSharp.Kernels.Tests;

/// <summary>The wavelet kernel moved from NDSharp.Wavelets without changing a bit (195 929 results compared by SHA-256 before and after). These tests check it
/// against values worked out by hand and against the properties of the transform.</summary>
public class WaveletTests
{
    static WaveletFilterBank Get(string name) => WaveletFilterBank.TryGet(name, out var b) ? b : throw new Xunit.Sdk.XunitException("no wavelet " + name);

    static double[] Signal(int n)
    {
        var x = new double[n];
        for (int i = 0; i < n; i++) x[i] = Math.Round(Math.Sin(i * 0.9) * 7 + (i % 5), 4);
        return x;
    }

    [Fact]
    public void HaarOfAShortSignalIsKnownByHand()
    {
        // pywt.dwt([1, 2, 3, 4], 'haar'): approximation (x[2k] + x[2k+1]) / √2, detail (x[2k] − x[2k+1]) / √2 (dec_hi = [−1/√2, 1/√2]).
        var (a, d) = WaveletTransform.Dwt<double>([1, 2, 3, 4], Get("haar"));
        double r = Math.Sqrt(0.5);
        Assert.Equal([3 * r, 7 * r], a, new DoubleComparer());
        Assert.Equal([-r, -r], d, new DoubleComparer());
    }

    sealed class DoubleComparer : IEqualityComparer<double>
    {
        public bool Equals(double x, double y) => Math.Abs(x - y) < 1e-14;
        public int GetHashCode(double v) => 0;
    }

    [Fact]
    public void TheCatalogueHasEveryDiscreteWavelet()
    {
        Assert.Equal(106, WaveletFilterBank.Names.Count);
        Assert.Contains("db4", WaveletFilterBank.Names);
        Assert.False(WaveletFilterBank.TryGet("nope", out _));
    }

    [Fact]
    public void FilterLengthsFollowTheFamilies()
    {
        Assert.Equal(2, Get("haar").FilterLength);
        Assert.Equal(8, Get("db4").FilterLength);
        Assert.Equal(6, Get("coif1").FilterLength);
        Assert.True(Get("db4").Orthogonal);
        Assert.False(Get("bior2.2").Orthogonal);
    }

    [Theory]
    [InlineData("zero", 7, 3, 4)]          // (7 + 3 − 1) / 2
    [InlineData("symmetric", 8, 4, 5)]     // (8 + 4 − 1) / 2
    [InlineData("periodization", 8, 4, 4)] // ceil(8 / 2)
    [InlineData("periodization", 7, 4, 4)] // ceil(7 / 2)
    public void CoefficientLength(string mode, int n, int filter, int expected)
        => Assert.Equal(expected, WaveletTransform.CoeffLength(n, filter, mode));

    [Fact]
    public void OrthogonalWaveletsReconstructEvenLengthSignals()
    {
        foreach (var name in WaveletFilterBank.Names.Where(n => Get(n).Orthogonal && n != "dmey")) // dmey (Meyer) is orthogonal only approximately: its 62 truncated taps alias on 64 samples
        foreach (var mode in new[] { "symmetric", "periodization", "zero", "constant", "periodic", "reflect" })
        {
            var w = Get(name);
            var x = Signal(64);
            var (a, d) = WaveletTransform.Dwt(x, w, mode);
            var y = WaveletTransform.Idwt(a, d, w, mode);
            Assert.Equal(x.Length, y.Length);
            // Exact away from the ends for every extension mode; periodization is exact everywhere.
            int margin = mode == "periodization" ? 0 : w.FilterLength;
            for (int i = margin; i < x.Length - margin; i++) Assert.True(Math.Abs(x[i] - y[i]) < 1e-9, $"{name} {mode} i={i} x={x[i]} y={y[i]}");
        }
    }

    [Fact]
    public void OneCoefficientArrayAloneIsTreatedAsZeroForTheOther()
    {
        var w = Get("db2");
        var (a, d) = WaveletTransform.Dwt(Signal(32), w, "periodization");
        var both = WaveletTransform.Idwt(a, d, w, "periodization");
        var onlyA = WaveletTransform.Idwt(a, null, w, "periodization");
        var onlyD = WaveletTransform.Idwt(null, d, w, "periodization");
        for (int i = 0; i < both.Length; i++) Assert.Equal(both[i], onlyA[i] + onlyD[i], 12);
    }

    [Fact]
    public void ArgumentErrors()
    {
        var w = Get("db2");
        Assert.Equal("Invalid input data length", Assert.Throws<ArgumentException>(() => WaveletTransform.Dwt<double>([], w)).Message);
        Assert.Equal("Unknown mode 'bogus'", Assert.Throws<ArgumentException>(() => WaveletTransform.Dwt(Signal(8), w, "bogus")).Message);
        Assert.Throws<ArgumentException>(() => WaveletTransform.Idwt<double>(null, null, w));
        Assert.Throws<ArgumentException>(() => WaveletTransform.Idwt(new double[3], new double[4], w));
        Assert.Throws<ArgumentException>(() => WaveletTransform.Idwt(new double[1], null, Get("db10"), "symmetric"));
    }
}
