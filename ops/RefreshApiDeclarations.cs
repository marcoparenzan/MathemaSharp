#:property TargetFramework=net11.0
using System.Text;
var root=Directory.GetCurrentDirectory();
string Csv(string s)=>"\""+s.Replace("\"","\"\"")+"\"";
void Scan(string destination,IEnumerable<string> files,string relativeRoot)
{
var csv=new StringBuilder("\"File\",\"Line\",\"Declaration\"\n");
foreach(var path in files.Order())
{
var lines=File.ReadAllLines(path);
for(int i=0;i<lines.Length;i++)if(System.Text.RegularExpressions.Regex.IsMatch(lines[i],@"^\s*public\s"))
csv.AppendLine($"{Csv(Path.GetRelativePath(relativeRoot,path).Replace('\\','/'))},{Csv((i+1).ToString())},{Csv(lines[i].Trim())}");
}
File.WriteAllText(Path.Combine(root,destination),csv.ToString());Console.WriteLine(destination);
}
Scan("ops/api-declarations.csv",Directory.EnumerateFiles(Path.Combine(root,"src","libs"),"*.cs",SearchOption.AllDirectories).Where(p=>!p.Split('\\').Any(s=>s is "bin" or "obj")),root);
var science=@"D:\Dev\Science";
var paths=new[]{"PySharp/src/PySharpLib/Runtime/Clr.cs","MatSharp/src/MatSharpLib/Runtime/GenericScalar.cs","MatSharp/src/MatSharpLib.NDSharp/TypedSolve.cs","MatSharp/src/MatSharpLib.NDSharp/MathemaFunctions.cs"}.Select(p=>Path.Combine(science,p.Replace('/',Path.DirectorySeparatorChar)));
Scan("ops/interop-api-declarations.csv",paths,science);
