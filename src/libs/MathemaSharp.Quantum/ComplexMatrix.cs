using System.Numerics;
using MathemaSharp.Core;

namespace MathemaSharp.Quantum;

/// <summary>
/// Dense complex matrix: operator on a finite-dimensional Hilbert space, generic over the .NET 11 Generic Math formats
/// (<c>T : IFloatingPointIeee754&lt;T&gt;, IMinMaxValue&lt;T&gt;</c>, the constraints of <see cref="Complex{T}"/>).
/// Shared backing array: copy explicitly before mutating an alias.
/// </summary>
public readonly struct ComplexMatrix<T> where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    private readonly Complex<T>[,] _values;

    public int Rows { get; }
    public int Cols { get; }

    public ComplexMatrix(Complex<T>[,] values)
    {
        _values = values;
        Rows = values.GetLength(0);
        Cols = values.GetLength(1);
    }

    public ComplexMatrix(int rows, int cols) : this(new Complex<T>[rows, cols]) { }

    public Complex<T> this[int row, int col]
    {
        get => _values[row, col];
        set => _values[row, col] = value;
    }

    public static ComplexMatrix<T> Identity(int n)
    {
        var result = new ComplexMatrix<T>(n, n);
        for (var i = 0; i < n; i++) result[i, i] = Complex<T>.One;
        return result;
    }

    public static ComplexMatrix<T> Zero(int rows, int cols) => new(rows, cols);

    public static ComplexMatrix<T> operator +(ComplexMatrix<T> a, ComplexMatrix<T> b)
    {
        RequireSameShape(a, b);
        var result = new ComplexMatrix<T>(a.Rows, a.Cols);
        for (var i = 0; i < a.Rows; i++)
            for (var j = 0; j < a.Cols; j++)
                result[i, j] = a[i, j] + b[i, j];
        return result;
    }

    public static ComplexMatrix<T> operator -(ComplexMatrix<T> a, ComplexMatrix<T> b)
    {
        RequireSameShape(a, b);
        var result = new ComplexMatrix<T>(a.Rows, a.Cols);
        for (var i = 0; i < a.Rows; i++)
            for (var j = 0; j < a.Cols; j++)
                result[i, j] = a[i, j] - b[i, j];
        return result;
    }

    public static ComplexMatrix<T> operator *(Complex<T> scalar, ComplexMatrix<T> a)
    {
        var result = new ComplexMatrix<T>(a.Rows, a.Cols);
        for (var i = 0; i < a.Rows; i++)
            for (var j = 0; j < a.Cols; j++)
                result[i, j] = scalar * a[i, j];
        return result;
    }

    public static ComplexMatrix<T> operator *(ComplexMatrix<T> a, Complex<T> scalar) => scalar * a;

    /// <summary>Multiplication by a real scalar.</summary>
    public static ComplexMatrix<T> operator *(T scalar, ComplexMatrix<T> a) => new Complex<T>(scalar, T.Zero) * a;

    public static ComplexMatrix<T> operator *(ComplexMatrix<T> a, T scalar) => scalar * a;

    public static ComplexMatrix<T> operator *(ComplexMatrix<T> a, ComplexMatrix<T> b)
    {
        if (a.Cols != b.Rows) throw new ArgumentException("Incompatible dimensions for the row-by-column product.");
        var result = new ComplexMatrix<T>(a.Rows, b.Cols);
        for (var i = 0; i < a.Rows; i++)
            for (var j = 0; j < b.Cols; j++)
            {
                var sum = Complex<T>.Zero;
                for (var k = 0; k < a.Cols; k++) sum += a[i, k] * b[k, j];
                result[i, j] = sum;
            }
        return result;
    }

    /// <summary>Â|ψ⟩: applica l'operatore a un ket.</summary>
    public static Complex<T>[] operator *(ComplexMatrix<T> a, ReadOnlySpan<Complex<T>> ket)
    {
        if (a.Cols != ket.Length) throw new ArgumentException("L'operatore e il ket hanno dimensioni incompatibili.");
        var result = new Complex<T>[a.Rows];
        for (var i = 0; i < a.Rows; i++)
        {
            var sum = Complex<T>.Zero;
            for (var j = 0; j < a.Cols; j++) sum += a[i, j] * ket[j];
            result[i] = sum;
        }
        return result;
    }

    /// <summary>Aggiunto hermitiano Â†: trasposta e coniugata.</summary>
    public ComplexMatrix<T> Dagger()
    {
        var result = new ComplexMatrix<T>(Cols, Rows);
        for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Cols; j++)
                result[j, i] = Complex<T>.Conjugate(this[i, j]);
        return result;
    }

    public Complex<T> Trace()
    {
        if (Rows != Cols) throw new InvalidOperationException("The trace requires a square matrix.");
        var sum = Complex<T>.Zero;
        for (var i = 0; i < Rows; i++) sum += this[i, i];
        return sum;
    }

    /// <summary>Default tolerance of the boolean checks: √ε of the type (≈ 1.5e-8 for double), that is, half the digits.</summary>
    public static T DefaultTolerance => T.Sqrt(T.BitIncrement(T.One) - T.One);

    /// <summary>Â = Â†: real eigenvalues, an orthonormal eigenvector basis guaranteed.</summary>
    public bool IsHermitian() => IsHermitian(DefaultTolerance);

    public bool IsHermitian(T tolerance)
    {
        if (Rows != Cols) return false;
        for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Cols; j++)
                if (!SameComplex(this[i, j], Complex<T>.Conjugate(this[j, i]), tolerance)) return false;
        return true;
    }

    /// <summary>Û†Û = I: preserva la norma, candidato valido a evoluzione temporale.</summary>
    public bool IsUnitary() => IsUnitary(DefaultTolerance);

    public bool IsUnitary(T tolerance)
    {
        if (Rows != Cols) return false;
        var product = Dagger() * this;
        var identity = Identity(Rows);
        for (var i = 0; i < Rows; i++)
            for (var j = 0; j < Cols; j++)
                if (!SameComplex(product[i, j], identity[i, j], tolerance)) return false;
        return true;
    }

    internal static bool SameComplex(Complex<T> a, Complex<T> b, T tolerance) =>
        Precision.NearlyEqual(a.Real, b.Real, tolerance, tolerance) && Precision.NearlyEqual(a.Imaginary, b.Imaginary, tolerance, tolerance);

    private static void RequireSameShape(ComplexMatrix<T> a, ComplexMatrix<T> b)
    {
        if (a.Rows != b.Rows || a.Cols != b.Cols) throw new ArgumentException("Dimensioni diverse.");
    }
}
