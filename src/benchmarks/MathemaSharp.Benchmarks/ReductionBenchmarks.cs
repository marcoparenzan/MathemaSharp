// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using BenchmarkDotNet.Attributes;
using MathemaSharp.Kernels;

/// <summary>The lane kernels on a 512 × 512 row-major array reduced along each axis, and on one long vector. The sum with the plain left-to-right method is the baseline
/// that the pairwise method (NumPy's) is measured against.</summary>
[MemoryDiagnoser]
public class ReductionBenchmarks
{
    const int Side = 512;
    double[] data = [];
    double[] result = new double[Side];
    Lane axis0, axis1, flat;

    [GlobalSetup]
    public void Setup()
    {
        data = Enumerable.Range(0, Side * Side).Select(i => Math.Sin(i * 0.001) * 10).ToArray();
        axis0 = Lane.ForRowMajorAxis([Side, Side], 0);
        axis1 = Lane.ForRowMajorAxis([Side, Side], 1);
        flat = Lane.Flat(data.Length);
    }

    [Benchmark(Baseline = true)] public double[] KernelSumAxis1Sequential() { Reductions.Sum<double>(data, axis1, result, NanPolicy.Propagate, SumMethod.Sequential); return result; }
    [Benchmark] public double[] KernelSumAxis1() { Reductions.Sum<double>(data, axis1, result, NanPolicy.Propagate, SumMethod.Pairwise); return result; }
    [Benchmark] public double[] KernelSumAxis0() { Reductions.Sum<double>(data, axis0, result, NanPolicy.Propagate, SumMethod.Pairwise); return result; }
    [Benchmark] public double[] KernelSumAll() { Reductions.Sum<double>(data, flat, result, NanPolicy.Propagate, SumMethod.Pairwise); return result; }
    [Benchmark] public double[] KernelVarTwoPassAxis1() { Reductions.Variance<double>(data, axis1, result, 0, NanPolicy.Propagate, VarianceMethod.TwoPass); return result; }
    [Benchmark] public double[] KernelVarWelfordAxis1() { Reductions.Variance<double>(data, axis1, result, 0); return result; }
    [Benchmark] public double[] KernelMaxAxis0() { Reductions.Max<double>(data, axis0, result, Span<int>.Empty, NanPolicy.Propagate); return result; }
    [Benchmark] public double[] KernelCumSumAxis1() { Scans.Scan<double>(data, axis1, scan, ScanOp.Sum); return scan; }
    readonly double[] scan = new double[Side * Side];
}
