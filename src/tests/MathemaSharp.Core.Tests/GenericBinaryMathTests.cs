// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericBinaryMathTests
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
        double[] x = [1, 2, 4], y = [2, 3, 2], output = new double[3];
        for (int i = 0; i < 100; i++) { GenericMath.Pow<double>(x, y, output); GenericMath.Log<double>(x, y, output); }
        Assert.Equal(0, AllocationProbe.MinimumBytes(() =>
        {
            for (int i = 0; i < 100; i++) { GenericMath.Pow<double>(x, y, output); GenericMath.Log<double>(x, y, output); }
        }));
    }

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        Assert.Equal(T.CreateChecked(8), GenericMath.Pow(T.CreateChecked(2), T.CreateChecked(3)));
        Assert.Equal(T.One, GenericMath.Pow(T.One, T.One));
        Assert.Equal(T.Zero, GenericMath.Log(T.One, T.CreateChecked(2)));
        T[] values = [T.Zero, T.NegativeZero, T.Epsilon, -T.Epsilon, T.One, -T.One,
            T.CreateChecked(0.5m), T.CreateChecked(-0.5m), T.CreateChecked(2), T.CreateChecked(-2),
            T.CreateChecked(3), T.CreateChecked(1000), T.PositiveInfinity, T.NegativeInfinity, T.NaN];
        foreach (var name in new[] { "Pow", "Log" })
        {
            var x = values.SelectMany(v => Enumerable.Repeat(v, values.Length)).ToArray();
            var y = values.SelectMany(_ => values).ToArray();
            for (int i = 0; i < x.Length; i++)
                SameBits(name == "Pow" ? T.Pow(x[i], y[i]) : T.Log(x[i], y[i]), Scalar(name, x[i], y[i]));
            var expected = x.Zip(y, (a, b) => Scalar(name, a, b)).ToArray();
            var savedX = Bytes(x); var savedY = Bytes(y);
            var output = Enumerable.Repeat(T.CreateChecked(7), x.Length + 1).ToArray();
            Apply(name, x, y, output);
            Assert.Equal(Bytes(expected), MemoryMarshal.AsBytes(output.AsSpan(0, x.Length)).ToArray());
            Assert.Equal(T.CreateChecked(7), output[^1]); Assert.Equal(savedX, Bytes(x)); Assert.Equal(savedY, Bytes(y));
            var inplaceX = (T[])x.Clone(); Apply(name, inplaceX, y, inplaceX); Assert.Equal(Bytes(expected), Bytes(inplaceX));
            var inplaceY = (T[])y.Clone(); Apply(name, x, inplaceY, inplaceY); Assert.Equal(Bytes(expected), Bytes(inplaceY));
            var both = (T[])values.Clone(); var bothExpected = values.Select(v => Scalar(name, v, v)).ToArray();
            Apply(name, both, both, both); Assert.Equal(Bytes(bothExpected), Bytes(both));
            T[] sentinel = [T.One];
            Assert.Throws<ArgumentException>(() => Apply(name, x, y, sentinel)); Assert.Equal(T.One, sentinel[0]);
            var savedOutput = Bytes(output);
            Assert.Throws<ArgumentException>(() => Apply(name, x.AsSpan(1), y, output)); Assert.Equal(savedOutput, Bytes(output));
            Assert.Throws<ArgumentException>(() => Apply(name, x, y.AsSpan(1), output)); Assert.Equal(savedOutput, Bytes(output));
            Apply(name, ReadOnlySpan<T>.Empty, ReadOnlySpan<T>.Empty, sentinel); Assert.Equal(T.One, sentinel[0]);
            Apply(name, ReadOnlySpan<T>.Empty, ReadOnlySpan<T>.Empty, Span<T>.Empty);
            T[] overlap = [T.One, T.CreateChecked(2), T.CreateChecked(3), T.CreateChecked(4)], separate = new T[3];
            var savedOverlap = Bytes(overlap);
            foreach (bool reverse in new[] { false, true })
            {
                int a = reverse ? 1 : 0, b = reverse ? 0 : 1;
                Assert.Throws<ArgumentException>(() => Apply(name, overlap.AsSpan(a, 3), separate, overlap.AsSpan(b, 3))); Assert.Equal(savedOverlap, Bytes(overlap));
                Assert.Throws<ArgumentException>(() => Apply(name, separate, overlap.AsSpan(a, 3), overlap.AsSpan(b, 3))); Assert.Equal(savedOverlap, Bytes(overlap));
            }
            Assert.Throws<ArgumentException>(() => Apply(name, overlap.AsSpan(0, 3), overlap.AsSpan(1, 3), overlap.AsSpan(0, 3))); Assert.Equal(savedOverlap, Bytes(overlap));
            Assert.Throws<ArgumentException>(() => Apply(name, overlap.AsSpan(1, 3), overlap.AsSpan(0, 3), overlap.AsSpan(0, 3))); Assert.Equal(savedOverlap, Bytes(overlap));
            var sharedExpected = Enumerable.Range(0, 3).Select(i => Scalar(name, overlap[i], overlap[i + 1])).ToArray();
            Apply(name, overlap.AsSpan(0, 3), overlap.AsSpan(1, 3), separate);
            Assert.Equal(Bytes(sharedExpected), Bytes(separate)); Assert.Equal(savedOverlap, Bytes(overlap));
            T[] twoBases = [T.CreateChecked(2), T.CreateChecked(2)];
            var tailExpected = Enumerable.Range(0, 2).Select(i => Scalar(name, overlap[i + 2], twoBases[i])).ToArray();
            Apply(name, overlap.AsSpan(2, 2), twoBases, overlap);
            Assert.Equal(Bytes(tailExpected), MemoryMarshal.AsBytes(overlap.AsSpan(0, 2)).ToArray());
            Assert.Equal(T.CreateChecked(3), overlap[2]); Assert.Equal(T.CreateChecked(4), overlap[3]);
        }
    }

    private static byte[] Bytes<T>(T[] values) where T : unmanaged => MemoryMarshal.AsBytes(values.AsSpan()).ToArray();
    private static void SameBits<T>(T expected, T actual) where T : unmanaged => Assert.Equal(Bytes(new[] { expected }), Bytes(new[] { actual }));
    private static T Scalar<T>(string name, T x, T y) where T : IFloatingPointIeee754<T> => name == "Pow" ? GenericMath.Pow(x, y) : GenericMath.Log(x, y);
    private static void Apply<T>(string name, ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> output) where T : IFloatingPointIeee754<T>
    {
        if (name == "Pow") GenericMath.Pow(x, y, output);
        else GenericMath.Log(x, y, output);
    }
}
