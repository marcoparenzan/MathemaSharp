#:property TargetFramework=net11.0
using System.Diagnostics;
var root=@"D:\Dev\BuildArtifacts\net11-pack\consumer";Directory.CreateDirectory(root);
File.WriteAllText(Path.Combine(root,"Consumer.csproj"),"""
<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net11.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><RestorePackagesPath>D:\Dev\BuildArtifacts\net11-pack\consumer-cache</RestorePackagesPath></PropertyGroup>
<ItemGroup>
<PackageReference Include="MathemaSharp.Core" Version="0.2.1-net11-preview.1" />
<PackageReference Include="MathemaSharp.NDSharp" Version="0.2.1-net11-preview.1" />
<PackageReference Include="PySharp.Interpreter" Version="2.6.1-net11-preview.2" />
<PackageReference Include="MatSharp.Interpreter" Version="0.1.1-net11-preview.3" />
<PackageReference Include="MatSharp.NDSharp" Version="0.1.1-net11-preview.3" />
<PackageReference Include="PlotSharp" Version="0.1.1-net11-preview.2" />
<PackageReference Include="JupyterSharp.Engine" Version="0.3.1-net11-preview.5" />
</ItemGroup>
</Project>
""");
File.WriteAllText(Path.Combine(root,"NuGet.Config"),"""
<configuration><packageSources><clear/><add key="local" value="D:\Dev\NuGetLocalFeed"/><add key="nuget.org" value="https://api.nuget.org/v3/index.json"/></packageSources><packageSourceMapping><clear/><packageSource key="local"><package pattern="MathemaSharp.*"/><package pattern="NDSharp*"/><package pattern="PySharp*"/><package pattern="MatSharp*"/><package pattern="PlotSharp*"/><package pattern="JupyterSharp*"/></packageSource><packageSource key="nuget.org"><package pattern="*"/></packageSource></packageSourceMapping></configuration>
""");
File.WriteAllText(Path.Combine(root,"Program.cs"),""""
using System.Numerics;
using MathemaSharp.Core;
using MathemaSharp.NDSharp;
using PySharpLib;
var tiny=Decimal128.Parse("1e-25");
var array=new Decimal128[]{Decimal128.One,tiny}.ToTypedNDArray();
if(array.Sum()-Decimal128.One!=tiny)throw new Exception("NDArray precision lost");
Complex<double>[] input=[new(1,0),new(2,0),new(3,0)];
var restored=Signals.Ifft<double>(Signals.Fft<double>(input));
for(int i=0;i<input.Length;i++)if(double.Abs(restored[i].Real-input[i].Real)>1e-12)throw new Exception("FFT roundtrip failed");
var a=new double[,]{{2,0},{0,4}}.ToTypedNDArray();
var b=new double[]{2,8}.ToTypedNDArray();
var x=a.FactorizeLu().Solve(b).ToVector();
if(x[0]!=1 || x[1]!=2)throw new Exception("Bridge solve failed");
var output=new StringWriter();
var pythonEngine = new PyEngine(output);
pythonEngine.SetVariable("ClrList", typeof(List<>));
pythonEngine.SetVariable("D128", typeof(Decimal128));
pythonEngine.Run("""
from mathema import decimal128
x = decimal128('1.0000000000000000000000001')
print(x - 1 == decimal128('1e-25'))
scope = {}
exec(compile('answer = 42', 'smoke.py', 'exec'), scope)
print(scope['answer'])
List128 = ClrList[D128]
values = List128()
values.Add(decimal128('1.0000000000000000000000001'))
print(values[0] - 1 == decimal128('1e-25'))
""");
if(output.ToString()!="True\n42\nTrue\n")throw new Exception("Packaged PySharp failed: "+output);
var matlab = new MatSharpLib.MatEngine();
MatSharpLib.NDSharpBridge.NDSharpFunctions.Register(matlab);
matlab.Run("a=[decimal128(1) decimal128(0);decimal128(0) decimal128(1)]; b=[decimal128('1.0000000000000000000000001');decimal128(2)]; x=a\\b;");
var precise = (MatSharpLib.Runtime.GenericScalar<Decimal128>)matlab.Eval("x(1)-decimal128(1)");
if (precise.Value != tiny) throw new Exception("Packaged MATLAB solver lost precision");
Console.WriteLine("NuGet .NET 11: Decimal128, NDArray, FFT Bluestein, solver bridge, PySharp operators/compile and MatSharp typed backslash OK");
"""");
foreach(var command in new[]{"restore","build","run"})
{
var psi=new ProcessStartInfo("dotnet"){WorkingDirectory=root,UseShellExecute=false};
psi.ArgumentList.Add(command);
if(command=="run"){psi.ArgumentList.Add("--project");psi.ArgumentList.Add("Consumer.csproj");psi.ArgumentList.Add("--no-build");}
else psi.ArgumentList.Add("Consumer.csproj");
using var child=Process.Start(psi)!;child.WaitForExit();if(child.ExitCode!=0)return child.ExitCode;
}
return 0;
