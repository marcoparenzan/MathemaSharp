using Ecosystem.Validation;

namespace Ecosystem.Validation.Tests;

public class TrxEvidenceTests
{
    [Theory]
    [InlineData("<TestRun />")]
    [InlineData("<Other />")]
    [InlineData("<TestRun><Results><UnitTestResult outcome='InProgress'/></Results></TestRun>")]
    [InlineData("<TestRun><Results><UnitTestResult /></Results></TestRun>")]
    public void RejectsEmptyOrIncompleteEvidence(string xml) => Assert.Throws<FormatException>(() => TrxEvidence.Parse(xml));

    [Fact]
    public void ParsesNamespacedResultsAndDoesNotCountSkipsAsPassed()
    {
        var evidence = TrxEvidence.Parse("""
            <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
            <Results>
            <UnitTestResult outcome="Passed" testName="pass" />
            <UnitTestResult outcome="Failed" testName="fail" />
            <UnitTestResult outcome="NotExecuted" testName="skip"><Output><ErrorInfo><Message> No server </Message></ErrorInfo></Output></UnitTestResult>
            </Results></TestRun>
            """);
        Assert.Equal(3, evidence.Results);
        Assert.Equal(1, evidence.Passed);
        Assert.Equal(1, evidence.Failed);
        Assert.Equal(new SkippedTest("skip", "No server"), Assert.Single(evidence.Skipped));
    }

    [Fact]
    public void MissingReasonIsExplicitAndSkipsSorted()
    {
        var evidence = TrxEvidence.Parse("<TestRun><Results><UnitTestResult outcome='NotExecuted' testName='z'/><UnitTestResult outcome='NotExecuted' testName='a'/></Results></TestRun>");
        Assert.Equal("a", evidence.Skipped[0].Name);
        Assert.Contains("not automatically approved", evidence.Skipped[0].Reason);
        Assert.Equal(0, evidence.Passed);
    }
}
