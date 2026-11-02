// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Text.Json;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class DoubleBinaryOracleTests
{
    [Theory]
    [InlineData("Pow", 46)]
    [InlineData("LogBase", 32)]
    public void IndependentReferencesStayWithinOneUlp(string function, int expectedCount)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "double-binary-oracle.json")));
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
            Assert.Equal(2, inputs.Length);
            double actual = function == "Pow" ? GenericMath.Pow(inputs[0], inputs[1]) : GenericMath.Log(inputs[0], inputs[1]);
            Assert.True(double.IsFinite(actual));
            ulong expected = Convert.ToUInt64(sample.GetProperty("expectedBits").GetString(), 16);
            ulong a = Ordered(BitConverter.DoubleToUInt64Bits(actual)), b = Ordered(expected);
            ulong distance = a > b ? a - b : b - a;
            // The reference canonicalizes mathematical zero to +0; -0 is adjacent
            // in this ordered encoding. Signed-zero semantics have separate bit tests.
            Assert.True(distance <= 1, $"{function}({inputs[0]:R},{inputs[1]:R}): {distance} ULP");
        }
        Assert.Equal(expectedCount, count);
    }

    private static ulong Ordered(ulong bits) => (bits & (1UL << 63)) != 0 ? ~bits : bits | (1UL << 63);
}
