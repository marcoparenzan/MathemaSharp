using System.Numerics;

namespace MathemaSharp.Core;

/// <summary>Constraint minimi: Complex non è un numero ordinato.</summary>
public static class NumericAlgorithms
{
    public static T Sum<T>(ReadOnlySpan<T> values) where T : INumberBase<T>
    {
        var sum = T.Zero;
        foreach (var value in values) sum += value;
        return sum;
    }

    public static T SumChecked<T>(ReadOnlySpan<T> values) where T : INumberBase<T>
    {
        var sum = T.Zero;
        foreach (var value in values) sum = checked(sum + value);
        return sum;
    }

    public static T Dot<T>(ReadOnlySpan<T> left, ReadOnlySpan<T> right) where T : INumberBase<T>
    {
        if (left.Length != right.Length) throw new ArgumentException("Lunghezze diverse.");
        var sum = T.Zero;
        for (var i = 0; i < left.Length; i++) sum += left[i] * right[i];
        return sum; // Per Complex è bilineare, NON prodotto hermitiano.
    }

    /// <summary>Coefficienti in ordine crescente: c0 + c1*x + c2*x².</summary>
    public static T Horner<T>(ReadOnlySpan<T> coefficients, T x) where T : INumberBase<T>
    {
        var result = T.Zero;
        for (var i = coefficients.Length - 1; i >= 0; i--) result = result * x + coefficients[i];
        return result;
    }

    public static T Mean<T>(ReadOnlySpan<T> values) where T : IFloatingPoint<T>
    {
        if (values.IsEmpty) throw new ArgumentException("La media richiede almeno un valore.");
        return Sum<T>(values) / T.CreateChecked(values.Length);
    }

    /// <summary>Kahan per valori finiti; non elimina tutti gli errori di cancellazione.</summary>
    public static T KahanSum<T>(ReadOnlySpan<T> values) where T : IFloatingPointIeee754<T>
    {
        var sum = T.Zero;
        var correction = T.Zero;
        foreach (var value in values)
        {
            var adjusted = value - correction;
            var next = sum + adjusted;
            correction = (next - sum) - adjusted;
            sum = next;
        }
        return sum;
    }

    public static T Gcd<T>(T a, T b) where T : IBinaryInteger<T>
    {
        a = T.Abs(a); b = T.Abs(b); // MinValue signed può generare overflow.
        while (b != T.Zero) (a, b) = (b, a % b);
        return a;
    }

    public static BigInteger Factorial(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);
        var result = BigInteger.One;
        for (var i = 2; i <= n; i++) result *= i;
        return result;
    }
}
