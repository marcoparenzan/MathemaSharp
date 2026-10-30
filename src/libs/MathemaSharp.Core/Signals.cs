using System.Numerics;

namespace MathemaSharp.Core;

/// <summary>Algorithms on signals, generic over the .NET 11 Generic Math formats (float, double, Half, Decimal32/64/128).</summary>
public static partial class Signals
{
    /// <summary>
    /// Educational O(N²) DFT, forward and unnormalised. Not an FFT.
    /// The angle is −2π·((k·n) mod N)/N: reducing the exponent with integer arithmetic before moving to reals avoids the loss of
    /// accuracy for large k·n, and makes the angles that are multiples of 2π exact.
    /// </summary>
    public static Complex<T>[] Dft<T>(ReadOnlySpan<Complex<T>> samples) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        var length = samples.Length;
        var output = new Complex<T>[length];
        var denominator = T.CreateChecked(length);
        for (var k = 0; k < length; k++)
            for (var n = 0; n < length; n++)
            {
                var angle = -T.Tau * T.CreateChecked(((long)k * n) % length) / denominator;
                output[k] += samples[n] * Complex<T>.FromPolarCoordinates(T.One, angle);
            }
        return output;
    }

    /// <summary>⟨left|right⟩ = Σ conj(left_i)·right_i: conjugation on the left, antilinear in the first argument.</summary>
    public static Complex<T> HermitianDot<T>(ReadOnlySpan<Complex<T>> left, ReadOnlySpan<Complex<T>> right) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (left.Length != right.Length) throw new ArgumentException("Lunghezze diverse.");
        var sum = Complex<T>.Zero;
        for (var i = 0; i < left.Length; i++) sum += Complex<T>.Conjugate(left[i]) * right[i];
        return sum;
    }

    /// <summary>
    /// Dot product with SIMD and a scalar tail. <see cref="Vector{T}"/> supports only primitive types (float, double, Half...): for
    /// formats without SIMD support, such as Decimal32/64/128, it falls back to the generic scalar product.
    /// Reordered reduction: not necessarily bit-identical to the scalar one.
    /// </summary>
    public static T VectorDot<T>(ReadOnlySpan<T> left, ReadOnlySpan<T> right) where T : IFloatingPointIeee754<T>
    {
        if (left.Length != right.Length) throw new ArgumentException("Lunghezze diverse.");
        if (!Vector.IsHardwareAccelerated || !Vector<T>.IsSupported) return NumericAlgorithms.Dot(left, right);
        var accumulator = Vector<T>.Zero;
        var i = 0;
        for (; i <= left.Length - Vector<T>.Count; i += Vector<T>.Count)
            accumulator += new Vector<T>(left.Slice(i)) * new Vector<T>(right.Slice(i));
        var sum = Vector.Sum(accumulator);
        for (; i < left.Length; i++) sum += left[i] * right[i];
        return sum;
    }
}
