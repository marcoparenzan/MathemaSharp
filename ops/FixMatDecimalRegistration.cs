#:property TargetFramework=net11.0
var p=@"D:\Dev\Science\MatSharp\src\MatSharpLib\Builtins\BuiltinRegistry.cs";
File.WriteAllText(p,File.ReadAllText(p).Replace("GenericNumbers.Register(it);","MatSharpLib.Builtins.GenericNumbers.Register(it);"));
