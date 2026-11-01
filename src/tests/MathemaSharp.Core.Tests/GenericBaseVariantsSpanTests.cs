// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericBaseVariantsSpanTests
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
        double[] input = [0, 0.25, -0.25], output = new double[3];
        for (int i = 0; i < 100; i++) Warm(input, output);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++) Warm(input, output);
        Assert.Equal(before, GC.GetAllocatedBytesForCurrentThread());
    }

    private static void Warm(double[] input, double[] output)
    {
        GenericMath.Exp2M1<double>(input, output); GenericMath.Exp10M1<double>(input, output);
        GenericMath.Log2P1<double>(input, output); GenericMath.Log10P1<double>(input, output);
    }

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        T[] input = [T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon,
            T.CreateChecked(0.00001m), T.CreateChecked(-0.00001m), T.CreateChecked(0.25m),
            T.CreateChecked(-0.25m), T.One, -T.One, T.BitIncrement(-T.One), T.BitDecrement(-T.One),
            T.CreateChecked(1000), T.CreateChecked(-1000), T.PositiveInfinity, T.NegativeInfinity, T.NaN];
        foreach (var function in new[] { "Exp2M1", "Exp10M1", "Log2P1", "Log10P1" })
        {
            var saved = Bytes(input); var expected = input.Select(v => Scalar(function, v)).ToArray();
            var output = Enumerable.Repeat(T.CreateChecked(7), input.Length + 1).ToArray();
            Apply(function, input, output);
            Assert.Equal(Bytes(expected), MemoryMarshal.AsBytes(output.AsSpan(0, input.Length)).ToArray());
            Assert.Equal(saved, Bytes(input)); Assert.Equal(T.CreateChecked(7), output[^1]);
            var inplace = (T[])input.Clone(); Apply(function, inplace, inplace); Assert.Equal(Bytes(expected), Bytes(inplace));
            T[] sentinel = [T.One];
            Assert.Throws<ArgumentException>(() => Apply(function, input, sentinel)); Assert.Equal(T.One, sentinel[0]);
            T[] overlap = [T.One, T.One, T.One, T.One]; var savedOverlap = Bytes(overlap);
            Assert.Throws<ArgumentException>(() => Apply(function, overlap.AsSpan(0, 3), overlap.AsSpan(1, 3))); Assert.Equal(savedOverlap, Bytes(overlap));
            Assert.Throws<ArgumentException>(() => Apply(function, overlap.AsSpan(1, 3), overlap.AsSpan(0, 3))); Assert.Equal(savedOverlap, Bytes(overlap));
            Apply(function, ReadOnlySpan<T>.Empty, sentinel); Assert.Equal(T.One, sentinel[0]);
            Apply(function, ReadOnlySpan<T>.Empty, Span<T>.Empty);
        }
    }

    private static byte[] Bytes<T>(T[] values) where T : unmanaged => MemoryMarshal.AsBytes(values.AsSpan()).ToArray();
    private static T Scalar<T>(string function, T value) where T : IFloatingPointIeee754<T> => function switch
    {
        "Exp2M1" => GenericMath.Exp2M1(value), "Exp10M1" => GenericMath.Exp10M1(value),
        "Log2P1" => GenericMath.Log2P1(value), _ => GenericMath.Log10P1(value)
    };
    private static void Apply<T>(string function, ReadOnlySpan<T> input, Span<T> output) where T : IFloatingPointIeee754<T>
    {
        switch (function)
        {
            case "Exp2M1": GenericMath.Exp2M1(input, output); break;
            case "Exp10M1": GenericMath.Exp10M1(input, output); break;
            case "Log2P1": GenericMath.Log2P1(input, output); break;
            default: GenericMath.Log10P1(input, output); break;
        }
    }
}
