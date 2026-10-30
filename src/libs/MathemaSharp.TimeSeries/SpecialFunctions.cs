using System.Globalization;
using System.Numerics;
using static MathemaSharp.TimeSeries.Num;

namespace MathemaSharp.TimeSeries;

/// <summary>Log gamma and regularised incomplete gamma, with Generic Math arithmetic.</summary>
/// <remarks>Lanczos coefficients read directly in T, with no passes through double.
/// The approximation stays at limited precision (about binary64 in ordinary cases), not 34 digits.
/// For Half and other low-precision formats the error can be larger. Failure to converge and non-finite results are reported.</remarks>
public static class SpecialFunctions
{
    private static class Coefficients<T> where T : IFloatingPointIeee754<T>
    {
        public static readonly T[] Lanczos = new[]
        {
            "0.99999999999980993", "676.5203681218851", "-1259.1392167224028", "771.32342877765313", "-176.61502916214059",
            "12.507343278686905", "-0.13857109526572012", "9.9843695780195716e-6", "1.5056327351493116e-7"
        }.Select(value => T.Parse(value, CultureInfo.InvariantCulture)).ToArray();
        public static readonly T ApproximationTolerance = T.Parse("1e-16", CultureInfo.InvariantCulture);
    }

    /// <summary>ln Γ(x) for positive x; +∞ returns +∞. NaN and non-positive values are rejected.</summary>
    public static T LogGamma<T>(T x) where T : IFloatingPointIeee754<T>
    {
        if (T.IsNaN(x) || x <= T.Zero) throw new ArgumentOutOfRangeException(nameof(x));
        if (T.IsPositiveInfinity(x)) return T.PositiveInfinity;
        var half = T.One / N<T>(2);
        if (x == T.One || x == N<T>(2)) return T.Zero;
        if (x == half) return T.Log(T.Pi) / N<T>(2);
        if (x < half) return LogGamma(x + T.One) - T.Log(x);
        x -= T.One;
        var coefficients = Coefficients<T>.Lanczos;
        var sum = coefficients[0];
        for (var i = 1; i < coefficients.Length; i++) sum += coefficients[i] / (x + N<T>(i));
        var t = x + N<T>(15) / N<T>(2);
        var result = half * T.Log(T.Tau) + (x + half) * T.Log(t) - t + T.Log(sum);
        if (!T.IsFinite(result)) throw new ArithmeticException("LogGamma not representable in the requested type.");
        return result;
    }

    /// <summary>Q(a,x), a positivo e finito, x non negativo. Q(a,0)=1 e Q(a,+∞)=0.
    /// Power series or Lentz continued fraction; at most 1000 iterations.</summary>
    public static T RegularizedGammaQ<T>(T a, T x) where T : IFloatingPointIeee754<T>
    {
        if (!T.IsFinite(a) || a <= T.Zero) throw new ArgumentOutOfRangeException(nameof(a));
        if (T.IsNaN(x) || x < T.Zero) throw new ArgumentOutOfRangeException(nameof(x));
        if (x == T.Zero) return T.One;
        if (T.IsPositiveInfinity(x)) return T.Zero;
        // Exact identity: avoids the clamping of the Lanczos coefficients for a=1.
        if (a == T.One) return T.Exp(-x);
        var tolerance = T.Max(Epsilon<T>(), Coefficients<T>.ApproximationTolerance);
        var logPrefix = a * T.Log(x) - x - LogGamma(a);
        if (!T.IsFinite(logPrefix)) throw new ArithmeticException("Gamma prefactor not representable.");
        if (x < a + T.One)
        {
            T term = T.One / a, sum = term;
            for (var n = 1; n <= 1000; n++)
            {
                term *= x / (a + N<T>(n));
                sum += term;
                if (!T.IsFinite(sum) || !T.IsFinite(term)) throw new ArithmeticException("Gamma series not representable.");
                if (T.Abs(term) <= T.Abs(sum) * tolerance)
                    return Probability(T.One - sum * T.Exp(logPrefix));
            }
            throw new ArithmeticException("Gamma series did not converge within 1000 iterations.");
        }
        var tiny = T.Sqrt(T.Epsilon);
        var b = x + T.One - a;
        var c = T.One / tiny;
        var d = T.One / b;
        var h = d;
        for (var i = 1; i <= 1000; i++)
        {
            var an = -N<T>(i) * (N<T>(i) - a);
            b += N<T>(2);
            d = an * d + b;
            if (T.Abs(d) < tiny) d = tiny;
            c = b + an / c;
            if (T.Abs(c) < tiny) c = tiny;
            d = T.One / d;
            var delta = d * c;
            h *= delta;
            if (!T.IsFinite(h) || !T.IsFinite(delta)) throw new ArithmeticException("Gamma continued fraction not representable.");
            if (T.Abs(delta - T.One) <= tolerance) return Probability(T.Exp(logPrefix) * h);
        }
        throw new ArithmeticException("Gamma continued fraction did not converge within 1000 iterations.");
    }

    /// <summary>P(χ²_k ≥ x), with k positive. Same limits as RegularizedGammaQ.</summary>
    public static T ChiSquareSurvival<T>(T x, int degreesOfFreedom) where T : IFloatingPointIeee754<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(degreesOfFreedom, 1);
        return RegularizedGammaQ(N<T>(degreesOfFreedom) / N<T>(2), x / N<T>(2));
    }

    private static T Probability<T>(T value) where T : IFloatingPointIeee754<T>
    {
        if (!T.IsFinite(value)) throw new ArithmeticException("Gamma probability not finite.");
        return T.Clamp(value, T.Zero, T.One);
    }
}
