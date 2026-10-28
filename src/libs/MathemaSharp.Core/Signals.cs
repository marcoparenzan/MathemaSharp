using System.Numerics;

namespace MathemaSharp.Core;

/// <summary>Algoritmi su segnali, generici sui formati di Generic Math di .NET 11 (float, double, Half, Decimal32/64/128).</summary>
public static partial class Signals
{
    /// <summary>
    /// DFT didattica O(N²), forward non normalizzata. Non una FFT.
    /// L'angolo è −2π·((k·n) mod N)/N: ridurre l'esponente con aritmetica intera prima di passare ai reali evita la perdita di
    /// accuratezza di k·n grandi, e rende esatti gli angoli multipli di 2π.
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

    /// <summary>⟨left|right⟩ = Σ conj(left_i)·right_i: coniugazione a sinistra, antilineare nel primo argomento.</summary>
    public static Complex<T> HermitianDot<T>(ReadOnlySpan<Complex<T>> left, ReadOnlySpan<Complex<T>> right) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        if (left.Length != right.Length) throw new ArgumentException("Lunghezze diverse.");
        var sum = Complex<T>.Zero;
        for (var i = 0; i < left.Length; i++) sum += Complex<T>.Conjugate(left[i]) * right[i];
        return sum;
    }

    /// <summary>
    /// Prodotto scalare con SIMD e coda scalare. <see cref="Vector{T}"/> supporta solo i tipi primitivi (float, double, Half...): per i
    /// formati senza supporto SIMD, come Decimal32/64/128, si ricade sul prodotto scalare generico.
    /// Riduzione riordinata: non necessariamente bit-identica allo scalare.
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
