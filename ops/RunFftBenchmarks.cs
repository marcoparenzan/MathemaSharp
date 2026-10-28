#:property TargetFramework=net11.0
using System.Diagnostics;
var output=@"D:\Dev\BuildArtifacts\MathemaSharp\benchmarks";
Directory.CreateDirectory(output);
var psi=new ProcessStartInfo("dotnet"){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true};
foreach(var arg in new[]{"run","--project","src/benchmarks/MathemaSharp.Benchmarks/MathemaSharp.Benchmarks.csproj","-c","Release","--","--filter","*FftBenchmarks*","--job","Short","--inProcess","--artifacts",output})psi.ArgumentList.Add(arg);
using var child=Process.Start(psi)!;
var stdout=child.StandardOutput.ReadToEndAsync();var stderr=child.StandardError.ReadToEndAsync();
child.WaitForExit();
var log=stdout.GetAwaiter().GetResult()+stderr.GetAwaiter().GetResult();
File.WriteAllText(Path.Combine(output,"fft-short-run.log"),log);
Console.WriteLine("Exit: "+child.ExitCode);
var index=log.LastIndexOf("// * Summary *",StringComparison.Ordinal);
Console.WriteLine(index>=0?log[index..]:log[^Math.Min(log.Length,6000)..]);
return child.ExitCode;
