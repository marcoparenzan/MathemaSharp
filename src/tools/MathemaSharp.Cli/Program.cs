using System.Globalization;
using System.Reflection;
using MathemaSharp.TimeSeries.Interop;

return MathemaCli.Run(args, Console.Out, Console.Error);

/// <summary>The mathema command line. A class so tests call it without spawning a process.</summary>
public static class MathemaCli
{
    public const string Usage = """
        mathema — MàthemaSharp command line (time-series tools on one CSV column)

          mathema stats      <data.csv> --column c
          mathema acf        <data.csv> --column c [--lags N]          autocorrelation (default 10 lags)
          mathema diff       <data.csv> --column c [--lag L]           differences at lag L (default 1)
          mathema forecast   <data.csv> --column c --method naive|mean|drift|seasonal-naive|ses
                                        [--horizon H] [--period P] [--alpha A]
          mathema selftest
          mathema --version | --help

        Options: --sep <c> field separator (default ','). Empty or non-numeric cells are skipped.
        Defaults: --horizon 6, --period 12 (seasonal-naive), --alpha fitted (ses).
        Output is plain text, one value per line with its index. Exit codes: 0 ok, 1 unknown command, 2 usage or input error.
        """;

    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        if (args.Length == 0 || args[0] is "help" or "--help" or "-h") { output.WriteLine(Usage); return 0; }
        try
        {
            switch (args[0])
            {
                case "--version" or "-v" or "version":
                    output.WriteLine(typeof(MathemaCli).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "unknown");
                    return 0;
                case "stats": return Stats(Load(args), output);
                case "acf": return Acf(Load(args), PositiveInt(args, "--lags", 10), output);
                case "diff": return Diff(Load(args), PositiveInt(args, "--lag", 1), output);
                case "forecast": return Forecast(args, Load(args), output);
                case "selftest": return SelfTest(output, error);
                default:
                    error.WriteLine($"Unknown command '{args[0]}'.");
                    error.WriteLine(Usage);
                    return 1;
            }
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or FormatException or UnauthorizedAccessException)
        {
            error.WriteLine($"Error: {ex.Message}");
            return 2;
        }
    }

    private static string? Option(string[] args, string name)
    {
        var i = Array.IndexOf(args, name);
        if (i < 0) return null;
        return i + 1 < args.Length ? args[i + 1] : throw new ArgumentException($"{name} needs a value.");
    }

    private static int PositiveInt(string[] args, string name, int fallback)
    {
        var text = Option(args, name);
        if (text is null) return fallback;
        return int.TryParse(text, out var n) && n >= 1 ? n : throw new ArgumentException($"{name} must be an integer of at least 1, got '{text}'.");
    }

    private static string F(double v) => v.ToString("0.######", CultureInfo.InvariantCulture);

    private static double[] Load(string[] args)
    {
        if (args.Length < 2 || args[1].StartsWith('-')) throw new ArgumentException("A CSV file is required.");
        if (!File.Exists(args[1])) throw new IOException($"File not found: {args[1]}");
        var column = Option(args, "--column") ?? throw new ArgumentException("--column is required.");
        var sep = Option(args, "--sep");
        if (sep is { Length: not 1 }) throw new ArgumentException("--sep must be a single character.");
        var lines = File.ReadAllLines(args[1]).Where(l => l.Length > 0).ToList();
        if (lines.Count < 2) throw new ArgumentException("The CSV needs a header row and at least one data row.");
        var s = sep is null ? ',' : sep[0];
        var header = lines[0].Split(s).Select(h => h.Trim().Trim('"')).ToArray();
        var at = Array.FindIndex(header, h => string.Equals(h, column, StringComparison.OrdinalIgnoreCase));
        if (at < 0) throw new ArgumentException($"No column '{column}'. Columns: {string.Join(", ", header)}.");
        var values = new List<double>();
        foreach (var line in lines.Skip(1))
        {
            var cells = line.Split(s);
            if (at < cells.Length && double.TryParse(cells[at].Trim().Trim('"'), NumberStyles.Float, CultureInfo.InvariantCulture, out var v) && double.IsFinite(v))
                values.Add(v);
        }
        if (values.Count == 0) throw new ArgumentException($"Column '{column}' has no numeric values.");
        return values.ToArray();
    }

    private static int Stats(double[] y, TextWriter output)
    {
        var sorted = y.OrderBy(v => v).ToArray();
        var mean = Statistics.Mean(y);
        var std = y.Length > 1 ? Math.Sqrt(y.Sum(v => (v - mean) * (v - mean)) / (y.Length - 1)) : double.NaN;
        output.WriteLine($"count {y.Length}");
        output.WriteLine($"mean  {F(mean)}");
        output.WriteLine($"std   {F(std)}");
        output.WriteLine($"min   {F(sorted[0])}");
        foreach (var p in new[] { 0.25, 0.5, 0.75 })
            output.WriteLine($"{F(p * 100)}%".PadRight(6) + F(Statistics.Quantile(y, p)));
        output.WriteLine($"max   {F(sorted[^1])}");
        return 0;
    }

    private static int Acf(double[] y, int lags, TextWriter output)
    {
        if (lags >= y.Length) throw new ArgumentException($"--lags ({lags}) must be smaller than the number of values ({y.Length}).");
        var acf = Statistics.Acf(y, lags);
        for (var k = 0; k < acf.Length; k++) output.WriteLine($"lag {k,3}  {F(acf[k])}");
        return 0;
    }

    private static int Diff(double[] y, int lag, TextWriter output)
    {
        if (lag >= y.Length) throw new ArgumentException($"--lag ({lag}) must be smaller than the number of values ({y.Length}).");
        var d = Statistics.Difference(y, lag);
        for (var i = 0; i < d.Length; i++) output.WriteLine($"{i + lag,5}  {F(d[i])}");
        return 0;
    }

    private static int Forecast(string[] args, double[] y, TextWriter output)
    {
        var method = Option(args, "--method") ?? throw new ArgumentException("--method is required: naive, mean, drift, seasonal-naive or ses.");
        var horizon = PositiveInt(args, "--horizon", 6);
        double[] f;
        switch (method)
        {
            case "naive": f = Forecasts.Naive(y, horizon); break;
            case "mean": f = Forecasts.Mean(y, horizon); break;
            case "drift":
                if (y.Length < 2) throw new ArgumentException("drift needs at least two values.");
                f = Forecasts.Drift(y, horizon); break;
            case "seasonal-naive":
                var period = PositiveInt(args, "--period", 12);
                if (period > y.Length) throw new ArgumentException($"--period ({period}) is longer than the series ({y.Length}).");
                f = Forecasts.SeasonalNaive(y, period, horizon); break;
            case "ses":
                var alphaText = Option(args, "--alpha");
                double alpha;
                if (alphaText is null) alpha = Smoothing.FitSesAlpha(y);
                else if (!double.TryParse(alphaText, NumberStyles.Float, CultureInfo.InvariantCulture, out alpha) || alpha is <= 0 or > 1)
                    throw new ArgumentException($"--alpha must be a number in (0, 1], got '{alphaText}'.");
                output.WriteLine($"alpha {F(alpha)}");
                f = Smoothing.Ses(y, alpha, horizon).Forecast; break;
            default: throw new ArgumentException($"Unknown method '{method}'. Use naive, mean, drift, seasonal-naive or ses.");
        }
        for (var i = 0; i < f.Length; i++) output.WriteLine($"+{i + 1,-3} {F(f[i])}");
        return 0;
    }

    private static int SelfTest(TextWriter output, TextWriter error)
    {
        double[] y = [1, 2, 3, 4, 5];
        var ok = Math.Abs(Statistics.Mean(y) - 3) < 1e-12 && Forecasts.Naive(y, 2).SequenceEqual([5.0, 5.0]) && Statistics.Difference(y, 1).All(d => Math.Abs(d - 1) < 1e-12);
        output.WriteLine(ok ? "selftest: ok" : "selftest: FAILED");
        if (!ok) error.WriteLine("A basic statistic or forecast on 1..5 was wrong.");
        return ok ? 0 : 1;
    }
}
