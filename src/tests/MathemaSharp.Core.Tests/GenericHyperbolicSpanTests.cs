// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericHyperbolicSpanTests
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
        double[] input = [0, 0.25, -1], output = new double[3];
        for (int i = 0; i < 100; i++)
        {
            GenericMath.Sinh<double>(input, output);
            GenericMath.Cosh<double>(input, output);
            GenericMath.Tanh<double>(input, output);
        }
        Assert.Equal(0, AllocationProbe.MinimumBytes(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                GenericMath.Sinh<double>(input, output);
                GenericMath.Cosh<double>(input, output);
                GenericMath.Tanh<double>(input, output);
            }
        }));
    }

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        T[] input = [T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon,
            T.CreateChecked(0.25m), -T.One, T.CreateChecked(1000), T.CreateChecked(-1000),
            T.NaN, T.PositiveInfinity, T.NegativeInfinity];
        foreach (var function in new[] { "Sinh", "Cosh", "Tanh" })
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
        }
    }

    private static byte[] Bytes<T>(T[] values) where T : unmanaged => MemoryMarshal.AsBytes(values.AsSpan()).ToArray();
    private static T Scalar<T>(string function, T value) where T : IHyperbolicFunctions<T>
        => function switch { "Sinh" => GenericMath.Sinh(value), "Cosh" => GenericMath.Cosh(value), _ => GenericMath.Tanh(value) };
    private static void Apply<T>(string function, ReadOnlySpan<T> input, Span<T> destination) where T : IHyperbolicFunctions<T>
    {
        switch (function)
        {
            case "Sinh": GenericMath.Sinh(input, destination); break;
            case "Cosh": GenericMath.Cosh(input, destination); break;
            default: GenericMath.Tanh(input, destination); break;
        }
    }
}
