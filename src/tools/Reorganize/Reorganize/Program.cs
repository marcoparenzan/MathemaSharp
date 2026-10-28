using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
var root=Directory.GetCurrentDirectory();
var libs=new[]{"Core","LinearAlgebra","Quantum","TimeSeries","NDSharp"};
var usage=new StringBuilder("\n## Librerie: Usage\n\nI sample didattici sono in `notebooks/<lib>/`; gli helper condivisi sono in `notebooks/shared/`. Gli output sono stati azzerati durante la migrazione: rieseguire i notebook prima di considerarli verificati. La console Lab resta un harness di verifica, non il catalogo dei sample.\n");
var reference=new StringBuilder("\n## Librerie: Reference\n\nInventario dei tipi pubblici estratto dai sorgenti. Per firme, vincoli e commenti completi consultare i file collegati e la documentazione XML prodotta dalla build. Non costituisce una conversione aggiuntiva dei kernel NDSharp.\n");
foreach(var lib in libs) {
 Directory.CreateDirectory($"notebooks/{lib}");
 usage.Append($"\n### MathemaSharp.{lib} — Usage\n\nNamespace: `MathemaSharp.{lib}`. Notebook: `notebooks/{lib}/`.\n\n");
 foreach(var p in Directory.GetFiles($"notebooks/{lib}","*.ipynb",SearchOption.AllDirectories)) usage.Append($"- [{Path.GetFileNameWithoutExtension(p)}]({p.Replace('\\','/')})\n");
 if(lib=="LinearAlgebra") usage.Append("\nUsare `LuFactorization<T>.Create(matrix)` e `Solve(rhs)` per riutilizzare la stessa fattorizzazione; i pivot quasi singolari sono rifiutati.\n");
 if(lib=="NDSharp") usage.Append("\nIl ponte usa dtype/storage runtime e contiene ancora conversioni via double: non è un kernel interamente Generic Math. Il notebook STL in TimeSeries mostra l'integrazione.\n");
 reference.Append($"\n### MathemaSharp.{lib} — Reference\n\n| Tipo pubblico | Sorgente |\n|---|---|\n");
 foreach(var p in Directory.GetFiles($"src/libs/MathemaSharp.{lib}","*.cs",SearchOption.AllDirectories).Where(p=>!p.Contains("/obj/")))
 foreach(Match m in Regex.Matches(File.ReadAllText(p),@"public\s+(?:(?:static|sealed|readonly|abstract|partial)\s+)*(?:class|record(?:\s+struct)?|struct|enum|interface)\s+(\w+)")) reference.Append($"| `{m.Groups[1].Value}` | [{Path.GetFileName(p)}]({p.Replace('\\','/')}) |\n");
}
File.AppendAllText("USAGE.md",usage.ToString()); File.AppendAllText("ARCHITECTURE.md",reference.ToString());
