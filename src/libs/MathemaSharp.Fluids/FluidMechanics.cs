// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp.Fluids;

/// <summary>The flow regime of a pipe flow, from its Reynolds number.</summary>
public enum FlowRegime
{
    /// <summary>Ordered flow in layers (Reynolds number below the laminar limit, 2300 by default).</summary>
    Laminar,
    /// <summary>Unstable flow between the laminar and the turbulent limit (2300 to 4000 by default).</summary>
    Transitional,
    /// <summary>Vortices and mixing (Reynolds number above the turbulent limit, 4000 by default).</summary>
    Turbulent,
}

/// <summary>Scalar correlations of classical fluid mechanics, for every IEEE Generic Math format: Reynolds number and flow regime, the Darcy friction factor (Hagen-Poiseuille
/// for laminar flow, Haaland for turbulent flow, a linear blend in between), Darcy-Weisbach pressure drop, the drag force of a body in a stream, the vortex-shedding
/// frequency and the Beaufort number of a wind speed. They are empirical or closed-form results calibrated on measurements and on simulations of the Navier-Stokes
/// equations. Arguments are the caller's to validate (no exceptions for negative or non-finite physical values).</summary>
public static class FluidMechanics
{
    /// <summary>Reynolds number Re = U L / ν (speed × length / kinematic viscosity): inertia against friction.</summary>
    public static T Reynolds<T>(T speed, T length, T kinematicViscosity) where T : IFloatingPointIeee754<T> => speed * length / kinematicViscosity;

    /// <summary>The regime of a flow at Reynolds number <paramref name="re"/>.</summary>
    public static FlowRegime Regime<T>(T re, T laminarLimit, T turbulentLimit) where T : IFloatingPointIeee754<T>
        => re < laminarLimit ? FlowRegime.Laminar : re < turbulentLimit ? FlowRegime.Transitional : FlowRegime.Turbulent;

    /// <summary>The regime with the usual limits 2300 and 4000.</summary>
    public static FlowRegime Regime<T>(T re) where T : IFloatingPointIeee754<T> => Regime(re, T.CreateChecked(2300), T.CreateChecked(4000));

    /// <summary>Haaland's explicit approximation of the turbulent Darcy friction factor: (−1.8 log10((ε/3.7)^1.11 + 6.9/Re))⁻².</summary>
    public static T HaalandFrictionFactor<T>(T re, T relativeRoughness) where T : IFloatingPointIeee754<T>
    {
        T term = T.Pow(relativeRoughness / T.CreateChecked(3.7), T.CreateChecked(1.11)) + T.CreateChecked(6.9) / re;
        return T.Pow(T.CreateChecked(-1.8) * T.Log10(term), T.CreateChecked(-2));
    }

    /// <summary>Darcy friction factor of a pipe: 64/Re when laminar (Re &lt; 2300), Haaland's formula when turbulent (Re &gt; 4000), a linear blend of the two end values between;
    /// zero for Re ≤ 0. <paramref name="relativeRoughness"/> is the wall roughness over the diameter.</summary>
    public static T DarcyFrictionFactor<T>(T re, T relativeRoughness) where T : IFloatingPointIeee754<T>
    {
        if (re <= T.Zero) return T.Zero;
        T sixtyFour = T.CreateChecked(64), laminarLimit = T.CreateChecked(2300), turbulentLimit = T.CreateChecked(4000);
        T lam = sixtyFour / re;
        T haaland = HaalandFrictionFactor(re, relativeRoughness);
        if (re < laminarLimit) return lam;
        if (re > turbulentLimit) return haaland;
        T t = (re - laminarLimit) / T.CreateChecked(1700);   // transition: linear blend
        return (T.One - t) * (sixtyFour / T.CreateChecked(2300.0)) + t * HaalandFrictionFactor(turbulentLimit, relativeRoughness);
    }

    /// <summary>Darcy-Weisbach pressure drop Δp = f (L/D) ρ v² / 2.</summary>
    public static T DarcyPressureDrop<T>(T frictionFactor, T length, T diameter, T density, T meanSpeed) where T : IFloatingPointIeee754<T>
        => frictionFactor * length / diameter * density * meanSpeed * meanSpeed / T.CreateChecked(2);

    /// <summary>Drag force F = ½ ρ Cd A v² of a body of frontal area <paramref name="area"/> in a stream of speed <paramref name="speed"/>.</summary>
    public static T DragForce<T>(T density, T dragCoefficient, T area, T speed) where T : IFloatingPointIeee754<T>
        => T.CreateChecked(0.5) * density * dragCoefficient * area * speed * speed;

    /// <summary>Frequency f = St U / D at which vortices are shed behind a cylinder of diameter D in a stream of speed U (the Strouhal number St is about 0.2 over a huge range of Reynolds numbers).</summary>
    public static T VortexSheddingFrequency<T>(T strouhal, T speed, T diameter) where T : IFloatingPointIeee754<T> => strouhal * speed / diameter;

    /// <summary>The Beaufort number (0 to 12) of a wind speed in m/s.</summary>
    public static int BeaufortNumber<T>(T speed) where T : IFloatingPointIeee754<T>
    {
        ReadOnlySpan<double> upper = [0.5, 1.6, 3.4, 5.5, 8.0, 10.8, 13.9, 17.2, 20.8, 24.5, 28.5, 32.7];
        for (int k = 0; k < upper.Length; k++)
            if (speed < T.CreateChecked(upper[k])) return k;
        return 12;
    }
}

/// <summary>The result of <see cref="PipeFlow.Solve{T}"/>.</summary>
public sealed record PipeFlowResult<T>(T MeanSpeed, T Reynolds, FlowRegime Regime, T FrictionFactor, T PressureDrop, T HydraulicPower, T[] ProfileRadius, T[] ProfileSpeed)
    where T : IFloatingPointIeee754<T>;

/// <summary>Steady flow in a straight circular pipe.</summary>
public static class PipeFlow
{
    /// <summary>Mean speed, Reynolds number, friction factor, pressure drop and hydraulic power of a flow of <paramref name="flowLitresPerMinute"/> in a pipe of diameter
    /// <paramref name="diameter"/> (m), length <paramref name="length"/> (m) and wall roughness <paramref name="roughnessMm"/> (mm), for a fluid of the given density and kinematic
    /// viscosity, with the speed profile across the section at <paramref name="profilePoints"/> points from wall to wall: the exact parabola of Hagen-Poiseuille when laminar,
    /// the 1/7 power law when not.</summary>
    public static PipeFlowResult<T> Solve<T>(T density, T kinematicViscosity, T diameter, T length, T flowLitresPerMinute, T roughnessMm, int profilePoints = 41)
        where T : IFloatingPointIeee754<T>
    {
        if (profilePoints < 2) throw new ArgumentOutOfRangeException(nameof(profilePoints), profilePoints, "Must be at least 2.");
        T thousand = T.CreateChecked(1000), sixty = T.CreateChecked(60), two = T.CreateChecked(2), four = T.CreateChecked(4);
        T q = flowLitresPerMinute / thousand / sixty;                  // m³/s
        T area = T.Pi * diameter * diameter / four;
        T v = q / area;
        T re = FluidMechanics.Reynolds(v, diameter, kinematicViscosity);
        T f = FluidMechanics.DarcyFrictionFactor(re, roughnessMm / thousand / diameter);
        T dp = FluidMechanics.DarcyPressureDrop(f, length, diameter, density, v);

        var r = new T[profilePoints];
        var u = new T[profilePoints];
        T laminarLimit = T.CreateChecked(2300), sixtyOverFortyNine = T.CreateChecked(60.0 / 49.0), seventh = T.CreateChecked(1.0 / 7);
        T span = T.CreateChecked(profilePoints - 1);
        for (int k = 0; k < profilePoints; k++)
        {
            T rr = -T.One + two * T.CreateChecked(k) / span;           // from wall to wall, −1..1
            r[k] = rr;
            u[k] = re < laminarLimit ? two * v * (T.One - rr * rr)
                                     : v * sixtyOverFortyNine * T.Pow(T.Max(T.Zero, T.One - T.Abs(rr)), seventh);
        }
        return new PipeFlowResult<T>(v, re, FluidMechanics.Regime(re), f, dp, dp * q, r, u);
    }
}
