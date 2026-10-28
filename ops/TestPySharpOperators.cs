#:property TargetFramework=net11.0
using System.Diagnostics;
var psi=new ProcessStartInfo("dotnet"){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true};
foreach(var arg in new[]{"test",@"D:\Dev\Science\PySharp\src\PySharp.Tests\PySharp.Tests.csproj","-c","Release","-p:UseArtifactsOutput=true",@"-p:ArtifactsPath=D:\Dev\BuildArtifacts\PySharp","--filter","FullyQualifiedName~ClrOperatorTests|FullyQualifiedName~MathemaModuleTests|FullyQualifiedName~GenericBindingTests","--verbosity","minimal"})psi.ArgumentList.Add(arg);
psi.Environment["DOTNET_ROLL_FORWARD"]="LatestMajor";
psi.Environment["DOTNET_ROLL_FORWARD_TO_PRERELEASE"]="1";
using var child=Process.Start(psi)!;var stdout=child.StandardOutput.ReadToEndAsync();var stderr=child.StandardError.ReadToEndAsync();child.WaitForExit();var log=stdout.GetAwaiter().GetResult()+stderr.GetAwaiter().GetResult();File.WriteAllText(@"D:\Dev\BuildArtifacts\PySharp\operator-tests.log",log);Console.WriteLine(log[^Math.Min(log.Length,4000)..]);return child.ExitCode;
