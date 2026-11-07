// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp.Core.Tests;

public class SpecialFunctionsTests
{
    // The oracle is CPython's math module (C99 libm), see scripts/gen_special_functions_oracle.py.
    private static double Rel(double actual, double expected) => expected == 0 ? Math.Abs(actual) : Math.Abs(actual - expected) / Math.Abs(expected);

    [Fact]
    public void ErfAndErfcAgreeWithLibmToAFewUlp()
    {
        double worstErf = 0, worstErfc = 0;
        foreach (var (x, erf, erfc) in SpecialFunctionsOracle.ErfPoints)
        {
            worstErf = Math.Max(worstErf, Rel(SpecialFunctions.Erf(x), erf));
            worstErfc = Math.Max(worstErfc, Rel(SpecialFunctions.Erfc(x), erfc));
        }
        Assert.True(worstErf < 5e-15, $"erf worst relative error {worstErf:E2}");
        Assert.True(worstErfc < 5e-15, $"erfc worst relative error {worstErfc:E2}");
    }

    [Fact]
    public void GammaAgreesWithLibm()
    {
        double worst = 0;
        foreach (var (x, gamma, _) in SpecialFunctionsOracle.GammaPoints)
        {
            var actual = SpecialFunctions.Gamma(x);
            if (double.IsInfinity(gamma)) { Assert.True(double.IsPositiveInfinity(actual), $"gamma({x}) should overflow, got {actual}"); continue; }
            worst = Math.Max(worst, Rel(actual, gamma));
        }
        Assert.True(worst < 2e-15, $"gamma worst relative error {worst:E2}");
    }

    [Fact]
    public void LogAbsGammaAgreesWithLibm()
    {
        double worst = 0;
        foreach (var (x, _, lgamma) in SpecialFunctionsOracle.GammaPoints)
            worst = Math.Max(worst, Math.Abs(SpecialFunctions.LogAbsGamma(x) - lgamma) / Math.Max(1, Math.Abs(lgamma)));
        Assert.True(worst < 5e-15, $"lgamma worst error {worst:E2}");
    }

    [Fact]
    public void GammaOfSmallIntegersIsExactlyTheFactorial()
    {
        double factorial = 1;
        for (var n = 1; n <= 23; n++)
        {
            if (n > 1) factorial *= n - 1;
            Assert.Equal(factorial, SpecialFunctions.Gamma((double)n));
        }
        Assert.Equal(0.0, SpecialFunctions.LogAbsGamma(1.0));
        Assert.Equal(0.0, SpecialFunctions.LogAbsGamma(2.0));
    }

    [Fact]
    public void ExpM1AndLogP1AreAccurateForSmallArguments()
    {
        double worstExp = 0, worstLog = 0;
        foreach (var (x, expected) in SpecialFunctionsOracle.ExpM1Points)
        {
            var actual = SpecialFunctions.ExpM1(x);
            if (double.IsInfinity(expected)) { Assert.True(double.IsPositiveInfinity(actual), $"expm1({x}) should overflow"); continue; }
            worstExp = Math.Max(worstExp, Rel(actual, expected));
        }
        foreach (var (x, expected) in SpecialFunctionsOracle.LogP1Points)
            worstLog = Math.Max(worstLog, Rel(SpecialFunctions.LogP1(x), expected));
        Assert.True(worstExp < 1e-15, $"expm1 worst relative error {worstExp:E2}");
        Assert.True(worstLog < 1e-15, $"log1p worst relative error {worstLog:E2}");
    }

    [Fact]
    public void ExpM1AndLogP1SpecialValuesAndFormats()
    {
        Assert.True(double.IsNegative(SpecialFunctions.ExpM1(-0.0)));
        Assert.True(double.IsNegative(SpecialFunctions.LogP1(-0.0)));
        Assert.Equal(-1.0, SpecialFunctions.ExpM1(double.NegativeInfinity));
        Assert.Equal(double.PositiveInfinity, SpecialFunctions.ExpM1(double.PositiveInfinity));
        Assert.Equal(double.PositiveInfinity, SpecialFunctions.ExpM1(1000.0));
        Assert.Equal(double.NegativeInfinity, SpecialFunctions.LogP1(-1.0));
        Assert.True(double.IsNaN(SpecialFunctions.LogP1(-2.0)));
        Assert.Equal(double.PositiveInfinity, SpecialFunctions.LogP1(double.PositiveInfinity));
        Assert.True(double.IsNaN(SpecialFunctions.ExpM1(double.NaN)));
        Assert.True(double.IsNaN(SpecialFunctions.LogP1(double.NaN)));
        // the point of the function: the naive forms lose everything below the epsilon
        Assert.Equal(0.0, Math.Exp(1e-20) - 1);
        Assert.Equal(1e-20, SpecialFunctions.ExpM1(1e-20));
        Assert.Equal(1e-20f, SpecialFunctions.ExpM1(1e-20f));
        Assert.True(Rel(double.CreateChecked(SpecialFunctions.ExpM1(Decimal64.CreateChecked(1e-12))), 1.0000000000005e-12) < 1e-14);
        Assert.True(Rel(double.CreateChecked(SpecialFunctions.LogP1(Decimal128.CreateChecked(1e-12))), 9.999999999995e-13) < 1e-14);
    }

    [Fact]
    public void SpecialValues()
    {
        Assert.Equal(1.0, SpecialFunctions.Erf(double.PositiveInfinity));
        Assert.Equal(-1.0, SpecialFunctions.Erf(double.NegativeInfinity));
        Assert.True(double.IsNegative(SpecialFunctions.Erf(-0.0)));
        Assert.Equal(0.0, SpecialFunctions.Erfc(double.PositiveInfinity));
        Assert.Equal(2.0, SpecialFunctions.Erfc(double.NegativeInfinity));
        Assert.True(double.IsNaN(SpecialFunctions.Erf(double.NaN)));
        Assert.True(double.IsNaN(SpecialFunctions.Erfc(double.NaN)));
        Assert.Equal(double.PositiveInfinity, SpecialFunctions.Gamma(double.PositiveInfinity));
        Assert.True(double.IsNaN(SpecialFunctions.Gamma(double.NegativeInfinity)));
        Assert.True(double.IsNaN(SpecialFunctions.Gamma(0.0)));
        Assert.True(double.IsNaN(SpecialFunctions.Gamma(-3.0)));
        Assert.True(double.IsNaN(SpecialFunctions.Gamma(double.NaN)));
        Assert.Equal(double.PositiveInfinity, SpecialFunctions.LogAbsGamma(0.0));
        Assert.Equal(double.PositiveInfinity, SpecialFunctions.LogAbsGamma(-2.0));
        Assert.Equal(double.PositiveInfinity, SpecialFunctions.LogAbsGamma(double.NegativeInfinity));
        Assert.True(double.IsNaN(SpecialFunctions.LogAbsGamma(double.NaN)));
    }

    [Fact]
    public void ErfIsOddAndErfcIsItsComplement()
    {
        for (var x = -6.0; x <= 6.0; x += 0.37)
        {
            Assert.Equal(-SpecialFunctions.Erf(x), SpecialFunctions.Erf(-x));
            Assert.True(Math.Abs(SpecialFunctions.Erf(x) + SpecialFunctions.Erfc(x) - 1) < 2e-15, $"erf + erfc at {x}");
        }
    }

    [Fact]
    public void GammaSatisfiesTheRecurrenceAndTheReflection()
    {
        for (var x = 0.3; x < 60; x += 1.7)
            Assert.True(Rel(SpecialFunctions.Gamma(x + 1), x * SpecialFunctions.Gamma(x)) < 5e-15, $"Γ(x+1) = xΓ(x) at {x}");
        for (var x = 0.05; x < 1; x += 0.07)
            Assert.True(Rel(SpecialFunctions.Gamma(x) * SpecialFunctions.Gamma(1 - x), Math.PI / Math.Sin(Math.PI * x)) < 1e-14, $"reflection at {x}");
    }

    [Fact]
    public void SinglePrecisionIsAccurateToFloat()
    {
        foreach (var (x, erf, erfc) in SpecialFunctionsOracle.ErfPoints.Where(p => Math.Abs(p.X) < 8))
        {
            Assert.True(Math.Abs(SpecialFunctions.Erf((float)x) - erf) < 2e-6, $"erf<float>({x})");
            if (erfc > 1e-30) Assert.True(Rel(SpecialFunctions.Erfc((float)x), erfc) < 2e-5 * Math.Max(1, Math.Abs(x)), $"erfc<float>({x})");
        }
        foreach (var (x, gamma, _) in SpecialFunctionsOracle.GammaPoints.Where(p => p.X > 0.5 && p.X < 30 && !double.IsInfinity(p.Gamma)))
            Assert.True(Rel(SpecialFunctions.Gamma((float)x), gamma) < 1e-6 * Math.Max(1, x), $"Γ<float>({x}) relative error {Rel(SpecialFunctions.Gamma((float)x), gamma):E2}");
    }

    [Fact]
    public void DecimalFormatsGiveTheSameValues()
    {
        foreach (var (x, erf, erfc) in SpecialFunctionsOracle.ErfPoints.Where(p => Math.Abs(p.X) < 6 && Math.Abs(p.X) > 1e-6))
        {
            Assert.True(Rel(double.CreateChecked(SpecialFunctions.Erf(Decimal64.CreateChecked(x))), erf) < 1e-14, $"erf<Decimal64>({x})");
            Assert.True(Rel(double.CreateChecked(SpecialFunctions.Erfc(Decimal128.CreateChecked(x))), erfc) < 1e-14, $"erfc<Decimal128>({x})");
        }
        foreach (var (x, gamma, lgamma) in SpecialFunctionsOracle.GammaPoints.Where(p => p.X > 0.1 && p.X < 100 && p.X != Math.Floor(p.X)))
        {
            Assert.True(Rel(double.CreateChecked(SpecialFunctions.Gamma(Decimal64.CreateChecked(x))), gamma) < 2e-14, $"Γ<Decimal64>({x})");
            Assert.True(Math.Abs(double.CreateChecked(SpecialFunctions.LogAbsGamma(Decimal128.CreateChecked(x))) - lgamma) / Math.Max(1, Math.Abs(lgamma)) < 1e-14, $"lgamma<Decimal128>({x})");
        }
    }
}
