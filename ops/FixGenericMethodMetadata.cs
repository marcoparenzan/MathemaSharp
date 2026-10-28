#:property TargetFramework=net11.0
var path=@"D:\Dev\Science\PySharp\src\PySharpLib\Runtime\Clr.cs";
var text=File.ReadAllText(path).Replace("\r\n","\n");
var begin=text.IndexOf("public sealed class ClrMethod");var end=text.IndexOf("/// <summary>Marshalling",begin);
var section=text[begin..end];
if(!section.Contains("public Type[]? TypeArguments"))section=section.Replace("    public ClrMethod(","    public Type[]? TypeArguments { get; }\n\n    public ClrMethod(");
if(!section.Contains("TypeArguments = typeArguments"))section=section.Replace("        Name = name;","        Name = name;\n        TypeArguments = typeArguments;");
File.WriteAllText(path,text[..begin]+section+text[end..]);
