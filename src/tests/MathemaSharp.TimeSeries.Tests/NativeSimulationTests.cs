using System.Numerics;
using MathemaSharp.TimeSeries;

namespace MathemaSharp.TimeSeries.Tests;

public class NativeSimulationTests
{
    private static void Verify<T>() where T : IFloatingPointIeee754<T>
    {
        T N(int n) => T.CreateChecked(n);
        var random = new Random(42);
        var calls = 0;
        T Source(Random received)
        {
            Assert.Same(random, received);
            calls++;
            return T.One;
        }
        var arma = Simulation.Arma(random, 3, T.Zero, new T[] {T.One / N(2)}, [], T.One, Source, burnIn: 1);
        Assert.Equal(new T[] {N(3) / N(2), N(7) / N(4), N(15) / N(8)}, arma);
        Assert.Equal(4, calls);
        Assert.Equal(new T[] {T.One, N(2), N(3)}, Simulation.RandomWalk(random, 3, T.Zero, T.Zero, T.One, Source));
        Assert.Equal(new T[] {T.One, T.One, T.One}, Simulation.Garch(random, new Garch11<T>(T.One, T.Zero, T.Zero), 3, Source, burnIn: 0));
        var model = new VarModel<T>([T.Zero], [new T[,] {{T.One / N(2)}}], new T[1, 1]);
        var values = Simulation.Var(random, model, 3, T.One, Source, burnIn: 0);
        Assert.Equal(T.One, values[0, 0]);
        Assert.Equal(N(3) / N(2), values[1, 0]);
        Assert.Equal(N(7) / N(4), values[2, 0]);
        Assert.Equal(13, calls);
    }

    [Fact] public void HalfNative() => Verify<Half>();
    [Fact] public void FloatNative() => Verify<float>();
    [Fact] public void DoubleNative() => Verify<double>();
    [Fact] public void Decimal32Native() => Verify<Decimal32>();
    [Fact] public void Decimal64Native() => Verify<Decimal64>();
    [Fact] public void Decimal128Native() => Verify<Decimal128>();

    [Fact]
    public void Decimal128InnovationsAreNotRoundedThroughDouble()
    {
        var tiny = Decimal128.One;
        for (var i = 0; i < 25; i++) tiny /= Decimal128.CreateChecked(10);
        var walk = Simulation.RandomWalk(new Random(1), 2, Decimal128.One, Decimal128.Zero, Decimal128.One, _ => tiny);
        Assert.Equal(tiny, walk[0] - Decimal128.One);
        Assert.Equal(tiny + tiny, walk[1] - Decimal128.One);
    }

    [Fact]
    public void DefaultSourceRemainsBitIdenticalToExplicitLegacySource()
    {
        Assert.Equal(Simulation.Arma<double>(new Random(1), 10, 0, [0.5], [], 1, 2),
            Simulation.Arma<double>(new Random(1), 10, 0, [0.5], [], 1, Simulation.StandardNormal<double>, 2));
        Assert.Equal(Simulation.RandomWalk<double>(new Random(1), 10, 0, 0, 1),
            Simulation.RandomWalk<double>(new Random(1), 10, 0, 0, 1, Simulation.StandardNormal<double>));
        var garch = new Garch11<double>(0.1, 0.1, 0.8);
        Assert.Equal(Simulation.Garch(new Random(1), garch, 10, 2),
            Simulation.Garch(new Random(1), garch, 10, Simulation.StandardNormal<double>, 2));
        var model = new VarModel<double>([0], [new double[,] {{0.5}}], new double[1,1]);
        var legacy = Simulation.Var(new Random(1), model, 10, 1, 2);
        var explicitSource = Simulation.Var(new Random(1), model, 10, 1, Simulation.StandardNormal<double>, 2);
        for (var i = 0; i < 10; i++) Assert.Equal(legacy[i,0], explicitSource[i,0]);
    }

    [Fact]
    public void InvalidSourcesAndLengthsAreRejected()
    {
        Assert.Throws<ArgumentNullException>(() => Simulation.RandomWalk<double>(new Random(1), 1, 0, 0, 1, null!));
        Assert.Throws<ArgumentNullException>(() => Simulation.RandomWalk<double>(null!, 1, 0, 0, 1, _ => 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Simulation.RandomWalk<double>(new Random(1), -1, 0, 0, 1, _ => 1));
        Assert.Throws<ArgumentException>(() => Simulation.RandomWalk<double>(new Random(1), 1, 0, 0, 1, _ => double.NaN));
        Assert.Throws<ArgumentException>(() => Simulation.Arma<double>(new Random(1), 1, 0, [], [], 1, _ => double.PositiveInfinity, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Simulation.Arma<double>(new Random(1), 1, 0, [], [], 1, _ => 1, -1));
        Assert.Throws<OverflowException>(() => Simulation.Arma<double>(new Random(1), int.MaxValue, 0, [], [], 1, _ => 1, 1));
    }
}
