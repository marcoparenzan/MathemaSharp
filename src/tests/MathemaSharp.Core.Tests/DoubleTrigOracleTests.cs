// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Text.Json;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class DoubleTrigOracleTests
{
    [Theory]
    [InlineData("Sin", 17)]
    [InlineData("Cos", 17)]
    [InlineData("Tan", 17)]
    [InlineData("Atan", 17)]
    [InlineData("Asin", 13)]
    [InlineData("Acos", 13)]
    [InlineData("Atan2", 16)]
    public void IndependentReferencesStayWithinOneUlp(string function, int expectedCount)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "double-trig-oracle.json")));
        Assert.Equal(1, document.RootElement.GetProperty("acceptanceMaxUlp").GetInt32());
        Assert.Equal("mpmath", document.RootElement.GetProperty("reference").GetString());
        Assert.Equal("1.3.0", document.RootElement.GetProperty("referenceVersion").GetString());
        Assert.Equal(new[] { 160, 200 }, document.RootElement.GetProperty("precisions").EnumerateArray().Select(v => v.GetInt32()).ToArray());
        int count = 0;
        foreach (var sample in document.RootElement.GetProperty("cases").EnumerateArray())
        {
            if (sample.GetProperty("function").GetString() != function) continue;
            count++;
            var inputs = sample.GetProperty("inputBits").EnumerateArray().Select(v => BitConverter.UInt64BitsToDouble(Convert.ToUInt64(v.GetString(), 16))).ToArray();
            Assert.Equal(function == "Atan2" ? 2 : 1, inputs.Length);
            double x = inputs[0];
            double actual = function switch
            {
                "Sin" => GenericMath.Sin(x), "Cos" => GenericMath.Cos(x), "Tan" => GenericMath.Tan(x),
                "Asin" => GenericMath.Asin(x), "Acos" => GenericMath.Acos(x), "Atan" => GenericMath.Atan(x),
                "Atan2" => GenericMath.Atan2(x, inputs[1]), _ => throw new InvalidOperationException()
            };
            Assert.True(double.IsFinite(actual));
            ulong expected = Convert.ToUInt64(sample.GetProperty("expectedBits").GetString(), 16);
            ulong a = Ordered(BitConverter.DoubleToUInt64Bits(actual)), b = Ordered(expected);
            ulong distance = a > b ? a - b : b - a;
            Assert.True(distance <= 1, $"{function}({string.Join(",", inputs.Select(v => v.ToString("R")))}): {distance} ULP");
        }
        Assert.Equal(expectedCount, count);
    }

    private static ulong Ordered(ulong bits) => (bits & (1UL << 63)) != 0 ? ~bits : bits | (1UL << 63);
}
