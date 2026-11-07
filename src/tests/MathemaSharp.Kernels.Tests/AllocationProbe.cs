// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
namespace MathemaSharp.Kernels.Tests;

/// <summary>Measures the managed bytes that a piece of work allocates on the current thread.</summary>
internal static class AllocationProbe
{
    /// <summary>The minimum, over up to <paramref name="rounds"/> runs of <paramref name="work"/>, of the bytes allocated on this thread.
    /// An allocation in the code under test appears in every run; an occasional allocation by the runtime on the test thread
    /// (tiered compilation, a callback of the test host) does not, and used to make single measurements fail now and then.</summary>
    public static long MinimumBytes(Action work, int rounds = 5)
    {
        long minimum = long.MaxValue;
        for (int r = 0; r < rounds && minimum > 0; r++)
        {
            long before = GC.GetAllocatedBytesForCurrentThread();
            work();
            minimum = Math.Min(minimum, GC.GetAllocatedBytesForCurrentThread() - before);
        }
        return minimum;
    }
}
