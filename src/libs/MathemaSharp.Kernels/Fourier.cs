// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp.Kernels;

/// <summary>Normalisation of a Fourier transform, named as in <c>numpy.fft</c>'s <c>norm</c> argument.</summary>
public enum FourierNorm
{
    /// <summary>No scaling forward, <c>1/n</c> inverse (NumPy default <c>"backward"</c>; MATLAB's convention).</summary>
    Backward,
    /// <summary>Both directions scaled by <c>1/√n</c> (<c>"ortho"</c>).</summary>
    Ortho,
    /// <summary><c>1/n</c> forward, no scaling inverse (<c>"forward"</c>).</summary>
    Forward,
}

/// <summary>One-dimensional discrete Fourier transform of <see cref="Complex"/> data, any length. Power-of-two lengths use an iterative radix-2 transform, other lengths
/// Bluestein's algorithm (a convolution of power-of-two length). Twiddle factors come from exact quadrant and octant reduction, so values such as <c>cos(π/2)</c> are exactly
/// zero (as in pocketfft) instead of 6e-17. Transforms are unnormalised; <see cref="Scale"/> gives the factor for a <see cref="FourierNorm"/>. Multi-dimensional transforms,
/// padding and axes are the caller's business: apply <see cref="Transform"/> to each line.</summary>
public static class Fourier
{
    /// <summary>cos and sin of 2πk/n as a complex number, with octant symmetry (exact zeros, ones and √½). <paramref name="n"/> must be positive.</summary>
    public static Complex Twiddle(long k, long n)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));
        long m = ((k % n) + n) % n;
        long four = 4 * m;
        int q = (int)(four / n);
        long r = four - q * n;            // remainder in [0, n): the angle within the quadrant is (π/2)·r/n
        double c, s;
        if (r == 0) { c = 1.0; s = 0.0; }
        else if (2 * r == n) { c = s = Math.Sqrt(0.5); }
        else if (2 * r < n)
        {
            double phi = Math.PI / 2 * ((double)r / n);
            c = Math.Cos(phi); s = Math.Sin(phi);
        }
        else
        {
            double phi = Math.PI / 2 * ((double)(n - r) / n);
            c = Math.Sin(phi); s = Math.Cos(phi);
        }
        return q switch
        {
            0 => new Complex(c, s),
            1 => new Complex(-s, c),
            2 => new Complex(-c, -s),
            _ => new Complex(s, -c),
        };
    }

    static bool IsPow2(int n) => n > 0 && (n & (n - 1)) == 0;

    /// <summary>In-place forward DFT (sign −1) of a power-of-two length.</summary>
    static void Radix2(Span<Complex> x)
    {
        int n = x.Length;
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1) j ^= bit;
            j ^= bit;
            if (i < j) (x[i], x[j]) = (x[j], x[i]);
        }
        var w = new Complex[Math.Max(n / 2, 1)];
        for (int j = 0; j < n / 2; j++) { var t = Twiddle(j, n); w[j] = new Complex(t.Real, -t.Imaginary); }
        for (int len = 2; len <= n; len <<= 1)
        {
            int half = len / 2, step = n / len;
            for (int i = 0; i < n; i += len)
                for (int j = 0; j < half; j++)
                {
                    var u = x[i + j];
                    var v = x[i + j + half] * w[j * step];
                    x[i + j] = u + v;
                    x[i + j + half] = u - v;
                }
        }
    }

    static void Forward(Span<Complex> x)
    {
        int n = x.Length;
        if (n <= 1) return;
        if (IsPow2(n)) { Radix2(x); return; }
        // Bluestein: X[k] = c[k] · Σ_j (x[j] c[j]) · conj(c[k−j]),  c[j] = exp(−iπj²/n)
        int m = 1;
        while (m < 2 * n - 1) m <<= 1;
        var chirp = new Complex[n];
        for (int k = 0; k < n; k++)
        {
            var t = Twiddle((long)k * k % (2L * n), 2L * n);
            chirp[k] = new Complex(t.Real, -t.Imaginary);
        }
        var a = new Complex[m];
        var b = new Complex[m];
        for (int k = 0; k < n; k++) a[k] = x[k] * chirp[k];
        b[0] = Complex.Conjugate(chirp[0]);
        for (int k = 1; k < n; k++) b[k] = b[m - k] = Complex.Conjugate(chirp[k]);
        Radix2(a);
        Radix2(b);
        for (int k = 0; k < m; k++) a[k] *= b[k];
        // inverse by conjugation
        for (int k = 0; k < m; k++) a[k] = Complex.Conjugate(a[k]);
        Radix2(a);
        for (int k = 0; k < n; k++) x[k] = chirp[k] * Complex.Conjugate(a[k]) / m;
    }

    /// <summary>In-place unnormalised transform: the forward DFT <c>X[k] = Σ x[j]·exp(−2πijk/n)</c>, or with <paramref name="inverse"/> the sum with the opposite sign (done by
    /// conjugating before and after, so it is exactly the forward algorithm). Any length; lengths 0 and 1 are unchanged.</summary>
    public static void Transform(Span<Complex> x, bool inverse)
    {
        if (!inverse) { Forward(x); return; }
        for (int i = 0; i < x.Length; i++) x[i] = Complex.Conjugate(x[i]);
        Forward(x);
        for (int i = 0; i < x.Length; i++) x[i] = Complex.Conjugate(x[i]);
    }

    /// <summary>Makes <paramref name="x"/>, the transform of real data, exactly Hermitian: the DC term (and the Nyquist term of an even length) is exactly real, the upper half mirrors
    /// the lower, and negative zeros become positive zeros. NumPy computes only half of such a transform with a real transform, so this is what its output looks like.</summary>
    public static void MakeHermitian(Span<Complex> x)
    {
        int n = x.Length;
        if (n == 0) return;
        x[0] = new Complex(x[0].Real, 0.0);
        if (n % 2 == 0 && n > 1) x[n / 2] = new Complex(x[n / 2].Real, 0.0);
        for (int i = 1; i <= (n - 1) / 2; i++)
            x[n - i] = new Complex(x[i].Real, -x[i].Imaginary + 0.0);
        for (int i = 0; i < n; i++) x[i] = new Complex(x[i].Real, x[i].Imaginary + 0.0);   // −0.0 → +0.0
    }

    /// <summary>The factor a transform of length <paramref name="n"/> is multiplied by for <paramref name="norm"/>.</summary>
    public static double Scale(FourierNorm norm, int n, bool inverse) => norm switch
    {
        FourierNorm.Backward => inverse ? 1.0 / n : 1.0,
        FourierNorm.Ortho => 1.0 / Math.Sqrt(n),
        FourierNorm.Forward => inverse ? 1.0 : 1.0 / n,
        _ => throw new ArgumentOutOfRangeException(nameof(norm)),
    };

    /// <summary>The real signal of length <c>result.Length</c> whose half spectrum (<c>n/2 + 1</c> coefficients) is <paramref name="half"/>: the missing upper half is the
    /// conjugate mirror, the imaginary parts of the DC and Nyquist terms are dropped, the inverse transform is taken and its real part multiplied by <paramref name="scale"/>
    /// (NumPy <c>irfft</c>).</summary>
    public static void InverseReal(ReadOnlySpan<Complex> half, Span<double> result, double scale)
    {
        int len = result.Length;
        int hl = len / 2 + 1;
        if (len < 1) throw new ArgumentException("The result must hold at least one sample.", nameof(result));
        if (half.Length < hl) throw new ArgumentException("The half spectrum is shorter than n/2 + 1.", nameof(half));
        var line = new Complex[len];
        for (int k = 0; k < hl && k < len; k++) line[k] = half[k];
        line[0] = new Complex(line[0].Real, 0);
        if (len % 2 == 0) line[len / 2] = new Complex(line[len / 2].Real, 0);
        for (int k = hl; k < len; k++) line[k] = Complex.Conjugate(line[len - k]);
        Transform(line, true);
        for (int k = 0; k < len; k++) result[k] = line[k].Real * scale;
    }

    /// <summary>The sample frequencies of a transform of length <c>result.Length</c> with sample spacing <paramref name="spacing"/>: <c>0, 1, …, ⌈n/2⌉−1, −⌊n/2⌋, …, −1</c> divided by
    /// <c>n·spacing</c> (NumPy <c>fftfreq</c>).</summary>
    public static void Frequencies(Span<double> result, double spacing)
    {
        int n = result.Length;
        if (n <= 0) throw new ArgumentException("n should be greater than 0.", nameof(result));
        double val = 1.0 / (n * spacing);
        int pos = (n - 1) / 2 + 1;
        for (int i = 0; i < pos; i++) result[i] = i * val;
        for (int i = pos; i < n; i++) result[i] = (-(n / 2) + (i - pos)) * val;
    }

    /// <summary>The non-negative sample frequencies of a real transform: <c>result.Length</c> must be <c>n/2 + 1</c> for the transform length n; <c>0, 1, …</c> divided by
    /// <c>n·spacing</c> (NumPy <c>rfftfreq</c>).</summary>
    public static void RealFrequencies(Span<double> result, int n, double spacing)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));
        if (result.Length != n / 2 + 1) throw new ArgumentException("The result must hold n/2 + 1 frequencies.", nameof(result));
        double val = 1.0 / (n * spacing);
        for (int i = 0; i < result.Length; i++) result[i] = i * val;
    }
}
