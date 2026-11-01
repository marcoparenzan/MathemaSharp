// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericRootsSpanTests
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
        double[] x = [0, 1, 8], y = [1, 2, 3], output = new double[3];
        for (int i = 0; i < 100; i++)
        {
            GenericMath.Cbrt<double>(x, output);
            GenericMath.RootN<double>(x, 3, output);
            GenericMath.Hypot<double>(x, y, output);
        }
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++)
        {
            GenericMath.Cbrt<double>(x, output);
            GenericMath.RootN<double>(x, 3, output);
            GenericMath.Hypot<double>(x, y, output);
        }
        Assert.Equal(before, GC.GetAllocatedBytesForCurrentThread());
    }

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        T[] input = [T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon, T.One, -T.One,
            T.CreateChecked(8), T.CreateChecked(-8), T.NaN, T.PositiveInfinity, T.NegativeInfinity];
        foreach (int? degree in new int?[] { null, int.MinValue, -3, -2, -1, 0, 1, 2, 3, int.MaxValue })
        {
            var expected = input.Select(v => degree.HasValue ? GenericMath.RootN(v, degree.Value) : GenericMath.Cbrt(v)).ToArray();
            var output = Enumerable.Repeat(T.CreateChecked(7), input.Length + 1).ToArray();
            var saved = Bytes(input);
            Apply(input, degree, output);
            Assert.Equal(Bytes(expected), MemoryMarshal.AsBytes(output.AsSpan(0, input.Length)).ToArray());
            Assert.Equal(saved, Bytes(input)); Assert.Equal(T.CreateChecked(7), output[^1]);
            var inplace = (T[])input.Clone(); Apply(inplace, degree, inplace);
            Assert.Equal(Bytes(expected), Bytes(inplace));
            T[] sentinel = [T.One];
            Assert.Throws<ArgumentException>(() => Apply(input, degree, sentinel)); Assert.Equal(T.One, sentinel[0]);
            T[] shared = [T.One, T.One, T.One, T.One]; var before = Bytes(shared);
            Assert.Throws<ArgumentException>(() => Apply(shared.AsSpan(0, 3), degree, shared.AsSpan(1, 3))); Assert.Equal(before, Bytes(shared));
            Assert.Throws<ArgumentException>(() => Apply(shared.AsSpan(1, 3), degree, shared.AsSpan(0, 3))); Assert.Equal(before, Bytes(shared));
            Apply(ReadOnlySpan<T>.Empty, degree, sentinel); Assert.Equal(T.One, sentinel[0]);
            Apply(ReadOnlySpan<T>.Empty, degree, Span<T>.Empty);
        }
        var x = input.SelectMany(v => Enumerable.Repeat(v, input.Length)).ToArray();
        var y = input.SelectMany(_ => input).ToArray();
        var lengths = x.Zip(y, (a, b) => GenericMath.Hypot(a, b)).ToArray();
        var destination = Enumerable.Repeat(T.CreateChecked(7), x.Length + 1).ToArray();
        var savedX = Bytes(x); var savedY = Bytes(y);
        GenericMath.Hypot<T>(x, y, destination);
        Assert.Equal(Bytes(lengths), MemoryMarshal.AsBytes(destination.AsSpan(0, x.Length)).ToArray());
        Assert.Equal(T.CreateChecked(7), destination[^1]); Assert.Equal(savedX, Bytes(x)); Assert.Equal(savedY, Bytes(y));
        var inplaceX = (T[])x.Clone(); GenericMath.Hypot<T>(inplaceX, y, inplaceX); Assert.Equal(Bytes(lengths), Bytes(inplaceX));
        var inplaceY = (T[])y.Clone(); GenericMath.Hypot<T>(x, inplaceY, inplaceY); Assert.Equal(Bytes(lengths), Bytes(inplaceY));
        var both = (T[])input.Clone(); var bothExpected = input.Select(v => GenericMath.Hypot(v, v)).ToArray();
        GenericMath.Hypot<T>(both, both, both); Assert.Equal(Bytes(bothExpected), Bytes(both));
        T[] shortOutput = [T.One];
        Assert.Throws<ArgumentException>(() => GenericMath.Hypot<T>(x, y, shortOutput)); Assert.Equal(T.One, shortOutput[0]);
        var beforeOutput = Bytes(destination);
        Assert.Throws<ArgumentException>(() => GenericMath.Hypot<T>(x.AsSpan(1), y, destination)); Assert.Equal(beforeOutput, Bytes(destination));
        Assert.Throws<ArgumentException>(() => GenericMath.Hypot<T>(x, y.AsSpan(1), destination)); Assert.Equal(beforeOutput, Bytes(destination));
        GenericMath.Hypot<T>(ReadOnlySpan<T>.Empty, ReadOnlySpan<T>.Empty, shortOutput); Assert.Equal(T.One, shortOutput[0]);
        GenericMath.Hypot<T>(ReadOnlySpan<T>.Empty, ReadOnlySpan<T>.Empty, Span<T>.Empty);
        T[] overlap = [T.One, T.CreateChecked(2), T.CreateChecked(3), T.CreateChecked(4)], separate = [T.One, T.One, T.One];
        var savedOverlap = Bytes(overlap);
        foreach (bool reversed in new[] { false, true })
        {
            int sourceOffset = reversed ? 1 : 0, outputOffset = reversed ? 0 : 1;
            Assert.Throws<ArgumentException>(() => GenericMath.Hypot<T>(overlap.AsSpan(sourceOffset, 3), separate, overlap.AsSpan(outputOffset, 3)));
            Assert.Equal(savedOverlap, Bytes(overlap));
            Assert.Throws<ArgumentException>(() => GenericMath.Hypot<T>(separate, overlap.AsSpan(sourceOffset, 3), overlap.AsSpan(outputOffset, 3)));
            Assert.Equal(savedOverlap, Bytes(overlap));
        }
        Assert.Throws<ArgumentException>(() => GenericMath.Hypot<T>(overlap.AsSpan(0, 3), overlap.AsSpan(1, 3), overlap.AsSpan(0, 3))); Assert.Equal(savedOverlap, Bytes(overlap));
        Assert.Throws<ArgumentException>(() => GenericMath.Hypot<T>(overlap.AsSpan(1, 3), overlap.AsSpan(0, 3), overlap.AsSpan(0, 3))); Assert.Equal(savedOverlap, Bytes(overlap));
        var overlappedExpected = Enumerable.Range(0, 3).Select(i => GenericMath.Hypot(overlap[i], overlap[i + 1])).ToArray();
        GenericMath.Hypot<T>(overlap.AsSpan(0, 3), overlap.AsSpan(1, 3), separate); Assert.Equal(Bytes(overlappedExpected), Bytes(separate)); Assert.Equal(savedOverlap, Bytes(overlap));
    }

    private static byte[] Bytes<T>(T[] values) where T : unmanaged => MemoryMarshal.AsBytes(values.AsSpan()).ToArray();
    private static void Apply<T>(ReadOnlySpan<T> input, int? degree, Span<T> output) where T : IRootFunctions<T>
    {
        if (degree.HasValue) GenericMath.RootN(input, degree.Value, output);
        else GenericMath.Cbrt(input, output);
    }
}
