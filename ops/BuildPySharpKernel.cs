#:property TargetFramework=net11.0
using System.Diagnostics;
if(args.Any(a=>a!="--source-plots"))throw new ArgumentException("Only optional --source-plots is supported.");
bool sourcePlots=args.Contains("--source-plots");
var output=sourcePlots?@"D:\Dev\BuildArtifacts\PySharp\kernel-source-plots":@"D:\Dev\BuildArtifacts\PySharp\kernel";
Directory.CreateDirectory(output);
var start=new ProcessStartInfo("dotnet"){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true};
foreach(var a in new[]{"build",@"D:\Dev\Science\PySharp\src\JupyterSharp.Kernels.PySharp\JupyterSharp.Kernels.PySharp.csproj","-c","Release","-o",output,"-p:UseArtifactsOutput=true",@"-p:ArtifactsPath=D:\Dev\BuildArtifacts\PySharp","-p:CopyLocalLockFileAssemblies=true"})start.ArgumentList.Add(a);
if(sourcePlots)start.ArgumentList.Add("-p:ScienceUseSourcePlots=true");
using var child=Process.Start(start)!;var stdout=child.StandardOutput.ReadToEndAsync();var stderr=child.StandardError.ReadToEndAsync();child.WaitForExit();var log=stdout.GetAwaiter().GetResult()+stderr.GetAwaiter().GetResult();File.WriteAllText(Path.Combine(output,"build.log"),log);Console.WriteLine(log[^Math.Min(log.Length,5000)..]);return child.ExitCode;
