// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class ConstantMetadataTests
{
    [Fact]
    public void AllDefinitionsMatchGeneratedMetadata()
    {
        using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "math-constants.json")));
        Assert.Equal(13, MathematicalConstants.Definitions.Count);
        Assert.Equal(13, MathematicalConstants.Definitions.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count());
        foreach (var record in fixture.RootElement.GetProperty("constants").EnumerateArray())
        {
            var definition = MathematicalConstants.Get(record.GetProperty("id").GetString()!);
            Assert.Equal(record.GetProperty("symbol").GetString(), definition.Symbol);
            Assert.Equal(record.GetProperty("value").GetString(), definition.DecimalValue);
            Assert.Equal(record.GetProperty("source").GetString(), definition.Source);
            Assert.Equal(record.GetProperty("unit").GetString(), definition.Unit);
            Assert.Equal(ConstantStatus.Mathematical, definition.Status);
            Assert.Equal(100, definition.SignificantDigits);
            Assert.Equal(new[] { 0, 0, 0, 0, 0, 0, 0 }, definition.UnitExponents.ToArray());
            Assert.Equal("0", definition.StandardUncertainty);
            Assert.Equal("0", definition.RelativeStandardUncertainty);
            Assert.Null(definition.SourceDate);
        }
    }

    [Fact]
    public void LookupIsOrdinalAndRejectsUnknownIds()
    {
        Assert.Same(MathematicalConstants.Definitions[0], MathematicalConstants.Get("Pi"));
        Assert.Throws<ArgumentNullException>(() => MathematicalConstants.Get(null!));
        Assert.Throws<KeyNotFoundException>(() => MathematicalConstants.Get("pi"));
        Assert.Throws<KeyNotFoundException>(() => MathematicalConstants.Get("speed of light in vacuum"));
    }

    [Fact]
    public void PublicCollectionCannotBeMutated()
    {
        var list = Assert.IsAssignableFrom<IList<ConstantDefinition>>(MathematicalConstants.Definitions);
        Assert.True(list.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => list[0] = list[1]);
        Assert.Throws<NotSupportedException>(() => list.Clear());
        Assert.Equal(13, MathematicalConstants.Definitions.Count);
    }

    [Fact] public void HalfConversion() => Check<Half>();
    [Fact] public void SingleConversion() => Check<float>();
    [Fact] public void DoubleConversion() => Check<double>();
    [Fact] public void Decimal32Conversion() => Check<Decimal32>();
    [Fact] public void Decimal64Conversion() => Check<Decimal64>();
    [Fact] public void Decimal128Conversion() => Check<Decimal128>();

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            foreach (var definition in MathematicalConstants.Definitions)
            {
                var expected = (T)typeof(MathConstants<T>).GetField(definition.Id)!.GetValue(null)!;
                var actual = definition.Value<T>();
                T[] a = [expected], b = [actual];
                Assert.Equal(MemoryMarshal.AsBytes(a.AsSpan()).ToArray(), MemoryMarshal.AsBytes(b.AsSpan()).ToArray());
            }
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }
}
