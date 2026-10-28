using System.Numerics;
namespace MathemaSharp.Core;

public static partial class Signals
{
    private static Complex<T>[] Transform<T>(ReadOnlySpan<Complex<T>> samples, bool inverse)
        where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = samples.Length;
        if (n == 0 || (n & (n - 1)) == 0) return TransformRadixTwo(samples, inverse);
        if (!inverse) return BluesteinForward(samples);
        var conjugated = new Complex<T>[n];
        for (int i = 0; i < n; i++) conjugated[i] = Complex<T>.Conjugate(samples[i]);
        var result = BluesteinForward<T>(conjugated);
        var denominator = T.CreateChecked(n);
        for (int i = 0; i < n; i++)
        {
            var value = Complex<T>.Conjugate(result[i]);
            result[i] = new Complex<T>(value.Real / denominator, value.Imaginary / denominator);
        }
        return result;
    }

    private static Complex<T>[] BluesteinForward<T>(ReadOnlySpan<Complex<T>> samples)
        where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        int n = samples.Length;
        long required = 2L * n - 1;
        if (required > (1L << 30)) throw new ArgumentOutOfRangeException(nameof(samples), "Convolution length exceeds supported array size.");
        int m = 1;
        while (m < required) m <<= 1;
        var denominator = T.CreateChecked(n);
        if (!T.IsFinite(denominator)) throw new ArithmeticException("Length is not representable in T.");
        var left = new Complex<T>[m]; var right = new Complex<T>[m];
        var chirp = new Complex<T>[n];
        for (int i = 0; i < n; i++)
        {
            if (!T.IsFinite(samples[i].Real) || !T.IsFinite(samples[i].Imaginary)) throw new ArgumentException("Samples must be finite.", nameof(samples));
            long phase = ((long)i * i) % (2L * n);
            // Divide before multiplying by pi to avoid a large floating-point angle.
            var ratio = T.CreateChecked(phase) / denominator;
            if (!T.IsFinite(ratio)) throw new ArithmeticException("Chirp phase is not representable in T.");
            chirp[i] = phase == 0 ? Complex<T>.One : Complex<T>.FromPolarCoordinates(T.One, -T.Pi * ratio);
            left[i] = samples[i] * chirp[i];
            right[i] = Complex<T>.Conjugate(chirp[i]);
            if (i > 0) right[m - i] = right[i];
        }
        var a = TransformRadixTwo<T>(left, false);
        var b = TransformRadixTwo<T>(right, false);
        for (int i = 0; i < m; i++) a[i] *= b[i];
        var convolution = TransformRadixTwo<T>(a, true);
        var output = new Complex<T>[n];
        for (int i = 0; i < n; i++)
        {
            output[i] = convolution[i] * chirp[i];
            if (!T.IsFinite(output[i].Real) || !T.IsFinite(output[i].Imaginary)) throw new ArithmeticException("Transform result is not representable in T.");
        }
        return output;
    }
}
