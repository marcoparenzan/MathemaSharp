using System.Numerics;
using static MathemaSharp.TimeSeries.Num;

namespace MathemaSharp.TimeSeries;

/// <summary>
/// VAR(p): Y_t = c + A_1 Y_{t−1} + … + A_p Y_{t−p} + ε_t, con Y_t vettore di k serie. A_i[r,c] misura l'effetto della serie c al lag i
/// sulla serie r: ogni serie dipende dai propri ritardi e da quelli delle altre (Sims, 1980). Dati come T[tempo, serie].
/// </summary>
public sealed record VarModel<T>(T[] Intercept, T[][,] Coefficients, T[,] ResidualCovariance) where T : IFloatingPointIeee754<T>
{
    public int Order => Coefficients.Length;
    public int Dimension => Intercept.Length;

    /// <summary>Previsione ricorsiva: result[h, serie].</summary>
    public T[,] Forecast(T[,] history, int horizon)
    {
        if (history.GetLength(1) != Dimension || history.GetLength(0) < Order) throw new ArgumentException("Storia incompatibile.");
        var rows = new List<T[]>();
        for (var t = history.GetLength(0) - Order; t < history.GetLength(0); t++)
        {
            var row = new T[Dimension];
            for (var c = 0; c < Dimension; c++) row[c] = history[t, c];
            rows.Add(row);
        }
        var result = new T[horizon, Dimension];
        for (var h = 0; h < horizon; h++)
        {
            var next = (T[])Intercept.Clone();
            for (var lag = 1; lag <= Order; lag++)
            {
                var contribution = MatrixAlgebra.Multiply<T>(Coefficients[lag - 1], rows[^lag]);
                for (var c = 0; c < Dimension; c++) next[c] += contribution[c];
            }
            rows.Add(next);
            for (var c = 0; c < Dimension; c++) result[h, c] = next[c];
        }
        return result;
    }

    /// <summary>
    /// Stazionarietà di un VAR(1) bivariato: autovalori di A_1 dentro il cerchio unitario. λ = (tr ± √(tr² − 4 det))/2: se il discriminante
    /// è negativo gli autovalori sono complessi coniugati con |λ|² = det, altrimenti sono reali.
    /// </summary>
    public bool IsStableOrder1Bivariate()
    {
        if (Order != 1 || Dimension != 2) throw new InvalidOperationException("Solo VAR(1) bivariato.");
        var a = Coefficients[0];
        var trace = a[0, 0] + a[1, 1];
        var det = a[0, 0] * a[1, 1] - a[0, 1] * a[1, 0];
        var discriminant = trace * trace - C<T>(4) * det;
        if (discriminant < T.Zero) return det < T.One;
        var root = T.Sqrt(discriminant);
        var two = C<T>(2);
        return T.Abs((trace + root) / two) < T.One && T.Abs((trace - root) / two) < T.One;
    }
}

/// <summary>Stima di un <see cref="VarModel{T}"/>.</summary>
public static class VarModel
{
    /// <summary>
    /// OLS equazione per equazione: con gli stessi regressori in ogni equazione coincide con lo stimatore SUR/GLS (Zellner).
    /// Covarianza dei residui con divisore T − kp − 1.
    /// </summary>
    public static VarModel<T> Fit<T>(T[,] y, int order) where T : IFloatingPointIeee754<T>
    {
        int n = y.GetLength(0), k = y.GetLength(1);
        if (order < 1) throw new ArgumentOutOfRangeException(nameof(order));
        var rows = n - order;
        var columns = 1 + k * order;
        if (rows <= columns) throw new ArgumentException("Serie troppo corta per ordine e dimensione.");
        var x = new T[rows, columns];
        for (var r = 0; r < rows; r++)
        {
            var t = order + r;
            x[r, 0] = T.One;
            for (var lag = 1; lag <= order; lag++)
                for (var c = 0; c < k; c++) x[r, 1 + (lag - 1) * k + c] = y[t - lag, c];
        }
        var intercept = new T[k];
        var coefficients = new T[order][,];
        for (var lag = 0; lag < order; lag++) coefficients[lag] = new T[k, k];
        var residuals = new T[rows, k];
        for (var eq = 0; eq < k; eq++)
        {
            var target = new T[rows];
            for (var r = 0; r < rows; r++) target[r] = y[order + r, eq];
            var beta = MatrixAlgebra.LeastSquares<T>(x, target);
            intercept[eq] = beta[0];
            for (var lag = 0; lag < order; lag++)
                for (var c = 0; c < k; c++) coefficients[lag][eq, c] = beta[1 + lag * k + c];
            var fitted = MatrixAlgebra.Multiply<T>(x, beta);
            for (var r = 0; r < rows; r++) residuals[r, eq] = target[r] - fitted[r];
        }
        var covariance = MatrixAlgebra.Multiply(MatrixAlgebra.Transpose(residuals), residuals);
        var dof = N<T>(rows - columns);
        for (var i = 0; i < k; i++)
            for (var j = 0; j < k; j++) covariance[i, j] /= dof;
        return new VarModel<T>(intercept, coefficients, covariance);
    }
}
