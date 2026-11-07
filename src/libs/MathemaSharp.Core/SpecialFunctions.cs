// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Globalization;
using System.Numerics;

namespace MathemaSharp.Core;

/// <summary>
/// The error function, the gamma function and the accurate small-argument exponential and logarithm, generic over the IEEE formats (<c>float</c>, <c>double</c>, <c>Decimal32/64/128</c>).
/// The algorithms are the ones libm-class implementations use, so in <c>double</c> the results agree with C99 <c>erf</c>, <c>erfc</c>,
/// <c>tgamma</c> and <c>lgamma</c> (CPython's <c>math</c> module) to a few units in the last place.
/// </summary>
/// <remarks>
/// <see cref="Erf{T}"/> and <see cref="Erfc{T}"/> need nothing but <c>exp</c>: a positive power series where it is stable, a continued fraction in the tail.
/// <see cref="Gamma{T}"/> and <see cref="LogAbsGamma{T}"/> use a rational Lanczos sum (g = 6.0246800407767296) whose coefficients are read in the format of the call
/// from decimal strings; they reach into the range of <c>float</c> (about 1e10 in the coefficients), so <c>Half</c> is not supported for them.
/// The accuracy is that of binary64 even in the wider decimal formats: the algorithm is not a 34-digit one.
/// </remarks>
public static class SpecialFunctions
{
    private static class Lanczos<T> where T : IFloatingPointIeee754<T>
    {
        public static readonly T G = P("6.024680040776729583740234375");
        public static readonly T GMinusHalf = P("5.524680040776729583740234375");
        public static readonly T[] Num =
        {
            P("23531376880.410759688572007674451636754734846804940"), P("42919803642.649098768957899047001988850926355848959"),
            P("35711959237.355668049440185451547166705960488635843"), P("17921034426.037209699919755754458931112671403265390"),
            P("6039542586.3520280050642916443072979210699388420708"), P("1439720407.3117216736632230727949123939715485786772"),
            P("248874557.86205415651146038641322942321632125127801"), P("31426415.585400194380614231628318205362874684987640"),
            P("2876370.6289353724412254090516208496135991145378768"), P("186056.26539522349504029498971604569928220784236328"),
            P("8071.6720023658162106380029022722506138218516325024"), P("210.82427775157934587250973392071336271166969580291"),
            P("2.5066282746310002701649081771338373386264310793408"),
        };
        public static readonly T[] Den =
        {
            T.Zero, P("39916800"), P("120543840"), P("150917976"), P("105258076"), P("45995730"), P("13339535"),
            P("2637558"), P("357423"), P("32670"), P("1925"), P("66"), T.One,
        };
        private static T P(string s) => T.Parse(s, CultureInfo.InvariantCulture);
    }

    private const int MaxIterations = 2000;

    private static T Ulp<T>() where T : IFloatingPointIeee754<T> => T.BitIncrement(T.One) - T.One;
    private static T Two<T>() where T : IFloatingPointIeee754<T> => T.One + T.One;
    private static T Half<T>() where T : IFloatingPointIeee754<T> => T.One / Two<T>();

    /// <summary>e^x − 1, accurate for small |x| (Kahan's formula, a few ulp). The BCL's <c>double.ExpM1</c> and <c>GenericMath.ExpM1</c> are <c>Exp(x) − 1</c> on this runtime:
    /// <c>ExpM1(1e-20)</c> is 0 and <c>ExpM1(1e-10)</c> is wrong from the eighth digit. ExpM1(±0) = ±0, ExpM1(−∞) = −1, ExpM1(+∞) = +∞, an overflow gives +∞, NaN gives NaN.</summary>
    public static T ExpM1<T>(T x) where T : IFloatingPointIeee754<T>
    {
        var u = T.Exp(x);
        if (u == T.One) return x;
        if (T.IsInfinity(u) || T.IsNaN(u)) return u;
        var um1 = u - T.One;
        if (um1 == -T.One) return um1;
        return um1 * x / T.Log(u);
    }

    /// <summary>ln(1 + x), accurate for small |x| (Goldberg's formula, a few ulp). The BCL's <c>double.LogP1</c> and <c>GenericMath.LogP1</c> are <c>Log(1 + x)</c> on this runtime.
    /// LogP1(±0) = ±0, LogP1(−1) = −∞, x &lt; −1 gives NaN, LogP1(+∞) = +∞, NaN gives NaN.</summary>
    public static T LogP1<T>(T x) where T : IFloatingPointIeee754<T>
    {
        var u = T.One + x;
        if (u == T.One) return x;
        if (T.IsInfinity(u) || T.IsNaN(u)) return T.Log(u);
        return T.Log(u) * x / (u - T.One);
    }

    /// <summary>The error function erf(x) = 2/√π ∫₀ˣ e^(−t²) dt. Odd, erf(±∞) = ±1, NaN gives NaN, erf(−0) = −0.</summary>
    public static T Erf<T>(T x) where T : IFloatingPointIeee754<T>
    {
        if (T.IsNaN(x)) return x;
        if (T.IsInfinity(x)) return T.CopySign(T.One, x);
        var a = T.Abs(x);
        return T.CopySign(a < Two<T>() ? ErfSeries(a) : T.One - ErfcFraction(a), x);
    }

    /// <summary>The complementary error function erfc(x) = 1 − erf(x), accurate in the tail (erfc(26) = 5.6e-296 in double). erfc(+∞) = 0, erfc(−∞) = 2, NaN gives NaN.</summary>
    public static T Erfc<T>(T x) where T : IFloatingPointIeee754<T>
    {
        if (T.IsNaN(x)) return x;
        if (T.IsPositiveInfinity(x)) return T.Zero;
        if (T.IsNegativeInfinity(x)) return Two<T>();
        var a = T.Abs(x);
        var positive = a < T.One ? T.One - ErfSeries(a) : ErfcFraction(a);
        return T.IsNegative(x) ? Two<T>() - positive : positive;
    }

    // erf(a) = 2/√π · e^(−a²) · Σ 2ⁿ a^(2n+1) / (2n+1)!!   — every term is positive, so there is no cancellation.
    private static T ErfSeries<T>(T a) where T : IFloatingPointIeee754<T>
    {
        var term = a;
        var sum = a;
        var a2 = a * a;
        var ulp = Ulp<T>() / (Two<T>() * Two<T>());
        for (var n = 0; n < MaxIterations; n++)
        {
            term *= Two<T>() * a2 / T.CreateChecked(2 * n + 3);
            sum += term;
            if (term <= ulp * sum) break;
        }
        return Two<T>() / T.Sqrt(T.Pi) * T.Exp(-a2) * sum;
    }

    // erfc(a) = e^(−a²)/√π / (a + (1/2)/(a + 1/(a + (3/2)/(a + 2/(a + …)))))  for a ≥ 1, by the modified Lentz method.
    private static T ErfcFraction<T>(T a) where T : IFloatingPointIeee754<T>
    {
        var tiny = T.Epsilon;
        var f = a;
        var c = a;
        var d = T.Zero;
        var ulp = Ulp<T>();
        for (var k = 1; k < MaxIterations; k++)
        {
            var an = T.CreateChecked(k) / Two<T>();
            d = a + an * d;
            d = d == T.Zero ? T.One / tiny : T.One / d;
            c = a + an / c;
            var delta = c * d;
            f *= delta;
            if (T.Abs(delta - T.One) <= ulp) break;
        }
        return T.Exp(-(a * a)) / T.Sqrt(T.Pi) / f;
    }

    private static T LanczosSum<T>(T x) where T : IFloatingPointIeee754<T>
    {
        var num = T.Zero;
        var den = T.Zero;
        var coefficients = Lanczos<T>.Num;
        var denominators = Lanczos<T>.Den;
        if (x < T.CreateChecked(5))
            for (var i = coefficients.Length; --i >= 0;) { num = num * x + coefficients[i]; den = den * x + denominators[i]; }
        else
            for (var i = 0; i < coefficients.Length; i++) { num = num / x + coefficients[i]; den = den / x + denominators[i]; }
        return num / den;
    }

    /// <summary>The gamma function Γ(x), for positive and negative arguments. The poles (zero and the negative integers) and −∞ give NaN, +∞ gives +∞,
    /// an argument whose result is out of range gives +∞ (for x &gt; 171.62 in double). Positive integers up to 23 return the exact factorial.</summary>
    public static T Gamma<T>(T x) where T : IFloatingPointIeee754<T>
    {
        if (T.IsNaN(x)) return x;
        if (T.IsPositiveInfinity(x)) return x;
        if (T.IsNegativeInfinity(x)) return T.NaN;
        if (x == T.Zero) return T.NaN;
        if (x < T.Zero && T.Floor(x) == x) return T.NaN;
        if (x > T.Zero && T.Floor(x) == x && x <= T.CreateChecked(23))
        {
            var factorial = T.One;
            for (var k = 2; T.CreateChecked(k) < x; k++) factorial *= T.CreateChecked(k);
            return factorial;
        }
        var ax = T.Abs(x);
        if (ax < T.CreateChecked(1e-20)) return T.One / x;
        if (ax > T.CreateChecked(200)) return x > T.Zero ? T.PositiveInfinity : T.Zero / -T.SinPi(x);

        var half = Half<T>();
        var y = ax + Lanczos<T>.GMinusHalf;
        T q, z;
        if (ax > Lanczos<T>.GMinusHalf) { q = y - ax; z = q - Lanczos<T>.GMinusHalf; }
        else { q = y - Lanczos<T>.GMinusHalf; z = q - ax; }
        z = z * Lanczos<T>.G / y;

        T r;
        // As libm does, the power y^(x-1/2) is split into two square-root factors where it would leave the range of the format (x >= 140 in double, about 26 in float).
        var split = ax >= T.CreateChecked(140);
        if (x < T.Zero)
        {
            r = -T.Pi / T.SinPi(ax) / ax * T.Exp(y) / LanczosSum(ax);
            r -= z * r;
            var power = split ? T.PositiveInfinity : T.Pow(y, ax - half);
            if (!T.IsInfinity(power)) r /= power;
            else { var root = T.Pow(y, ax / Two<T>() - half / Two<T>()); r /= root; r /= root; }
        }
        else
        {
            r = LanczosSum(ax) / T.Exp(y);
            r -= z * r;
            var power = split ? T.PositiveInfinity : T.Pow(y, ax - half);
            if (!T.IsInfinity(power)) r *= power;
            else { var root = T.Pow(y, ax / Two<T>() - half / Two<T>()); r *= root; r *= root; }
        }
        return r;
    }

    /// <summary>ln |Γ(x)|, for every real x. The poles (zero and the negative integers) and ±∞ give +∞, NaN gives NaN; Γ(1) and Γ(2) give exactly 0.</summary>
    public static T LogAbsGamma<T>(T x) where T : IFloatingPointIeee754<T>
    {
        if (T.IsNaN(x)) return x;
        if (T.IsInfinity(x)) return T.PositiveInfinity;
        if (x == T.One || x == Two<T>()) return T.Zero;
        var ax = T.Abs(x);
        if (ax < T.CreateChecked(1e-20)) return -T.Log(ax);
        var half = Half<T>();
        var r = T.Log(LanczosSum(ax)) - Lanczos<T>.G;
        r += (ax - half) * (T.Log(ax + Lanczos<T>.G - half) - T.One);
        if (x < T.Zero) r = T.Log(T.Pi) - T.Log(T.Abs(T.SinPi(ax))) - T.Log(ax) - r;
        return r;
    }
}
