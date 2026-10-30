using System.Numerics;
namespace MathemaSharp.LinearAlgebra;

/// <summary>
/// Minimal real linear algebra on <c>T[,]</c>, generic over <c>T : IFloatingPointIeee754&lt;T&gt;</c>: just enough for regressions with few
/// regressors and filters with a small state. It is not a BLAS: for generic N-dimensional arrays use NDSharp (bridge in MathemaSharp.NDSharp).
/// </summary>
public static class MatrixAlgebra
{
    private static T N<T>(int value) where T : IFloatingPointIeee754<T> => T.CreateChecked(value);
    private static T Epsilon<T>() where T : IFloatingPointIeee754<T> => T.BitIncrement(T.One) - T.One;
    /// <summary>L1 norm; the empty vector has norm zero.</summary>
    public static T NormL1<T>(ReadOnlySpan<T> x) where T : IFloatingPointIeee754<T>
    {
        var sum = T.Zero;
        foreach (var value in x) sum += T.Abs(value);
        return sum;
    }

    /// <summary>L2 norm with Hypot: avoids premature overflow of the squares.</summary>
    public static T NormL2<T>(ReadOnlySpan<T> x) where T : IFloatingPointIeee754<T>
    {
        var norm = T.Zero;
        foreach (var value in x) norm = T.Hypot(norm, value);
        return norm;
    }

    public static T NormInfinity<T>(ReadOnlySpan<T> x) where T : IFloatingPointIeee754<T>
    {
        var norm = T.Zero;
        foreach (var value in x) norm = T.Max(norm, T.Abs(value));
        return norm;
    }

    public static T FrobeniusNorm<T>(T[,] a) where T : IFloatingPointIeee754<T>
    {
        var norm = T.Zero;
        foreach (var value in a) norm = T.Hypot(norm, value);
        return norm;
    }

    public static T Determinant2x2<T>(T[,] a) where T : IFloatingPointIeee754<T>
    {
        Require2x2(a);
        return a[0, 0] * a[1, 1] - a[0, 1] * a[1, 0];
    }

    /// <summary>Educational 2×2 inverse. Rejects non-finite inputs and nearly singular matrices on the normalised scale.</summary>
    public static T[,] Inverse2x2<T>(T[,] a) where T : IFloatingPointIeee754<T>
    {
        Require2x2(a);
        var scale = T.Zero;
        foreach (var value in a)
        {
            if (!T.IsFinite(value)) throw new ArgumentException("Servono elementi finiti.");
            scale = T.Max(scale, T.Abs(value));
        }
        if (scale == T.Zero) throw new ArgumentException("Matrice singolare.");
        var p = a[0, 0] / scale;
        var q = a[0, 1] / scale;
        var r = a[1, 0] / scale;
        var s = a[1, 1] / scale;
        var det = p * s - q * r;
        if (T.Abs(det) <= T.CreateChecked(32) * (T.BitIncrement(T.One) - T.One))
            throw new ArgumentException("Matrice singolare o numericamente quasi singolare.");
        return new T[,] { { (s / det) / scale, (-q / det) / scale }, { (-r / det) / scale, (p / det) / scale } };
    }

    private static void Require2x2<T>(T[,] a)
    {
        if (a.GetLength(0) != 2 || a.GetLength(1) != 2)
            throw new ArgumentException("A 2×2 matrix is required.");
    }

    public static T[,] Multiply<T>(T[,] a, T[,] b) where T : INumber<T>
    {
        if (a.GetLength(1) != b.GetLength(0)) throw new ArgumentException("Dimensioni incompatibili.");
        var result = new T[a.GetLength(0), b.GetLength(1)];
        for (var i = 0; i < a.GetLength(0); i++)
            for (var k = 0; k < a.GetLength(1); k++)
            {
                var aik = a[i, k];
                for (var j = 0; j < b.GetLength(1); j++) result[i, j] += aik * b[k, j];
            }
        return result;
    }

    public static T[] Multiply<T>(T[,] a, ReadOnlySpan<T> x) where T : INumber<T>
    {
        if (a.GetLength(1) != x.Length) throw new ArgumentException("Dimensioni incompatibili.");
        var result = new T[a.GetLength(0)];
        for (var i = 0; i < result.Length; i++)
            for (var j = 0; j < x.Length; j++) result[i] += a[i, j] * x[j];
        return result;
    }

    public static T[,] Transpose<T>(T[,] a)
    {
        var result = new T[a.GetLength(1), a.GetLength(0)];
        for (var i = 0; i < a.GetLength(0); i++)
            for (var j = 0; j < a.GetLength(1); j++) result[j, i] = a[i, j];
        return result;
    }

    public static T[,] Identity<T>(int n) where T : INumber<T>
    {
        var result = new T[n, n];
        for (var i = 0; i < n; i++) result[i, i] = T.One;
        return result;
    }

    /// <summary>
    /// Least squares min‖Xβ − y‖₂ with Householder QR. It does not form XᵀX: the normal equations square the condition
    /// condizionamento, la QR no. Richiede righe ≥ colonne, dati finiti e colonne linearmente indipendenti.
    /// Normalises the system on the matrix scale; threshold min(4096 ε, √ε) and normalised reflectors.
    /// It does not estimate conditioning. Unrepresentable intermediates are reported with ArithmeticException.
    /// </summary>
    public static T[] LeastSquares<T>(T[,] x, ReadOnlySpan<T> y) where T : IFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNull(x);
        int m = x.GetLength(0), n = x.GetLength(1);
        if (y.Length != m) throw new ArgumentException("Lunghezze diverse.");
        if (m < n) throw new ArgumentException("Servono almeno tante osservazioni quanti regressori.");
        var a = (T[,])x.Clone();
        var b = y.ToArray();
        var diagonal = new T[n];
        var scale = T.Zero;
        foreach (var v in x)
        {
            if (!T.IsFinite(v)) throw new ArgumentException("Regressori finiti richiesti.", nameof(x));
            scale = T.Max(scale, T.Abs(v));
        }
        foreach (var v in y)
            if (!T.IsFinite(v)) throw new ArgumentException("Termine noto finito richiesto.", nameof(y));
        if (n == 0) return [];
        if (scale == T.Zero) throw new ArgumentException("Regressori linearmente dipendenti.", nameof(x));
        for (var i = 0; i < m; i++)
        {
            b[i] /= scale;
            if (!T.IsFinite(b[i])) throw new ArithmeticException("Normalised system not representable.");
            for (var j = 0; j < n; j++) a[i, j] /= scale;
        }
        var epsilon = Epsilon<T>();
        var threshold = T.Min(N<T>(4096) * epsilon, T.Sqrt(epsilon));
        var two = N<T>(2);

        for (var k = 0; k < n; k++)
        {
            var norm = T.Zero;
            for (var i = k; i < m; i++) norm = T.Hypot(norm, a[i, k]);
            if (!T.IsFinite(norm)) throw new ArithmeticException("QR norm not representable.");
            if (norm <= threshold) throw new ArgumentException("Regressori linearmente dipendenti.");
            // Reflector first scaled by norm, then made unit: avoids squares of large values.
            var sign = a[k, k] > T.Zero ? T.One : -T.One;
            var alpha = -sign * norm;
            for (var i = k; i < m; i++) a[i, k] /= norm;
            a[k, k] += sign;
            var vNorm = T.Zero;
            for (var i = k; i < m; i++) vNorm = T.Hypot(vNorm, a[i, k]);
            for (var i = k; i < m; i++) a[i, k] /= vNorm;
            // H = I − 2vvᵀ/‖v‖² applicata alle colonne successive e al termine noto.
            for (var j = k + 1; j < n; j++)
            {
                var s = T.Zero;
                for (var i = k; i < m; i++) s += a[i, k] * a[i, j];
                var f = two * s;
                if (!T.IsFinite(f)) throw new ArithmeticException("QR projection not representable.");
                for (var i = k; i < m; i++) a[i, j] -= f * a[i, k];
            }
            var sb = T.Zero;
            for (var i = k; i < m; i++) sb += a[i, k] * b[i];
            var fb = two * sb;
            if (!T.IsFinite(fb)) throw new ArithmeticException("Projection of the right-hand side not representable.");
            for (var i = k; i < m; i++) b[i] -= fb * a[i, k];
            diagonal[k] = alpha;
        }

        var beta = new T[n];
        for (var k = n - 1; k >= 0; k--)
        {
            var s = b[k];
            for (var j = k + 1; j < n; j++) s -= a[k, j] * beta[j];
            beta[k] = s / diagonal[k];
            if (!T.IsFinite(beta[k])) throw new ArithmeticException("Solution not representable.");
        }
        return beta;
    }
}
