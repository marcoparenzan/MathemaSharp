// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Writes scalar Sin for each input (radians) into the destination prefix.
    /// Domain, special values and accuracy are identical to Sin(T). Empty input is a no-op;
    /// extra destination elements remain unchanged. Destination must be at least input length.
    /// Exact in-place operation is allowed; partial overlap is rejected before writing.
    /// No allocation is introduced by the facade; custom T may allocate or throw.</summary>
    public static void Sin<T>(ReadOnlySpan<T> input, Span<T> destination) where T : ITrigonometricFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Sin(input[i]);
    }

    /// <summary>Writes scalar Cos for each input (radians) into the destination prefix.
    /// Domain, special values and accuracy are identical to Cos(T). Empty input is a no-op;
    /// extra destination elements remain unchanged. Destination must be at least input length.
    /// Exact in-place operation is allowed; partial overlap is rejected before writing.
    /// No allocation is introduced by the facade; custom T may allocate or throw.</summary>
    public static void Cos<T>(ReadOnlySpan<T> input, Span<T> destination) where T : ITrigonometricFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Cos(input[i]);
    }

    /// <summary>Writes scalar Tan for each input (radians) into the destination prefix.
    /// Domain, special values and accuracy are identical to Tan(T). Empty input is a no-op;
    /// extra destination elements remain unchanged. Destination must be at least input length.
    /// Exact in-place operation is allowed; partial overlap is rejected before writing.
    /// No allocation is introduced by the facade; custom T may allocate or throw.</summary>
    public static void Tan<T>(ReadOnlySpan<T> input, Span<T> destination) where T : ITrigonometricFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Tan(input[i]);
    }

    /// <summary>Writes scalar Asin for each input into the destination prefix (radians).
    /// Domain [-1, 1], special values and BCL fast-profile accuracy match Asin(T).
    /// Empty input is a no-op; extra destination elements remain unchanged.
    /// Destination must be at least input length. Exact in-place is allowed;
    /// partial overlap is rejected before writing. No facade allocation is introduced;
    /// custom T may allocate or throw, without rollback of earlier writes.</summary>
    public static void Asin<T>(ReadOnlySpan<T> input, Span<T> destination) where T : ITrigonometricFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Asin(input[i]);
    }

    /// <summary>Writes scalar Acos for each input into the destination prefix (radians).
    /// Domain [-1, 1], special values and BCL fast-profile accuracy match Acos(T).
    /// Empty input is a no-op; extra destination elements remain unchanged.
    /// Destination must be at least input length. Exact in-place is allowed;
    /// partial overlap is rejected before writing. No facade allocation is introduced;
    /// custom T may allocate or throw, without rollback of earlier writes.</summary>
    public static void Acos<T>(ReadOnlySpan<T> input, Span<T> destination) where T : ITrigonometricFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Acos(input[i]);
    }

    /// <summary>Writes scalar Atan for each input into the destination prefix (radians).
    /// All real values, special values and BCL fast-profile accuracy match Atan(T).
    /// Empty input is a no-op; extra destination elements remain unchanged.
    /// Destination must be at least input length. Exact in-place is allowed;
    /// partial overlap is rejected before writing. No facade allocation is introduced;
    /// custom T may allocate or throw, without rollback of earlier writes.</summary>
    public static void Atan<T>(ReadOnlySpan<T> input, Span<T> destination) where T : ITrigonometricFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Atan(input[i]);
    }

    private static void ValidateDestination<T>(ReadOnlySpan<T> input, Span<T> destination)
    {
        if (destination.Length < input.Length)
            throw new ArgumentException("Destination is shorter than input.", nameof(destination));
        if (input.Overlaps(destination[..input.Length], out int offset) && offset != 0)
            throw new ArgumentException("Partial overlap is not supported.", nameof(destination));
    }
}
