// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using MathemaSharp;

namespace CommonCore.Language.Tests;

public class FSharpCorpusTests
{
    [Fact]
    public async Task FSharpRunnerMatchesCSharpBitsAndRepresentabilityOutcomes()
    {
        var runner = typeof(FSharpCorpusTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(x => x.Key == "FSharpRunnerPath").Value!;
        Assert.True(File.Exists(runner), "F# runner was not built: " + runner);
        var start = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        start.ArgumentList.Add(runner);
        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            throw;
        }
        var output = await stdout;
        var errors = await stderr;
        Assert.True(process.ExitCode == 0, errors);
        Assert.Equal("", errors);
        var expected = new List<string>();
        Emit<Half>(expected); Emit<float>(expected); Emit<double>(expected);
        Emit<Decimal32>(expected); Emit<Decimal64>(expected); Emit<Decimal128>(expected);
        var actual = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(624, expected.Count);
        Assert.Equal(624, actual.Length);
        Assert.Equal(expected.Order(StringComparer.Ordinal), actual.Order(StringComparer.Ordinal));
    }

    private static void Emit<T>(List<string> output) where T : unmanaged, IFloatingPointIeee754<T>
    {
        var format = typeof(T).Name;
        (string Name, Func<T, T> Operation)[] operations =
        [ ("Sin", GenericMath.Sin<T>), ("Cos", GenericMath.Cos<T>), ("Tan", GenericMath.Tan<T>),
          ("Sqrt", GenericMath.Sqrt<T>), ("Exp", GenericMath.Exp<T>), ("Log", GenericMath.Log<T>) ];
        foreach (var definition in MathematicalConstants.Definitions)
        {
            var value = definition.Value<T>();
            output.Add($"math|{format}|{definition.Id}|{Bits(value)}");
            foreach (var operation in operations)
                output.Add($"scalar|{format}|{definition.Id}|{operation.Name}|{Bits(operation.Operation(value))}");
        }
        foreach (var definition in PhysicalConstants.Definitions)
        {
            try
            {
                var value = PhysicalConstants.Get<T>(definition.Id);
                output.Add($"physical|{format}|{definition.Id}|{Bits(value.Value)}|{Bits(value.StandardUncertainty)}");
            }
            catch (ArithmeticException error)
            {
                output.Add($"physical-error|{format}|{definition.Id}|{error.GetType().Name}");
            }
        }
    }

    private static string Bits<T>(T value) where T : unmanaged
        => Convert.ToHexString(MemoryMarshal.AsBytes(new ReadOnlySpan<T>([value])));
}
