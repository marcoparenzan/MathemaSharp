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

    private static void ValidateDestination<T>(ReadOnlySpan<T> input, Span<T> destination)
    {
        if (destination.Length < input.Length)
            throw new ArgumentException("Destination is shorter than input.", nameof(destination));
        if (input.Overlaps(destination[..input.Length], out int offset) && offset != 0)
            throw new ArgumentException("Partial overlap is not supported.", nameof(destination));
    }
}
