// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using MathemaSharp.LinearAlgebra;
using Xunit;

namespace MathemaSharp.LinearAlgebra.Tests;

/// <summary>The dense routines moved from NDSharp (738 numpy.linalg results compared by SHA-256 before and after). These tests check them against values
/// worked out by hand and against the defining equations, with no array library involved.</summary>
public class DenseLinearAlgebraTests
{
    static double[] Matrix(int n, int salt)
    {
        var m = new double[n * n];
        for (int i = 0; i < n; i++) for (int j = 0; j < n; j++) m[i * n + j] = Math.Round(Math.Sin(i * 1.7 + j * 0.9 + salt) * 5 + ((i * 3 + j * 7 + salt) % 4), 3);
        return m;
    }

    [Fact]
    public void LuSolveGivesTheSolutionOfASmallSystem()
    {
        // [[2, 1], [1, 3]] x = [3, 5]  =>  x = [0.8, 1.4]
        var lu = new double[] { 2, 1, 1, 3 };
        var piv = new int[2];
        Assert.False(DenseLinearAlgebra.Lu(lu, 2, piv));
        var b = new double[] { 3, 5 };
        DenseLinearAlgebra.LuSolve(lu, 2, piv, b, 1);
        Assert.Equal(0.8, b[0], 12);
        Assert.Equal(1.4, b[1], 12);
    }

    [Fact]
    public void LuReportsASingularMatrix()
    {
        Assert.True(DenseLinearAlgebra.Lu(new double[] { 1, 2, 2, 4 }, 2, new int[2]));
        Assert.True(DenseLinearAlgebra.Lu(new double[] { 0, 0, 0, 0 }, 2, new int[2]));
    }

    [Fact]
    public void LuSolveInvertsAMatrix()
    {
        const int n = 6;
        var a = Matrix(n, 1);
        var lu = (double[])a.Clone(); var piv = new int[n];
        Assert.False(DenseLinearAlgebra.Lu(lu, n, piv));
        var inv = new double[n * n];
        for (int i = 0; i < n; i++) inv[i * n + i] = 1;
        DenseLinearAlgebra.LuSolve(lu, n, piv, inv, n);
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                double s = 0; for (int k = 0; k < n; k++) s += a[i * n + k] * inv[k * n + j];
                Assert.Equal(i == j ? 1.0 : 0.0, s, 9);
            }
    }

    [Fact]
    public void SignAndLogDeterminant()
    {
        var (sign, log) = DenseLinearAlgebra.SignAndLogDeterminant<double>([1, 2, 3, 4], 2);   // det = −2
        Assert.Equal(-1.0, sign);
        Assert.Equal(Math.Log(2), log, 12);
        var (s0, l0) = DenseLinearAlgebra.SignAndLogDeterminant<double>([1, 2, 2, 4], 2);       // singular
        Assert.Equal(0.0, s0);
        Assert.True(double.IsNegativeInfinity(l0));
        var input = new double[] { 4, 3, 6, 3 };
        DenseLinearAlgebra.SignAndLogDeterminant<double>(input, 2);
        Assert.Equal(new double[] { 4, 3, 6, 3 }, input);                              // not modified
    }

    [Fact]
    public void SymmetricEigenOfAKnownMatrix()
    {
        var (w, v) = DenseLinearAlgebra.SymmetricEigen<double>([2, 1, 1, 2], 2);   // eigenvalues 1 and 3
        Assert.Equal(1.0, w[0], 12);
        Assert.Equal(3.0, w[1], 12);
        // columns are orthonormal eigenvectors
        for (int c = 0; c < 2; c++)
        {
            double a0 = 2 * v[0 * 2 + c] + v[1 * 2 + c], a1 = v[0 * 2 + c] + 2 * v[1 * 2 + c];
            Assert.Equal(w[c] * v[0 * 2 + c], a0, 12);
            Assert.Equal(w[c] * v[1 * 2 + c], a1, 12);
        }
    }

    [Fact]
    public void ThinSvdOfADiagonalMatrixSortsTheSingularValues()
    {
        var (u, s, v) = DenseLinearAlgebra.ThinSvd<double>([3, 0, 0, 4], 2, 2);
        Assert.Equal(4.0, s[0], 12);
        Assert.Equal(3.0, s[1], 12);
        Assert.Equal(2 * 2, u.Length);
        Assert.Equal(2 * 2, v.Length);
    }

    [Fact]
    public void ThinSvdReconstructsATallMatrix()
    {
        const int m = 7, n = 4;
        var a = new double[m * n];
        for (int i = 0; i < m; i++) for (int j = 0; j < n; j++) a[i * n + j] = Math.Round(Math.Cos(i * 0.8 + j * 1.3) * 3 + j, 3);
        var (u, s, v) = DenseLinearAlgebra.ThinSvd(a, m, n);
        for (int i = 0; i < m; i++)
            for (int j = 0; j < n; j++)
            {
                double x = 0; for (int k = 0; k < n; k++) x += u[i * n + k] * s[k] * v[j * n + k];
                Assert.Equal(a[i * n + j], x, 9);
            }
        for (int k = 1; k < n; k++) Assert.True(s[k - 1] >= s[k]);
    }

    [Fact]
    public void CompleteBasisIsOrthogonal()
    {
        var q = DenseLinearAlgebra.CompleteBasis<double>([1, 0, 0], 3, 1);   // one unit column of a 3 × 3 basis
        for (int a = 0; a < 3; a++)
            for (int b = 0; b < 3; b++)
            {
                double d = 0; for (int i = 0; i < 3; i++) d += q[i * 3 + a] * q[i * 3 + b];
                Assert.Equal(a == b ? 1.0 : 0.0, d, 12);
            }
    }
}

public class GeneralEigenTests
{
    static Complex[] AsComplex(params double[] x) => x.Select(v => new Complex(v, 0)).ToArray();

    [Fact]
    public void TriangularMatrixHasItsDiagonalAsEigenvalues()
    {
        var (w, _) = GeneralEigen.Solve(AsComplex(2, 5, 7, 0, 3, 1, 0, 0, 11), 3, false, true);
        Assert.Equal([2.0, 3.0, 11.0], w.Select(z => z.Real).OrderBy(x => x).ToArray(), new Tol());
        Assert.All(w, z => Assert.Equal(0.0, z.Imaginary));
    }

    sealed class Tol : IEqualityComparer<double>
    {
        public bool Equals(double x, double y) => Math.Abs(x - y) < 1e-10;
        public int GetHashCode(double v) => 0;
    }

    [Fact]
    public void RotationHasPurelyImaginaryEigenvalues()
    {
        var (w, _) = GeneralEigen.Solve(AsComplex(0, -1, 1, 0), 2, false, true);
        var imag = w.Select(z => z.Imaginary).OrderBy(x => x).ToArray();
        Assert.Equal(-1.0, imag[0], 10);
        Assert.Equal(1.0, imag[1], 10);
        Assert.All(w, z => Assert.Equal(0.0, z.Real, 10));
    }

    [Fact]
    public void EigenvectorsSatisfyAvEqualsWv()
    {
        const int n = 6;
        var a = new Complex[n * n];
        for (int i = 0; i < n; i++) for (int j = 0; j < n; j++) a[i * n + j] = new Complex(Math.Round(Math.Sin(i * 1.7 + j * 0.9) * 5, 3), Math.Round(Math.Cos(i + 2 * j) * 2, 3));
        var (w, v) = GeneralEigen.Solve(a, n, true, false);
        for (int c = 0; c < n; c++)
            for (int i = 0; i < n; i++)
            {
                Complex av = 0; for (int k = 0; k < n; k++) av += a[i * n + k] * v![k * n + c];
                Assert.True((av - w[c] * v![i * n + c]).Magnitude < 1e-8, $"column {c} row {i}");
            }
    }

    [Fact]
    public void EmptyOneByOneAndZeroMatrices()
    {
        Assert.Empty(GeneralEigen.Solve([], 0, true, true).w);
        Assert.Equal(new Complex(5, 0), GeneralEigen.Solve([new Complex(5, 0)], 1, true, true).w[0]);
        var (w, v) = GeneralEigen.Solve(new Complex[9], 3, true, true);
        Assert.All(w, z => Assert.Equal(Complex.Zero, z));
        Assert.Equal(Complex.One, v![0]);
        Assert.Equal(Complex.One, v[4]);
    }
}

/// <summary>The same routines in other IEEE formats: float and Half keep the iteration convergent (tolerances scale with the epsilon) and give results within the format's precision.</summary>
public class DenseLinearAlgebraGenericTests
{
    static T[] Matrix<T>(int n, int salt) where T : IFloatingPointIeee754<T>
    {
        var m = new T[n * n];
        for (int i = 0; i < n; i++) for (int j = 0; j < n; j++) m[i * n + j] = T.CreateChecked(Math.Round(Math.Sin(i * 1.7 + j * 0.9 + salt) * 5 + ((i * 3 + j * 7 + salt) % 4), 2));
        return m;
    }

    static double Tolerance<T>(double scale) where T : IFloatingPointIeee754<T> => scale * double.CreateChecked(T.BitIncrement(T.One) - T.One);

    [Theory]
    [InlineData(typeof(double))]
    [InlineData(typeof(float))]
    [InlineData(typeof(Half))]
    public void EveryFormat(Type type) => typeof(DenseLinearAlgebraGenericTests).GetMethod(nameof(Run), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.MakeGenericMethod(type).Invoke(null, null);

    static void Run<T>() where T : IFloatingPointIeee754<T>
    {
        // LU solve and inverse (a well-conditioned 4 × 4)
        const int n = 4;
        var a = new T[] { T.CreateChecked(4), T.One, T.Zero, T.CreateChecked(2), T.One, T.CreateChecked(5), T.One, T.Zero, T.Zero, T.One, T.CreateChecked(6), T.One, T.CreateChecked(2), T.Zero, T.One, T.CreateChecked(7) };
        var lu = (T[])a.Clone(); var piv = new int[n];
        Assert.False(DenseLinearAlgebra.Lu(lu, n, piv));
        var inv = new T[n * n];
        for (int i = 0; i < n; i++) inv[i * n + i] = T.One;
        DenseLinearAlgebra.LuSolve(lu, n, piv, inv, n);
        double tol = Tolerance<T>(64);
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                T s = T.Zero; for (int k = 0; k < n; k++) s += a[i * n + k] * inv[k * n + j];
                Assert.True(Math.Abs(double.CreateChecked(s) - (i == j ? 1.0 : 0.0)) < tol, $"{typeof(T).Name} inverse [{i},{j}]");
            }

        // log-determinant of a diagonal matrix
        var diag = new T[] { T.CreateChecked(2), T.Zero, T.Zero, T.CreateChecked(-4) };
        var (sign, log) = DenseLinearAlgebra.SignAndLogDeterminant<T>(diag, 2);
        Assert.Equal(-1.0, double.CreateChecked(sign));
        Assert.True(Math.Abs(double.CreateChecked(log) - Math.Log(8)) < tol, typeof(T).Name);

        // symmetric eigenvalues of [[2,1],[1,2]] are 1 and 3
        var (w, _) = DenseLinearAlgebra.SymmetricEigen(new T[] { T.CreateChecked(2), T.One, T.One, T.CreateChecked(2) }, 2);
        Assert.True(Math.Abs(double.CreateChecked(w[0]) - 1) < tol && Math.Abs(double.CreateChecked(w[1]) - 3) < tol, typeof(T).Name);

        // thin SVD reconstructs a tall matrix, singular values descending
        const int m = 6, c = 3;
        var tall = new T[m * c];
        for (int i = 0; i < m; i++) for (int j = 0; j < c; j++) tall[i * c + j] = T.CreateChecked(Math.Round(Math.Cos(i * 0.8 + j * 1.3) * 3 + j, 2));
        var (u, sv, v) = DenseLinearAlgebra.ThinSvd(tall, m, c);
        for (int i = 0; i < m; i++)
            for (int j = 0; j < c; j++)
            {
                T x = T.Zero; for (int k = 0; k < c; k++) x += u[i * c + k] * sv[k] * v[j * c + k];
                Assert.True(Math.Abs(double.CreateChecked(x) - double.CreateChecked(tall[i * c + j])) < Tolerance<T>(256), $"{typeof(T).Name} svd [{i},{j}]");
            }
        for (int k = 1; k < c; k++) Assert.True(sv[k - 1] >= sv[k]);
    }
}
