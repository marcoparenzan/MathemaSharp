using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MathemaSharp.Core;
using MathemaSharp.Quantum;
using MathemaSharp.TimeSeries;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddTransient<Lab>();
using var host = builder.Build();
var topic = builder.Configuration["topic"] ?? "all";
Environment.ExitCode = host.Services.GetRequiredService<Lab>().Run(topic);

sealed class Lab(ILogger<Lab> logger)
{
    public int Run(string topic)
    {
        Dictionary<string, Action> examples = new()
        {
            ["generic"] = Generic, ["precision"] = Floating, ["decimal"] = Money,
            ["complex"] = ComplexNumbers, ["integers"] = Integers, ["simd"] = Simd, ["quantum"] = Quantum, ["timeseries"] = TimeSeries, ["formats"] = Formats
        };
        if (topic != "all" && !examples.ContainsKey(topic))
        {
            logger.LogError("Topic sconosciuto: {Topic}. Usare all, generic, precision, decimal, complex, integers, simd, quantum, timeseries, formats.", topic);
            return 2;
        }
        Console.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription}; TFM compilato: net11.0");
        foreach (var (name, action) in examples)
            if (topic == "all" || name == topic) { Console.WriteLine($"\n=== {name} ==="); action(); }
        return 0;
    }

    private static void Generic()
    {
        Console.WriteLine($"Sum<int>: {NumericAlgorithms.Sum<int>([1, 2, 3])}");
        Console.WriteLine($"Mean<decimal>: {NumericAlgorithms.Mean<decimal>([1m, 2m, 3m])}");
        Console.WriteLine($"Sum<Complex<double>>: {NumericAlgorithms.Sum<Complex<double>>([new(1, 2), new(3, 4)])}");
        Console.WriteLine($"Horner x²+2x+1 a x=3: {NumericAlgorithms.Horner<double>([1, 2, 1], 3)}");
        Console.WriteLine($"300 -> byte saturating {byte.CreateSaturating(300)}, truncating {byte.CreateTruncating(300)}");
        try { Console.WriteLine(byte.CreateChecked(300)); } catch (OverflowException) { Console.WriteLine("checked: OverflowException"); }
    }
    private static void Floating()
    {
        Console.WriteLine($"double 0.1+0.2: {0.1 + 0.2:R}; ==0.3: {0.1 + 0.2 == 0.3}");
        Console.WriteLine($"NearlyEqual: {Precision.NearlyEqual(0.1 + 0.2, 0.3)}");
        Console.WriteLine($"ULP a 1: {double.BitIncrement(1) - 1:R}; double.Epsilon: {double.Epsilon:R} (non machine epsilon)");
        double[] data = [1e16, 1, 1, -1e16];
        Console.WriteLine($"Somma naive: {NumericAlgorithms.Sum<double>(data)}; Kahan: {NumericAlgorithms.KahanSum<double>(data)}");
        Console.WriteLine($"Half 2048+1: {(Half)((Half)2048 + (Half)1)}; float 16777216+1: {16777216f + 1f}");
    }
    private static void Money()
    {
        Console.WriteLine($"decimal 0.1+0.2 == 0.3: {0.1m + 0.2m == 0.3m}; 1/3: {1m / 3m}");
        decimal[] lines = [0.005m, 0.005m, 0.005m];
        foreach (var mode in new[] { MidpointRounding.ToEven, MidpointRounding.AwayFromZero })
            Console.WriteLine($"{mode}: per riga {Precision.InvoiceTotal(lines, mode, true)}, sul totale {Precision.InvoiceTotal(lines, mode, false)}");
        Console.WriteLine($"1.00m: scala {Precision.DecimalScale(1.00m)}, valore uguale a 1m: {1.00m == 1m}");
        Console.WriteLine("decimal NON è Decimal128 IEEE 754: le librerie generiche usano Decimal32/64/128 (topic formats).");
    }
    private static void ComplexNumbers()
    {
        var z = new Complex<double>(3, 4);
        Console.WriteLine($"z={z}; modulo={z.GetMagnitude()}; fase={z.GetPhase()}; sqrt(-1)={Complex<double>.Sqrt(-Complex<double>.One)}");
        Console.WriteLine($"<z,z> hermitiano: {Signals.HermitianDot<double>([z], [z])}; bilineare: {NumericAlgorithms.Dot<Complex<double>>([z], [z])}");
        Console.WriteLine($"DFT impulso: {string.Join(", ", Signals.Dft<double>([Complex<double>.One, Complex<double>.Zero, Complex<double>.Zero, Complex<double>.Zero]))}");
        // Stessi algoritmi, altri formati IEEE di Generic Math di .NET 11.
        Console.WriteLine($"DFT in Decimal64, bin 1 di [1,0,0,0] = {Signals.Dft<Decimal64>([Complex<Decimal64>.One, Complex<Decimal64>.Zero, Complex<Decimal64>.Zero, Complex<Decimal64>.Zero])[1]}");
    }
    private static void Integers()
    {
        Console.WriteLine($"GCD(48,18): {NumericAlgorithms.Gcd(48, 18)}; 50!: {NumericAlgorithms.Factorial(50)}");
        Console.WriteLine($"Int128: {Int128.MaxValue}; BigInteger non ha limite fisso ma alloca memoria.");
        try { NumericAlgorithms.SumChecked<int>([int.MaxValue, 1]); } catch (OverflowException) { Console.WriteLine("SumChecked: overflow rilevato"); }
    }
    private static void Quantum()
    {
        var psi = QubitStates<double>.Superposition(1, Complex<double>.ImaginaryOne);
        Console.WriteLine($"|ψ> = (|0> + i|1>)/√2; <ψ|ψ> = {QuantumAlgorithms.InnerProduct(psi, psi)}; <0|1> = {QuantumAlgorithms.InnerProduct(QubitStates<double>.Zero, QubitStates<double>.One)}");
        Console.WriteLine($"<Z> su |0>: {QuantumAlgorithms.ExpectationValue(Pauli<double>.Z, QubitStates<double>.Zero).Real}; <X> su |+>: {QuantumAlgorithms.ExpectationValue(Pauli<double>.X, QubitStates<double>.Plus).Real:F3}; <Y> su |ψ>: {QuantumAlgorithms.ExpectationValue(Pauli<double>.Y, psi).Real:F3}");
        var (isEigen, value) = QuantumAlgorithms.CheckEigenstate(Pauli<double>.Z, QubitStates<double>.One);
        Console.WriteLine($"Z|1> = a|1>: {isEigen}, a = {value.Real}");
        var comm = QuantumAlgorithms.Commutator(Pauli<double>.X, Pauli<double>.Y);
        Console.WriteLine($"[X,Y] = 2iZ: elemento (0,0) = {comm[0, 0]}, (1,1) = {comm[1, 1]}");
        var (lp, _, lm, _) = QuantumAlgorithms.DiagonalizeHermitian2(Pauli<double>.X);
        Console.WriteLine($"Autovalori di X: {lp:F3}, {lm:F3}");
        var u = QuantumAlgorithms.TimeEvolution(Pauli<double>.X, Math.PI / 2);
        var evolved = QuantumAlgorithms.Apply(u, QubitStates<double>.Zero);
        Console.WriteLine($"U(π/2) per H=X unitario: {u.IsUnitary(1e-6)}; U|0> = [{evolved[0]:F3}, {evolved[1]:F3}]");
        var rho = QuantumAlgorithms.DensityMatrix(psi);
        var bloch = QuantumAlgorithms.BlochVector(rho);
        Console.WriteLine($"Purezza Tr(ρ²) = {QuantumAlgorithms.Purity(rho):F3}; Bloch = ({bloch.X:F3}, {bloch.Y:F3}, {bloch.Z:F3})");
    }
    private static void TimeSeries()
    {
        var random = new Random(2026);
        const int period = 12;
        var y = Enumerable.Range(0, 96).Select(t => 100 + 0.8 * t + 10 * Math.Sin(2 * Math.PI * t / period) + 2 * Simulation.StandardNormal(random)).ToArray();
        double[] train = y[..84], test = y[84..];
        var hw = ExponentialSmoothing.HoltWinters(train, period, 0.3, 0.05, 0.2, 12);
        var damped = ExponentialSmoothing.HoltWinters(train, period, 0.3, 0.05, 0.2, 12, phi: 0.9);
        Console.WriteLine($"MASE (m=12) su 12 mesi: seasonal naive {Accuracy.Mase(test, ReferenceForecasts.SeasonalNaive(train, period, 12), train, period):F3}, " +
            $"Holt-Winters {Accuracy.Mase(test, hw.Forecast, train, period):F3}, damped φ=0.9 {Accuracy.Mase(test, damped.Forecast, train, period):F3}");

        var ar = Simulation.Arma<double>(random, 5000, 0, [0.5, -0.3], [], 1);
        var yw = ArModel.FitYuleWalker(ar, 2);
        Console.WriteLine($"AR(2) vero φ=(0.5, -0.3); Yule-Walker φ=({yw.Phi[0]:F3}, {yw.Phi[1]:F3}); PACF lag 1..4: {string.Join(", ", SeriesStatistics.Pacf(ar, 4).Select(p => p.ToString("F3")))}");

        var arma = Simulation.Arma<double>(random, 5000, 0, [0.6], [0.4], 1);
        var hr = ArimaModel.FitHannanRissanen(arma, 1, 0, 1);
        var lb = SeriesStatistics.LjungBox(hr.Residuals(arma)[1..], 20, 2);
        Console.WriteLine($"ARMA(1,1) vero (0.6, 0.4); Hannan-Rissanen ({hr.Phi[0]:F3}, {hr.Theta[0]:F3}); Ljung-Box Q={lb.Statistic:F1}, p={lb.PValue:F3}");

        var alpha = KalmanFilter.LocalLevelSteadyStateGain(0.5, 2);
        Console.WriteLine($"Local level q=0.5, r=2: guadagno stazionario K={alpha:F4} = α del SES equivalente; SES ≡ ARIMA(0,1,1) con θ=α−1={alpha - 1:F4}");

        var garch = Garch11.FitVarianceTargeting<double>(Simulation.Garch(random, new Garch11<double>(0.05, 0.1, 0.85), 5000));
        Console.WriteLine($"GARCH(1,1) vero (α=0.10, β=0.85); stima (α={garch.Alpha:F3}, β={garch.Beta:F3}), σ̄²={garch.UnconditionalVariance:F3}");

        var stl = Stl.Decompose(y, period, robustIterations: 2, innerIterations: 1);
        Console.WriteLine($"STL: trend {stl.Trend[0]:F1} → {stl.Trend[^1]:F1}; ampiezza stagionale {stl.Seasonal.Max() - stl.Seasonal.Min():F1} (vera 20); σ residuo {Math.Sqrt(stl.Remainder.Average(r => r * r)):F2} (vero 2)");
    }
    /// <summary>Gli stessi algoritmi generici su formati IEEE diversi di Generic Math: l'errore segue la precisione del tipo.</summary>
    private static void Formats()
    {
        Console.WriteLine($"{"formato",-11} {"ε (ULP a 1)",-12} {"errore DFT",-12} {"errore U(π/2)|0⟩",-18} {"LS Hilbert 7×7",-16}");
        Row<float>(); Row<double>(); Row<Decimal64>(); Row<Decimal128>();
        Console.WriteLine("DFT: picco 8 di un coseno; U: exp(−iXπ/2)|0⟩ = −i|1⟩; LS: minimi quadrati QR su una matrice di Hilbert (cond ≈ 5e8), soluzione esatta = 1.");

        static void Row<T>() where T : IFloatingPointIeee754<T>, IMinMaxValue<T>
        {
            static string E(double v) => v == 0 ? "0" : v.ToString("E1");
            static T N(int v) => T.CreateChecked(v);
            var epsilon = double.CreateChecked(T.BitIncrement(T.One) - T.One);

            // DFT di un coseno a frequenza 3 su 16 campioni, con il coseno calcolato in T: picco esatto 8 al bin 3.
            var samples = new Complex<T>[16];
            for (var i = 0; i < 16; i++) samples[i] = new Complex<T>(T.Cos(T.Tau * N(3 * i) / N(16)), T.Zero);
            var dftError = double.CreateChecked(T.Abs(Signals.Dft<T>(samples)[3].GetMagnitude() - N(8))); // differenza calcolata in T, non dopo la conversione in double

            // exp(−iXπ/2)|0⟩ = −i|1⟩.
            var evolved = QuantumAlgorithms.Apply(QuantumAlgorithms.TimeEvolution(Pauli<T>.X, T.Pi / N(2)), QubitStates<T>.Zero);
            var qError = double.CreateChecked((evolved[1] - new Complex<T>(T.Zero, -T.One)).GetMagnitude());

            // Minimi quadrati su Hilbert 7×7 con termine noto H·1: la soluzione è il vettore di uno.
            const int n = 7;
            var hilbert = new T[n, n];
            var rhs = new T[n];
            for (var i = 0; i < n; i++)
                for (var j = 0; j < n; j++) { hilbert[i, j] = T.One / N(i + j + 1); rhs[i] += hilbert[i, j]; }
            string lsError;
            try
            {
                var beta = MatrixAlgebra.LeastSquares<T>(hilbert, rhs);
                lsError = E(beta.Max(b => double.CreateChecked(T.Abs(b - T.One))));
            }
            catch (ArgumentException) { lsError = "singolare"; }

            Console.WriteLine($"{typeof(T).Name,-11} {E(epsilon),-12} {E(dftError),-12} {E(qError),-18} {lsError,-16}");
        }
    }
    private static void Simd()
    {
        double[] a = Enumerable.Range(0, 19).Select(i => (double)i).ToArray();
        Console.WriteLine($"SIMD hardware: {Vector.IsHardwareAccelerated}; lanes double: {Vector<double>.Count}");
        Console.WriteLine($"Dot scalare: {NumericAlgorithms.Dot<double>(a, a)}; SIMD con coda: {Signals.VectorDot(a, a)}");
    }
}
