// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Text.Json;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class SiDimensionsTests
{
    [Theory]
    [InlineData("J", "kg m^2 s^-2")]
    [InlineData("N", "kg m s^-2")]
    [InlineData("Pa", "kg m^-1 s^-2")]
    [InlineData("W", "kg m^2 s^-3")]
    [InlineData("C", "A s")]
    [InlineData("V", "kg m^2 s^-3 A^-1")]
    [InlineData("F", "kg^-1 m^-2 s^4 A^2")]
    [InlineData("ohm", "kg m^2 s^-3 A^-2")]
    [InlineData("S", "kg^-1 m^-2 s^3 A^2")]
    [InlineData("Wb", "kg m^2 s^-2 A^-1")]
    [InlineData("T", "kg s^-2 A^-1")]
    [InlineData("H", "kg m^2 s^-2 A^-2")]
    [InlineData("lm", "cd sr")]
    [InlineData("lx", "cd m^-2")]
    public void CoherentDerivedSymbolsMatchBaseProducts(string derived, string product)
        => Assert.Equal(SiUnitDimensions.Parse(product), SiUnitDimensions.Parse(derived));

    [Fact]
    public void ProductsPowersAndDimensionlessSymbolsAreExplicit()
    {
        Assert.Equal(new SiDimensions(3, -1, -2, 0, 0, 0, 0), SiUnitDimensions.Parse("m^3 kg^-1 s^-2"));
        Assert.Equal(SiDimensions.Dimensionless, SiUnitDimensions.Parse("rad sr 1"));
        Assert.Equal(SiDimensions.Dimensionless, SiUnitDimensions.Parse("  "));
        Assert.Equal(SiUnitDimensions.Parse("K mol"), new SiDimensions(0, 0, 0, 0, 1, 1, 0));
    }

    [Theory]
    [InlineData("MeV")]
    [InlineData("u")]
    [InlineData("MHz T^-1")]
    [InlineData("m/s")]
    [InlineData("m^1.5")]
    [InlineData("m^")]
    [InlineData("M")]
    [InlineData("m^2147483648")]
    [InlineData("J^2147483647")]
    [InlineData("m^2147483647 m")]
    public void UnsupportedOrOverflowingExpressionsFailWithoutInference(string text)
    {
        Assert.False(SiUnitDimensions.TryParse(text, out var result));
        Assert.Equal(SiDimensions.Dimensionless, result);
        Assert.Throws<FormatException>(() => SiUnitDimensions.Parse(text));
    }

    [Fact]
    public void NullAndCheckedVectorOverflowHaveContracts()
    {
        Assert.False(SiUnitDimensions.TryParse(null, out _));
        Assert.Throws<ArgumentNullException>(() => SiUnitDimensions.Parse(null!));
        Assert.Throws<OverflowException>(() => new SiDimensions(int.MaxValue, 0, 0, 0, 0, 0, 0).Power(2));
    }

    [Fact]
    public void ImportedSourceHasExplicitPartialCoherentCoverage()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "codata2022.json")));
        var rows = document.RootElement.GetProperty("records").EnumerateArray().ToArray();
        int supported = rows.Count(row => SiUnitDimensions.TryParse(row.GetProperty("unit").GetString(), out _));
        Assert.Equal(293, supported);
        Assert.Equal(62, rows.Length - supported);
    }
}
