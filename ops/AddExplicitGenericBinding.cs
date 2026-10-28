#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\PySharp\src\PySharpLib";
var path=Path.Combine(root,"Runtime","Clr.cs");var text=File.ReadAllText(path);
text=text.Replace("public string Name { get; }\n\n    public ClrMethod", "public string Name { get; }\n    public Type[]? TypeArguments { get; }\n\n    public ClrMethod");
text=text.Replace("public ClrMethod(object? target, Type type, string name)","public ClrMethod(object? target, Type type, string name, Type[]? typeArguments = null)");
text=text.Replace("        Name = name;\n    }\n\n    public override string ToString() => $\"<clr method", "        Name = name;\n        TypeArguments = typeArguments;\n    }\n\n    public override string ToString() => $\"<clr method");
var old="var candidates = method.Type.GetMethods(flags).Where(m => m.Name == method.Name).ToArray();";
if(!text.Contains(old))throw new Exception("Candidate point missing");
text=text.Replace(old,"""
var methods = method.Type.GetMethods(flags).Where(m => m.Name == method.Name);
        var closed = new List<MethodInfo>();
        foreach (var candidate in methods)
        {
            if (method.TypeArguments is null)
            {
                if (!candidate.ContainsGenericParameters) closed.Add(candidate);
            }
            else if (candidate.IsGenericMethodDefinition && candidate.GetGenericArguments().Length == method.TypeArguments.Length)
            {
                try { closed.Add(candidate.MakeGenericMethod(method.TypeArguments)); }
                catch (ArgumentException) { /* This overload's constraints do not accept the explicit types. */ }
            }
        }
        var candidates = closed.ToArray();
""");
text=text.Replace("            var ps = candidate.GetParameters();", "            if (candidate.ContainsGenericParameters) continue;\n            var ps = candidate.GetParameters();\n            if (ps.Any(p => p.ParameterType.IsByRefLike || p.ParameterType.IsByRef)) continue;");
var marker="    /// <summary>Construct a CLR instance (ClrType called like a function).</summary>";
text=text.Replace(marker,"""
    /// <summary>Select explicit generic arguments for a CLR method group using method[Type] or method[TypeA, TypeB].</summary>
    public static ClrMethod SelectGeneric(ClrMethod method, object index)
    {
        var values = index is PyTuple tuple ? tuple.Items.ToArray() : new[] { index };
        if (values.Length == 0 || values.Any(v => v is not ClrType))
            throw PyErr.TypeError("Generic arguments must be CLR types, not numeric values or constructor functions");
        var types = values.Cast<ClrType>().Select(t => t.Type).ToArray();
        if (!method.Type.GetMethods(method.Target is null ? StaticFlags : InstanceFlags)
            .Any(m => m.Name == method.Name && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == types.Length))
            throw PyErr.TypeError("No generic method has this type-argument arity");
        return new ClrMethod(method.Target, method.Type, method.Name, types);
    }

"""+marker);
File.WriteAllText(path,text);
path=Path.Combine(root,"Interpretation","Interp.cs");text=File.ReadAllText(path);
var start="public object GetItem(object obj, object index)\n    {";
if(!text.Contains(start))throw new Exception("Index dispatch point missing");
text=text.Replace(start,start+"\n        if (obj is ClrMethod method) return ClrBinder.SelectGeneric(method, index);");
File.WriteAllText(path,text);Console.WriteLine("Explicit method generic binding added.");
