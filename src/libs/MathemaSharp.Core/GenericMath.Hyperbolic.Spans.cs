// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp;

public static partial class GenericMath
{
    /// <summary>Writes scalar Sinh for each input into the destination prefix.
    /// Domain, special values and fast-profile accuracy are identical to Sinh(T).
    /// Empty input is a no-op; extra destination elements remain unchanged.
    /// Destination must be at least input length. Exact in-place operation is allowed;
    /// partial overlap is rejected before writing. The facade introduces no allocation;
    /// custom T may allocate or throw, and element failures do not roll back prior writes.</summary>
    public static void Sinh<T>(ReadOnlySpan<T> input, Span<T> destination) where T : IHyperbolicFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Sinh(input[i]);
    }

    /// <summary>Writes scalar Cosh for each input into the destination prefix.
    /// Domain, special values and fast-profile accuracy are identical to Cosh(T).
    /// Empty input is a no-op; extra destination elements remain unchanged.
    /// Destination must be at least input length. Exact in-place operation is allowed;
    /// partial overlap is rejected before writing. The facade introduces no allocation;
    /// custom T may allocate or throw, and element failures do not roll back prior writes.</summary>
    public static void Cosh<T>(ReadOnlySpan<T> input, Span<T> destination) where T : IHyperbolicFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Cosh(input[i]);
    }

    /// <summary>Writes scalar Tanh for each input into the destination prefix.
    /// Domain, special values and fast-profile accuracy are identical to Tanh(T).
    /// Empty input is a no-op; extra destination elements remain unchanged.
    /// Destination must be at least input length. Exact in-place operation is allowed;
    /// partial overlap is rejected before writing. The facade introduces no allocation;
    /// custom T may allocate or throw, and element failures do not roll back prior writes.</summary>
    public static void Tanh<T>(ReadOnlySpan<T> input, Span<T> destination) where T : IHyperbolicFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Tanh(input[i]);
    }

    /// <summary>Writes scalar Asinh for each input into the destination prefix.
    /// Domain, special values and fast-profile accuracy are identical to Asinh(T).
    /// Empty input is a no-op; extra destination elements remain unchanged.
    /// Destination must be at least input length. Exact in-place operation is allowed;
    /// partial overlap is rejected before writing. The facade introduces no allocation;
    /// custom T may allocate or throw, and element failures do not roll back prior writes.</summary>
    public static void Asinh<T>(ReadOnlySpan<T> input, Span<T> destination) where T : IHyperbolicFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Asinh(input[i]);
    }

    /// <summary>Writes scalar Acosh for each input into the destination prefix.
    /// Real domain: x ≥ 1; IEEE inputs below one produce NaN. Other special values and
    /// fast-profile accuracy are identical to Acosh(T). Empty input is a no-op;
    /// extra destination elements remain unchanged. Destination must be at least input length.
    /// Exact in-place is allowed; partial overlap is rejected before writing.
    /// The facade introduces no allocation; custom T may allocate or throw,
    /// and element failures do not roll back prior writes.</summary>
    public static void Acosh<T>(ReadOnlySpan<T> input, Span<T> destination) where T : IHyperbolicFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Acosh(input[i]);
    }

    /// <summary>Writes scalar Atanh for each input into the destination prefix.
    /// Real finite domain: -1 &lt; x &lt; 1; IEEE endpoints produce signed infinities and
    /// inputs outside [-1,1] produce NaN. Other special values and fast-profile accuracy
    /// are identical to Atanh(T). Empty input is a no-op; extra destination elements stay unchanged.
    /// Destination must be at least input length. Exact in-place is allowed;
    /// partial overlap is rejected before writing. The facade introduces no allocation;
    /// custom T may allocate or throw, and element failures do not roll back prior writes.</summary>
    public static void Atanh<T>(ReadOnlySpan<T> input, Span<T> destination) where T : IHyperbolicFunctions<T>
    {
        ValidateDestination(input, destination);
        for (int i = 0; i < input.Length; i++) destination[i] = Atanh(input[i]);
    }
}
