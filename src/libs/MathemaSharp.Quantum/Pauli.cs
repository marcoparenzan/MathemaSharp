using System.Numerics;

namespace MathemaSharp.Quantum;

/// <summary>Operatori di spin-1/2. Base computazionale {|0⟩,|1⟩}: autostati di Z con autovalori +1,-1. Generico: <c>Pauli&lt;double&gt;.X</c>, <c>Pauli&lt;Decimal128&gt;.Y</c>...</summary>
public static class Pauli<T> where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    public static ComplexMatrix<T> I => ComplexMatrix<T>.Identity(2);

    public static ComplexMatrix<T> X => new(new Complex<T>[,]
    {
        { Complex<T>.Zero, Complex<T>.One },
        { Complex<T>.One, Complex<T>.Zero }
    });

    public static ComplexMatrix<T> Y => new(new Complex<T>[,]
    {
        { Complex<T>.Zero, -Complex<T>.ImaginaryOne },
        { Complex<T>.ImaginaryOne, Complex<T>.Zero }
    });

    public static ComplexMatrix<T> Z => new(new Complex<T>[,]
    {
        { Complex<T>.One, Complex<T>.Zero },
        { Complex<T>.Zero, -Complex<T>.One }
    });
}

/// <summary>Stati base e combinazioni comuni di un singolo qubit, come ket (array di due ampiezze).</summary>
public static class QubitStates<T> where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
{
    private static T InvSqrt2 => T.One / T.Sqrt(T.CreateChecked(2));

    public static Complex<T>[] Zero => [Complex<T>.One, Complex<T>.Zero];
    public static Complex<T>[] One => [Complex<T>.Zero, Complex<T>.One];
    public static Complex<T>[] Plus => [InvSqrt2, InvSqrt2];
    public static Complex<T>[] Minus => [InvSqrt2, -InvSqrt2];
    public static Complex<T>[] PlusI => [InvSqrt2, new Complex<T>(T.Zero, InvSqrt2)];
    public static Complex<T>[] MinusI => [InvSqrt2, new Complex<T>(T.Zero, -InvSqrt2)];

    /// <summary>α|0⟩+β|1⟩ normalizzato. Richiede |α|²+|β|² &gt; 0.</summary>
    public static Complex<T>[] Superposition(Complex<T> alpha, Complex<T> beta) => QuantumAlgorithms.Normalize<T>([alpha, beta]);
}
