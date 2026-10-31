// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Writes scalar integral Round results into the destination prefix using explicit mode.
    /// Special values/accuracy follow T.Round. Short destinations, shifted overlap and invalid modes
    /// throw before writes; exact in-place is allowed. Empty input validates mode, then writes nothing.
    /// Output tail unchanged. No facade allocation; custom T may allocate/throw during arithmetic.</summary>
    public static void Round<T>(ReadOnlySpan<T> input, Span<T> destination, RoundingMode mode) where T : IFloatingPoint<T>
    {
        var bclMode = ToBclMode(mode);
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = T.Round(input[i], bclMode);
    }

    /// <summary>Writes scalar decimal-digit Round results into the destination prefix using explicit mode.
    /// Digits follow T's supported BCL range; negative-digit rounding is not added. Mode, destination
    /// and digits are validated before writes, including empty input; digit validation calls T.Round
    /// on T.Zero once. Exact in-place allowed; shifted overlap rejected. Output tail unchanged.
    /// No facade allocation; custom T arithmetic can allocate/throw and leave a written prefix.</summary>
    public static void Round<T>(ReadOnlySpan<T> input, Span<T> destination, int digits, RoundingMode mode) where T : IFloatingPoint<T>
    {
        var bclMode = ToBclMode(mode);
        ValidateDestination(input, destination);
        _ = T.Round(T.Zero, digits, bclMode);
        for (int i = 0; i < input.Length; i++) destination[i] = T.Round(input[i], digits, bclMode);
    }
}
