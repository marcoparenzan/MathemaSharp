// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericInverseTrigSpanTests
{
    [Fact] public void HalfFormat() => Check<Half>();
    [Fact] public void SingleFormat() => Check<float>();
    [Fact] public void DoubleFormat() => Check<double>();
    [Fact] public void Decimal32Format() => Check<Decimal32>();
    [Fact] public void Decimal64Format() => Check<Decimal64>();
    [Fact] public void Decimal128Format() => Check<Decimal128>();

    [Fact]
    public void WarmDoubleSpanCallsAllocateNoManagedBytes()
    {
        double[] input = [0, -0.0, 0.25, -1, 1], output = new double[5];
        for (int i = 0; i < 100; i++)
        {
            GenericMath.Asin<double>(input, output);
            GenericMath.Acos<double>(input, output);
            GenericMath.Atan<double>(input, output);
        }
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++)
        {
            GenericMath.Asin<double>(input, output);
            GenericMath.Acos<double>(input, output);
            GenericMath.Atan<double>(input, output);
        }
        Assert.Equal(before, GC.GetAllocatedBytesForCurrentThread());
    }

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        T[] input = [T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon,
            T.CreateChecked(0.25m), T.CreateChecked(-0.25m), T.One, -T.One,
            T.BitDecrement(T.One), T.BitIncrement(T.One), T.BitIncrement(-T.One),
            T.BitDecrement(-T.One), T.CreateChecked(1000), T.CreateChecked(-1000),
            T.NaN, T.PositiveInfinity, T.NegativeInfinity];
        foreach (var function in new[] { "Asin", "Acos", "Atan" })
        {
            var savedInput = Bytes(input);
            var expected = input.Select(value => Scalar(function, value)).ToArray();
            var output = Enumerable.Repeat(T.CreateChecked(7), input.Length + 1).ToArray();
            Apply(function, input, output);
            Assert.Equal(Bytes(expected), MemoryMarshal.AsBytes(output.AsSpan(0, input.Length)).ToArray());
            Assert.Equal(savedInput, Bytes(input));
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
            Apply(function, ReadOnlySpan<T>.Empty, Span<T>.Empty);
            // Disjoint slices of shared backing storage are allowed.
            T[] shared = [T.CreateChecked(0.25m), T.CreateChecked(0.5m), T.One, T.One];
            T[] sharedExpected = [Scalar(function, shared[0]), Scalar(function, shared[1])];
            Apply(function, shared.AsSpan(0, 2), shared.AsSpan(2, 2));
            Assert.Equal(Bytes(sharedExpected), MemoryMarshal.AsBytes(shared.AsSpan(2, 2)).ToArray());
            Assert.Equal(T.CreateChecked(0.25m), shared[0]);
            Assert.Equal(T.CreateChecked(0.5m), shared[1]);
        }
    }

    private static byte[] Bytes<T>(T[] values) where T : unmanaged => MemoryMarshal.AsBytes(values.AsSpan()).ToArray();
    private static T Scalar<T>(string function, T value) where T : ITrigonometricFunctions<T>
        => function switch { "Asin" => GenericMath.Asin(value), "Acos" => GenericMath.Acos(value), _ => GenericMath.Atan(value) };
    private static void Apply<T>(string function, ReadOnlySpan<T> input, Span<T> destination) where T : ITrigonometricFunctions<T>
    {
        switch (function)
        {
            case "Asin": GenericMath.Asin(input, destination); break;
            case "Acos": GenericMath.Acos(input, destination); break;
            default: GenericMath.Atan(input, destination); break;
        }
    }
}
