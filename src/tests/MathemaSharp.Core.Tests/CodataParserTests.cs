// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using Xunit;

namespace MathemaSharp.Core.Tests;

public class CodataParserTests
{
    private static string Row(string name, string value, string uncertainty, string unit = "")
        => name.PadRight(60) + value.PadRight(25) + uncertainty.PadRight(25) + unit;
    private static string Table(params string[] rows) => "2022 CODATA adjustment\n" + new string('-', 125) + "\n" + string.Join('\n', rows);

    [Fact]
    public void PreservesMeasuredAndTruncatedExactSourceFields()
    {
        var records = CodataParser.Parse(Table(Row("sample measured", "1.25 e-3", "0.01 e-3", "kg"),
            Row("sample exact derived", "2.71...", "(exact)")), 2022);
        Assert.Equal(2, records.Count);
        Assert.Equal("1.25e-3", records[0].DecimalValue);
        Assert.False(records[0].IsExact);
        Assert.Equal("kg", records[0].Unit);
        Assert.Equal("2.71...", records[1].ValueText);
        Assert.True(records[1].IsExact);
        Assert.True(records[1].ValueIsTruncated);
        Assert.Equal("2.71", records[1].DecimalValue);
        Assert.Equal("0", records[1].DecimalUncertainty);
        var list = Assert.IsAssignableFrom<IList<CodataRecord>>(records);
        Assert.Throws<NotSupportedException>(() => list.Clear());
    }

    [Fact]
    public void RejectsWrongEditionAndEmptyOrMalformedTable()
    {
        Assert.Throws<FormatException>(() => CodataParser.Parse(Table(Row("x", "1", "0.1")), 2018));
        Assert.Throws<FormatException>(() => CodataParser.Parse(Table(), 2022));
        Assert.Throws<FormatException>(() => CodataParser.Parse(Table("short row"), 2022));
        Assert.Throws<ArgumentNullException>(() => CodataParser.Parse(null!, 2022));
    }

    [Theory]
    [InlineData("not numeric", "0.1")]
    [InlineData("1", "-0.1")]
    [InlineData("1", "exact")]
    [InlineData("1", "NaN")]
    [InlineData("1...25", "0.1")]
    [InlineData("1", "0.1...")]
    public void RejectsInvalidNumericFields(string value, string uncertainty)
        => Assert.Throws<FormatException>(() => CodataParser.Parse(Table(Row("sample", value, uncertainty)), 2022));

    [Fact]
    public void ImportedPinnedSourceKeepsEditionAndExactTruncatedFlags()
    {
        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "codata2022.json")));
        Assert.Equal(2022, document.RootElement.GetProperty("edition").GetInt32());
        Assert.Equal("77fb90e66c40db3e6eb16630bc9c88e4c7c8beddbe5e71be406f2f26e3f67e67", document.RootElement.GetProperty("sha256").GetString());
        var rows = document.RootElement.GetProperty("records").EnumerateArray().ToArray();
        Assert.Equal(355, rows.Length);
        Assert.Equal(81, rows.Count(row => row.GetProperty("isExact").GetBoolean()));
        Assert.Equal(62, rows.Count(row => row.GetProperty("valueIsTruncated").GetBoolean()));
        Assert.Equal(355, rows.Select(row => row.GetProperty("name").GetString()).Distinct(StringComparer.Ordinal).Count());
        foreach (var row in rows)
        {
            Assert.Equal(row.GetProperty("valueText").GetString()!.Contains("...", StringComparison.Ordinal), row.GetProperty("valueIsTruncated").GetBoolean());
            if (row.GetProperty("isExact").GetBoolean())
            {
                Assert.Equal("(exact)", row.GetProperty("uncertaintyText").GetString());
                Assert.Equal("0", row.GetProperty("decimalUncertainty").GetString());
            }
        }
        Assert.True(rows.Single(row => row.GetProperty("name").GetString() == "speed of light in vacuum").GetProperty("isExact").GetBoolean());
        Assert.False(rows.Single(row => row.GetProperty("name").GetString() == "vacuum mag. permeability").GetProperty("isExact").GetBoolean());
    }

    [Fact]
    public void RejectsDuplicateQuantityNames()
        => Assert.Throws<FormatException>(() => CodataParser.Parse(Table(Row("same", "1", "0.1"), Row("same", "2", "0.2")), 2022));
}
