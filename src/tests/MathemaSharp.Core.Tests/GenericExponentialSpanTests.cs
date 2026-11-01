// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericExponentialSpanTests
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
        double[] input = [0, -0.0, 0.25, -1], output = new double[4];
        for (int i = 0; i < 100; i++)
        {
            GenericMath.Exp2<double>(input, output);
            GenericMath.Exp10<double>(input, output);
            GenericMath.ExpM1<double>(input, output);
        }
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++)
        {
            GenericMath.Exp2<double>(input, output);
            GenericMath.Exp10<double>(input, output);
            GenericMath.ExpM1<double>(input, output);
        }
        Assert.Equal(before, GC.GetAllocatedBytesForCurrentThread());
    }

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        var nearZero = T.BitIncrement(T.One) - T.One;
        T[] input = [T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon, nearZero, -nearZero,
            T.CreateChecked(0.001m), T.CreateChecked(-0.001m), T.One, -T.One,
            T.CreateChecked(10000), T.CreateChecked(-10000), T.NaN, T.PositiveInfinity, T.NegativeInfinity];
        foreach (var function in new[] { "Exp2", "Exp10", "ExpM1" })
        {
            var savedInput = Bytes(input);
            var expected = input.Select(value => Scalar(function, value)).ToArray();
            var output = Enumerable.Repeat(T.CreateChecked(7), input.Length + 1).ToArray();
            Apply(function, input, output);
            Assert.Equal(Bytes(expected), MemoryMarshal.AsBytes(output.AsSpan(0, input.Length)).ToArray());
            Assert.Equal(savedInput, Bytes(input));
            Assert.Equal(T.CreateChecked(7), output[^1]);
            if (function == "ExpM1")
            {
                Assert.True(T.IsZero(output[1]));
                Assert.True(T.IsNegative(output[1]));
            }
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
    private static T Scalar<T>(string function, T value) where T : IExponentialFunctions<T>
        => function switch { "Exp2" => GenericMath.Exp2(value), "Exp10" => GenericMath.Exp10(value), _ => GenericMath.ExpM1(value) };
    private static void Apply<T>(string function, ReadOnlySpan<T> input, Span<T> destination) where T : IExponentialFunctions<T>
    {
        switch (function)
        {
            case "Exp2": GenericMath.Exp2(input, destination); break;
            case "Exp10": GenericMath.Exp10(input, destination); break;
            default: GenericMath.ExpM1(input, destination); break;
        }
    }
}
