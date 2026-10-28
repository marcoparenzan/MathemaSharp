#:property TargetFramework=net11.0
using System.Diagnostics;
var source=File.ReadAllText(@"D:\Dev\Science\PySharp\src\PySharp.Tests\M6_Stdlib\StdlibTests.cs");
var begin=source.IndexOf("public void MutableMapping_pop_popitem_setdefault_and_clear_are_real_mixin_methods()");
var rawStart=source.IndexOf("Run(\"\"\"",begin)+7;var rawEnd=source.IndexOf("\"\"\"));",rawStart);
var snippet=string.Join("\n",source[rawStart..rawEnd].Split('\n').Select(l=>l.StartsWith("            ")?l[12..]:l));
var psi=new ProcessStartInfo(@"D:\Dev\Science\PySharp\tools\oracle\.venv\Scripts\python.exe") {RedirectStandardOutput=true,RedirectStandardError=true,UseShellExecute=false};psi.ArgumentList.Add("-c");psi.ArgumentList.Add(snippet);
using var process=Process.Start(psi)!;var output=process.StandardOutput.ReadToEnd();var error=process.StandardError.ReadToEnd();process.WaitForExit();Console.WriteLine("Exit: "+process.ExitCode);Console.WriteLine(output);Console.WriteLine(error);
File.WriteAllText(@"D:\Dev\BuildArtifacts\net11-migration\keyerror-cpython.txt",output+error);
