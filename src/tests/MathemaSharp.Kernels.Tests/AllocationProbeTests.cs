// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
namespace MathemaSharp.Kernels.Tests;

public class AllocationProbeTests
{
    private static object? _sink;

    [Fact]
    public void ReportsAnAllocationThatHappensInEveryRound() => Assert.True(AllocationProbe.MinimumBytes(() => _sink = new byte[100]) >= 100);

    [Fact]
    public void ReportsZeroForWorkThatDoesNotAllocate()
    {
        var data = new double[16];
        Assert.Equal(0, AllocationProbe.MinimumBytes(() => { for (int i = 0; i < data.Length; i++) data[i] += 1; }));
    }
}
