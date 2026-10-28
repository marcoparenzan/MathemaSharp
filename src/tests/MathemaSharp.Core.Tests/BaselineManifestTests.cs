using System.Text.Json;

namespace MathemaSharp.Core.Tests;

public class BaselineManifestTests
{
    [Fact]
    public void ManifestExplicitlySelectsVerifiedNotebooksOnly()
    {
        var root=NotebookTests.RepositoryRoot().FullName;
        using var document=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"ops","baseline-notebooks.json")));
        var paths=document.RootElement.GetProperty("notebooks").EnumerateArray().Select(value=>value.GetString()!).ToArray();
        Assert.Equal(24,paths.Length);
        Assert.Equal(paths.Length,paths.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        foreach(var path in paths)
        {
            Assert.True(File.Exists(Path.Combine(root,path)),path);
            Assert.StartsWith("notebooks/",path);
            Assert.Equal(3,path.Split('/').Length);
            if (path.Contains("NDSharp")) Assert.Equal("notebooks/NDSharp/01-generic-math.ipynb", path);
            Assert.DoesNotContain("pysharp",path.ToLowerInvariant());
            Assert.DoesNotContain("Scenarios",path);
        }
        Assert.Contains("notebooks/LinearAlgebra/13-solve-diagnostics.ipynb",paths);
        Assert.Contains("notebooks/Quantum/13-independent-oracles.ipynb",paths);
        Assert.Contains("notebooks/TimeSeries/13-independent-oracles.ipynb",paths);
    }
    [Fact]
    public void ExclusionReasonsAreExplicit()
    {
        var root=NotebookTests.RepositoryRoot().FullName;
        using var document=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"ops","baseline-notebooks.json")));
        var reasons=document.RootElement.GetProperty("exclusionReasons");
        foreach(var name in new[] {"NDSharp","PySharp","Scenarios","Other"})
            Assert.False(string.IsNullOrWhiteSpace(reasons.GetProperty(name).GetString()));
    }
}
