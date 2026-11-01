// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class PhysicalSiConstantsTests
{
    [Fact]
    public void SevenDefinitionsMatchExactUntruncatedNistRows()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "codata2022.json")));
        Assert.Equal(7, PhysicalConstants.Definitions.Count);
        foreach (var definition in PhysicalConstants.Definitions)
        {
            var row = document.RootElement.GetProperty("records").EnumerateArray().Single(x => x.GetProperty("name").GetString() == definition.Id);
            Assert.True(row.GetProperty("isExact").GetBoolean());
            Assert.False(row.GetProperty("valueIsTruncated").GetBoolean());
            Assert.Equal(row.GetProperty("decimalValue").GetString(), definition.DecimalValue);
            Assert.Equal(row.GetProperty("unit").GetString(), definition.Unit);
            Assert.Equal(SiUnitDimensions.Parse(definition.Unit), definition.Dimensions);
            Assert.Equal(ConstantStatus.ExactSi, definition.Status);
            Assert.Equal("0", definition.StandardUncertainty);
            Assert.Equal(document.RootElement.GetProperty("sha256").GetString(), definition.SourceSha256);
            Assert.Equal(2022, definition.SourceEdition);
        }
    }

    [Fact]
    public void DefinitionsAreReadOnlyAndUnknownMeasuredIdsAreRejected()
    {
        var list = Assert.IsAssignableFrom<IList<PhysicalConstantDefinition>>(PhysicalConstants.Definitions);
        Assert.Throws<NotSupportedException>(() => list.Clear());
        Assert.Throws<KeyNotFoundException>(() => PhysicalConstants.Get<double>("vacuum mag. permeability"));
        Assert.Throws<ArgumentNullException>(() => PhysicalConstants.Get<double>(null!));
    }

    [Fact] public void HalfFormat() => Check<Half>();
    [Fact] public void SingleFormat() => Check<float>();
    [Fact] public void DoubleFormat() => Check<double>();
    [Fact] public void Decimal32Format() => Check<Decimal32>();
    [Fact] public void Decimal64Format() => Check<Decimal64>();
    [Fact] public void Decimal128Format() => Check<Decimal128>();

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        foreach (var definition in PhysicalConstants.Definitions)
        {
            var expected = T.Parse(definition.DecimalValue, CultureInfo.InvariantCulture);
            if (!T.IsFinite(expected))
                Assert.Throws<OverflowException>(() => PhysicalConstants.Get<T>(definition.Id));
            else if (T.IsZero(expected))
                Assert.Throws<ArithmeticException>(() => PhysicalConstants.Get<T>(definition.Id));
            else
            {
                var actual = PhysicalConstants.Get<T>(definition.Id);
                T[] a = [actual.Value], b = [expected];
                Assert.Equal(MemoryMarshal.AsBytes(a.AsSpan()).ToArray(), MemoryMarshal.AsBytes(b.AsSpan()).ToArray());
                Assert.Same(definition, actual.Definition);
                Assert.Equal(T.Zero, actual.StandardUncertainty);
            }
        }
    }

    [Fact]
    public void Binary64FacadeValuesMatchTypedLookup()
    {
        Assert.Equal(PhysicalConstants.Get<double>("speed of light in vacuum").Value, PhysicalConstants.SpeedOfLight);
        Assert.Equal(PhysicalConstants.Get<double>("Planck constant").Value, PhysicalConstants.Planck);
        Assert.Equal(PhysicalConstants.Get<double>("Boltzmann constant").Value, PhysicalConstants.Boltzmann);
        Assert.Equal(PhysicalConstants.Get<double>("elementary charge").Value, PhysicalConstants.ElementaryCharge);
        Assert.Equal(PhysicalConstants.Get<double>("Avogadro constant").Value, PhysicalConstants.Avogadro);
        Assert.Equal(PhysicalConstants.Get<double>("hyperfine transition frequency of Cs-133").Value, PhysicalConstants.CesiumFrequency);
        Assert.Equal(PhysicalConstants.Get<double>("luminous efficacy").Value, PhysicalConstants.LuminousEfficacy);
    }
}
