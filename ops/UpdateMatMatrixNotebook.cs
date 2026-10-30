#:property TargetFramework=net11.0
using System.Text.Json.Nodes;
var path="notebooks/MatSharp/01-decimal-scalars.ipynb";
var doc=JsonNode.Parse(File.ReadAllText(path))!;
var cells=doc["cells"]!.AsArray();
cells.Add(new JsonObject{["cell_type"]="markdown",["metadata"]=new JsonObject(),["source"]=new JsonArray(JsonValue.Create("## Typed real matrices\nConcatenation, transposition, product, indexing and assignment preserve T. MATLAB broadcasting on the leading axes. Real 2D matrices only; no typed or complex solver."))});
var code="""
using System.Numerics;
engine.Run("A=[decimal128(1) decimal128(2); decimal128(3) decimal128(4)]; B=A.'; C=A*B;");
var c=(GenericScalar<Decimal128>)engine.GetVariable("C")!;
var expected=new Decimal128[] {Decimal128.CreateChecked(5),Decimal128.CreateChecked(11),Decimal128.CreateChecked(11),Decimal128.CreateChecked(25)};
if(!c.ToArray().SequenceEqual(expected))throw new Exception("Matrix product mismatch");
Console.WriteLine("typed_matrix_product: OK");
engine.Run("copy=A; A(2,1)=decimal128('1e-25'); v=A(2,1); original=copy(2,1);");
var v=(GenericScalar<Decimal128>)engine.GetVariable("v")!;
if(v.Value!=Decimal128.Parse("1e-25"))throw new Exception("Assignment lost precision");
var original=(GenericScalar<Decimal128>)engine.GetVariable("original")!;
if(original.Value!=Decimal128.CreateChecked(3))throw new Exception("Copy on write lost");
Console.WriteLine("typed_index_assignment_and_copy: OK");
""";
cells.Add(new JsonObject{["cell_type"]="code",["metadata"]=new JsonObject(),["execution_count"]=null,["outputs"]=new JsonArray(),["source"]=new JsonArray(JsonValue.Create(code))});
foreach(var cell in cells){var source=cell!["source"]!.AsArray();for(int i=0;i<source.Count;i++)source[i]=source[i]!.GetValue<string>().Replace("Scalars only: concatenation, growth, indexed assignment and generic matrices not implemented.","Typed real 2D scalars and matrices; matrix solve/division, complex numbers and higher dimensions are not implemented.");}
File.WriteAllText(path,doc.ToJsonString());
