using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using MathemaSharp.Core;
using System.Numerics;

var config = BenchmarkDotNet.Configs.DefaultConfig.Instance
    .WithArtifactsPath(@"D:\Dev\BuildArtifacts\MathemaSharp\benchmarks");
BenchmarkSwitcher.FromAssembly(typeof(SumBenchmarks).Assembly).Run(args, config);

[MemoryDiagnoser]
public class SumBenchmarks
{
    [Params(32, 4096)] public int N { get; set; }
    private double[] doubles = [];
    private float[] singles = [];
    private decimal[] decimals = [];
    [GlobalSetup]
    public void Setup()
    {
                // The same exactly representable values: measure cost, not conversion noise.
        doubles = Enumerable.Range(0, N).Select(i => (i % 17) * 0.125).ToArray();
        singles = doubles.Select(x => (float)x).ToArray();
        decimals = doubles.Select(x => (decimal)x).ToArray();
    }
    [Benchmark(Baseline = true)] public double HandwrittenDouble()
    {
        var sum = 0d;
        foreach (var x in doubles) sum += x;
        return sum;
    }
    [Benchmark] public double GenericDouble() => NumericAlgorithms.Sum<double>(doubles);
    [Benchmark] public float GenericFloat() => NumericAlgorithms.Sum<float>(singles);
    [Benchmark] public decimal GenericDecimal() => NumericAlgorithms.Sum<decimal>(decimals);
    [Benchmark] public double KahanDouble() => NumericAlgorithms.KahanSum<double>(doubles);
    [Benchmark] public double LinqDouble() => doubles.Sum();
}

[MemoryDiagnoser]
public class DotBenchmarks
{
    [Params(31, 4099)] public int N { get; set; }
    private double[] left = [];
    private double[] right = [];
    [GlobalSetup] public void Setup()
    {
        left = Enumerable.Range(0, N).Select(i => (i % 17) * 0.125).ToArray();
        right = Enumerable.Range(0, N).Select(i => (i % 13) * 0.25).ToArray();
    }
    [Benchmark(Baseline = true)] public double Scalar() => NumericAlgorithms.Dot<double>(left, right);
    [Benchmark] public double Simd() => Signals.VectorDot(left, right);
}

[MemoryDiagnoser]
public class IntegerBenchmarks
{
    [Params(20, 100, 500)] public int N { get; set; }
    [Benchmark] public BigInteger Factorial() => NumericAlgorithms.Factorial(N);
}

[MemoryDiagnoser]
public class FftBenchmarks
{
    [Params(64, 1009, 1024)] public int N { get; set; }
    private Complex<double>[] samples = [];
    private Complex<double>[] spectrum = [];
    [GlobalSetup]
    public void Setup()
    {
        samples = Enumerable.Range(0, N).Select(i => new Complex<double>((i % 17) / 16.0, (i % 11) / 8.0)).ToArray();
        spectrum = Signals.Fft<double>(samples);
    }
    [Benchmark] public Complex<double>[] Forward() => Signals.Fft<double>(samples);
    [Benchmark] public Complex<double>[] Inverse() => Signals.Ifft<double>(spectrum);
}

[MemoryDiagnoser]
public class ComplexBenchmarks
{
    [Params(8, 64)] public int N { get; set; }
    private Complex<double>[] samples = [];
    [GlobalSetup] public void Setup() => samples = Enumerable.Range(0, N).Select(i => new Complex<double>(Math.Sin(i), 0)).ToArray();
    [Benchmark] public Complex<double>[] Dft() => Signals.Dft<double>(samples);
}
