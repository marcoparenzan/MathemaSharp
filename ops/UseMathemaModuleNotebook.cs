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
doc["cells"]![0]!["source"]=new JsonArray(JsonValue.Create("# Modulo mathema e operatori CLR\nCostruttori importabili dal runtime PySharp; nessun helper locale né facciata aritmetica. Richiede .NET 11 per i tipi IEEE Decimal. Accetta stringhe, interi e valori CLR dello stesso tipo; float impliciti e conversioni fra dtype diversi rifiutati. Arrotondamento/range restano quelli del tipo scelto."));
File.WriteAllText(path,doc.ToJsonString());
