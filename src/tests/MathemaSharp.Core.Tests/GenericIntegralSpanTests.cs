// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericIntegralSpanTests
{
    [Fact] public void HalfFormat() => CheckIeee<Half>();
    [Fact] public void SingleFormat() => CheckIeee<float>();
    [Fact] public void DoubleFormat() => CheckIeee<double>();
    [Fact] public void Decimal32Format() => CheckIeee<Decimal32>();
    [Fact] public void Decimal64Format() => CheckIeee<Decimal64>();
    [Fact] public void Decimal128Format() => CheckIeee<Decimal128>();
    [Fact] public void BclDecimalFormat() => Check<decimal>([0m, -0m, 2.5m, -2.5m, 0.25m, -0.25m]);

    private static void CheckIeee<T>() where T : unmanaged, IFloatingPointIeee754<T>
        => Check<T>([T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon, T.One, -T.One,
            T.CreateChecked(2.5m), T.CreateChecked(-2.5m), T.NaN, T.PositiveInfinity, T.NegativeInfinity]);

    private static void Check<T>(T[] input) where T : unmanaged, IFloatingPoint<T>
    {
        foreach (var function in new[] { "Floor", "Ceiling", "Truncate" })
        {
            var expected = input.Select(x => Scalar(function, x)).ToArray();
            var before = Bytes(input);
            var output = Enumerable.Repeat(T.CreateChecked(7), input.Length + 1).ToArray();
            Apply(function, input, output);
            Assert.Equal(Bytes(expected), MemoryMarshal.AsBytes(output.AsSpan(0, input.Length)).ToArray());
            Assert.Equal(before, Bytes(input));
            Assert.Equal(T.CreateChecked(7), output[^1]);
            var inplace = (T[])input.Clone();
            Apply(function, inplace, inplace);
            Assert.Equal(Bytes(expected), Bytes(inplace));
            T[] shortOutput = [T.One];
            Assert.Throws<ArgumentException>(() => Apply(function, input, shortOutput));
            Assert.Equal(T.One, shortOutput[0]);
            T[] overlap = [T.One, T.CreateChecked(2), T.CreateChecked(3), T.CreateChecked(4)];
            var saved = Bytes(overlap);
            Assert.Throws<ArgumentException>(() => Apply(function, overlap.AsSpan(0, 3), overlap.AsSpan(1, 3)));
            Assert.Equal(saved, Bytes(overlap));
            Assert.Throws<ArgumentException>(() => Apply(function, overlap.AsSpan(1, 3), overlap.AsSpan(0, 3)));
            Assert.Equal(saved, Bytes(overlap));
            Apply(function, ReadOnlySpan<T>.Empty, shortOutput);
            Assert.Equal(T.One, shortOutput[0]);
        }
    }

    [Fact]
    public void WarmDoubleCallsAllocateNoManagedBytes()
    {
        double[] input = [-2.5, 0.25, 1], output = new double[3];
        for (int i = 0; i < 100; i++)
        {
            GenericMath.Floor<double>(input, output);
            GenericMath.Ceiling<double>(input, output);
            GenericMath.Truncate<double>(input, output);
        }
        Assert.Equal(0, AllocationProbe.MinimumBytes(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                GenericMath.Floor<double>(input, output);
                GenericMath.Ceiling<double>(input, output);
                GenericMath.Truncate<double>(input, output);
            }
        }));
    }

    private static byte[] Bytes<T>(T[] values) where T : unmanaged => MemoryMarshal.AsBytes(values.AsSpan()).ToArray();
    private static T Scalar<T>(string function, T value) where T : IFloatingPoint<T>
        => function switch { "Floor" => GenericMath.Floor(value), "Ceiling" => GenericMath.Ceiling(value), _ => GenericMath.Truncate(value) };
    private static void Apply<T>(string function, ReadOnlySpan<T> input, Span<T> output) where T : IFloatingPoint<T>
    {
        switch (function)
        {
            case "Floor": GenericMath.Floor(input, output); break;
            case "Ceiling": GenericMath.Ceiling(input, output); break;
            default: GenericMath.Truncate(input, output); break;
        }
    }
}
