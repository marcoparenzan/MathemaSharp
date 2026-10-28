#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\MatSharp\src";
foreach(var f in Directory.GetFiles(root,"*.cs",SearchOption.AllDirectories).Where(p=>!p.Contains("\\obj\\")&&!p.Contains("\\bin\\")).Where(p=>Path.GetFileName(p) is "Mat.cs" or "MatEngine.cs" or "Bridge.cs")) {Console.WriteLine(f);Console.WriteLine(string.Join("\n",File.ReadLines(f).Take(85)));}
