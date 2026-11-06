// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using MathemaSharp.Kernels;
using MathemaSharp.LinearAlgebra;

namespace MathemaSharp.Fluids;

/// <summary>Two-dimensional incompressible Navier-Stokes flow in a square cavity whose top side slides (the lid-driven cavity), for every IEEE Generic Math format.
/// <code>∂u/∂t + (u·∇)u = −(1/ρ) ∇p + ν ∇²u        ∇·u = 0</code>
/// Method: finite differences on a regular grid and Chorin's projection method. From the incompressibility constraint a Poisson equation for the pressure is derived
/// and solved with <see cref="GridPoisson.Sor{T}"/>; then the velocity is advanced with the pressure gradient, the viscous term and the convection
/// (<see cref="GridStencils.HybridAdvection{T}"/>). The grid operators come from <see cref="GridStencils"/>, the stream function from <see cref="Calculus"/>, sums from
/// <see cref="Reductions"/>. Fields are indexed <c>[j, i]</c>: <c>j</c> = row (y axis, 0 at the bottom), <c>i</c> = column (x axis). With <c>T = double</c> the
/// results are the ones the original implementation in the Navier-Stokes article produced, bit for bit.</summary>
public sealed class CavityFlow<T> where T : struct, IFloatingPointIeee754<T>
{
    /// <summary>Points per side.</summary>
    public int N { get; }
    /// <summary>Side of the cavity.</summary>
    public T L { get; }
    /// <summary>Speed of the sliding lid.</summary>
    public T LidSpeed { get; }
    /// <summary>Kinematic viscosity, from the Reynolds number: ν = U L / Re.</summary>
    public T Nu { get; }
    public T Rho { get; }
    public T Dt { get; }
    public T Dx { get; }
    public T Reynolds => LidSpeed * L / Nu;

    public T[,] U { get; }
    public T[,] V { get; }
    public T[,] P { get; }

    public T Time { get; private set; } = T.Zero;
    public int Step { get; private set; }

    /// <summary>Total kinetic energy every 20 steps and at the end of a run.</summary>
    public List<(T Time, T Energy)> EnergyHistory { get; } = new();

    private readonly int _pressureIterations;
    private readonly T[,] _un, _vn, _b;
    private readonly T _omega, _peclet;

    /// <param name="n">Points per side (at least 3).</param>
    /// <param name="reynolds">Reynolds number U L / ν.</param>
    /// <param name="lidSpeed">Speed of the lid.</param>
    /// <param name="length">Side of the cavity.</param>
    /// <param name="rho">Density.</param>
    /// <param name="dt">Time step; by default the largest that respects both the diffusive (0.2 dx²/ν) and the convective (CFL, 0.2 dx/U) stability limits.</param>
    /// <param name="pressureIterations">Sweeps of the pressure solver per time step.</param>
    public CavityFlow(int n = 81, T? reynolds = null, T? lidSpeed = null, T? length = null, T? rho = null, T? dt = null, int pressureIterations = 20)
    {
        if (n < 3) throw new ArgumentOutOfRangeException(nameof(n), n, "Must be at least 3.");
        if (pressureIterations < 1) throw new ArgumentOutOfRangeException(nameof(pressureIterations), pressureIterations, "Must be at least 1.");
        T re = reynolds ?? T.CreateChecked(100), lid = lidSpeed ?? T.One, len = length ?? T.One, density = rho ?? T.One;
        N = n; L = len; LidSpeed = lid; Rho = density;
        Nu = lid * len / re;
        Dx = len / T.CreateChecked(n - 1);
        T fifth = T.CreateChecked(0.2);
        // Time step: respects both diffusive and convective stability (CFL)
        Dt = dt ?? T.Min(fifth * Dx * Dx / Nu, fifth * Dx / lid);
        _pressureIterations = pressureIterations;
        _omega = T.CreateChecked(1.7);
        _peclet = T.CreateChecked(2);

        U = new T[n, n]; V = new T[n, n]; P = new T[n, n];
        _un = new T[n, n]; _vn = new T[n, n]; _b = new T[n, n];
        ApplyVelocityBoundaries();
    }

    /// <summary>Advances until steady state (the velocity change per unit of time below <paramref name="tolerance"/>, after at least 100 steps) or at most <paramref name="maxSteps"/> steps.</summary>
    public void Run(int maxSteps = 40_000, T? tolerance = null, Action<CavityFlow<T>>? progress = null, int progressEvery = 1000, CancellationToken cancellation = default)
    {
        if (maxSteps < 0) throw new ArgumentOutOfRangeException(nameof(maxSteps));
        if (progressEvery < 1) throw new ArgumentOutOfRangeException(nameof(progressEvery));
        T tol = tolerance ?? T.CreateChecked(1e-6);
        for (int s = 0; s < maxSteps; s++)
        {
            cancellation.ThrowIfCancellationRequested();
            T change = StepOnce();
            if (Step % 20 == 0) EnergyHistory.Add((Time, KineticEnergy()));
            if (progress != null && Step % progressEvery == 0) progress(this);
            if (Step > 100 && change / Dt < tol) break;
        }
        EnergyHistory.Add((Time, KineticEnergy()));
    }

    /// <summary>One time step. Returns the maximum change in velocity.</summary>
    public T StepOnce()
    {
        int n = N;
        T dx = Dx, dy = Dx, dt = Dt, rho = Rho, nu = Nu;
        T two = T.CreateChecked(2), twoDx = two * dx, twoDy = two * dy;
        Array.Copy(U, _un, U.Length);
        Array.Copy(V, _vn, V.Length);

        // 1) Right-hand side of the Poisson equation: forces the divergence to zero
        for (int j = 1; j < n - 1; j++)
        for (int i = 1; i < n - 1; i++)
        {
            T dudx = GridStencils.CentralX(_un, j, i, twoDx);
            T dvdy = GridStencils.CentralY(_vn, j, i, twoDy);
            T dudy = GridStencils.CentralY(_un, j, i, twoDy);
            T dvdx = GridStencils.CentralX(_vn, j, i, twoDx);
            _b[j, i] = rho * ((dudx + dvdy) / dt - dudx * dudx - two * dudy * dvdx - dvdy * dvdy);
        }

        // 2) Poisson for the pressure by successive over-relaxation; walls: zero pressure gradient, at the top the reference pressure 0
        GridPoisson.Sor(P, _b, dx, dy, _omega, _pressureIterations, ApplyPressureBoundaries);

        // 3) Momentum: convection + pressure gradient + viscous diffusion
        T dx2 = dx * dx, dy2 = dy * dy, maxChange = T.Zero;
        for (int j = 1; j < n - 1; j++)
        for (int i = 1; i < n - 1; i++)
        {
            T u = _un[j, i], v = _vn[j, i];
            T lapU = GridStencils.Laplacian(_un, j, i, dx2, dy2);
            T lapV = GridStencils.Laplacian(_vn, j, i, dx2, dy2);
            T convU = GridStencils.HybridAdvection(_un, j, i, u, v, dx, dy, nu, _peclet);
            T convV = GridStencils.HybridAdvection(_vn, j, i, u, v, dx, dy, nu, _peclet);
            T dpdx = GridStencils.CentralX(P, j, i, twoDx);
            T dpdy = GridStencils.CentralY(P, j, i, twoDy);

            U[j, i] = u + dt * (-convU - dpdx / rho + nu * lapU);
            V[j, i] = v + dt * (-convV - dpdy / rho + nu * lapV);

            maxChange = T.Max(maxChange, T.Max(T.Abs(U[j, i] - u), T.Abs(V[j, i] - v)));
        }

        ApplyVelocityBoundaries();
        Time += dt;
        Step++;
        return maxChange;
    }

    private void ApplyPressureBoundaries(T[,] p)
    {
        int n = N;
        for (int k = 0; k < n; k++)
        {
            p[k, n - 1] = p[k, n - 2];
            p[k, 0] = p[k, 1];
            p[0, k] = p[1, k];
            p[n - 1, k] = T.Zero;
        }
    }

    private void ApplyVelocityBoundaries()
    {
        int n = N;
        for (int k = 0; k < n; k++)
        {
            U[0, k] = T.Zero; V[0, k] = T.Zero;            // bottom
            U[k, 0] = T.Zero; V[k, 0] = T.Zero;            // left
            U[k, n - 1] = T.Zero; V[k, n - 1] = T.Zero;    // right
            U[n - 1, k] = LidSpeed; V[n - 1, k] = T.Zero;  // sliding lid
        }
    }

    /// <summary>Total kinetic energy ½ ρ Σ (u² + v²) dx², the sum taken row by row with <see cref="Reductions.Sum{T}"/>.</summary>
    public T KineticEnergy()
    {
        var squares = new T[N * N];
        int p = 0;
        for (int j = 0; j < N; j++)
            for (int i = 0; i < N; i++, p++) squares[p] = U[j, i] * U[j, i] + V[j, i] * V[j, i];
        var sum = new T[1];
        Reductions.Sum<T>(squares, Lane.Flat(squares.Length), sum);
        return T.CreateChecked(0.5) * Rho * sum[0] * Dx * Dx;
    }

    /// <summary>Magnitude of the velocity at every point.</summary>
    public T[,] Speed()
    {
        var s = new T[N, N];
        for (int j = 0; j < N; j++)
            for (int i = 0; i < N; i++) s[j, i] = T.Sqrt(U[j, i] * U[j, i] + V[j, i] * V[j, i]);
        return s;
    }

    /// <summary>Stream function ψ: its level curves are the streamlines (the cumulative trapezoid integral of u up the columns).</summary>
    public T[,] StreamFunction() => Calculus.CumulativeTrapezoidAlongRows(U, Dx);

    /// <summary>Profile of u along the vertical x = L/2, in units of the lid speed, with y in units of L (for the comparison with Ghia et al.).</summary>
    public (T[] Y, T[] U) VerticalCenterline()
    {
        T x = L / T.CreateChecked(2);
        T fi = x / Dx;
        int i0 = Math.Min(int.CreateTruncating(T.Floor(fi)), N - 2);
        T w = fi - T.CreateChecked(i0);
        var ys = new T[N];
        var us = new T[N];
        for (int j = 0; j < N; j++)
        {
            ys[j] = T.CreateChecked(j) * Dx / L;
            us[j] = ((T.One - w) * U[j, i0] + w * U[j, i0 + 1]) / LidSpeed;
        }
        return (ys, us);
    }

    /// <summary>Mean divergence (root mean square), dimensionless (· L/U): how well ∇·u = 0 is respected. The rows under the lid are excluded: at the top corners the velocity
    /// jumps from 0 to U, a singularity of the problem itself, not of the method.</summary>
    public T RmsDivergence() => GridStencils.RmsDivergence(U, V, Dx, Dx, 1, N - 3, 1, N - 1) * L / LidSpeed;
}
