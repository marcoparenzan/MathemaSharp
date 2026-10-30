#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp\src\JupyterSharp.Kernels.PySharp";
foreach(var f in Directory.EnumerateFiles(root,"*",SearchOption.AllDirectories).Where(p=>!p.Contains("\\obj\\")&&!p.Contains("\\bin\\"))){Console.WriteLine(f);if(Path.GetExtension(f) is ".json" or ".csproj")Console.WriteLine(File.ReadAllText(f));}
