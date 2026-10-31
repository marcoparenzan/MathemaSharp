// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Writes scalar Floor for each input into the destination prefix.
    /// Domain/special values/accuracy match Floor(T). Empty input is a no-op; output tail unchanged.
    /// Short output and shifted overlap throw before writes; exact in-place supported.
    /// No facade allocation; custom T may allocate/throw during arithmetic.</summary>
    public static void Floor<T>(ReadOnlySpan<T> input, Span<T> destination) where T : IFloatingPoint<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Floor(input[i]);
    }

    /// <summary>Writes scalar Ceiling for each input into the destination prefix.
    /// Domain/special values/accuracy match Ceiling(T). Empty input is a no-op; output tail unchanged.
    /// Short output and shifted overlap throw before writes; exact in-place supported.
    /// No facade allocation; custom T may allocate/throw during arithmetic.</summary>
    public static void Ceiling<T>(ReadOnlySpan<T> input, Span<T> destination) where T : IFloatingPoint<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Ceiling(input[i]);
    }

    /// <summary>Writes scalar Truncate for each input into the destination prefix.
    /// Domain/special values/accuracy match Truncate(T). Empty input is a no-op; output tail unchanged.
    /// Short output and shifted overlap throw before writes; exact in-place supported.
    /// No facade allocation; custom T may allocate/throw during arithmetic.</summary>
    public static void Truncate<T>(ReadOnlySpan<T> input, Span<T> destination) where T : IFloatingPoint<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Truncate(input[i]);
    }
}
