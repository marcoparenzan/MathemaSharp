// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericInverseHyperbolicSpanTests
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
        double[] input = [0, 0.25, -1, 2], output = new double[4];
        for (int i = 0; i < 100; i++)
        {
            GenericMath.Asinh<double>(input, output);
            GenericMath.Acosh<double>(input, output);
            GenericMath.Atanh<double>(input, output);
        }
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++)
        {
            GenericMath.Asinh<double>(input, output);
            GenericMath.Acosh<double>(input, output);
            GenericMath.Atanh<double>(input, output);
        }
        Assert.Equal(before, GC.GetAllocatedBytesForCurrentThread());
    }

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        T[] input = [T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon,
            T.CreateChecked(0.25m), T.One, -T.One, T.BitDecrement(T.One), T.BitIncrement(T.One),
            T.BitDecrement(-T.One), T.BitIncrement(-T.One), T.CreateChecked(2), T.CreateChecked(-2),
            T.CreateChecked(1000), T.NaN, T.PositiveInfinity, T.NegativeInfinity];
        foreach (var function in new[] { "Asinh", "Acosh", "Atanh" })
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
        => function switch { "Asinh" => GenericMath.Asinh(value), "Acosh" => GenericMath.Acosh(value), _ => GenericMath.Atanh(value) };
    private static void Apply<T>(string function, ReadOnlySpan<T> input, Span<T> destination) where T : IHyperbolicFunctions<T>
    {
        switch (function)
        {
            case "Asinh": GenericMath.Asinh(input, destination); break;
            case "Acosh": GenericMath.Acosh(input, destination); break;
            default: GenericMath.Atanh(input, destination); break;
        }
    }
}
