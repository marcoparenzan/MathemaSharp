#:property TargetFramework=net11.0
using System.Text.Json.Nodes;
foreach(var scenario in new[]{"A-magazzino-frigorifero","D-fotovoltaico"}) {
 var path=Directory.GetFiles(Path.Combine("notebooks","TimeSeries","Scenarios",scenario),"*csharp.ipynb").Single();
 var doc=JsonNode.Parse(File.ReadAllText(path))!;
 foreach(var cell in doc["cells"]!.AsArray()) { var source=cell!["source"]!.AsArray();for(int i=0;i<source.Count;i++)source[i]=source[i]!.GetValue<string>().Replace("Path.Combine(Here(), \"data\")",$"Path.GetFullPath(Path.Combine(Here(), \"../../../../scenari/{scenario}/data\"))"); }
 File.WriteAllText(path,doc.ToJsonString());
}
