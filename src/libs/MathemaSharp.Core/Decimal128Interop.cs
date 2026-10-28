using System.Globalization;
using System.Numerics;

namespace MathemaSharp.Core.Interop;

/// <summary>Explicit Decimal128 facade for reflection-based interpreters. Values remain CLR objects, not Python float.</summary>
/// <remarks>Parse strings rather than Python floating literals to avoid prior binary64 rounding. No generic binder support is implied.</remarks>
public static class Decimal128Interop
{
    public static Decimal128 Parse(string value) => Decimal128.Parse(value, CultureInfo.InvariantCulture);
    public static string Format(Decimal128 value) => value.ToString("G", CultureInfo.InvariantCulture);
    public static bool Equal(Decimal128 left, Decimal128 right) => left == right;
    public static Decimal128 Add(Decimal128 left, Decimal128 right) => left + right;
    public static Decimal128 Subtract(Decimal128 left, Decimal128 right) => left - right;
    public static Decimal128 Sum(Decimal128[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return NumericAlgorithms.Sum<Decimal128>(values);
    }
    public static Decimal128[] Copy(Decimal128[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return (Decimal128[])values.Clone();
    }
    public static Complex<Decimal128> Complex(Decimal128 real, Decimal128 imaginary) => new(real, imaginary);
    public static Decimal128 Real(Complex<Decimal128> value) => value.Real;
    public static Decimal128 Imaginary(Complex<Decimal128> value) => value.Imaginary;
    public static Complex<Decimal128> HermitianDot(Complex<Decimal128>[] left, Complex<Decimal128>[] right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return Signals.HermitianDot<Decimal128>(left, right);
    }
}
