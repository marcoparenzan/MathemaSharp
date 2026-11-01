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

    /// <summary>Writes scalar Atan2(y[i], x[i]) in radians into the destination prefix.
    /// Inputs must have equal lengths; no broadcasting is performed. Domain, signed-zero
    /// branch cuts, special values and BCL fast-profile accuracy match Atan2(T, T).
    /// Destination must be at least input length; extra elements remain unchanged.
    /// Empty inputs are a no-op. Exact in-place on either or both inputs is allowed;
    /// partial overlap with either input is rejected before writing. Input-input overlap
    /// alone is allowed. No facade allocation is introduced; custom T may allocate or
    /// throw, without rollback of earlier writes.</summary>
    public static void Atan2<T>(ReadOnlySpan<T> y, ReadOnlySpan<T> x, Span<T> destination) where T : IFloatingPointIeee754<T>
    {
        if (y.Length != x.Length)
            throw new ArgumentException("Inputs must have equal lengths.", nameof(x));
        ValidateDestination(y, destination);
        ValidateDestination(x, destination);
        for (int i = 0; i < y.Length; i++) destination[i] = Atan2(y[i], x[i]);
    }

    private static void ValidateDestination<T>(ReadOnlySpan<T> input, Span<T> destination)
    {
        if (destination.Length < input.Length)
            throw new ArgumentException("Destination is shorter than input.", nameof(destination));
        if (input.Overlaps(destination[..input.Length], out int offset) && offset != 0)
            throw new ArgumentException("Partial overlap is not supported.", nameof(destination));
    }
}
