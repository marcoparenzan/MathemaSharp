#:property TargetFramework=net11.0
using System.Text.Json.Nodes;
foreach(var path in Directory.GetFiles("notebooks/TimeSeries","*.ipynb",SearchOption.AllDirectories)) {
 var doc=JsonNode.Parse(File.ReadAllText(path))!;bool changed=false;
 foreach(var cell in doc["cells"]!.AsArray()) {
  var source=cell!["source"]!.AsArray();
  for(int i=0;i<source.Count;i++){
   var old=source[i]!.GetValue<string>();
   var value=old.Replace("os.path.join(root, \"artifacts\", \"bin\", \"MathemaSharp.TimeSeries\", \"release\", \"MathemaSharp.TimeSeries.dll\")","\"D:/Dev/BuildArtifacts/MathemaSharp/bin/MathemaSharp.TimeSeries/release/MathemaSharp.TimeSeries.dll\"")
   .Replace("repository Numbers","repository MathemaSharp");
   if(value!=old){source[i]=value;changed=true;}
  }
 }
 if(changed){File.WriteAllText(path,doc.ToJsonString());Console.WriteLine(path);}
}
