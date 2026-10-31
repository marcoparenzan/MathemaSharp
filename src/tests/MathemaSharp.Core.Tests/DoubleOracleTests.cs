// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Text.Json;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class DoubleOracleTests
{
    [Theory]
    [InlineData("Sqrt")]
    [InlineData("Exp")]
    [InlineData("Log")]
    public void IndependentDecimalReferencesStayWithinOneUlp(string function)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "double-elementary-oracle.json")));
        Assert.Equal(1, document.RootElement.GetProperty("acceptanceMaxUlp").GetInt32());
        int count = 0;
        foreach (var sample in document.RootElement.GetProperty("cases").EnumerateArray())
        {
            if (sample.GetProperty("function").GetString() != function) continue;
            count++;
            var inputBits = Convert.ToUInt64(sample.GetProperty("inputBits").GetString(), 16);
            var expected = Convert.ToUInt64(sample.GetProperty("expectedBits").GetString(), 16);
            var input = BitConverter.UInt64BitsToDouble(inputBits);
            var actual = function switch
            {
                "Sqrt" => GenericMath.Sqrt(input),
                "Exp" => GenericMath.Exp(input),
                "Log" => GenericMath.Log(input),
                _ => throw new InvalidOperationException()
            };
            Assert.False(double.IsNaN(actual));
            // Outputs are finite; ordered encoding also distinguishes signed zeros.
            var a = Ordered(BitConverter.DoubleToUInt64Bits(actual));
            var b = Ordered(expected);
            ulong distance = a > b ? a - b : b - a;
            Assert.True(distance <= 1, $"{function} input={inputBits:x16}: distance={distance} ULP, actual={actual:R}");
        }
        Assert.True(count >= 13, "Reference corpus is incomplete.");
    }

    private static ulong Ordered(ulong bits) => (bits & (1UL << 63)) != 0 ? ~bits : bits | (1UL << 63);
}
