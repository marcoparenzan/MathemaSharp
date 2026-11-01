// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class GenericAtan2SpanTests
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
        double[] y = [0, -0.0, 0.25, -1], x = [1, -1, 2, -2], output = new double[4];
        for (int i = 0; i < 100; i++) GenericMath.Atan2<double>(y, x, output);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++) GenericMath.Atan2<double>(y, x, output);
        Assert.Equal(before, GC.GetAllocatedBytesForCurrentThread());
    }

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        T[] values = [T.Zero, T.NegativeZero, T.One, -T.One, T.Epsilon, -T.Epsilon,
            T.CreateChecked(0.25m), T.CreateChecked(-0.25m), T.CreateChecked(1000),
            T.CreateChecked(-1000), T.PositiveInfinity, T.NegativeInfinity, T.NaN];
        var y = values.SelectMany(value => Enumerable.Repeat(value, values.Length)).ToArray();
        var x = values.SelectMany(_ => values).ToArray();
        var savedY = Bytes(y); var savedX = Bytes(x);
        var expected = y.Zip(x, (a, b) => GenericMath.Atan2(a, b)).ToArray();
        var output = Enumerable.Repeat(T.CreateChecked(7), y.Length + 1).ToArray();
        GenericMath.Atan2<T>(y, x, output);
        Assert.Equal(Bytes(expected), MemoryMarshal.AsBytes(output.AsSpan(0, y.Length)).ToArray());
        Assert.Equal(T.CreateChecked(7), output[^1]);
        Assert.Equal(savedY, Bytes(y)); Assert.Equal(savedX, Bytes(x));
        var inplaceY = (T[])y.Clone();
        GenericMath.Atan2<T>(inplaceY, x, inplaceY);
        Assert.Equal(Bytes(expected), Bytes(inplaceY)); Assert.Equal(savedX, Bytes(x));
        var inplaceX = (T[])x.Clone();
        GenericMath.Atan2<T>(y, inplaceX, inplaceX);
        Assert.Equal(Bytes(expected), Bytes(inplaceX)); Assert.Equal(savedY, Bytes(y));
        var both = (T[])values.Clone();
        var bothExpected = values.Select(value => GenericMath.Atan2(value, value)).ToArray();
        GenericMath.Atan2<T>(both, both, both);
        Assert.Equal(Bytes(bothExpected), Bytes(both));

        T[] sentinel = [T.One];
        Assert.Throws<ArgumentException>(() => GenericMath.Atan2<T>(y, x, sentinel));
        Assert.Equal(T.One, sentinel[0]);
        var mismatchOutput = (T[])output.Clone(); var savedOutput = Bytes(mismatchOutput);
        Assert.Throws<ArgumentException>(() => GenericMath.Atan2<T>(y, x.AsSpan(1), mismatchOutput));
        Assert.Equal(savedOutput, Bytes(mismatchOutput));
        Assert.Throws<ArgumentException>(() => GenericMath.Atan2<T>(y.AsSpan(1), x, mismatchOutput));
        Assert.Equal(savedOutput, Bytes(mismatchOutput));
        GenericMath.Atan2<T>(ReadOnlySpan<T>.Empty, ReadOnlySpan<T>.Empty, sentinel);
        Assert.Equal(T.One, sentinel[0]);
        GenericMath.Atan2<T>(ReadOnlySpan<T>.Empty, ReadOnlySpan<T>.Empty, Span<T>.Empty);
        Assert.Throws<ArgumentException>(() => GenericMath.Atan2<T>(ReadOnlySpan<T>.Empty, sentinel, sentinel));
        Assert.Equal(T.One, sentinel[0]);

        T[] shared = [T.One, T.CreateChecked(2), T.CreateChecked(3), T.CreateChecked(4)];
        T[] independent = [T.One, T.One, T.One];
        var savedShared = Bytes(shared);
        // Reject both overlap directions against each input, before any writes.
        Assert.Throws<ArgumentException>(() => GenericMath.Atan2<T>(shared.AsSpan(0, 3), independent, shared.AsSpan(1, 3)));
        Assert.Equal(savedShared, Bytes(shared));
        Assert.Throws<ArgumentException>(() => GenericMath.Atan2<T>(shared.AsSpan(1, 3), independent, shared.AsSpan(0, 3)));
        Assert.Equal(savedShared, Bytes(shared));
        Assert.Throws<ArgumentException>(() => GenericMath.Atan2<T>(independent, shared.AsSpan(0, 3), shared.AsSpan(1, 3)));
        Assert.Equal(savedShared, Bytes(shared));
        Assert.Throws<ArgumentException>(() => GenericMath.Atan2<T>(independent, shared.AsSpan(1, 3), shared.AsSpan(0, 3)));
        Assert.Equal(savedShared, Bytes(shared));
        // Exact alias with y cannot conceal a dangerous shifted overlap with x (and vice versa).
        Assert.Throws<ArgumentException>(() => GenericMath.Atan2<T>(shared.AsSpan(0, 3), shared.AsSpan(1, 3), shared.AsSpan(0, 3)));
        Assert.Equal(savedShared, Bytes(shared));
        Assert.Throws<ArgumentException>(() => GenericMath.Atan2<T>(shared.AsSpan(1, 3), shared.AsSpan(0, 3), shared.AsSpan(0, 3)));
        Assert.Equal(savedShared, Bytes(shared));
        // Read-only inputs may overlap each other when output is separate.
        T[] separate = new T[3];
        var sharedExpected = Enumerable.Range(0, 3).Select(i => GenericMath.Atan2(shared[i], shared[i + 1])).ToArray();
        GenericMath.Atan2<T>(shared.AsSpan(0, 3), shared.AsSpan(1, 3), separate);
        Assert.Equal(Bytes(sharedExpected), Bytes(separate)); Assert.Equal(savedShared, Bytes(shared));
        // Only the written destination prefix participates in overlap validation.
        var tailExpected = Enumerable.Range(0, 2).Select(i => GenericMath.Atan2(shared[i + 2], independent[i])).ToArray();
        GenericMath.Atan2<T>(shared.AsSpan(2, 2), independent.AsSpan(0, 2), shared);
        Assert.Equal(Bytes(tailExpected), MemoryMarshal.AsBytes(shared.AsSpan(0, 2)).ToArray());
        Assert.Equal(T.CreateChecked(3), shared[2]); Assert.Equal(T.CreateChecked(4), shared[3]);
    }

    private static byte[] Bytes<T>(T[] values) where T : unmanaged => MemoryMarshal.AsBytes(values.AsSpan()).ToArray();
}
