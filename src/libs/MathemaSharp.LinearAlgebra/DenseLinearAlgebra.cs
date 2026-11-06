// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp.LinearAlgebra;

/// <summary>Dense real linear algebra on flat row-major <c>T[]</c> matrices, for every IEEE Generic Math format: the routines behind <c>numpy.linalg</c> in NDSharp,
/// with no dependency on any array type. Moved from NDSharp on 2026-11-06 and made generic; with <c>T = double</c> the arithmetic is the one NDSharp had (738
/// results compared by SHA-256 before and after). Complex and non-symmetric eigenproblems are in <see cref="GeneralEigen"/>.</summary>
/// <remarks>Tolerances written for double (1e-300, 1e-15, 1e-12) are kept as they are for any format at least as precise as double and scaled by the machine
/// epsilon for narrower formats (float, Half), so the same iteration stops where it should for each.</remarks>
public static class DenseLinearAlgebra
{
    private const double DoubleEpsilon = 2.220446049250313e-16;

    private static T Eps<T>() where T : IFloatingPointIeee754<T> => T.BitIncrement(T.One) - T.One;

    /// <summary>A tolerance given for double: as written when <typeparamref name="T"/> is at least as precise, scaled by the epsilon of <typeparamref name="T"/> otherwise.</summary>
    private static T Tol<T>(double literal) where T : IFloatingPointIeee754<T>
    {
        var eps = Eps<T>();
        return eps <= T.CreateTruncating(DoubleEpsilon) ? T.CreateTruncating(literal) : T.CreateTruncating(literal / DoubleEpsilon) * eps;
    }

    /// <summary>The smallest magnitude treated as non-zero: 1e-300 for double, the smallest positive value for formats whose range does not reach it.</summary>
    private static T Tiny<T>() where T : IFloatingPointIeee754<T>
    {
        var t = T.CreateTruncating(1e-300);
        return t > T.Zero ? t : T.Epsilon;
    }

    /// <summary>The sign of a value as Math.Sign gives it (−1, +1; 0 only for a zero), throwing <see cref="ArithmeticException"/> for NaN like Math.Sign.</summary>
    private static T Sign<T>(T x) where T : IFloatingPointIeee754<T>
    {
        if (T.IsNaN(x)) throw new ArithmeticException("Function does not accept floating point Not-a-Number values.");
        return x > T.Zero ? T.One : x < T.Zero ? -T.One : T.Zero;
    }

    /// <summary>LU with partial pivoting (LAPACK dgetf2 order). Returns the pivot rows and whether the matrix is singular.</summary>
    public static bool Lu<T>(T[] a, int n, int[] piv) where T : IFloatingPointIeee754<T>
    {
        bool singular = false;
        for (int j = 0; j < n; j++)
        {
            int p = j;
            T max = T.Abs(a[j * n + j]);
            for (int i = j + 1; i < n; i++)
                if (T.Abs(a[i * n + j]) > max) { max = T.Abs(a[i * n + j]); p = i; }
            piv[j] = p;
            if (a[p * n + j] != T.Zero)
            {
                if (p != j)
                    for (int k = 0; k < n; k++) (a[j * n + k], a[p * n + k]) = (a[p * n + k], a[j * n + k]);
                T pivot = a[j * n + j];
                for (int i = j + 1; i < n; i++) a[i * n + j] /= pivot;
            }
            else singular = true;
            for (int i = j + 1; i < n; i++)
            {
                T l = a[i * n + j];
                if (l != T.Zero)
                    for (int k = j + 1; k < n; k++) a[i * n + k] -= l * a[j * n + k];
            }
        }
        return singular;
    }

    /// <summary>Solves with an LU factorization in place on <paramref name="b"/> (n × m, row-major).</summary>
    public static void LuSolve<T>(T[] lu, int n, int[] piv, T[] b, int m) where T : IFloatingPointIeee754<T>
    {
        for (int j = 0; j < n; j++)
            if (piv[j] != j)
                for (int k = 0; k < m; k++) (b[j * m + k], b[piv[j] * m + k]) = (b[piv[j] * m + k], b[j * m + k]);
        for (int i = 0; i < n; i++)
            for (int k = 0; k < i; k++)
            {
                T l = lu[i * n + k];
                if (l != T.Zero) for (int c = 0; c < m; c++) b[i * m + c] -= l * b[k * m + c];
            }
        for (int i = n - 1; i >= 0; i--)
        {
            for (int k = i + 1; k < n; k++)
            {
                T u = lu[i * n + k];
                if (u != T.Zero) for (int c = 0; c < m; c++) b[i * m + c] -= u * b[k * m + c];
            }
            T d = lu[i * n + i];
            for (int c = 0; c < m; c++) b[i * m + c] /= d;
        }
    }

    /// <summary>Symmetric eigendecomposition by cyclic Jacobi rotations: ascending eigenvalues, orthonormal
    /// column eigenvectors. (LAPACK's eigenvector signs are an implementation detail we do not reproduce.)</summary>
    public static (T[] w, T[] v) SymmetricEigen<T>(T[] a, int n) where T : IFloatingPointIeee754<T>
    {
        var m = (T[])a.Clone();
        var v = new T[n * n];
        for (int i = 0; i < n; i++) v[i * n + i] = T.One;
        T tiny = Tiny<T>(), two = T.CreateChecked(2), signTol = Tol<T>(1e-12);
        for (int sweep = 0; sweep < 100; sweep++)
        {
            T off = T.Zero;
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++) off += m[i * n + j] * m[i * n + j];
            if (off < tiny) break;
            for (int p = 0; p < n - 1; p++)
                for (int q = p + 1; q < n; q++)
                {
                    T apq = m[p * n + q];
                    if (T.Abs(apq) < tiny) continue;
                    T theta = (m[q * n + q] - m[p * n + p]) / (two * apq);
                    T t = Sign(theta == T.Zero ? T.One : theta) / (T.Abs(theta) + T.Sqrt(theta * theta + T.One));
                    T c = T.One / T.Sqrt(t * t + T.One), s = t * c;
                    for (int k = 0; k < n; k++)
                    {
                        T akp = m[k * n + p], akq = m[k * n + q];
                        m[k * n + p] = c * akp - s * akq;
                        m[k * n + q] = s * akp + c * akq;
                    }
                    for (int k = 0; k < n; k++)
                    {
                        T apk = m[p * n + k], aqk = m[q * n + k];
                        m[p * n + k] = c * apk - s * aqk;
                        m[q * n + k] = s * apk + c * aqk;
                    }
                    for (int k = 0; k < n; k++)
                    {
                        T vkp = v[k * n + p], vkq = v[k * n + q];
                        v[k * n + p] = c * vkp - s * vkq;
                        v[k * n + q] = s * vkp + c * vkq;
                    }
                }
        }
        var order = Enumerable.Range(0, n).OrderBy(i => m[i * n + i]).ToArray();
        var w = order.Select(i => m[i * n + i]).ToArray();
        var vs = new T[n * n];
        for (int j = 0; j < n; j++)
        {
            int src = order[j];
            // Sign convention: the component of largest magnitude is positive.
            int big = 0;
            for (int i = 1; i < n; i++) if (T.Abs(v[i * n + src]) > T.Abs(v[big * n + src]) + signTol) big = i;
            T sgn = v[big * n + src] < T.Zero ? -T.One : T.One;
            for (int i = 0; i < n; i++) vs[i * n + j] = sgn * v[i * n + src];
        }
        return (w, vs);
    }

    /// <summary>Thin SVD of an m×n matrix (m ≥ n) by one-sided Jacobi; returns U (m×n), s (n), V (n×n), singular values descending.</summary>
    public static (T[] u, T[] s, T[] v) ThinSvd<T>(T[] a, int m, int n) where T : IFloatingPointIeee754<T>
    {
        var u = (T[])a.Clone();
        var v = new T[n * n];
        for (int i = 0; i < n; i++) v[i * n + i] = T.One;
        T skip = Tol<T>(1e-15), two = T.CreateChecked(2);
        for (int sweep = 0; sweep < 60; sweep++)
        {
            bool rotated = false;
            for (int p = 0; p < n - 1; p++)
                for (int q = p + 1; q < n; q++)
                {
                    T alpha = T.Zero, beta = T.Zero, gamma = T.Zero;
                    for (int i = 0; i < m; i++)
                    {
                        alpha += u[i * n + p] * u[i * n + p];
                        beta += u[i * n + q] * u[i * n + q];
                        gamma += u[i * n + p] * u[i * n + q];
                    }
                    if (T.Abs(gamma) <= skip * T.Sqrt(alpha * beta) || gamma == T.Zero) continue;
                    rotated = true;
                    T zeta = (beta - alpha) / (two * gamma);
                    T t = Sign(zeta == T.Zero ? T.One : zeta) / (T.Abs(zeta) + T.Sqrt(T.One + zeta * zeta));
                    T c = T.One / T.Sqrt(T.One + t * t), s = c * t;
                    for (int i = 0; i < m; i++)
                    {
                        T up = u[i * n + p], uq = u[i * n + q];
                        u[i * n + p] = c * up - s * uq;
                        u[i * n + q] = s * up + c * uq;
                    }
                    for (int i = 0; i < n; i++)
                    {
                        T vp = v[i * n + p], vq = v[i * n + q];
                        v[i * n + p] = c * vp - s * vq;
                        v[i * n + q] = s * vp + c * vq;
                    }
                }
            if (!rotated) break;
        }
        var sv = new T[n];
        for (int j = 0; j < n; j++)
        {
            T norm = T.Zero;
            for (int i = 0; i < m; i++) norm += u[i * n + j] * u[i * n + j];
            sv[j] = T.Sqrt(norm);
        }
        var order = Enumerable.Range(0, n).OrderByDescending(j => sv[j]).ToArray();
        var us = new T[m * n];
        var vsorted = new T[n * n];
        var ss = new T[n];
        for (int jj = 0; jj < n; jj++)
        {
            int j = order[jj];
            ss[jj] = sv[j];
            for (int i = 0; i < m; i++) us[i * n + jj] = sv[j] > T.Zero ? u[i * n + j] / sv[j] : T.Zero;
            for (int i = 0; i < n; i++) vsorted[i * n + jj] = v[i * n + j];
        }
        return (us, ss, vsorted);
    }

    /// <summary>Completes the orthonormal columns of <paramref name="u"/> (rows×have) to a full rows×rows orthogonal matrix.</summary>
    public static T[] CompleteBasis<T>(T[] u, int rows, int have) where T : IFloatingPointIeee754<T>
    {
        var q = new T[rows * rows];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < have; j++) q[i * rows + j] = u[i * have + j];
        T minNorm = Tol<T>(1e-8);
        int col = have;
        for (int e = 0; e < rows && col < rows; e++)
        {
            var vec = new T[rows];
            vec[e] = T.One;
            for (int pass = 0; pass < 2; pass++)
                for (int j = 0; j < col; j++)
                {
                    T dot = T.Zero;
                    for (int i = 0; i < rows; i++) dot += vec[i] * q[i * rows + j];
                    for (int i = 0; i < rows; i++) vec[i] -= dot * q[i * rows + j];
                }
            T sum = T.Zero;
            foreach (var x in vec) sum += x * x;
            T norm = T.Sqrt(sum);
            if (norm < minNorm) continue;
            for (int i = 0; i < rows; i++) q[i * rows + col] = vec[i] / norm;
            col++;
        }
        return q;
    }

    /// <summary>Sign and natural logarithm of |det| of an n × n matrix by LU, as numpy's <c>slogdet</c> computes them: (0, −∞) for a singular matrix. The input is not modified.</summary>
    public static (T sign, T logDet) SignAndLogDeterminant<T>(ReadOnlySpan<T> matrix, int n) where T : IFloatingPointIeee754<T>
    {
        var lu = matrix.ToArray();
        var piv = new int[n];
        if (Lu(lu, n, piv)) return (T.Zero, T.NegativeInfinity);
        int changes = 0;
        for (int i = 0; i < n; i++) if (piv[i] != i) changes++;
        T sign = changes % 2 == 1 ? -T.One : T.One, acc = T.Zero;
        for (int i = 0; i < n; i++)
        {
            T e = lu[i * n + i];
            if (e < T.Zero) { sign = -sign; e = -e; }
            acc += T.Log(e);
        }
        return (sign, acc);
    }
}
