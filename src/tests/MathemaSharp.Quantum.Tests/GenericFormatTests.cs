using System.Numerics;
using MathemaSharp.Core;

namespace MathemaSharp.Quantum.Tests;

/// <summary>The quantum library is generic over <c>T : IFloatingPointIeee754&lt;T&gt;, IMinMaxValue&lt;T&gt;</c> (the constraints of <see cref="Complex{T}"/>).</summary>
public class GenericFormatTests
{
    private static T V<T>(double x) where T : IFloatingPointIeee754<T> => T.CreateChecked(x);

    [Fact] public void PauliAlgebraHoldsInEveryFormat()
    {
        Check<float>(1e-5);
        Check<double>(1e-12);
        Check<Decimal64>(1e-12);
        Check<Decimal128>(1e-12);

        static void Check<T>(double tolerance) where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
        {
            // [X,Y] = 2iZ
            var commutator = QuantumAlgorithms.Commutator(Pauli<T>.X, Pauli<T>.Y);
            var expected = new Complex<T>(T.Zero, V<T>(2)) * Pauli<T>.Z;
            for (var i = 0; i < 2; i++)
                for (var j = 0; j < 2; j++)
                    Assert.True((commutator[i, j] - expected[i, j]).GetMagnitude() <= V<T>(tolerance), $"{typeof(T).Name} [X,Y]");

            // ⟨Y⟩ = 1 su (|0⟩ + i|1⟩)/√2, e Bloch = (0,1,0)
            var psi = QubitStates<T>.Superposition(Complex<T>.One, Complex<T>.ImaginaryOne);
            Assert.True(T.Abs(QuantumAlgorithms.ExpectationValue(Pauli<T>.Y, psi).Real - T.One) <= V<T>(tolerance), $"{typeof(T).Name} <Y>");
            var bloch = QuantumAlgorithms.BlochVector(QuantumAlgorithms.DensityMatrix<T>(psi));
            Assert.True(T.Abs(bloch.X) <= V<T>(tolerance) && T.Abs(bloch.Y - T.One) <= V<T>(tolerance) && T.Abs(bloch.Z) <= V<T>(tolerance), $"{typeof(T).Name} Bloch");
        }
    }

    [Fact] public void TimeEvolutionIsUnitaryAndExactInDecimal128()
    {
        // exp(−iXπ/2)|0⟩ = −i|1⟩. With 24 Taylor terms after scaling the theoretical error is ~1e-33: in Decimal128 it is verified to 1e-28,
        // a level that no double (ε ≈ 2e-16) can reach.
        var u = QuantumAlgorithms.TimeEvolution(Pauli<Decimal128>.X, Decimal128.Pi / V<Decimal128>(2));
        Assert.True(u.IsUnitary());
        var evolved = QuantumAlgorithms.Apply(u, QubitStates<Decimal128>.Zero);
        Assert.True(evolved[0].GetMagnitude() < V<Decimal128>(1e-28), "amplitude of |0⟩");
        Assert.True((evolved[1] - new Complex<Decimal128>(Decimal128.Zero, -Decimal128.One)).GetMagnitude() < V<Decimal128>(1e-28), "−i|1⟩");
        // The default tolerance scales with the format: √ε.
        Assert.True(ComplexMatrix<Decimal128>.DefaultTolerance < V<Decimal128>(1e-15));
        Assert.True(ComplexMatrix<double>.DefaultTolerance > 1e-9);
    }
}
