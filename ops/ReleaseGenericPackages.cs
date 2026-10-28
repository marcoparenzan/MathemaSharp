#:property TargetFramework=net11.0
using System.Diagnostics;
using System.Xml.Linq;
var science=@"D:\Dev\Science";
var feed=@"D:\Dev\NuGetLocalFeed";
var nd=Path.Combine(science,"NDSharp","src","NDSharp","NDSharp.csproj");
var doc=XDocument.Load(nd); doc.Descendants("Version").Single().Value="2.8.0"; doc.Save(nd);
void Run(params string[] args) {
 var p=new ProcessStartInfo("dotnet"){UseShellExecute=false};foreach(var a in args)p.ArgumentList.Add(a);
 using var child=Process.Start(p)!;child.WaitForExit();if(child.ExitCode!=0)throw new Exception("dotnet failed: "+string.Join(' ',args));
}
Run("pack",nd,"-c","Release","--output",feed,"--nologo","-v","quiet");
Run("pack",Path.Combine(science,"MathemaSharp","src","MathemaSharp.slnx"),"-c","Release","--output",feed,"--nologo","-v","quiet");
var smoke=@"D:\Dev\BuildArtifacts\MathemaSharp\nuget-smoke";
Directory.CreateDirectory(smoke);
File.WriteAllText(Path.Combine(smoke,"Smoke.csproj"),"""
<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net11.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup>
<ItemGroup><PackageReference Include="NDSharp" Version="2.8.0" /></ItemGroup>
</Project>
""");
File.WriteAllText(Path.Combine(smoke,"Program.cs"),"""
using NDSharp;
var array = new NDArray<decimal>(new decimal[] { 1m, 0.0000000000000000000000001m }, 2);
if(array.Sum()-1m!=0.0000000000000000000000001m) throw new Exception("Precision lost");
if(array.Slice(0,1,2,-1).Dot(new NDArray<decimal>(new decimal[]{1m,1m},2))!=array.Sum()) throw new Exception("View dot failed");
Console.WriteLine("NuGet NDSharp 2.8.0: storage and strided dot OK");
""");
Run("restore",Path.Combine(smoke,"Smoke.csproj"),"--source",feed);
Run("run","--project",Path.Combine(smoke,"Smoke.csproj"),"--no-restore");
