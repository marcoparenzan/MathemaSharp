// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
namespace MathemaSharp.LinearAlgebra;

/// <summary>Dense real linear algebra on flat row-major <c>double[]</c> matrices: the routines behind <c>numpy.linalg</c> in NDSharp, with no dependency on any array type.
/// Moved from NDSharp on 2026-11-06 without changing the arithmetic (738 results compared by SHA-256 before and after). Working in double; a caller that wants
/// float32 results rounds afterwards. Complex and non-symmetric eigenproblems are in <see cref="GeneralEigen"/>.</summary>
public static class DenseLinearAlgebra
{
    /// <summary>LU with partial pivoting (LAPACK dgetf2 order). Returns the pivot rows and whether the matrix is singular.</summary>
    public static bool Lu(double[] a, int n, int[] piv)
    {
        bool singular = false;
        for (int j = 0; j < n; j++)
        {
            int p = j;
            double max = Math.Abs(a[j * n + j]);
            for (int i = j + 1; i < n; i++)
                if (Math.Abs(a[i * n + j]) > max) { max = Math.Abs(a[i * n + j]); p = i; }
            piv[j] = p;
            if (a[p * n + j] != 0)
            {
                if (p != j)
                    for (int k = 0; k < n; k++) (a[j * n + k], a[p * n + k]) = (a[p * n + k], a[j * n + k]);
                double pivot = a[j * n + j];
                for (int i = j + 1; i < n; i++) a[i * n + j] /= pivot;
            }
            else singular = true;
            for (int i = j + 1; i < n; i++)
            {
                double l = a[i * n + j];
                if (l != 0)
                    for (int k = j + 1; k < n; k++) a[i * n + k] -= l * a[j * n + k];
            }
        }
        return singular;
    }

    /// <summary>Solves with an LU factorization in place on <paramref name="b"/> (n × m, row-major).</summary>
    public static void LuSolve(double[] lu, int n, int[] piv, double[] b, int m)
    {
        for (int j = 0; j < n; j++)
            if (piv[j] != j)
                for (int k = 0; k < m; k++) (b[j * m + k], b[piv[j] * m + k]) = (b[piv[j] * m + k], b[j * m + k]);
        for (int i = 0; i < n; i++)
            for (int k = 0; k < i; k++)
            {
                double l = lu[i * n + k];
                if (l != 0) for (int c = 0; c < m; c++) b[i * m + c] -= l * b[k * m + c];
            }
        for (int i = n - 1; i >= 0; i--)
        {
            for (int k = i + 1; k < n; k++)
            {
                double u = lu[i * n + k];
                if (u != 0) for (int c = 0; c < m; c++) b[i * m + c] -= u * b[k * m + c];
            }
            double d = lu[i * n + i];
            for (int c = 0; c < m; c++) b[i * m + c] /= d;
        }
    }

    /// <summary>Symmetric eigendecomposition by cyclic Jacobi rotations: ascending eigenvalues, orthonormal
    /// column eigenvectors. (LAPACK's eigenvector signs are an implementation detail we do not reproduce.)</summary>
    public static (double[] w, double[] v) SymmetricEigen(double[] a, int n)
    {
        var m = (double[])a.Clone();
        var v = new double[n * n];
        for (int i = 0; i < n; i++) v[i * n + i] = 1.0;
        for (int sweep = 0; sweep < 100; sweep++)
        {
            double off = 0;
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++) off += m[i * n + j] * m[i * n + j];
            if (off < 1e-300) break;
            for (int p = 0; p < n - 1; p++)
                for (int q = p + 1; q < n; q++)
                {
                    double apq = m[p * n + q];
                    if (Math.Abs(apq) < 1e-300) continue;
                    double theta = (m[q * n + q] - m[p * n + p]) / (2 * apq);
                    double t = Math.Sign(theta == 0 ? 1 : theta) / (Math.Abs(theta) + Math.Sqrt(theta * theta + 1));
                    double c = 1 / Math.Sqrt(t * t + 1), s = t * c;
                    for (int k = 0; k < n; k++)
                    {
                        double akp = m[k * n + p], akq = m[k * n + q];
                        m[k * n + p] = c * akp - s * akq;
                        m[k * n + q] = s * akp + c * akq;
                    }
                    for (int k = 0; k < n; k++)
                    {
                        double apk = m[p * n + k], aqk = m[q * n + k];
                        m[p * n + k] = c * apk - s * aqk;
                        m[q * n + k] = s * apk + c * aqk;
                    }
                    for (int k = 0; k < n; k++)
                    {
                        double vkp = v[k * n + p], vkq = v[k * n + q];
                        v[k * n + p] = c * vkp - s * vkq;
                        v[k * n + q] = s * vkp + c * vkq;
                    }
                }
        }
        var order = Enumerable.Range(0, n).OrderBy(i => m[i * n + i]).ToArray();
        var w = order.Select(i => m[i * n + i]).ToArray();
        var vs = new double[n * n];
        for (int j = 0; j < n; j++)
        {
            int src = order[j];
            // Sign convention: the component of largest magnitude is positive.
            int big = 0;
            for (int i = 1; i < n; i++) if (Math.Abs(v[i * n + src]) > Math.Abs(v[big * n + src]) + 1e-12) big = i;
            double sgn = v[big * n + src] < 0 ? -1 : 1;
            for (int i = 0; i < n; i++) vs[i * n + j] = sgn * v[i * n + src];
        }
        return (w, vs);
    }

    /// <summary>Thin SVD of an m×n matrix (m ≥ n) by one-sided Jacobi; returns U (m×n), s (n), V (n×n), singular values descending.</summary>
    public static (double[] u, double[] s, double[] v) ThinSvd(double[] a, int m, int n)
    {
        var u = (double[])a.Clone();
        var v = new double[n * n];
        for (int i = 0; i < n; i++) v[i * n + i] = 1.0;
        for (int sweep = 0; sweep < 60; sweep++)
        {
            bool rotated = false;
            for (int p = 0; p < n - 1; p++)
                for (int q = p + 1; q < n; q++)
                {
                    double alpha = 0, beta = 0, gamma = 0;
                    for (int i = 0; i < m; i++)
                    {
                        alpha += u[i * n + p] * u[i * n + p];
                        beta += u[i * n + q] * u[i * n + q];
                        gamma += u[i * n + p] * u[i * n + q];
                    }
                    if (Math.Abs(gamma) <= 1e-15 * Math.Sqrt(alpha * beta) || gamma == 0) continue;
                    rotated = true;
                    double zeta = (beta - alpha) / (2 * gamma);
                    double t = Math.Sign(zeta == 0 ? 1 : zeta) / (Math.Abs(zeta) + Math.Sqrt(1 + zeta * zeta));
                    double c = 1 / Math.Sqrt(1 + t * t), s = c * t;
                    for (int i = 0; i < m; i++)
                    {
                        double up = u[i * n + p], uq = u[i * n + q];
                        u[i * n + p] = c * up - s * uq;
                        u[i * n + q] = s * up + c * uq;
                    }
                    for (int i = 0; i < n; i++)
                    {
                        double vp = v[i * n + p], vq = v[i * n + q];
                        v[i * n + p] = c * vp - s * vq;
                        v[i * n + q] = s * vp + c * vq;
                    }
                }
            if (!rotated) break;
        }
        var sv = new double[n];
        for (int j = 0; j < n; j++)
        {
            double norm = 0;
            for (int i = 0; i < m; i++) norm += u[i * n + j] * u[i * n + j];
            sv[j] = Math.Sqrt(norm);
        }
        var order = Enumerable.Range(0, n).OrderByDescending(j => sv[j]).ToArray();
        var us = new double[m * n];
        var vsorted = new double[n * n];
        var ss = new double[n];
        for (int jj = 0; jj < n; jj++)
        {
            int j = order[jj];
            ss[jj] = sv[j];
            for (int i = 0; i < m; i++) us[i * n + jj] = sv[j] > 0 ? u[i * n + j] / sv[j] : 0;
            for (int i = 0; i < n; i++) vsorted[i * n + jj] = v[i * n + j];
        }
        return (us, ss, vsorted);
    }

    /// <summary>Completes the orthonormal columns of <paramref name="u"/> (rows×have) to a full rows×rows orthogonal matrix.</summary>
    public static double[] CompleteBasis(double[] u, int rows, int have)
    {
        var q = new double[rows * rows];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < have; j++) q[i * rows + j] = u[i * have + j];
        int col = have;
        for (int e = 0; e < rows && col < rows; e++)
        {
            var vec = new double[rows];
            vec[e] = 1.0;
            for (int pass = 0; pass < 2; pass++)
                for (int j = 0; j < col; j++)
                {
                    double dot = 0;
                    for (int i = 0; i < rows; i++) dot += vec[i] * q[i * rows + j];
                    for (int i = 0; i < rows; i++) vec[i] -= dot * q[i * rows + j];
                }
            double norm = Math.Sqrt(vec.Sum(x => x * x));
            if (norm < 1e-8) continue;
            for (int i = 0; i < rows; i++) q[i * rows + col] = vec[i] / norm;
            col++;
        }
        return q;
    }

    /// <summary>Sign and natural logarithm of |det| of an n × n matrix by LU, as numpy's <c>slogdet</c> computes them: (0, −∞) for a singular matrix. The input is not modified.</summary>
    public static (double sign, double logDet) SignAndLogDeterminant(ReadOnlySpan<double> matrix, int n)
    {
        var lu = matrix.ToArray();
        var piv = new int[n];
        if (Lu(lu, n, piv)) return (0, double.NegativeInfinity);
        int changes = 0;
        for (int i = 0; i < n; i++) if (piv[i] != i) changes++;
        double sign = changes % 2 == 1 ? -1.0 : 1.0, acc = 0;
        for (int i = 0; i < n; i++)
        {
            double e = lu[i * n + i];
            if (e < 0) { sign = -sign; e = -e; }
            acc += Math.Log(e);
        }
        return (sign, acc);
    }
}
