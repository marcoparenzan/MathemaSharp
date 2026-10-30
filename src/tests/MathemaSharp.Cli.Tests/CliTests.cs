namespace MathemaSharp.Cli.Tests;

public class CliTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("mathema-cli-").FullName;
    private string Csv(string text) { var p = Path.Combine(_dir, Guid.NewGuid().ToString("N") + ".csv"); File.WriteAllText(p, text); return p; }
    public void Dispose() => Directory.Delete(_dir, true);

    private static (int Code, string Out, string Err) Run(params string[] args)
    {
        var o = new StringWriter(); var e = new StringWriter();
        return (MathemaCli.Run(args, o, e), o.ToString(), e.ToString());
    }

    private const string Series = "m,v\n1,10\n2,12\n3,11\n4,15\n5,14\n6,18\n7,17\n8,21\n";

    [Fact]
    public void NoArgumentsPrintsUsage()
    {
        var (code, output, _) = Run();
        Assert.Equal(0, code);
        Assert.Contains("mathema stats", output);
    }

    [Fact]
    public void UnknownCommandExitsOne() => Assert.Equal(1, Run("fft").Code);

    [Fact]
    public void StatsMatchHandComputedValues()
    {
        var (code, output, _) = Run("stats", Csv(Series), "--column", "v");
        Assert.Equal(0, code);
        Assert.Contains("count 8", output);
        Assert.Contains("mean  14.75", output);
        Assert.Contains("min   10", output);
        Assert.Contains("max   21", output);
        Assert.Contains("50%   14.5", output);
    }

    [Fact]
    public void AcfStartsAtOne()
    {
        var (code, output, _) = Run("acf", Csv(Series), "--column", "v", "--lags", "3");
        Assert.Equal(0, code);
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(4, lines.Length);
        Assert.EndsWith(" 1", lines[0].TrimEnd());
    }

    [Fact]
    public void DifferenceUsesTheLag()
    {
        var (code, output, _) = Run("diff", Csv("v\n1\n4\n9\n16\n"), "--column", "v");
        Assert.Equal(0, code);
        var values = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1].Trim()).ToArray();
        Assert.Equal(["3", "5", "7"], values);
    }

    [Theory]
    [InlineData("naive", "21")]
    [InlineData("mean", "14.75")]
    public void ConstantForecastsRepeatTheirValue(string method, string expected)
    {
        var (code, output, _) = Run("forecast", Csv(Series), "--column", "v", "--method", method, "--horizon", "2");
        Assert.Equal(0, code);
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
        Assert.All(lines, l => Assert.EndsWith(expected, l.TrimEnd()));
    }

    [Fact]
    public void DriftExtendsTheAverageSlope()
    {
        var (_, output, _) = Run("forecast", Csv(Series), "--column", "v", "--method", "drift", "--horizon", "1");
        Assert.Contains("22.571429", output);
    }

    [Fact]
    public void SeasonalNaiveRepeatsTheLastPeriod()
    {
        var (_, output, _) = Run("forecast", Csv(Series), "--column", "v", "--method", "seasonal-naive", "--period", "4", "--horizon", "4");
        var values = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1].Trim()).ToArray();
        Assert.Equal(["14", "18", "17", "21"], values);
    }

    [Fact]
    public void EmptyAndTextCellsAreSkipped()
    {
        var (code, output, _) = Run("stats", Csv("v\n1\n\nx\n3\n"), "--column", "v");
        Assert.Equal(0, code);
        Assert.Contains("count 2", output);
    }

    [Fact]
    public void BadArgumentsExitTwo()
    {
        var file = Csv(Series);
        Assert.Equal(2, Run("stats", file, "--column", "nope").Code);        // unknown column
        Assert.Equal(2, Run("stats", file).Code);                            // --column missing
        Assert.Equal(2, Run("forecast", file, "--column", "v", "--method", "arima").Code); // unknown method
        Assert.Equal(2, Run("forecast", file, "--column", "v").Code);        // --method missing
        Assert.Equal(2, Run("stats").Code);                                  // file missing
    }

    [Fact]
    public void LagsAndAlphaAreValidated()
    {
        var file = Csv(Series);
        Assert.Equal(2, Run("acf", file, "--column", "v", "--lags", "8").Code);
        Assert.Equal(2, Run("acf", file, "--column", "v", "--lags", "0").Code);
        Assert.Equal(2, Run("forecast", file, "--column", "v", "--method", "ses", "--alpha", "1.5").Code);
        Assert.Equal(2, Run("forecast", file, "--column", "v", "--method", "seasonal-naive", "--period", "99").Code);
        Assert.Equal(2, Run("stats", Path.Combine(_dir, "none.csv"), "--column", "v").Code);
    }

    [Fact]
    public void SelfTestPasses() => Assert.Equal(0, Run("selftest").Code);
}
