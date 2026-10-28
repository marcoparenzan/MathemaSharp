using System.Numerics;
namespace MathemaSharp.Core;

public static partial class Signals
{
    /// <summary>FFT with negative sign and no forward normalization; radix-two for powers of two, Bluestein otherwise.
    /// Empty input is accepted. Input is not modified. Bluestein allocates a power-of-two convolution of length at least 2N-1.</summary>
    public static Complex<T>[] Fft<T>(ReadOnlySpan<Complex<T>> samples)
        where T : IFloatingPointIeee754<T>, IMinMaxValue<T> => Transform(samples, false);

    /// <summary>Inverse FFT for arbitrary lengths, positive sign and division by N. Ordinary arithmetic in T; no overflow guarantee.</summary>
    public static Complex<T>[] Ifft<T>(ReadOnlySpan<Complex<T>> samples)
        where T : IFloatingPointIeee754<T>, IMinMaxValue<T> => Transform(samples, true);

    private static Complex<T>[] TransformRadixTwo<T>(ReadOnlySpan<Complex<T>> samples, bool inverse)
        where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = samples.Length;
        if (n == 0) return [];
        if ((n & (n - 1)) != 0) throw new ArgumentException("Radix-two FFT requires a power-of-two length.", nameof(samples));
        var denominator = T.CreateChecked(n);
        if (!T.IsFinite(denominator)) throw new ArithmeticException("Length is not representable in T.");
        var data = samples.ToArray();
        foreach (var value in data)
            if (!T.IsFinite(value.Real) || !T.IsFinite(value.Imaginary)) throw new ArgumentException("Samples must be finite.", nameof(samples));
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1) j ^= bit;
            j ^= bit;
            if (i < j) (data[i], data[j]) = (data[j], data[i]);
        }
        for (int width = 2; width <= n;)
        {
            int half = width / 2;
            var divisor = T.CreateChecked(width);
            for (int k = 0; k < half; k++)
            {
                var angle = (inverse ? T.Tau : -T.Tau) * (T.CreateChecked(k) / divisor);
                var twiddle = k == 0 ? Complex<T>.One : Complex<T>.FromPolarCoordinates(T.One, angle);
                for (int start = 0; start < n; start += width)
                {
                    var left = data[start + k];
                    var right = data[start + k + half] * twiddle;
                    data[start + k] = left + right;
                    data[start + k + half] = left - right;
                }
            }
            if (width == n) break;
            width *= 2;
        }
        if (inverse)
            for (int i = 0; i < n; i++) data[i] = new Complex<T>(data[i].Real / denominator, data[i].Imaginary / denominator);
        foreach (var value in data)
            if (!T.IsFinite(value.Real) || !T.IsFinite(value.Imaginary)) throw new ArithmeticException("Transform result is not representable in T.");
        return data;
    }
}
