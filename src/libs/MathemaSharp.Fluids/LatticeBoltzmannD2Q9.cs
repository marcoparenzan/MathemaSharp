// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using System.Runtime.InteropServices;
using MathemaSharp.Kernels;

namespace MathemaSharp.Fluids;

/// <summary>Flow past a circular cylinder by the lattice Boltzmann method on the D2Q9 lattice, for every IEEE Generic Math format.
/// Instead of velocity and pressure, packets of particles move on a grid (nine directions), collide and relax towards equilibrium (BGK collision); on scales large
/// compared with the grid their average behaviour obeys the Navier-Stokes equations. Solid cells bounce the packets back (no-slip wall); the flow enters at the left with a uniform
/// speed, leaves freely at the right and is periodic top and bottom. Everything is in lattice units: the Reynolds number Re = U D / ν is the only number that matters.
/// With <c>T = double</c> the results are the ones the original implementation in the Navier-Stokes article produced, bit for bit.</summary>
public sealed class LatticeBoltzmannD2Q9<T> where T : struct, IFloatingPointIeee754<T>
{
    // The 9 directions of the D2Q9 lattice, their weights and the opposite direction of each
    private static readonly int[] Cx = [0, 1, 0, -1, 0, 1, -1, -1, 1];
    private static readonly int[] Cy = [0, 0, 1, 0, -1, 1, 1, -1, -1];
    private static readonly int[] Opp = [0, 3, 4, 1, 2, 7, 8, 5, 6];
    private static readonly T[] CxT = Cx.Select(c => T.CreateChecked(c)).ToArray();
    private static readonly T[] CyT = Cy.Select(c => T.CreateChecked(c)).ToArray();
    private static readonly T[] W =
    [
        T.CreateChecked(4.0 / 9),
        T.CreateChecked(1.0 / 9), T.CreateChecked(1.0 / 9), T.CreateChecked(1.0 / 9), T.CreateChecked(1.0 / 9),
        T.CreateChecked(1.0 / 36), T.CreateChecked(1.0 / 36), T.CreateChecked(1.0 / 36), T.CreateChecked(1.0 / 36),
    ];

    /// <summary>Upper bound for the inlet speed in lattice units (about Mach 0.5); beyond it the scheme is not meaningful.</summary>
    public const double MaxInletSpeed = 0.3;

    public int Nx { get; }
    public int Ny { get; }
    /// <summary>Cylinder diameter in cells.</summary>
    public T Diameter { get; }
    /// <summary>Inlet speed (lattice units).</summary>
    public T InletSpeed { get; }
    public T Reynolds { get; }
    public int Step { get; private set; }
    public T CenterX { get; }
    public T CenterY { get; }
    /// <summary>The cylinder: true in the cells that are solid, row-major (<c>y * Nx + x</c>).</summary>
    public bool[] Solid { get; }

    /// <summary>Transverse velocity at a probe downstream of the cylinder, in units of the inlet speed, every <see cref="ProbeEvery"/> steps.</summary>
    public List<T> ProbeV { get; } = new();
    public int ProbeEvery { get; } = 5;
    /// <summary>Steps during which a brief sideways gust breaks the symmetry (in nature small imperfections do it; a simulation needs a nudge).</summary>
    public int KickSteps { get; } = 600;

    private T[] _f, _ftmp;
    private readonly T _omega;
    private readonly int _probeIndex;

    public LatticeBoltzmannD2Q9(T reynolds, int nx, int ny, T diameter, T inletSpeed)
    {
        if (!(reynolds > T.Zero)) throw new ArgumentOutOfRangeException(nameof(reynolds), reynolds, "Must be greater than zero.");
        if (nx < 3) throw new ArgumentOutOfRangeException(nameof(nx), nx, "Must be at least 3.");
        if (ny < 3) throw new ArgumentOutOfRangeException(nameof(ny), ny, "Must be at least 3.");
        if (!(diameter > T.Zero)) throw new ArgumentOutOfRangeException(nameof(diameter), diameter, "Must be greater than zero.");
        if (!(inletSpeed > T.Zero)) throw new ArgumentOutOfRangeException(nameof(inletSpeed), inletSpeed, "Must be greater than zero.");
        // The cylinder must fit in the channel; lattice Boltzmann is only valid at low lattice Mach number (speed of sound 1/√3), so the inlet speed is capped.
        if (diameter >= T.CreateChecked(ny)) throw new ArgumentOutOfRangeException(nameof(diameter), diameter, "Must be smaller than the channel height ny.");
        if (inletSpeed > T.CreateChecked(MaxInletSpeed)) throw new ArgumentOutOfRangeException(nameof(inletSpeed), inletSpeed, $"Must not exceed {MaxInletSpeed} (lattice units).");
        Nx = nx; Ny = ny; Diameter = diameter; InletSpeed = inletSpeed; Reynolds = reynolds;
        T nu = inletSpeed * diameter / reynolds;     // viscosity that gives the requested Reynolds number
        T tau = T.CreateChecked(3) * nu + T.CreateChecked(0.5);
        _omega = T.One / tau;

        T two = T.CreateChecked(2);
        CenterX = T.CreateChecked(nx) / T.CreateChecked(5.0);
        CenterY = T.CreateChecked(ny) / two + T.CreateChecked(0.5);   // slightly off axis: breaks the symmetry and triggers the vortices
        Solid = new bool[nx * ny];
        T r = diameter / two;
        for (int y = 0; y < ny; y++)
        for (int x = 0; x < nx; x++)
        {
            T dx = T.CreateChecked(x) - CenterX, dy = T.CreateChecked(y) - CenterY;
            Solid[y * nx + x] = dx * dx + dy * dy <= r * r;
        }

        _f = new T[9 * nx * ny];
        _ftmp = new T[9 * nx * ny];
        for (int n = 0; n < nx * ny; n++)
            for (int k = 0; k < 9; k++)
                _f[k * nx * ny + n] = Equilibrium(k, T.One, inletSpeed, T.Zero);

        int px = int.CreateTruncating(CenterX + T.CreateChecked(4) * diameter), py = int.CreateTruncating(CenterY);
        _probeIndex = py * nx + Math.Min(px, nx - 2);
    }

    /// <summary>Equilibrium distribution of direction <paramref name="k"/> for density ρ and velocity (ux, uy).</summary>
    private static T Equilibrium(int k, T rho, T ux, T uy)
    {
        T three = T.CreateChecked(3);
        T cu = three * (CxT[k] * ux + CyT[k] * uy);
        return W[k] * rho * (T.One + cu + T.CreateChecked(0.5) * cu * cu - T.CreateChecked(1.5) * (ux * ux + uy * uy));
    }

    public void Run(int steps, Action<LatticeBoltzmannD2Q9<T>>? progress = null, int progressEvery = 200, CancellationToken cancellation = default)
    {
        if (steps < 0) throw new ArgumentOutOfRangeException(nameof(steps));
        if (progressEvery < 1) throw new ArgumentOutOfRangeException(nameof(progressEvery));
        for (int s = 0; s < steps; s++)
        {
            cancellation.ThrowIfCancellationRequested();
            StepOnce();
            if (progress != null && Step % progressEvery == 0) progress(this);
        }
    }

    public void StepOnce()
    {
        int nx = Nx, ny = Ny, nn = nx * ny;
        var f = _f; var ftmp = _ftmp;

        // 1) Collision (BGK): each cell relaxes towards the local equilibrium. In solid cells the particles bounce back (no-slip wall).
        Parallel.For(0, ny, y =>
        {
            var fl = new T[9];
            for (int x = 0; x < nx; x++)
            {
                int n = y * nx + x;
                for (int k = 0; k < 9; k++) fl[k] = f[k * nn + n];
                if (Solid[n])
                {
                    for (int k = 0; k < 9; k++) f[k * nn + n] = fl[Opp[k]];
                    continue;
                }
                T rho = T.Zero, ux = T.Zero, uy = T.Zero;
                for (int k = 0; k < 9; k++) { rho += fl[k]; ux += fl[k] * CxT[k]; uy += fl[k] * CyT[k]; }
                ux /= rho; uy /= rho;
                for (int k = 0; k < 9; k++)
                    f[k * nn + n] = fl[k] + _omega * (Equilibrium(k, rho, ux, uy) - fl[k]);
            }
        });

        // 2) Streaming: each packet moves to the neighbouring cell along its direction. Periodic top and bottom, inlet on the left, free outlet on the right.
        Parallel.For(0, ny, y =>
        {
            for (int x = 0; x < nx; x++)
            {
                int n = y * nx + x;
                for (int k = 0; k < 9; k++)
                {
                    int xs = x - Cx[k], ys = (y - Cy[k] + ny) % ny;
                    if (xs < 0) xs = 0; else if (xs >= nx) xs = nx - 1;
                    ftmp[k * nn + n] = f[k * nn + ys * nx + xs];
                }
            }
        });
        (_f, _ftmp) = (_ftmp, _f);
        f = _f;

        // 3) Boundary conditions: uniform wind at the inlet, zero gradient at the outlet. At the start a brief sideways gust breaks the symmetry.
        T gust = Step < KickSteps ? T.CreateChecked(0.15) * InletSpeed : T.Zero;
        for (int y = 0; y < ny; y++)
        {
            int nIn = y * nx, nOut = y * nx + nx - 1;
            for (int k = 0; k < 9; k++)
            {
                f[k * nn + nIn] = Equilibrium(k, T.One, InletSpeed, gust);
                f[k * nn + nOut] = f[k * nn + nOut - 1];
            }
        }

        Step++;
        if (Step % ProbeEvery == 0) ProbeV.Add(Velocity(_probeIndex).uy / InletSpeed);
    }

    private (T ux, T uy) Velocity(int n)
    {
        int nn = Nx * Ny;
        T rho = T.Zero, ux = T.Zero, uy = T.Zero;
        for (int k = 0; k < 9; k++) { T v = _f[k * nn + n]; rho += v; ux += v * CxT[k]; uy += v * CyT[k]; }
        return (ux / rho, uy / rho);
    }

    /// <summary>Vorticity (local rotation of the fluid) in units of U/D, indexed <c>[y, x]</c>: positive and negative values are opposite vortices. Zero at the border of the grid.</summary>
    public T[,] Vorticity()
    {
        var ux = new T[Ny, Nx];
        var uy = new T[Ny, Nx];
        for (int y = 0; y < Ny; y++)
        for (int x = 0; x < Nx; x++)
        {
            int n = y * Nx + x;
            if (Solid[n]) continue;
            var (a, b) = Velocity(n);
            ux[y, x] = a; uy[y, x] = b;
        }
        var w = new T[Ny, Nx];
        T twoU = T.CreateChecked(2) * InletSpeed;
        for (int y = 1; y < Ny - 1; y++)
        for (int x = 1; x < Nx - 1; x++)
            w[y, x] = GridStencils.Curl(ux, uy, y, x) / twoU * Diameter;
        return w;
    }

    /// <summary>Vortex shedding frequency from the probe (upward zero crossings after discarding the first half as transient), as the Strouhal number St = f·D/U.
    /// Null if the wake is not yet periodic (fewer than 40 samples or fewer than 3 crossings).</summary>
    public T? Strouhal()
    {
        int skip = ProbeV.Count / 2;               // discard the initial transient
        if (ProbeV.Count - skip < 40) return null;
        var tail = CollectionsMarshal.AsSpan(ProbeV).Slice(skip);
        var mean = new T[1];
        Reductions.Mean<T>(tail, Lane.Flat(tail.Length), mean);
        var crossings = Calculus.UpwardCrossings<T>(tail, mean[0], skip);
        if (Calculus.MeanPeriod<T>(crossings) is not { } spacing) return null;
        T period = spacing * T.CreateChecked(ProbeEvery);
        return Diameter / (period * InletSpeed);
    }
}
