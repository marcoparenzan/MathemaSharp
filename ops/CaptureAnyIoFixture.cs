#:property TargetFramework=net11.0
var p=@"D:\Dev\Science\PySharp\src\PySharp.Tests\M16_FastApi\FastApiInstallFixture.cs";
var t=File.ReadAllText(p);
var anchor="        installer.Install(\"h11\");";
t=t.Replace(anchor,anchor+"""

        // Preserve source/version evidence for reproducibility of the unpinned dependency probe.
        var evidence = Path.Combine(@"D:\Dev\BuildArtifacts\PySharp", "dependency-evidence", "anyio");
        Directory.CreateDirectory(evidence);
        foreach (var source in Directory.EnumerateFiles(Path.Combine(SitePackages, "anyio"), "*.py", SearchOption.AllDirectories))
        {
            var target = Path.Combine(evidence, Path.GetRelativePath(Path.Combine(SitePackages, "anyio"), source));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(source, target, true);
        }
        foreach (var metadata in Directory.EnumerateFiles(SitePackages, "METADATA", SearchOption.AllDirectories).Where(p => Path.GetFileName(Path.GetDirectoryName(p)!).StartsWith("anyio-")))
            File.Copy(metadata, Path.Combine(evidence, "METADATA"), true);
""");File.WriteAllText(p,t);
