#:property TargetFramework=net11.0
using System.Text.Json;
using var doc=JsonDocument.Parse(File.ReadAllText("notebooks/TimeSeries/pysharp/11-precision-interop.ipynb"));
foreach(var c in doc.RootElement.GetProperty("cells").EnumerateArray())if(c.GetProperty("cell_type").GetString()=="code")Console.WriteLine(c.GetProperty("outputs").ToString());
