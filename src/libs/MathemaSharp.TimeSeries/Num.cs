using System.Numerics;

namespace MathemaSharp.TimeSeries;

/// <summary>
/// Piccoli aiuti per scrivere algoritmi generici su <c>T : IFloatingPointIeee754&lt;T&gt;</c> (float, double, Half, Decimal32/64/128 di .NET 11).
/// Le costanti numeriche sono scritte come double e convertite: la precisione di una costante non supera quindi quella di un double (~1e-16).
/// </summary>
internal static class Num
{
    /// <summary>Converte una costante double in T.</summary>
    public static T C<T>(double value) where T : INumberBase<T> => T.CreateChecked(value);

    /// <summary>ε di macchina del tipo: distanza fra 1 e il numero successivo (ULP a 1).</summary>
    public static T Epsilon<T>() where T : IFloatingPointIeee754<T> => T.BitIncrement(T.One) - T.One;

    /// <summary>Intero → T (per divisori e contatori).</summary>
    public static T N<T>(int value) where T : INumberBase<T> => T.CreateChecked(value);

    /// <summary>T → int arrotondando al più vicino (per indici ricavati da ascisse reali).</summary>
    public static int RoundToInt<T>(T value) where T : IFloatingPointIeee754<T> => int.CreateChecked(T.Round(value));
}
