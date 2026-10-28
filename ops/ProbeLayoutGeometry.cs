#:property TargetFramework=net11.0
#:property EnableTrimAnalyzer=false
using System.Reflection;
using System.Runtime.Loader;
using System.Diagnostics;
var dir=@"D:\Dev\BuildArtifacts\PySharp\bin\PySharp.Tests\release";
AssemblyLoadContext.Default.Resolving+=(_,n)=>File.Exists(Path.Combine(dir,n.Name+".dll"))?Assembly.LoadFrom(Path.Combine(dir,n.Name+".dll")):null;
System.Runtime.InteropServices.NativeLibrary.Load(Path.Combine(dir,"runtimes","win-x64","native","libSkiaSharp.dll"));
var asm=Assembly.LoadFrom(Path.Combine(dir,"PySharp.Tests.dll"));
var source="""
import numpy as np
import matplotlib.pyplot as plt
fig, ax = plt.subplots()
im = ax.imshow(np.arange(12).reshape(3, 4))
fig.colorbar(im, ax=ax)
fig.tight_layout()
print(tuple(ax.get_position().bounds))
print(tuple(fig.axes[1].get_position().bounds))
print(ax.get_yticks())
""";
var nd=Assembly.LoadFrom(Path.Combine(dir,"NDSharp.Plot.dll")); Console.WriteLine(nd.Location+" "+nd.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);
Console.WriteLine("PYSHARP");Console.WriteLine(asm.GetType("PySharp.Tests.Py")!.GetMethod("Run")!.Invoke(null,new object[]{source}));
var psi=new ProcessStartInfo(@"D:\Dev\Science\PySharp\tools\oracle\.venv\Scripts\python.exe") {RedirectStandardOutput=true,UseShellExecute=false};psi.Environment["MPLBACKEND"]="Agg";psi.ArgumentList.Add("-c");psi.ArgumentList.Add(source);
using var process=Process.Start(psi)!;Console.WriteLine("CPYTHON\n"+process.StandardOutput.ReadToEnd());process.WaitForExit();
