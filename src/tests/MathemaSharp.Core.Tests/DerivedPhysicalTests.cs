// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace MathemaSharp.Core.Tests;

public class DerivedPhysicalTests
{
    [Fact]
    public void DerivedMetadataAndDimensionsPreserveFormulaProvenance()
    {
        var definitions = PhysicalConstants.Definitions.Where(x => x.Status == ConstantStatus.ExactDerived).ToArray();
        Assert.Equal(3, definitions.Length);
        Assert.All(definitions, definition =>
        {
            Assert.Equal("0", definition.StandardUncertainty);
            Assert.NotNull(definition.Formula);
            Assert.Contains("Generated", definition.Source);
            Assert.Equal(2022, definition.SourceEdition);
        });
        var gas = PhysicalConstants.Get<double>("molar gas constant").Definition;
        var faraday = PhysicalConstants.Get<double>("Faraday constant").Definition;
        var reduced = PhysicalConstants.Get<double>("reduced Planck constant").Definition;
        Assert.Equal("N_A*k", gas.Formula);
        Assert.Equal("N_A*e", faraday.Formula);
        Assert.Equal("h/(2*pi)", reduced.Formula);
        Assert.Null(gas.GeneratedPrecisionDigits);
        Assert.Null(faraday.GeneratedPrecisionDigits);
        Assert.Equal(100, reduced.GeneratedPrecisionDigits);
        var avogadro = PhysicalConstants.Get<double>("Avogadro constant").Definition;
        Assert.Equal(avogadro.Dimensions + PhysicalConstants.Get<double>("Boltzmann constant").Definition.Dimensions, gas.Dimensions);
        Assert.Equal(avogadro.Dimensions + PhysicalConstants.Get<double>("elementary charge").Definition.Dimensions, faraday.Dimensions);
        Assert.Equal(PhysicalConstants.Get<double>("Planck constant").Definition.Dimensions, reduced.Dimensions);
        Assert.Equal(PhysicalConstants.MolarGas, PhysicalConstants.Get<double>(gas.Id).Value);
        Assert.Equal(PhysicalConstants.Faraday, PhysicalConstants.Get<double>(faraday.Id).Value);
        Assert.Equal(PhysicalConstants.ReducedPlanck, PhysicalConstants.Get<double>(reduced.Id).Value);
    }

    [Fact]
    public void TerminatingExactProductsMatchDecimal128Arithmetic()
    {
        var avogadro = PhysicalConstants.Get<Decimal128>("Avogadro constant").Value;
        Assert.Equal(avogadro * PhysicalConstants.Get<Decimal128>("Boltzmann constant").Value,
            PhysicalConstants.Get<Decimal128>("molar gas constant").Value);
        Assert.Equal(avogadro * PhysicalConstants.Get<Decimal128>("elementary charge").Value,
            PhysicalConstants.Get<Decimal128>("Faraday constant").Value);
    }

    [Fact] public void HalfFormat() => Check<Half>();
    [Fact] public void SingleFormat() => Check<float>();
    [Fact] public void DoubleFormat() => Check<double>();
    [Fact] public void Decimal32Format() => Check<Decimal32>();
    [Fact] public void Decimal64Format() => Check<Decimal64>();
    [Fact] public void Decimal128Format() => Check<Decimal128>();

    private static void Check<T>() where T : unmanaged, IFloatingPointIeee754<T>
    {
        foreach (var definition in PhysicalConstants.Definitions.Where(x => x.Status == ConstantStatus.ExactDerived))
        {
            var expected = T.Parse(definition.DecimalValue, CultureInfo.InvariantCulture);
            if (!T.IsFinite(expected)) Assert.Throws<OverflowException>(() => PhysicalConstants.Get<T>(definition.Id));
            else if (T.IsZero(expected)) Assert.Throws<ArithmeticException>(() => PhysicalConstants.Get<T>(definition.Id));
            else
            {
                var actual = PhysicalConstants.Get<T>(definition.Id);
                T[] a = [expected], b = [actual.Value];
                Assert.Equal(MemoryMarshal.AsBytes(a.AsSpan()).ToArray(), MemoryMarshal.AsBytes(b.AsSpan()).ToArray());
                Assert.Equal(T.Zero, actual.StandardUncertainty);
            }
        }
    }
}
