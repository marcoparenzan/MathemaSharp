// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Writes scalar Cbrt for each input into the destination prefix.
    /// Domain, special values and fast-profile accuracy match Cbrt(T). Destination
    /// must be at least input length; tails are unchanged and empty input is a no-op.
    /// Exact in-place is allowed; partial overlap is rejected before writes.
    /// No facade allocation; custom T may allocate or throw without rollback.</summary>
    public static void Cbrt<T>(ReadOnlySpan<T> input, Span<T> destination) where T : IRootFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Cbrt(input[i]);
    }

    /// <summary>Writes scalar Hypot(x[i], y[i]) into the destination prefix.
    /// Inputs must have equal length (no broadcasting). Domain, special values and
    /// fast-profile accuracy match Hypot(T, T). Destination must be at least input
    /// length; tails are unchanged and empty input is a no-op. Exact in-place on either
    /// or both inputs is allowed; partial overlap with either is rejected before writes.
    /// Input-input overlap alone is allowed. No facade allocation; custom T may
    /// allocate or throw without rollback.</summary>
    public static void Hypot<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) where T : IRootFunctions<T>
    {
        if (x.Length != y.Length)
            throw new ArgumentException("Inputs must have equal lengths.", nameof(y));
        ValidateDestination(x, destination);
        ValidateDestination(y, destination);
        for (int i = 0; i < x.Length; i++) destination[i] = Hypot(x[i], y[i]);
    }

    /// <summary>Writes scalar RootN(input[i], n) with a uniform degree n into the
    /// destination prefix. Domain, degree-zero behaviour, special values and fast-profile
    /// accuracy match RootN(T, int). Destination must be at least input length; tails
    /// are unchanged and empty input is a no-op (no scalar call). Exact in-place is
    /// allowed; partial overlap is rejected before writes. No facade allocation;
    /// custom T may allocate or throw without rollback of earlier writes.</summary>
    public static void RootN<T>(ReadOnlySpan<T> input, int n, Span<T> destination) where T : IRootFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = RootN(input[i], n);
    }
}
