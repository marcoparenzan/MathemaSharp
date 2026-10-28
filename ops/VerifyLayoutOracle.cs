#:property TargetFramework=net11.0
using System.Diagnostics;
var psi=new ProcessStartInfo(@"D:\Dev\Science\PySharp\tools\oracle\.venv\Scripts\python.exe") {RedirectStandardOutput=true,RedirectStandardError=true,UseShellExecute=false};
psi.Environment["MPLBACKEND"]="Agg";
psi.ArgumentList.Add(@"D:\Dev\Science\PySharp\src\PySharp.Tests\Oracle\matplotlib\P5_layout.py");
using var process=Process.Start(psi)!;var output=process.StandardOutput.ReadToEnd();var error=process.StandardError.ReadToEnd();process.WaitForExit();Console.WriteLine("Exit: "+process.ExitCode);Console.WriteLine(output);Console.WriteLine(error);
File.WriteAllText(@"D:\Dev\BuildArtifacts\net11-migration\layout-cpython.txt",output+error);
var p=@"D:\Dev\Science\PySharp\src\PySharp.Tests\M6_Stdlib\StdlibTests.cs";
if(File.ReadAllText(@"D:\Dev\BuildArtifacts\net11-migration\keyerror-cpython.txt").Contains("KeyError: 'missing'")) File.WriteAllText(p,File.ReadAllText(p).Replace("KeyError: missing\\n", "KeyError: 'missing'\\n"));
