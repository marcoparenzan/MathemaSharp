using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
var root=Directory.GetCurrentDirectory();
var libs=new[]{"Core","LinearAlgebra","Quantum","TimeSeries","NDSharp"};
var usage=new StringBuilder("\n## Libraries: Usage\n\nThe educational samples are in `notebooks/<lib>/`; the shared helpers are in `notebooks/shared/`. The outputs were cleared during the migration: re-run the notebooks before considering them verified. The Lab console remains a verification harness, not the catalogue of the samples.\n");
var reference=new StringBuilder("\n## Libraries: Reference\n\nInventory of the public types extracted from the sources. For full signatures, constraints and comments consult the linked files and the XML documentation produced by the build. It is not an additional conversion of the NDSharp kernels.\n");
foreach(var lib in libs) {
 Directory.CreateDirectory($"notebooks/{lib}");
 usage.Append($"\n### MathemaSharp.{lib} — Usage\n\nNamespace: `MathemaSharp.{lib}`. Notebook: `notebooks/{lib}/`.\n\n");
 foreach(var p in Directory.GetFiles($"notebooks/{lib}","*.ipynb",SearchOption.AllDirectories)) usage.Append($"- [{Path.GetFileNameWithoutExtension(p)}]({p.Replace('\\','/')})\n");
  if(lib=="LinearAlgebra") usage.Append("\nUse `LuFactorization<T>.Create(matrix)` and `Solve(rhs)` to reuse the same factorisation; nearly singular pivots are rejected.\n");
  if(lib=="NDSharp") usage.Append("\nThe bridge uses runtime dtype/storage and still contains conversions via double: it is not an entirely Generic Math kernel. The STL notebook in TimeSeries shows the integration.\n");
 reference.Append($"\n### MathemaSharp.{lib} — Reference\n\n| Tipo pubblico | Sorgente |\n|---|---|\n");
 foreach(var p in Directory.GetFiles($"src/libs/MathemaSharp.{lib}","*.cs",SearchOption.AllDirectories).Where(p=>!p.Contains("/obj/")))
 foreach(Match m in Regex.Matches(File.ReadAllText(p),@"public\s+(?:(?:static|sealed|readonly|abstract|partial)\s+)*(?:class|record(?:\s+struct)?|struct|enum|interface)\s+(\w+)")) reference.Append($"| `{m.Groups[1].Value}` | [{Path.GetFileName(p)}]({p.Replace('\\','/')}) |\n");
}
File.AppendAllText("USAGE.md",usage.ToString()); File.AppendAllText("ARCHITECTURE.md",reference.ToString());
