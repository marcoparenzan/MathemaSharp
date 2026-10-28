#:property TargetFramework=net11.0
using System.Text.Json.Nodes;
var path=Path.Combine(Directory.GetCurrentDirectory(),"notebooks","MatSharp","01-decimal-scalars.ipynb");
var doc=JsonNode.Parse(File.ReadAllText(path))!;
var cells=(JsonArray)doc["cells"]!;
if(!cells.Any(c=>c?["id"]?.ToString()=="typed-left-divide"))
cells.Add(new JsonObject{
["cell_type"]="code",["id"]="typed-left-divide",["execution_count"]=null,
["metadata"]=new JsonObject{["vscode"]=new JsonObject{["languageId"]="csharp"}},["outputs"]=new JsonArray(),
["source"]=new JsonArray("#r \"D:/Dev/BuildArtifacts/MathemaSharp/bin/MathemaSharp.LinearAlgebra/release/MathemaSharp.LinearAlgebra.dll\"\n", "#r \"D:/Dev/BuildArtifacts/MatSharp/bin/MatSharpLib.NDSharp/release/MatSharpLib.NDSharp.dll\"\n",
"MatSharpLib.NDSharpBridge.NDSharpFunctions.Register(engine);\n",
"engine.Run(\"aa=[decimal128(1) decimal128(0);decimal128(0) decimal128(1)]; bb=[decimal128('1.0000000000000000000000001');decimal128(2)]; xx=aa\\\\bb;\");\n",
"var precise = (GenericScalar<Decimal128>)engine.Eval(\"xx(1)-decimal128(1)\");\n",
"if (precise.Value != Decimal128.Parse(\"1e-25\")) throw new Exception(\"Typed solver lost precision\");\n",
"Console.WriteLine(\"typed_left_divide_precision: OK\");")});
File.WriteAllText(path,doc.ToJsonString(new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
