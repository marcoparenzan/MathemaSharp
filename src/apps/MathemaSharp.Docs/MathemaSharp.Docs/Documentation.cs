using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MathemaSharp.Docs;

public sealed record DocumentEntry(string Path, string Title);

public sealed class Documentation
{
    private readonly string root;
    private readonly MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().DisableHtml().Build();
    public Documentation(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configured = configuration["Documentation:Root"];
        var directory = new DirectoryInfo(environment.ContentRootPath);
        while (configured is null && directory is not null && !File.Exists(System.IO.Path.Combine(directory.FullName, "PROMPTS.md"))) directory = directory.Parent;
        root = System.IO.Path.GetFullPath(configured ?? directory?.FullName ?? throw new InvalidOperationException("Configurare Documentation:Root con il percorso del repository."));
    }

    public IReadOnlyList<DocumentEntry> List() => Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories)
        .Select(file => System.IO.Path.GetRelativePath(root, file).Replace('\\', '/'))
        .Where(path => Resolve(path, false) is not null)
        .Select(path => new DocumentEntry(path, Title(path)))
        .OrderBy(entry => entry.Path).ToArray();

    private string Title(string path)
    {
        var heading = File.ReadLines(System.IO.Path.Combine(root, path)).FirstOrDefault(line => line.StartsWith("# "));
        return heading?[2..] ?? System.IO.Path.GetFileNameWithoutExtension(path);
    }

    public string? Resolve(string path, bool asset)
    {
        if (string.IsNullOrWhiteSpace(path) || System.IO.Path.IsPathRooted(path)) return null;
        path = path.Replace('\\', '/');
        var allowedRoot = path.StartsWith("refs/", StringComparison.Ordinal) || path.StartsWith("scenari/", StringComparison.Ordinal) || path.StartsWith("notebooks/", StringComparison.Ordinal);
        var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
        if (asset ? !allowedRoot || ext is not (".png" or ".jpg" or ".jpeg" or ".webp" or ".ipynb")
            : ext != ".md" || !(allowedRoot || !path.Contains('/')) || path == "PROMPTS.md") return null;
        var full = System.IO.Path.GetFullPath(System.IO.Path.Combine(root, path));
        if (!full.StartsWith(root + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(full)) return null;
        // Reject symlinks/junctions, including parent directories.
        for (FileSystemInfo? item = new FileInfo(full); item is not null && item.FullName != root;
            item = item is FileInfo file ? file.Directory : ((DirectoryInfo)item).Parent)
            if ((item.Attributes & FileAttributes.ReparsePoint) != 0) return null;
        return full;
    }

    public string? Render(string path)
    {
        var file = Resolve(path, false);
        if (file is null) return null;
        var document = Markdown.Parse(File.ReadAllText(file), pipeline);
        foreach (var link in document.Descendants<LinkInline>())
        {
            var url = link.Url;
            if (string.IsNullOrEmpty(url) || url.StartsWith('#')) continue;
            if (Uri.TryCreate(url, UriKind.Absolute, out var absolute))
            {
                if (absolute.Scheme is not ("http" or "https" or "mailto")) link.Url = "#";
                continue;
            }
            var hash = url.IndexOf('#');
            var fragment = hash >= 0 ? url[hash..] : "";
            var relative = Uri.UnescapeDataString(hash >= 0 ? url[..hash] : url);
            var full = System.IO.Path.GetFullPath(System.IO.Path.Combine(root, System.IO.Path.GetDirectoryName(path) ?? "", relative));
            if (Directory.Exists(full)) full = System.IO.Path.Combine(full, "README.md");
            var target = System.IO.Path.GetRelativePath(root, full).Replace('\\', '/');
            link.Url = Resolve(target, link.IsImage || !target.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) is null ? "#" :
                (target.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ? "/?doc=" : "/assets/documentation?path=") + Uri.EscapeDataString(target) + fragment;
        }
        return document.ToHtml(pipeline);
    }
}
