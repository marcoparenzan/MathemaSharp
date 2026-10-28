using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using MathemaSharp.Docs;

namespace MathemaSharp.Docs.Tests;

public class DocumentationTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly Documentation docs;
    public DocumentationTests()
    {
        Directory.CreateDirectory(Path.Combine(root, "refs"));
        File.WriteAllText(Path.Combine(root, "README.md"), "# Home\n[Next](refs/topic.md)\n<script>alert(1)</script>\n[Bad](javascript:alert(1))");
        File.WriteAllText(Path.Combine(root, "refs/topic.md"), "# Topic");
        File.WriteAllText(Path.Combine(root, "PROMPTS.md"), "Private");
        File.WriteAllText(Path.Combine(root, "secret.json"), "secret");
        docs = new Documentation(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Documentation:Root"] = root }).Build(), new EnvironmentStub());
    }
    [Fact] public void ListsOnlyAllowedMarkdown() => Assert.Equal(2, docs.List().Count);
    [Theory]
    [InlineData("../README.md")]
    [InlineData("PROMPTS.md")]
    [InlineData("secret.json")]
    [InlineData("refs/../../README.md")]
    public void RejectsUnsafePaths(string path) => Assert.Null(docs.Resolve(path, false));
    [Fact] public void RewritesLinksAndDisablesHtml()
    {
        var html = docs.Render("README.md")!;
        Assert.Contains("/?doc=refs%2Ftopic.md", html);
        Assert.DoesNotContain("<script>", html);
        Assert.DoesNotContain("href=\"javascript:", html);
    }
    [Fact] public void MissingDocumentReturnsNull() => Assert.Null(docs.Render("missing.md"));
    public void Dispose() => Directory.Delete(root, true);
    private sealed class EnvironmentStub : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = ".";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
