// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using BenchmarkDotNet.Attributes;
using MathemaSharp.Kernels;
using NDSharp;

/// <summary>The lane kernels against the NDSharp reductions they sit beside (same results, bit for bit, where the method is Pairwise/TwoPass)
/// on a 512 × 512 row-major array reduced along each axis, and on one long vector.</summary>
[MemoryDiagnoser]
public class ReductionBenchmarks
{
    const int Side = 512;
    double[] data = [];
    double[] result = new double[Side];
    NDArray array = null!;
    Lane axis0, axis1, flat;

    [GlobalSetup]
    public void Setup()
    {
        data = Enumerable.Range(0, Side * Side).Select(i => Math.Sin(i * 0.001) * 10).ToArray();
        array = NDArray.FromArrayView(new ArrayView<double>(data, [Side, Side], [Side, 1]), DType.Float64);
        axis0 = Lane.ForRowMajorAxis([Side, Side], 0);
        axis1 = Lane.ForRowMajorAxis([Side, Side], 1);
        flat = Lane.Flat(data.Length);
    }

    [Benchmark(Baseline = true)] public NDArray NDSharpSumAxis1() => np.Sum(array, [1]);
    [Benchmark] public double[] KernelSumAxis1() { Reductions.Sum<double>(data, axis1, result, NanPolicy.Propagate, SumMethod.Pairwise); return result; }
    [Benchmark] public NDArray NDSharpSumAxis0() => np.Sum(array, [0]);
    [Benchmark] public double[] KernelSumAxis0() { Reductions.Sum<double>(data, axis0, result, NanPolicy.Propagate, SumMethod.Pairwise); return result; }
    [Benchmark] public NDArray NDSharpSumAll() => np.Sum(array);
    [Benchmark] public double[] KernelSumAll() { Reductions.Sum<double>(data, flat, result, NanPolicy.Propagate, SumMethod.Pairwise); return result; }
    [Benchmark] public NDArray NDSharpVarAxis1() => np.Var(array, [1]);
    [Benchmark] public double[] KernelVarTwoPassAxis1() { Reductions.Variance<double>(data, axis1, result, 0, NanPolicy.Propagate, VarianceMethod.TwoPass); return result; }
    [Benchmark] public double[] KernelVarWelfordAxis1() { Reductions.Variance<double>(data, axis1, result, 0); return result; }
    [Benchmark] public NDArray NDSharpMaxAxis0() => np.Max(array, [0]);
    [Benchmark] public double[] KernelMaxAxis0() { Reductions.Max<double>(data, axis0, result, Span<int>.Empty, NanPolicy.Propagate); return result; }
    [Benchmark] public NDArray NDSharpCumSumAxis1() => np.CumSum(array, 1);
    [Benchmark] public double[] KernelCumSumAxis1() { Scans.Scan<double>(data, axis1, scan, ScanOp.Sum); return scan; }
    readonly double[] scan = new double[Side * Side];
}
