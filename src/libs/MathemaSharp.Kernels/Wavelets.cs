// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp.Kernels;

/// <summary>The filter bank of a discrete wavelet (PyWavelets' <c>Wavelet</c>): decomposition and reconstruction low-pass and high-pass filters.
/// The tables are generated from PyWavelets (haar, dbN, symN, coifN, biorX.Y, rbioX.Y, dmey) by <c>tools/oracle/gen_wavelets.py</c> in the NDSharp repository.</summary>
public sealed class WaveletFilterBank
{
    public string Name { get; }
    public double[] DecLo { get; }
    public double[] DecHi { get; }
    public double[] RecLo { get; }
    public double[] RecHi { get; }
    public bool Orthogonal { get; }
    public int FilterLength => DecLo.Length;

    private WaveletFilterBank(string name, double[] dl, double[] dh, double[] rl, double[] rh, bool orth)
        => (Name, DecLo, DecHi, RecLo, RecHi, Orthogonal) = (name, dl, dh, rl, rh, orth);

    /// <summary>The names of the available discrete wavelets, in PyWavelets' order.</summary>
    public static IReadOnlyList<string> Names => WaveletFilters.All.Select(w => w.Name).ToList();

    /// <summary>The filter bank called <paramref name="name"/> (for example <c>"db4"</c>), or false when there is none.</summary>
    public static bool TryGet(string name, out WaveletFilterBank bank)
    {
        foreach (var w in WaveletFilters.All)
            if (w.Name == name)
            {
                bank = new WaveletFilterBank(w.Name, w.DecLo, w.DecHi, w.RecLo, w.RecHi, w.Orthogonal);
                return true;
            }
        bank = null!;
        return false;
    }
}

/// <summary>One level of the discrete wavelet transform of a signal and its inverse (<c>pywt.dwt</c> / <c>pywt.idwt</c>): a port of PyWavelets' convolution-based
/// transforms, with the same extension modes and output lengths, for every IEEE Generic Math format (the filter coefficients are converted to the format of the signal;
/// with <c>double</c> the arithmetic is the one NDSharp.Wavelets had, 195 929 results identical by SHA-256). Modes: <c>zero</c>, <c>constant</c>, <c>symmetric</c>,
/// <c>reflect</c>, <c>periodic</c>, <c>antisymmetric</c>, <c>antireflect</c>, <c>smooth</c>, <c>periodization</c>. Invalid arguments throw <see cref="ArgumentException"/>.
/// Several levels, axes and n-D arrays are the caller's business: apply these to each line.</summary>
public static class WaveletTransform
{
    /// <summary>Length of each coefficient array for a signal of <paramref name="inputLength"/> and a filter of <paramref name="filterLength"/>.</summary>
    public static int CoeffLength(int inputLength, int filterLength, string mode)
        => mode == "periodization" ? (inputLength + 1) / 2 : (inputLength + filterLength - 1) / 2;

    private static int Mod(int a, int n) => ((a % n) + n) % n;

    private static T[] Convert<T>(double[] filter) where T : IFloatingPointIeee754<T>
    {
        var r = new T[filter.Length];
        for (int i = 0; i < r.Length; i++) r[i] = T.CreateChecked(filter[i]);
        return r;
    }

    /// <summary>Value of the signal extended beyond its ends according to a PyWavelets mode.</summary>
    private static T Ext<T>(ReadOnlySpan<T> x, int i, string mode) where T : IFloatingPointIeee754<T>
    {
        int n = x.Length;
        if (i >= 0 && i < n) return x[i];
        switch (mode)
        {
            case "zero": return T.Zero;
            case "constant": return i < 0 ? x[0] : x[n - 1];
            case "symmetric":
            {
                int m = Mod(i, 2 * n);
                return m < n ? x[m] : x[2 * n - 1 - m];
            }
            case "reflect":
            {
                if (n == 1) return x[0];
                int m = Mod(i, 2 * n - 2);
                return m < n ? x[m] : x[2 * n - 2 - m];
            }
            case "periodic": return x[Mod(i, n)];
            case "antisymmetric":
            {
                int q = (int)Math.Floor((double)i / n);
                int r = i - q * n;
                T v = (q & 1) == 0 ? x[r] : x[n - 1 - r];
                return (q & 1) == 0 ? v : -v;
            }
            case "antireflect":
            {
                T two = T.CreateChecked(2);
                return i < 0 ? two * x[0] - Ext(x, -i, mode) : two * x[n - 1] - Ext(x, 2 * (n - 1) - i, mode);
            }
            case "smooth":
                if (n == 1) return x[0];
                return i < 0 ? x[0] + T.CreateChecked(i) * (x[1] - x[0]) : x[n - 1] + T.CreateChecked(i - (n - 1)) * (x[n - 1] - x[n - 2]);
            default:
                throw new ArgumentException($"Unknown mode '{mode}'");
        }
    }

    /// <summary>One level of the transform of a signal: (approximation, detail).</summary>
    public static (T[] cA, T[] cD) Dwt<T>(ReadOnlySpan<T> x, WaveletFilterBank w, string mode = "symmetric") where T : IFloatingPointIeee754<T>
    {
        int f = w.FilterLength, n = x.Length;
        if (n < 1) throw new ArgumentException("Invalid input data length");
        int outLen = CoeffLength(n, f, mode);
        var a = new T[outLen];
        var d = new T[outLen];
        var decLo = Convert<T>(w.DecLo);
        var decHi = Convert<T>(w.DecHi);
        if (mode == "periodization")
        {
            T[] xe = new T[n % 2 == 1 ? n + 1 : n];
            x.CopyTo(xe);
            if (n % 2 == 1) xe[n] = x[n - 1];
            int ne = xe.Length;
            for (int k = 0; k < outLen; k++)
            {
                T sa = T.Zero, sd = T.Zero;
                for (int j = 0; j < f; j++)
                {
                    T v = xe[Mod(2 * k + f / 2 - j, ne)];
                    sa += decLo[j] * v;
                    sd += decHi[j] * v;
                }
                a[k] = sa; d[k] = sd;
            }
            return (a, d);
        }
        for (int k = 0; k < outLen; k++)
        {
            T sa = T.Zero, sd = T.Zero;
            for (int j = 0; j < f; j++)
            {
                T v = Ext(x, 2 * k + 1 - j, mode);
                sa += decLo[j] * v;
                sd += decHi[j] * v;
            }
            a[k] = sa; d[k] = sd;
        }
        return (a, d);
    }

    /// <summary>Inverse of <see cref="Dwt{T}"/>; either coefficient array may be <c>null</c> (treated as zero).</summary>
    public static T[] Idwt<T>(T[]? cA, T[]? cD, WaveletFilterBank w, string mode = "symmetric") where T : IFloatingPointIeee754<T>
    {
        if (cA is null && cD is null) throw new ArgumentException("At least one coefficient parameter must be specified.");
        int m = (cA ?? cD)!.Length;
        if (cA is not null && cD is not null && cA.Length != cD.Length)
            throw new ArgumentException("Coefficient arrays must have the same length");
        int f = w.FilterLength;
        var recLo = Convert<T>(w.RecLo);
        var recHi = Convert<T>(w.RecHi);
        if (mode == "periodization")
        {
            var outp = new T[2 * m];
            for (int i = 0; i < 2 * m; i++)
            {
                T s = T.Zero;
                for (int k = 0; k < m; k++)
                {
                    // A filter longer than the signal wraps around more than once: sum every aliased tap.
                    for (int j = Mod(i - 2 * k + f / 2 - 1, 2 * m); j < f; j += 2 * m)
                    {
                        if (cA is not null) s += cA[k] * recLo[j];
                        if (cD is not null) s += cD[k] * recHi[j];
                    }
                }
                outp[i] = s;
            }
            return outp;
        }
        int len = 2 * m - f + 2;
        if (len < 1) throw new ArgumentException("Coefficient arrays are too short for this wavelet");
        var y = new T[len];
        for (int i = 0; i < len; i++)
        {
            T s = T.Zero;
            for (int k = 0; k < m; k++)
            {
                int j = i + f - 2 - 2 * k;
                if (j < 0 || j >= f) continue;
                if (cA is not null) s += cA[k] * recLo[j];
                if (cD is not null) s += cD[k] * recHi[j];
            }
            y[i] = s;
        }
        return y;
    }
}
