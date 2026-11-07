// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericPiTrigSpanTests
{
    [Fact] public void HalfFormat() => Check<Half>();
    [Fact] public void SingleFormat() => Check<float>();
    [Fact] public void DoubleFormat() => Check<double>();
    [Fact] public void Decimal32Format() => Check<Decimal32>();
    [Fact] public void Decimal64Format() => Check<Decimal64>();
    [Fact] public void Decimal128Format() => Check<Decimal128>();

    [Fact]
    public void WarmDoubleCallsAllocateNoManagedBytes()
    {
        double[] input = [0, 0.25, -1], output = new double[3];
        for (int i = 0; i < 100; i++) Warm(input, output);
        Assert.Equal(0, AllocationProbe.MinimumBytes(() =>
        {
            for (int i = 0; i < 100; i++) Warm(input, output);
        }));
    }

    private static void Warm(double[] input, double[] output)
    {
        GenericMath.SinPi<double>(input, output); GenericMath.CosPi<double>(input, output);
        GenericMath.TanPi<double>(input, output); GenericMath.AsinPi<double>(input, output);
        GenericMath.AcosPi<double>(input, output); GenericMath.AtanPi<double>(input, output);
    }

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        var half = T.CreateChecked(0.5m);
        T[] input = [T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon, half, -half,
            T.BitIncrement(half), T.BitDecrement(half), T.One, -T.One,
            T.BitIncrement(T.One), T.BitDecrement(T.One), T.BitIncrement(-T.One),
            T.BitDecrement(-T.One), T.CreateChecked(1000), T.PositiveInfinity,
            T.NegativeInfinity, T.NaN];
        foreach (var function in new[] { "SinPi", "CosPi", "TanPi", "AsinPi", "AcosPi", "AtanPi" })
        {
            var savedInput = Bytes(input);
            var expected = input.Select(value => Scalar(function, value)).ToArray();
            var output = Enumerable.Repeat(T.CreateChecked(7), input.Length + 1).ToArray();
            Apply(function, input, output);
            Assert.Equal(Bytes(expected), MemoryMarshal.AsBytes(output.AsSpan(0, input.Length)).ToArray());
            Assert.Equal(savedInput, Bytes(input)); Assert.Equal(T.CreateChecked(7), output[^1]);
            var inplace = (T[])input.Clone(); Apply(function, inplace, inplace);
            Assert.Equal(Bytes(expected), Bytes(inplace));
            T[] sentinel = [T.One];
            Assert.Throws<ArgumentException>(() => Apply(function, input, sentinel)); Assert.Equal(T.One, sentinel[0]);
            T[] overlap = [T.One, T.One, T.One, T.One]; var before = Bytes(overlap);
            Assert.Throws<ArgumentException>(() => Apply(function, overlap.AsSpan(0, 3), overlap.AsSpan(1, 3))); Assert.Equal(before, Bytes(overlap));
            Assert.Throws<ArgumentException>(() => Apply(function, overlap.AsSpan(1, 3), overlap.AsSpan(0, 3))); Assert.Equal(before, Bytes(overlap));
            Apply(function, ReadOnlySpan<T>.Empty, sentinel); Assert.Equal(T.One, sentinel[0]);
            Apply(function, ReadOnlySpan<T>.Empty, Span<T>.Empty);
            // Overlap confined to untouched output tail is safe.
            T[] shared = [T.One, T.One, half, -half];
            T[] sharedExpected = [Scalar(function, half), Scalar(function, -half)];
            Apply(function, shared.AsSpan(2, 2), shared);
            Assert.Equal(Bytes(sharedExpected), MemoryMarshal.AsBytes(shared.AsSpan(0, 2)).ToArray());
            Assert.Equal(half, shared[2]); Assert.Equal(-half, shared[3]);
        }
    }

    private static byte[] Bytes<T>(T[] values) where T : unmanaged => MemoryMarshal.AsBytes(values.AsSpan()).ToArray();
    private static T Scalar<T>(string function, T value) where T : ITrigonometricFunctions<T>
        => function switch
        {
            "SinPi" => GenericMath.SinPi(value), "CosPi" => GenericMath.CosPi(value),
            "TanPi" => GenericMath.TanPi(value), "AsinPi" => GenericMath.AsinPi(value),
            "AcosPi" => GenericMath.AcosPi(value), _ => GenericMath.AtanPi(value)
        };
    private static void Apply<T>(string function, ReadOnlySpan<T> input, Span<T> destination) where T : ITrigonometricFunctions<T>
    {
        switch (function)
        {
            case "SinPi": GenericMath.SinPi(input, destination); break;
            case "CosPi": GenericMath.CosPi(input, destination); break;
            case "TanPi": GenericMath.TanPi(input, destination); break;
            case "AsinPi": GenericMath.AsinPi(input, destination); break;
            case "AcosPi": GenericMath.AcosPi(input, destination); break;
            default: GenericMath.AtanPi(input, destination); break;
        }
    }
}
