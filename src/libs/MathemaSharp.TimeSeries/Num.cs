using System.Numerics;

namespace MathemaSharp.TimeSeries;

/// <summary>
/// Small helpers for writing generic algorithms on <c>T : IFloatingPointIeee754&lt;T&gt;</c> (float, double, Half, .NET 11 Decimal32/64/128).
/// Numeric constants are written as double and converted: the precision of a constant therefore does not exceed that of a double (~1e-16).
/// </summary>
internal static class Num
{
    /// <summary>Converts a double constant to T.</summary>
    public static T C<T>(double value) where T : INumberBase<T> => T.CreateChecked(value);

    /// <summary>Machine ε of the type: distance between 1 and the next number (ULP at 1).</summary>
    public static T Epsilon<T>() where T : IFloatingPointIeee754<T> => T.BitIncrement(T.One) - T.One;

    /// <summary>Integer → T (for divisors and counters).</summary>
    public static T N<T>(int value) where T : INumberBase<T> => T.CreateChecked(value);

    /// <summary>T → int rounding to the nearest (for indices derived from real abscissae).</summary>
    public static int RoundToInt<T>(T value) where T : IFloatingPointIeee754<T> => int.CreateChecked(T.Round(value));
}
