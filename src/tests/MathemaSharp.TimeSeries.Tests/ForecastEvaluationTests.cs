using MathemaSharp.TimeSeries;

namespace MathemaSharp.TimeSeries.Tests;

public class ForecastEvaluationTests
{
    [Fact]
    public void PatchesAreOrderedCompleteAndIndependent()
    {
        int[] history = [0, 1, 2, 3, 4, 5, 6];
        var patches = HistoryPatches.Extract<int>(history, 3, 2);
        Assert.Equal(3, patches.Length);
        Assert.Equal(new[] { 2, 3, 4 }, patches[1]);
        patches[0][2] = 99;
        Assert.Equal(2, history[2]);
        Assert.Equal(2, patches[1][0]);
        Assert.Empty(HistoryPatches.Extract<int>(history, 8));
    }

    [Fact]
    public void RollingOriginPassesOnlyPastAndAlignsGap()
    {
        double[] series = [0, 1, 2, 3, 4, 5, 6, 7, 8];
        var seen = new List<int>();
        var folds = RollingOrigin.Evaluate<double>(series, 3, 2, (train, h) =>
        {
            seen.Add(train.Length);
            Assert.Equal(train.Length - 1, train[^1]);
            return ReferenceForecasts.Drift<double>(train, h);
        }, step: 2, gap: 1);
        Assert.Equal(new[] { 3, 5 }, seen);
        Assert.Equal(new[] { 4d, 5d }, folds[0].Actual);
        Assert.All(folds, f => { Assert.Equal(0d, f.Mae); Assert.Equal(0d, f.Rmse); });
    }

    [Fact]
    public void NaiveHasKnownMultiStepErrors()
    {
        double[] series = [0, 1, 2, 3, 4];
        var folds = RollingOrigin.Evaluate<double>(series, 3, 2,
            (train, h) => ReferenceForecasts.Naive<double>(train, h));
        Assert.Single(folds);
        Assert.Equal(1.5, folds[0].Mae);
        Assert.Equal(Math.Sqrt(2.5), folds[0].Rmse);
    }

    [Fact]
    public void CallbackCannotMutateSource()
    {
        double[] series = [1, 2, 3, 4];
        RollingOrigin.Evaluate<double>(series, 2, 1, (train, h) =>
        {
            train[0] = 100;
            return new double[h];
        });
        Assert.Equal(1d, series[0]);
    }

    [Fact]
    public void RejectsInvalidParametersAndPredictions()
    {
        double[] series = [1, 2, 3];
        Assert.Throws<ArgumentOutOfRangeException>(() => HistoryPatches.Extract<double>(series, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => HistoryPatches.Extract<double>(series, 1, 0));
        Assert.Throws<ArgumentException>(() => RollingOrigin.Evaluate<double>(series, 3, 1, (_, h) => new double[h]));
        Assert.Throws<ArgumentException>(() => RollingOrigin.Evaluate<double>(series, 1, 1, (_, _) => []));
        Assert.Throws<ArgumentException>(() => RollingOrigin.Evaluate<double>(series, 1, 1, (_, _) => [double.NaN]));
        Assert.Throws<ArgumentException>(() => RollingOrigin.Evaluate<double>([1, double.PositiveInfinity], 1, 1, (_, _) => [0]));
    }
}
