// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class MathConstantsTests
{
    [Fact] public void HalfFormat() => Check<Half>();
    [Fact] public void SingleFormat() => Check<float>();
    [Fact] public void DoubleFormat() => Check<double>();
    [Fact] public void Decimal32Format() => Check<Decimal32>();
    [Fact] public void Decimal64Format() => Check<Decimal64>();
    [Fact] public void Decimal128Format() => Check<Decimal128>();

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "math-constants.json")));
        Assert.Equal("mpmath 1.3.0", document.RootElement.GetProperty("reference").GetString());
        Assert.Equal(9, document.RootElement.GetProperty("constants").GetArrayLength());
        foreach (var constant in document.RootElement.GetProperty("constants").EnumerateArray())
        {
            Assert.Equal(100, constant.GetProperty("significantDigits").GetInt32());
            T actual = constant.GetProperty("id").GetString() switch
            {
                "Pi" => MathConstants<T>.Pi, "Tau" => MathConstants<T>.Tau, "E" => MathConstants<T>.E,
                "Sqrt2" => MathConstants<T>.Sqrt2, "Ln2" => MathConstants<T>.Ln2, "Ln10" => MathConstants<T>.Ln10,
                "Sqrt3" => MathConstants<T>.Sqrt3, "Log2E" => MathConstants<T>.Log2E, "Log10E" => MathConstants<T>.Log10E,
                _ => throw new InvalidOperationException()
            };
            string type = typeof(T).Name;
            if (constant.GetProperty("binaryBits").TryGetProperty(type, out var expected))
            {
                ulong bits = typeof(T) == typeof(Half) ? (ushort)BitConverter.HalfToInt16Bits((Half)(object)actual)
                    : typeof(T) == typeof(float) ? BitConverter.SingleToUInt32Bits((float)(object)actual)
                    : BitConverter.DoubleToUInt64Bits((double)(object)actual);
                Assert.Equal(Convert.ToUInt64(expected.GetString(), 16), bits);
            }
            else
            {
                var rounded = T.Parse(constant.GetProperty("decimalValues").GetProperty(type).GetString()!, CultureInfo.InvariantCulture);
                T[] a = [actual], b = [rounded];
                Assert.Equal(MemoryMarshal.AsBytes(a.AsSpan()).ToArray(), MemoryMarshal.AsBytes(b.AsSpan()).ToArray());
            }
            Assert.True(T.IsFinite(actual));
            if (constant.GetProperty("id").GetString() is "Pi" or "Tau" or "E")
                Assert.True(actual > T.CreateChecked(2));
            else
                Assert.True(actual > T.Zero);
        }
    }
}
