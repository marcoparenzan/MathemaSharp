// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class MeasuredConstantsTests
{
    [Fact]
    public void MeasuredMetadataAndUncertaintyMatchNist()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "codata2022.json")));
        Assert.Equal(10, PhysicalConstants.Definitions.Count);
        var measured = PhysicalConstants.Definitions.Where(x => x.Status == ConstantStatus.Measured).ToArray();
        Assert.Equal(3, measured.Length);
        foreach (var definition in measured)
        {
            var source = document.RootElement.GetProperty("records").EnumerateArray().Single(x => x.GetProperty("name").GetString() == definition.Id);
            Assert.False(source.GetProperty("isExact").GetBoolean());
            Assert.False(source.GetProperty("valueIsTruncated").GetBoolean());
            Assert.Equal(source.GetProperty("decimalValue").GetString(), definition.DecimalValue);
            Assert.Equal(source.GetProperty("decimalUncertainty").GetString(), definition.StandardUncertainty);
            Assert.Equal(source.GetProperty("unit").GetString(), definition.Unit);
            Assert.Equal(SiUnitDimensions.Parse(definition.Unit), definition.Dimensions);
            Assert.Equal(2022, definition.SourceEdition);
        }
        var mu0 = PhysicalConstants.Get<double>("vacuum mag. permeability");
        Assert.Equal(ConstantStatus.Measured, mu0.Definition.Status);
        Assert.True(mu0.StandardUncertainty > 0);
        Assert.Equal(PhysicalConstants.VacuumPermeability, mu0.Value);
        Assert.Equal(PhysicalConstants.GravitationalConstant, PhysicalConstants.Get<double>("Newtonian constant of gravitation").Value);
        Assert.Equal(PhysicalConstants.FineStructure, PhysicalConstants.Get<double>("fine-structure constant").Value);
    }

    [Fact] public void HalfFormat() => Check<Half>();
    [Fact] public void SingleFormat() => Check<float>();
    [Fact] public void DoubleFormat() => Check<double>();
    [Fact] public void Decimal32Format() => Check<Decimal32>();
    [Fact] public void Decimal64Format() => Check<Decimal64>();
    [Fact] public void Decimal128Format() => Check<Decimal128>();

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        foreach (var definition in PhysicalConstants.Definitions.Where(x => x.Status == ConstantStatus.Measured))
        {
            var expected = T.Parse(definition.DecimalValue, CultureInfo.InvariantCulture);
            var uncertainty = T.Parse(definition.StandardUncertainty, CultureInfo.InvariantCulture);
            if (!T.IsFinite(expected) || !T.IsFinite(uncertainty))
                Assert.Throws<OverflowException>(() => PhysicalConstants.Get<T>(definition.Id));
            else if (T.IsZero(expected) || T.IsZero(uncertainty))
                Assert.Throws<ArithmeticException>(() => PhysicalConstants.Get<T>(definition.Id));
            else
            {
                var result = PhysicalConstants.Get<T>(definition.Id);
                SameBits(expected, result.Value);
                SameBits(uncertainty, result.StandardUncertainty);
                Assert.True(result.StandardUncertainty > T.Zero);
                Assert.Same(definition, result.Definition);
            }
        }
    }

    private static void SameBits<T>(T expected, T actual) where T : unmanaged
    {
        T[] a = [expected], b = [actual];
        Assert.Equal(MemoryMarshal.AsBytes(a.AsSpan()).ToArray(), MemoryMarshal.AsBytes(b.AsSpan()).ToArray());
    }
}
