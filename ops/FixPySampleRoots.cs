#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp";
var props=Path.Combine(root,"Directory.Build.props");var text=File.ReadAllText(props).Replace("</Project>"," <ItemGroup><AssemblyMetadata Include=\"RepositoryRoot\" Value=\"$(MSBuildThisFileDirectory)\" /></ItemGroup>\n</Project>");File.WriteAllText(props,text);
foreach(var p in Directory.EnumerateFiles(Path.Combine(root,"src","PySharp.Tests"),"*.cs",SearchOption.AllDirectories)) {
var t=File.ReadAllText(p);var old="Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, \"..\", \"..\", \"..\", \"..\", \"..\", \"samples\"))";
if(t.Contains(old)){File.WriteAllText(p,t.Replace(old,"Path.Combine(TestRepository.Root, \"samples\")"));Console.WriteLine(p);}
}
File.WriteAllText(Path.Combine(root,"src","PySharp.Tests","TestRepository.cs"),"""
using System.Reflection;
namespace PySharp.Tests;
internal static class TestRepository
{
    public static string Root => typeof(TestRepository).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a=>a.Key=="RepositoryRoot").Value!;
}
""");
