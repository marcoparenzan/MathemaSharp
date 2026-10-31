// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Writes scalar Sqrt for each input into the destination prefix.
    /// Domain, special values and accuracy match Sqrt(T). Empty input is a no-op;
    /// output tail is unchanged. Short destinations and shifted overlaps throw before writes;
    /// exact in-place is supported. No facade allocation; custom T may allocate or throw.</summary>
    public static void Sqrt<T>(ReadOnlySpan<T> input, Span<T> destination) where T : IRootFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Sqrt(input[i]);
    }

    /// <summary>Writes scalar Exp for each input into the destination prefix.
    /// Domain, special values and accuracy match Exp(T). Empty input is a no-op;
    /// output tail is unchanged. Short destinations and shifted overlaps throw before writes;
    /// exact in-place is supported. No facade allocation; custom T may allocate or throw.</summary>
    public static void Exp<T>(ReadOnlySpan<T> input, Span<T> destination) where T : IExponentialFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Exp(input[i]);
    }

    /// <summary>Writes scalar Log for each input into the destination prefix.
    /// Domain, special values and accuracy match Log(T). Empty input is a no-op;
    /// output tail is unchanged. Short destinations and shifted overlaps throw before writes;
    /// exact in-place is supported. No facade allocation; custom T may allocate or throw.</summary>
    public static void Log<T>(ReadOnlySpan<T> input, Span<T> destination) where T : ILogarithmicFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Log(input[i]);
    }
}
