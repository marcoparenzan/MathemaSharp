#:property TargetFramework=net11.0
var p=@"D:\Dev\Science\PySharp\Directory.Build.props";
if(File.Exists(p))throw new Exception("Existing configuration must be merged, not replaced");
File.WriteAllText(p,"""
<Project>
 <PropertyGroup>
  <UseArtifactsOutput>true</UseArtifactsOutput>
  <ArtifactsPath>D:\Dev\BuildArtifacts\PySharp</ArtifactsPath>
  <PackageOutputPath>D:\Dev\NuGetLocalFeed</PackageOutputPath>
 </PropertyGroup>
</Project>
""");
