using System.Numerics;

namespace MathemaSharp.LinearAlgebra;

/// <summary>Real LU factorisation with partial pivoting. Computations entirely in the T format.</summary>
/// <remarks>Square, non-empty, finite input. The pivot threshold is relative to the global scale.
/// It is not a conditioning estimate; nearly singular matrices are rejected.</remarks>
public sealed class LuFactorization<T> where T : IFloatingPointIeee754<T>
{
    private readonly T[,] lu;
    private readonly int[] permutation;
    private readonly int sign;
    private readonly T scale;
    public int Order => permutation.Length;

    private LuFactorization(T[,] lu, int[] permutation, int sign, T scale)
        => (this.lu, this.permutation, this.sign, this.scale) = (lu, permutation, sign, scale);

    public static LuFactorization<T> Create(T[,] matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        var n = matrix.GetLength(0);
        if (n == 0 || matrix.GetLength(1) != n) throw new ArgumentException("A non-empty square matrix is required.", nameof(matrix));
        var scale = T.Zero;
        foreach (var value in matrix)
        {
            if (!T.IsFinite(value)) throw new ArgumentException("The matrix must contain finite values.", nameof(matrix));
            scale = T.Max(scale, T.Abs(value));
        }
        if (scale == T.Zero) throw new ArgumentException("Matrice singolare.", nameof(matrix));
        var lu = new T[n,n];
        var permutation = new int[n];
        for (var i = 0; i < n; i++)
        {
            permutation[i] = i;
            for (var j = 0; j < n; j++) lu[i,j] = matrix[i,j] / scale;
        }
        var threshold = T.CreateChecked(n) * (T.BitIncrement(T.One) - T.One);
        var sign = 1;
        for (var k = 0; k < n; k++)
        {
            var pivot = k;
            for (var i = k + 1; i < n; i++)
                if (T.Abs(lu[i,k]) > T.Abs(lu[pivot,k])) pivot = i;
            if (!T.IsFinite(lu[pivot,k]) || T.Abs(lu[pivot,k]) <= threshold)
                throw new ArgumentException("Matrice singolare o numericamente quasi singolare.", nameof(matrix));
            if (pivot != k)
            {
                (permutation[k], permutation[pivot]) = (permutation[pivot], permutation[k]);
                for (var j = 0; j < n; j++) (lu[k,j], lu[pivot,j]) = (lu[pivot,j], lu[k,j]);
                sign = -sign;
            }
            for (var i = k + 1; i < n; i++)
            {
                lu[i,k] /= lu[k,k];
                for (var j = k + 1; j < n; j++) lu[i,j] -= lu[i,k] * lu[k,j];
            }
        }
        return new(lu, permutation, sign, scale);
    }

    /// <summary>Solves Ax=b without forming the inverse. Does not modify b.</summary>
    public T[] Solve(ReadOnlySpan<T> b)
    {
        if (b.Length != Order) throw new ArgumentException("Dimensioni incompatibili.", nameof(b));
        foreach (var value in b)
            if (!T.IsFinite(value)) throw new ArgumentException("The right-hand side must be finite.", nameof(b));
        var x = new T[Order];
        for (var i = 0; i < Order; i++)
        {
            var value = b[permutation[i]] / scale;
            for (var j = 0; j < i; j++) value -= lu[i,j] * x[j];
            x[i] = value;
        }
        for (var i = Order - 1; i >= 0; i--)
        {
            var value = x[i];
            for (var j = i + 1; j < Order; j++) value -= lu[i,j] * x[j];
            x[i] = value / lu[i,i];
        }
        return x;
    }

    /// <summary>Determinant of the factorised matrix; it can exceed the range of T.</summary>
    public T Determinant()
    {
        var result = T.CreateChecked(sign);
        for (var i = 0; i < Order; i++) result *= lu[i,i] * scale;
        return result;
    }

    /// <summary>Inverse obtained by solving for each column of the identity.</summary>
    public T[,] Inverse()
    {
        var result = new T[Order,Order];
        var unit = new T[Order];
        for (var j = 0; j < Order; j++)
        {
            unit[j] = T.One;
            var column = Solve(unit);
            for (var i = 0; i < Order; i++) result[i,j] = column[i];
            unit[j] = T.Zero;
        }
        return result;
    }
}
