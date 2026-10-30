#:property TargetFramework=net11.0
using System.Text.Json.Nodes;
var path="notebooks/TimeSeries/pysharp/12-clr-operators.ipynb";
var doc=JsonNode.Parse(File.ReadAllText(path))!;
var code=doc["cells"]![1]!;
var source=string.Concat(code["source"]!.AsArray().Select(s=>s!.GetValue<string>()));
var index=source.IndexOf("for make in");
if(index<0)throw new Exception("Loop not found");
source="from mathema import decimal32, decimal64, decimal128\n"+source[index..];
code["source"]=new JsonArray(JsonValue.Create(source));
code["outputs"]=new JsonArray();code["execution_count"]=null;
doc["cells"]![0]!["source"]=new JsonArray(JsonValue.Create("# The mathema module and CLR operators\nConstructors importable from the PySharp runtime; no local helper and no arithmetic facade. Requires .NET 11 for the IEEE Decimal types. Accepts strings, integers and CLR values of the same type; implicit floats and conversions between different dtypes are rejected. Rounding/range remain those of the chosen type."));
File.WriteAllText(path,doc.ToJsonString());
