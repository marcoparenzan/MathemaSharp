// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Writes scalar Exp2M1 into the destination prefix, including signed zeros.
    /// Domain/special values/accuracy match the scalar. Empty input is a no-op; tails
    /// stay unchanged. Destination must be at least input length; exact in-place is
    /// allowed, partial overlap rejected before writes. No facade allocation; custom
    /// T may allocate or throw without rollback of earlier writes.</summary>
    public static void Exp2M1<T>(ReadOnlySpan<T> input, Span<T> destination) where T : IExponentialFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Exp2M1(input[i]);
    }

    /// <summary>Writes scalar Exp10M1 into the destination prefix, including signed zeros.
    /// Domain/special values/accuracy match the scalar. Empty input is a no-op; tails
    /// stay unchanged. Destination must be at least input length; exact in-place is
    /// allowed, partial overlap rejected before writes. No facade allocation; custom
    /// T may allocate or throw without rollback of earlier writes.</summary>
    public static void Exp10M1<T>(ReadOnlySpan<T> input, Span<T> destination) where T : IExponentialFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Exp10M1(input[i]);
    }

    /// <summary>Writes scalar Log2P1 into the destination prefix, including signed zeros.
    /// Domain/special values/accuracy match the scalar. Empty input is a no-op; tails
    /// stay unchanged. Destination must be at least input length; exact in-place is
    /// allowed, partial overlap rejected before writes. No facade allocation; custom
    /// T may allocate or throw without rollback of earlier writes.</summary>
    public static void Log2P1<T>(ReadOnlySpan<T> input, Span<T> destination) where T : ILogarithmicFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Log2P1(input[i]);
    }

    /// <summary>Writes scalar Log10P1 into the destination prefix, including signed zeros.
    /// Domain/special values/accuracy match the scalar. Empty input is a no-op; tails
    /// stay unchanged. Destination must be at least input length; exact in-place is
    /// allowed, partial overlap rejected before writes. No facade allocation; custom
    /// T may allocate or throw without rollback of earlier writes.</summary>
    public static void Log10P1<T>(ReadOnlySpan<T> input, Span<T> destination) where T : ILogarithmicFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Log10P1(input[i]);
    }
}
