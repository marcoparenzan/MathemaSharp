// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Writes scalar SinPi into the destination prefix. Domain, special values
    /// and BCL fast accuracy match SinPi(T). Empty input is a no-op; tails are unchanged.
    /// Destination must be at least input length; exact in-place is allowed and partial
    /// overlap is rejected before writes. No facade allocation; custom T may throw or
    /// allocate without rollback of earlier writes.</summary>
    public static void SinPi<T>(ReadOnlySpan<T> input, Span<T> destination) where T : ITrigonometricFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = SinPi(input[i]);
    }

    /// <summary>Writes scalar CosPi into the destination prefix. Domain, special values
    /// and BCL fast accuracy match CosPi(T). Empty input is a no-op; tails are unchanged.
    /// Destination must be at least input length; exact in-place is allowed and partial
    /// overlap is rejected before writes. No facade allocation; custom T may throw or
    /// allocate without rollback of earlier writes.</summary>
    public static void CosPi<T>(ReadOnlySpan<T> input, Span<T> destination) where T : ITrigonometricFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = CosPi(input[i]);
    }

    /// <summary>Writes scalar TanPi into the destination prefix. Domain, special values
    /// and BCL fast accuracy match TanPi(T), including pole behaviour. Empty input is
    /// a no-op; tails are unchanged. Destination must be at least input length; exact
    /// in-place is allowed and partial overlap is rejected before writes. No facade
    /// allocation; custom T may throw or allocate without rollback of earlier writes.</summary>
    public static void TanPi<T>(ReadOnlySpan<T> input, Span<T> destination) where T : ITrigonometricFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = TanPi(input[i]);
    }

    /// <summary>Writes scalar AsinPi into the destination prefix. Domain, special values
    /// and BCL fast accuracy match AsinPi(T). Empty input is a no-op; tails are unchanged.
    /// Destination must be at least input length; exact in-place is allowed and partial
    /// overlap is rejected before writes. No facade allocation; custom T may throw or
    /// allocate without rollback of earlier writes.</summary>
    public static void AsinPi<T>(ReadOnlySpan<T> input, Span<T> destination) where T : ITrigonometricFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = AsinPi(input[i]);
    }

    /// <summary>Writes scalar AcosPi into the destination prefix. Domain, special values
    /// and BCL fast accuracy match AcosPi(T). Empty input is a no-op; tails are unchanged.
    /// Destination must be at least input length; exact in-place is allowed and partial
    /// overlap is rejected before writes. No facade allocation; custom T may throw or
    /// allocate without rollback of earlier writes.</summary>
    public static void AcosPi<T>(ReadOnlySpan<T> input, Span<T> destination) where T : ITrigonometricFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = AcosPi(input[i]);
    }

    /// <summary>Writes scalar AtanPi into the destination prefix. Domain, special values
    /// and BCL fast accuracy match AtanPi(T). Empty input is a no-op; tails are unchanged.
    /// Destination must be at least input length; exact in-place is allowed and partial
    /// overlap is rejected before writes. No facade allocation; custom T may throw or
    /// allocate without rollback of earlier writes.</summary>
    public static void AtanPi<T>(ReadOnlySpan<T> input, Span<T> destination) where T : ITrigonometricFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = AtanPi(input[i]);
    }
}
