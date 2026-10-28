#:property TargetFramework=net11.0
var p=@"D:\Dev\Science\PySharp\src\PySharpLib\Modules\AstPublic.cs";
var t=File.ReadAllText(p).Replace("Get(n,\"targets\")).Select", "Get(n,\"targets\")).Items.Select").Replace("Get(n,\"body\")).Select", "Get(n,\"body\")).Items.Select");File.WriteAllText(p,t);
