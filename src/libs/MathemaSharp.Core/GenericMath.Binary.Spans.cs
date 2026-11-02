// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Writes scalar Pow(x[i], y[i]) into the destination prefix. Inputs
    /// must have equal length, without broadcasting. Domain/special values/accuracy
    /// match Pow(T, T). Empty inputs are a no-op; destination must be at least input
    /// length, and tails stay unchanged. Exact in-place on either or both inputs is
    /// allowed; partial overlap with either is rejected before writes. Input-input
    /// overlap alone is allowed. No facade allocation; custom T may allocate or throw
    /// without rollback of earlier writes.</summary>
    public static void Pow<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) where T : IPowerFunctions<T>
    {
        if (x.Length != y.Length)
            throw new ArgumentException("Inputs must have equal lengths.", nameof(y));
        ValidateDestination(x, destination);
        ValidateDestination(y, destination);
        for (int i = 0; i < x.Length; i++) destination[i] = Pow(x[i], y[i]);
    }

    /// <summary>Writes scalar Log(x[i], newBase[i]) into the destination prefix.
    /// Inputs must have equal length, without broadcasting. Domain/special values/accuracy
    /// match Log(T, T). Empty inputs are a no-op; destination must be at least input
    /// length, and tails stay unchanged. Exact in-place on either or both inputs is
    /// allowed; partial overlap with either is rejected before writes. Input-input
    /// overlap alone is allowed. No facade allocation; custom T may allocate or throw
    /// without rollback of earlier writes.</summary>
    public static void Log<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> newBase, Span<T> destination) where T : ILogarithmicFunctions<T>
    {
        if (x.Length != newBase.Length)
            throw new ArgumentException("Inputs must have equal lengths.", nameof(newBase));
        ValidateDestination(x, destination);
        ValidateDestination(newBase, destination);
        for (int i = 0; i < x.Length; i++) destination[i] = Log(x[i], newBase[i]);
    }
}
