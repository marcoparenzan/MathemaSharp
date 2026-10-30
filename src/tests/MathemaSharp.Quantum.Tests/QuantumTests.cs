using System.Numerics;
using MathemaSharp.Core;
using MathemaSharp.Quantum;

namespace MathemaSharp.Quantum.Tests;

public class QuantumTests
{
    [Fact] public void InnerProductAndNorm()
    {
        Assert.Equal(Complex<double>.One, QuantumAlgorithms.InnerProduct(QubitStates<double>.Zero, QubitStates<double>.Zero));
        Assert.Equal(Complex<double>.Zero, QuantumAlgorithms.InnerProduct(QubitStates<double>.Zero, QubitStates<double>.One));
        Assert.True(Precision.NearlyEqual(1, QuantumAlgorithms.Norm(QubitStates<double>.Plus)));
    }

    [Fact] public void NormalizeRejectsZeroVector() =>
        Assert.Throws<ArgumentException>(() => QuantumAlgorithms.Normalize<double>([Complex<double>.Zero, Complex<double>.Zero]));

    [Fact] public void SuperpositionIsNormalized()
    {
        var psi = QubitStates<double>.Superposition(3, 4);
        Assert.True(Precision.NearlyEqual(1, QuantumAlgorithms.Norm(psi)));
        Assert.True(Precision.NearlyEqual(0.36, (psi[0] * Complex<double>.Conjugate(psi[0])).Real, 1e-9, 1e-9));
        Assert.True(Precision.NearlyEqual(0.64, (psi[1] * Complex<double>.Conjugate(psi[1])).Real, 1e-9, 1e-9));
    }

    [Fact] public void ProjectorIsIdempotentAndTraceOne()
    {
        var p = QuantumAlgorithms.Projector(QubitStates<double>.Plus);
        Assert.True(Precision.NearlyEqual(1, p.Trace().Real, 1e-9, 1e-9));
        var p2 = p * p;
        for (var i = 0; i < 2; i++)
            for (var j = 0; j < 2; j++)
                Assert.True(Precision.NearlyEqual(p[i, j].Real, p2[i, j].Real, 1e-9, 1e-9) && Precision.NearlyEqual(p[i, j].Imaginary, p2[i, j].Imaginary, 1e-9, 1e-9));
    }

    [Theory]
    [MemberData(nameof(PauliMatrices))]
    public void PauliMatricesAreHermitianAndUnitary(ComplexMatrix<double> pauli)
    {
        Assert.True(pauli.IsHermitian());
        Assert.True(pauli.IsUnitary());
    }

    public static IEnumerable<object[]> PauliMatrices()
    {
        yield return [Pauli<double>.I];
        yield return [Pauli<double>.X];
        yield return [Pauli<double>.Y];
        yield return [Pauli<double>.Z];
    }

    [Fact] public void ExpectationValuesOfZ()
    {
        Assert.True(Precision.NearlyEqual(1, QuantumAlgorithms.ExpectationValue(Pauli<double>.Z, QubitStates<double>.Zero).Real, 1e-9, 1e-9));
        Assert.True(Precision.NearlyEqual(-1, QuantumAlgorithms.ExpectationValue(Pauli<double>.Z, QubitStates<double>.One).Real, 1e-9, 1e-9));
        Assert.True(Precision.NearlyEqual(1, QuantumAlgorithms.ExpectationValue(Pauli<double>.X, QubitStates<double>.Plus).Real, 1e-9, 1e-9));
    }

    [Fact] public void CommutatorOfPauliMatricesIs2iZ()
    {
        var commutator = QuantumAlgorithms.Commutator(Pauli<double>.X, Pauli<double>.Y);
        var expected = 2 * Complex<double>.ImaginaryOne * Pauli<double>.Z;
        for (var i = 0; i < 2; i++)
            for (var j = 0; j < 2; j++)
                Assert.True(Precision.NearlyEqual(commutator[i, j].Real, expected[i, j].Real, 1e-9, 1e-9) && Precision.NearlyEqual(commutator[i, j].Imaginary, expected[i, j].Imaginary, 1e-9, 1e-9));
    }

    [Fact] public void CommutingOperatorsHaveZeroCommutator()
    {
        var commutator = QuantumAlgorithms.Commutator(Pauli<double>.Z, Pauli<double>.I);
        for (var i = 0; i < 2; i++)
            for (var j = 0; j < 2; j++)
                Assert.Equal(Complex<double>.Zero, commutator[i, j]);
    }

    [Fact] public void CheckEigenstateOfZ()
    {
        var (isEigenstate, eigenvalue) = QuantumAlgorithms.CheckEigenstate(Pauli<double>.Z, QubitStates<double>.Zero);
        Assert.True(isEigenstate);
        Assert.True(Precision.NearlyEqual(1, eigenvalue.Real, 1e-9, 1e-9));

        var (isNotEigenstate, _) = QuantumAlgorithms.CheckEigenstate(Pauli<double>.Z, QubitStates<double>.Plus);
        Assert.False(isNotEigenstate);
    }

    [Theory]
    [MemberData(nameof(PauliEigenpairs))]
    public void DiagonalizeHermitian2MatchesKnownPauliSpectrum(ComplexMatrix<double> pauli, Complex<double>[] expectedPlus, Complex<double>[] expectedMinus)
    {
        var (lambdaPlus, vectorPlus, lambdaMinus, vectorMinus) = QuantumAlgorithms.DiagonalizeHermitian2(pauli);
        Assert.True(Precision.NearlyEqual(1, lambdaPlus, 1e-9, 1e-9));
        Assert.True(Precision.NearlyEqual(-1, lambdaMinus, 1e-9, 1e-9));
        AssertSameStateUpToGlobalPhase(expectedPlus, vectorPlus);
        AssertSameStateUpToGlobalPhase(expectedMinus, vectorMinus);
    }

    public static IEnumerable<object[]> PauliEigenpairs()
    {
        yield return [Pauli<double>.Z, QubitStates<double>.Zero, QubitStates<double>.One];
        yield return [Pauli<double>.X, QubitStates<double>.Plus, QubitStates<double>.Minus];
    }

    private static void AssertSameStateUpToGlobalPhase(Complex<double>[] expected, Complex<double>[] actual)
    {
        // |⟨expected|actual⟩| = 1 for normalised vectors that differ only by a global phase.
        var overlap = QuantumAlgorithms.InnerProduct(expected, actual).GetMagnitude();
        Assert.True(Precision.NearlyEqual(1, overlap, 1e-9, 1e-9));
    }

    [Fact] public void MatrixExpOfZeroIsIdentity()
    {
        var exp = QuantumAlgorithms.MatrixExp(ComplexMatrix<double>.Zero(2, 2));
        var identity = ComplexMatrix<double>.Identity(2);
        for (var i = 0; i < 2; i++)
            for (var j = 0; j < 2; j++)
                Assert.True(Precision.NearlyEqual(exp[i, j].Real, identity[i, j].Real, 1e-9, 1e-9) && Precision.NearlyEqual(exp[i, j].Imaginary, identity[i, j].Imaginary, 1e-9, 1e-9));
    }

    [Fact] public void TimeEvolutionOfZIsUnitaryAndMatchesClosedForm()
    {
        var u = QuantumAlgorithms.TimeEvolution(Pauli<double>.Z, Math.PI);
        Assert.True(u.IsUnitary(1e-6));
        // exp(-i*Z*pi) = diag(exp(-i*pi), exp(i*pi)) = diag(-1,-1) = -I.
        var minusIdentity = (-Complex<double>.One) * ComplexMatrix<double>.Identity(2);
        for (var i = 0; i < 2; i++)
            for (var j = 0; j < 2; j++)
                Assert.True(Precision.NearlyEqual(u[i, j].Real, minusIdentity[i, j].Real, 1e-6, 1e-6) && Precision.NearlyEqual(u[i, j].Imaginary, minusIdentity[i, j].Imaginary, 1e-6, 1e-6));
    }

    [Fact] public void DensityMatrixOfPureStateHasPurityOne()
    {
        var rho = QuantumAlgorithms.DensityMatrix(QubitStates<double>.PlusI);
        Assert.True(Precision.NearlyEqual(1, QuantumAlgorithms.Purity(rho), 1e-9, 1e-9));
    }

    [Fact] public void BlochVectorOfBasisStates()
    {
        var zero = QuantumAlgorithms.BlochVector(QuantumAlgorithms.DensityMatrix(QubitStates<double>.Zero));
        Assert.True(Precision.NearlyEqual(0, zero.X, 1e-9, 1e-9) && Precision.NearlyEqual(0, zero.Y, 1e-9, 1e-9) && Precision.NearlyEqual(1, zero.Z, 1e-9, 1e-9));

        var plus = QuantumAlgorithms.BlochVector(QuantumAlgorithms.DensityMatrix(QubitStates<double>.Plus));
        Assert.True(Precision.NearlyEqual(1, plus.X, 1e-9, 1e-9) && Precision.NearlyEqual(0, plus.Y, 1e-9, 1e-9) && Precision.NearlyEqual(0, plus.Z, 1e-9, 1e-9));
    }

    [Fact] public void OperatorApplicationMatchesMatrixVectorMultiplication()
    {
        var result = QuantumAlgorithms.Apply(Pauli<double>.X, QubitStates<double>.Zero);
        Assert.Equal(QubitStates<double>.One[0], result[0]);
        Assert.Equal(QubitStates<double>.One[1], result[1]);
    }

    [Fact] public void DaggerAndIdentityContracts()
    {
        var y = Pauli<double>.Y;
        var yDagger = y.Dagger();
        for (var i = 0; i < 2; i++)
            for (var j = 0; j < 2; j++)
                Assert.Equal(Complex<double>.Conjugate(y[j, i]), yDagger[i, j]);
        Assert.Throws<ArgumentException>(() => Pauli<double>.X * new ComplexMatrix<double>(3, 3));
    }
}
