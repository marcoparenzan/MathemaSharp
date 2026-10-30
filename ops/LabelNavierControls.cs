#:property TargetFramework=net10.0
var root="../articoli/2026.10 NavierStokes/src/NavierStokes.Web/Components/Pages";
(string Page,(string Binding,string Label)[] Fields)[] pages=[
 ("Quadro",[("Side","Cabinet side"),("FanSpeed","Air speed"),("FluidName","Fluid")]),
 ("Tubo",[("fluidName","Fluid"),("diameterMm","Inner diameter"),("lengthM","Pipe length"),("flowLpm","Flow rate"),("roughnessMm","Pipe material")]),
 ("Vento",[("shapeName","Shape"),("areaM2","Exposed area"),("windKmh","Wind")]),
 ("Fischio",[("Preset","Object"),("diameterMm","Diameter"),("windKmh","Wind")])
];
foreach(var page in pages)
{
 var path=Path.Combine(root,page.Page+".razor");var text=File.ReadAllText(path);
 foreach(var field in page.Fields)
 {
  var old="@bind=\""+field.Binding+"\"";var replacement="aria-label=\""+field.Label+"\" "+old;
  if(text.Contains("aria-label=\""+field.Label+"\"",StringComparison.Ordinal))continue;
  if(text.IndexOf(old,StringComparison.Ordinal)<0||text.IndexOf(old,StringComparison.Ordinal)!=text.LastIndexOf(old,StringComparison.Ordinal))throw new Exception("Missing or ambiguous binding: "+field.Binding);
  text=text.Replace(old,replacement,StringComparison.Ordinal);
 }
 File.WriteAllText(path,text);Console.WriteLine(page.Page+": accessible control names set.");
}
return 0;
