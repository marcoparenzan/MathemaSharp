// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using MathemaSharp.Core;
using MathemaSharp.Fluids;
using Xunit;

namespace MathemaSharp.Fluids.Tests;

public class FluidMechanicsTests
{
    [Fact]
    public void ReynoldsAndRegime()
    {
        Assert.Equal(2500.0, FluidMechanics.Reynolds(1.0, 0.05, 2e-5), 9);
        Assert.Equal(FlowRegime.Laminar, FluidMechanics.Regime(2299.0));
        Assert.Equal(FlowRegime.Transitional, FluidMechanics.Regime(2300.0));
        Assert.Equal(FlowRegime.Transitional, FluidMechanics.Regime(3999.0));
        Assert.Equal(FlowRegime.Turbulent, FluidMechanics.Regime(4000.0));
    }

    [Fact]
    public void FrictionFactorLaminarTurbulentAndBlend()
    {
        Assert.Equal(0.0, FluidMechanics.DarcyFrictionFactor(0.0, 0.0));
        Assert.Equal(64.0 / 1000, FluidMechanics.DarcyFrictionFactor(1000.0, 0.0), 12);   // Hagen-Poiseuille
        // smooth pipe at Re = 1e5: 0.0180
        Assert.Equal(0.0180, FluidMechanics.DarcyFrictionFactor(1e5, 0.0), 3);
        // the blend joins the laminar value at 2300 and the turbulent one at 4000, linearly
        double atLaminar = FluidMechanics.DarcyFrictionFactor(2300.0, 0.001), atTurbulent = FluidMechanics.DarcyFrictionFactor(4000.0, 0.001);
        double mid = FluidMechanics.DarcyFrictionFactor(3150.0, 0.001);
        Assert.Equal((atLaminar + atTurbulent) / 2, mid, 4);
    }

    [Fact]
    public void DragAndSheddingAndBeaufort()
    {
        Assert.Equal(0.5 * 1.2 * 1.0 * 2.0 * 100.0, FluidMechanics.DragForce(1.2, 1.0, 2.0, 10.0), 12);
        Assert.Equal(0.2 * 10 / 0.5, FluidMechanics.VortexSheddingFrequency(0.2, 10.0, 0.5), 12);
        Assert.Equal(0, FluidMechanics.BeaufortNumber(0.2));
        Assert.Equal(4, FluidMechanics.BeaufortNumber(6.0));
        Assert.Equal(12, FluidMechanics.BeaufortNumber(40.0));
    }

    [Fact]
    public void LaminarPipeFlowHasTheParabolicProfileAndTheExactPressureDrop()
    {
        // water-like fluid, 1 cm pipe, 1 m, 0.5 l/min: laminar, f = 64/Re, centre speed = 2 x mean speed
        var r = PipeFlow.Solve(998.2, 1.004e-6, 0.01, 1.0, 0.5, 0.0);
        Assert.Equal(FlowRegime.Laminar, r.Regime);
        Assert.Equal(64.0 / r.Reynolds, r.FrictionFactor, 12);
        Assert.Equal(2 * r.MeanSpeed, r.ProfileSpeed[20], 12);
        Assert.Equal(0.0, r.ProfileSpeed[0], 12);
        // Hagen-Poiseuille: dp = 32 mu L v / D^2
        Assert.Equal(32 * 998.2 * 1.004e-6 * 1.0 * r.MeanSpeed / (0.01 * 0.01), r.PressureDrop, 6);
        Assert.Equal(r.PressureDrop * 0.5 / 1000 / 60, r.HydraulicPower, 12);
    }

    [Fact]
    public void TurbulentPipeFlowUsesThePowerLaw()
    {
        var r = PipeFlow.Solve(998.2, 1.004e-6, 0.05, 10.0, 600.0, 0.045);
        Assert.Equal(FlowRegime.Turbulent, r.Regime);
        Assert.Equal(60.0 / 49.0 * r.MeanSpeed, r.ProfileSpeed[20], 12);
    }

    [Fact]
    public void WorksInSingleAndHalfPrecision()
    {
        Assert.Equal(0.0180f, FluidMechanics.DarcyFrictionFactor(1e5f, 0f), 3f);
        Assert.Equal(3.125, double.CreateChecked(FluidMechanics.DragForce((Half)1, (Half)1, (Half)1, (Half)2.5)), 0.01);
    }
}

public class AcousticsTests
{
    [Fact]
    public void MidiNotes()
    {
        Assert.Equal(69, Acoustics.MidiNoteNumber(440.0));
        Assert.Equal(60, Acoustics.MidiNoteNumber(261.63));
        Assert.Equal(81, Acoustics.MidiNoteNumber(880.0));
        Assert.Equal(69, Acoustics.MidiNoteNumber(442.0, 442.0));
        Assert.Equal(261.6256, Acoustics.FrequencyOfMidiNote<double>(60), 3);
        Assert.Equal(440.0, Acoustics.FrequencyOfMidiNote<double>(69), 12);
    }
}

public class CavityFlowTests
{
    static CavityFlow<T> Run<T>(int n, int steps) where T : struct, IFloatingPointIeee754<T>
    {
        var flow = new CavityFlow<T>(n, T.CreateChecked(100));
        flow.Run(steps);
        return flow;
    }

    [Fact]
    public void ALidDrivenCavityDevelopsAPrimaryVortex()
    {
        var flow = Run<double>(31, 1500);
        // the fluid under the lid moves with it (positive u) and returns lower down (negative u): the centre-line profile changes sign
        var (_, u) = flow.VerticalCenterline();
        Assert.True(u[^2] > 0.3);
        Assert.Contains(u, v => v < -0.05);
        Assert.True(flow.KineticEnergy() > 0);
        Assert.True(flow.RmsDivergence() < 0.5);
        Assert.Equal(1500, flow.Step);
        Assert.True(flow.EnergyHistory.Count > 10);
    }

    [Fact]
    public void TheWallsStayStillAndTheLidMoves()
    {
        var flow = Run<double>(21, 200);
        for (int k = 1; k < 20; k++) Assert.Equal(1.0, flow.U[20, k]);   // the lid (its two end points belong to the side walls: the boundary loop writes them last)
        for (int k = 0; k < 20; k++)
        {
            Assert.Equal(0.0, flow.U[0, k]); Assert.Equal(0.0, flow.U[k, 0]); Assert.Equal(0.0, flow.U[k, 20]);   // bottom, left and right walls
        }
    }

    [Fact]
    public void SingleAndDoublePrecisionAgree()
    {
        var d = Run<double>(21, 300); var f = Run<float>(21, 300);
        double worst = 0;
        for (int j = 0; j < 21; j++) for (int i = 0; i < 21; i++) worst = Math.Max(worst, Math.Abs(d.U[j, i] - f.U[j, i]));
        Assert.True(worst < 5e-3, $"max difference {worst}");
    }

    [Fact]
    public void ArgumentsAreChecked()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CavityFlow<double>(2));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CavityFlow<double>(10, pressureIterations: 0));
    }

    [Fact]
    public void ACancelledRunStops()
    {
        var flow = new CavityFlow<double>(11);
        using var cts = new CancellationTokenSource(); cts.Cancel();
        Assert.Throws<OperationCanceledException>(() => flow.Run(100, cancellation: cts.Token));
    }
}

public class LatticeBoltzmannTests
{
    [Fact]
    public void TheCylinderIsSolidAndTheWakeShedsVortices()
    {
        var lbm = new LatticeBoltzmannD2Q9<double>(100, 140, 50, 8, 0.08);
        Assert.Contains(lbm.Solid, s => s);
        Assert.False(lbm.Solid[0]);
        lbm.Run(4500);   // the wake needs about 4000 steps to become periodic on this small grid
        Assert.Equal(4500, lbm.Step);
        Assert.Equal(900, lbm.ProbeV.Count);
        var st = lbm.Strouhal();
        Assert.NotNull(st);
        Assert.InRange(st!.Value, 0.1, 0.4);   // St is about 0.2 for a cylinder at Re = 100
        var w = lbm.Vorticity();
        Assert.Contains(w.Cast<double>(), v => Math.Abs(v) > 0.05);
    }

    [Fact]
    public void ArgumentsAreChecked()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LatticeBoltzmannD2Q9<double>(100, 140, 50, 60, 0.08));
        Assert.Throws<ArgumentOutOfRangeException>(() => new LatticeBoltzmannD2Q9<double>(100, 140, 50, 8, 0.5));
        Assert.Throws<ArgumentOutOfRangeException>(() => new LatticeBoltzmannD2Q9<double>(-1, 140, 50, 8, 0.08));
    }

    [Fact]
    public void SingleAndDoublePrecisionStartTheSame()
    {
        var d = new LatticeBoltzmannD2Q9<double>(100, 60, 30, 6, 0.08); var f = new LatticeBoltzmannD2Q9<float>(100f, 60, 30, 6f, 0.08f);
        d.Run(50); f.Run(50);
        var wd = d.Vorticity(); var wf = f.Vorticity();
        double worst = 0;
        for (int y = 0; y < 30; y++) for (int x = 0; x < 60; x++) worst = Math.Max(worst, Math.Abs(wd[y, x] - wf[y, x]));
        Assert.True(worst < 0.05, $"max difference {worst}");
    }
}
