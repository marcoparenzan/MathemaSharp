// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
namespace MathemaSharp;

/// <summary>Integer exponents of the seven SI base dimensions. Dimension vectors do not
/// encode unit scale, offsets, or physical values; angles use SI dimensionless convention.</summary>
/// <param name="Metres">Length exponent.</param>
/// <param name="Kilograms">Mass exponent.</param>
/// <param name="Seconds">Time exponent.</param>
/// <param name="Amperes">Electric-current exponent.</param>
/// <param name="Kelvins">Temperature exponent.</param>
/// <param name="Moles">Amount-of-substance exponent.</param>
/// <param name="Candelas">Luminous-intensity exponent.</param>
public readonly record struct SiDimensions(int Metres, int Kilograms, int Seconds, int Amperes, int Kelvins, int Moles, int Candelas)
{
    /// <summary>Dimensionless vector; all exponents zero.</summary>
    public static SiDimensions Dimensionless => default;

    /// <summary>Raises dimensions to an integer power by multiplying exponents.
    /// Checked integer overflow throws OverflowException; no unit-scale computation occurs.</summary>
    public SiDimensions Power(int exponent) => new(
        checked(Metres * exponent), checked(Kilograms * exponent), checked(Seconds * exponent),
        checked(Amperes * exponent), checked(Kelvins * exponent), checked(Moles * exponent), checked(Candelas * exponent));

    /// <summary>Dimensions of a product: adds exponents with checked overflow.</summary>
    public static SiDimensions operator +(SiDimensions left, SiDimensions right) => new(
        checked(left.Metres + right.Metres), checked(left.Kilograms + right.Kilograms),
        checked(left.Seconds + right.Seconds), checked(left.Amperes + right.Amperes),
        checked(left.Kelvins + right.Kelvins), checked(left.Moles + right.Moles), checked(left.Candelas + right.Candelas));
}
