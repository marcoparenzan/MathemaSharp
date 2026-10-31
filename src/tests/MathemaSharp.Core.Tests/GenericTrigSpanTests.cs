// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericTrigSpanTests
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
            GenericMath.Sin<double>(input, output);
            GenericMath.Cos<double>(input, output);
            GenericMath.Tan<double>(input, output);
        }
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++)
        {
            GenericMath.Sin<double>(input, output);
            GenericMath.Cos<double>(input, output);
            GenericMath.Tan<double>(input, output);
        }
        long after = GC.GetAllocatedBytesForCurrentThread();
        Assert.Equal(before, after);
    }

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        T[] input = [T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon,
            T.CreateChecked(0.25m), -T.One, T.Pi, T.NaN, T.PositiveInfinity, T.NegativeInfinity];
        foreach (var function in new[] { "Sin", "Cos", "Tan" })
        {
            var before = MemoryMarshal.AsBytes(input.AsSpan()).ToArray();
            var expected = input.Select(value => Scalar(function, value)).ToArray();
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

    private static byte[] Bytes<T>(T[] values) where T : unmanaged => MemoryMarshal.AsBytes(values.AsSpan()).ToArray();
    private static T Scalar<T>(string function, T value) where T : ITrigonometricFunctions<T>
        => function switch { "Sin" => GenericMath.Sin(value), "Cos" => GenericMath.Cos(value), _ => GenericMath.Tan(value) };
    private static void Apply<T>(string function, ReadOnlySpan<T> input, Span<T> destination) where T : ITrigonometricFunctions<T>
    {
        switch (function)
        {
            case "Sin": GenericMath.Sin(input, destination); break;
            case "Cos": GenericMath.Cos(input, destination); break;
            default: GenericMath.Tan(input, destination); break;
        }
    }
}
