// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericRoundSpanTests
{
    [Fact] public void HalfFormat() => CheckIeee<Half>();
    [Fact] public void SingleFormat() => CheckIeee<float>();
    [Fact] public void DoubleFormat() => CheckIeee<double>();
    [Fact] public void Decimal32Format() => CheckIeee<Decimal32>();
    [Fact] public void Decimal64Format() => CheckIeee<Decimal64>();
    [Fact] public void Decimal128Format() => CheckIeee<Decimal128>();
    [Fact] public void BclDecimalFormat() => Check<decimal>([0m, -0m, 2.5m, -2.5m, 1.25m, -1.25m]);

    private static void CheckIeee<T>() where T : unmanaged, IFloatingPointIeee754<T>
        => Check<T>([T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon, T.NaN,
            T.PositiveInfinity, T.NegativeInfinity, T.CreateChecked(2.5m), T.CreateChecked(-2.5m),
            T.CreateChecked(1.25m), T.CreateChecked(-1.25m)]);

    private static void Check<T>(T[] input) where T : unmanaged, IFloatingPoint<T>
    {
        foreach (var mode in Enum.GetValues<RoundingMode>())
        foreach (var digits in new int?[] { null, 1 })
        {
            var expected = input.Select(x => digits is int d ? GenericMath.Round(x, d, mode) : GenericMath.Round(x, mode)).ToArray();
            var before = Bytes(input);
            var output = Enumerable.Repeat(T.CreateChecked(7), input.Length + 1).ToArray();
            Apply(input, output, digits, mode);
            Assert.Equal(Bytes(expected), MemoryMarshal.AsBytes(output.AsSpan(0, input.Length)).ToArray());
            Assert.Equal(before, Bytes(input));
            Assert.Equal(T.CreateChecked(7), output[^1]);
            var inplace = (T[])input.Clone();
            Apply(inplace, inplace, digits, mode);
            Assert.Equal(Bytes(expected), Bytes(inplace));
            T[] shortOutput = [T.One];
            Assert.Throws<ArgumentException>(() => Apply(input, shortOutput, digits, mode));
            Assert.Equal(T.One, shortOutput[0]);
            T[] overlap = [T.One, T.CreateChecked(2), T.CreateChecked(3), T.CreateChecked(4)];
            var saved = Bytes(overlap);
            Assert.Throws<ArgumentException>(() => Apply(overlap.AsSpan(0, 3), overlap.AsSpan(1, 3), digits, mode));
            Assert.Throws<ArgumentException>(() => Apply(overlap.AsSpan(1, 3), overlap.AsSpan(0, 3), digits, mode));
            Assert.Equal(saved, Bytes(overlap));
            Apply(ReadOnlySpan<T>.Empty, shortOutput, digits, mode);
            Assert.Equal(T.One, shortOutput[0]);
        }
        T[] destination = [T.One];
        Assert.Throws<ArgumentOutOfRangeException>(() => GenericMath.Round<T>([], destination, (RoundingMode)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => GenericMath.Round<T>([], destination, 0, (RoundingMode)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => GenericMath.Round<T>([], destination, -1, RoundingMode.HalfToEven));
        Assert.Throws<ArgumentOutOfRangeException>(() => GenericMath.Round<T>([T.One], destination, (RoundingMode)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => GenericMath.Round<T>([T.One], destination, -1, RoundingMode.HalfToEven));
        // Direct BCL probes: the six IEEE formats return One for huge digits;
        // BCL decimal rejects the same request. Preserve that type-specific contract.
        if (typeof(T) == typeof(decimal))
            Assert.Throws<ArgumentOutOfRangeException>(() => GenericMath.Round<T>([T.One], destination, int.MaxValue, RoundingMode.HalfToEven));
        else
        {
            Assert.Equal(T.One, T.Round(T.One, int.MaxValue, MidpointRounding.ToEven));
            GenericMath.Round<T>([T.One], destination, int.MaxValue, RoundingMode.HalfToEven);
            Assert.Equal(T.One, destination[0]);
        }
        Assert.Equal(T.One, destination[0]);
    }

    private static byte[] Bytes<T>(T[] values) where T : unmanaged => MemoryMarshal.AsBytes(values.AsSpan()).ToArray();
    private static void Apply<T>(ReadOnlySpan<T> input, Span<T> output, int? digits, RoundingMode mode) where T : IFloatingPoint<T>
    {
        if (digits is int d) GenericMath.Round(input, output, d, mode);
        else GenericMath.Round(input, output, mode);
    }
}
