#:property TargetFramework=net11.0
using System.Diagnostics;
var start=new ProcessStartInfo(@"D:\Dev\Science\MathemaSharp\ops\.venv-oracles\Scripts\python.exe"){UseShellExecute=false};
start.ArgumentList.Add(Path.GetFullPath("ops/generate-fft-oracles.py"));
using var process=Process.Start(start)!;process.WaitForExit();return process.ExitCode;
