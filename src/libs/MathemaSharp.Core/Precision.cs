using System.Numerics;

namespace MathemaSharp.Core;

public static class Precision
{
    /// <summary>
    /// Comparison with absolute and relative tolerance, generic over <c>T : IFloatingPointIeee754&lt;T&gt;</c> (float, double, Half, Decimal32/64/128).
    /// The default tolerances are min(4096 ε, √ε), with ε = ULP at 1 (≈ 9e-13 for double).
    /// The √ε limit avoids tolerances larger than one in low-precision formats.
    /// For scale-sensitive applications use the overload with explicit tolerances.
    /// </summary>
    public static bool NearlyEqual<T>(T a, T b) where T : IFloatingPointIeee754<T>
    {
        var epsilon = T.BitIncrement(T.One) - T.One;
        var tolerance = T.Min(T.CreateChecked(4096) * epsilon, T.Sqrt(epsilon));
        return NearlyEqual(a, b, tolerance, tolerance);
    }

    public static bool NearlyEqual<T>(T a, T b, T absoluteTolerance, T relativeTolerance) where T : IFloatingPointIeee754<T>
    {
        if (!T.IsFinite(absoluteTolerance) || absoluteTolerance < T.Zero) throw new ArgumentOutOfRangeException(nameof(absoluteTolerance));
        if (!T.IsFinite(relativeTolerance) || relativeTolerance < T.Zero) throw new ArgumentOutOfRangeException(nameof(relativeTolerance));
        if (T.IsNaN(a) || T.IsNaN(b)) return false;
        if (a == b) return true;
        if (!T.IsFinite(a) || !T.IsFinite(b)) return false;
        var difference = T.Abs(a - b);
        if (difference <= absoluteTolerance) return true;
        // Normalizzazione evita overflow sia in a-b sia nel prodotto tolleranza*scala.
        var scale = T.Max(T.Abs(a), T.Abs(b));
        return T.Abs(a / scale - b / scale) <= relativeTolerance;
    }

    public static decimal InvoiceTotal(ReadOnlySpan<decimal> amounts, MidpointRounding rounding, bool roundEachLine)
    {
        var sum = 0m;
        foreach (var amount in amounts) sum += roundEachLine ? decimal.Round(amount, 2, rounding) : amount;
        return decimal.Round(sum, 2, rounding);
    }

    public static int DecimalScale(decimal value) => (decimal.GetBits(value)[3] >> 16) & 0xff;
}
