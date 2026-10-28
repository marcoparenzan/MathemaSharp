#:property TargetFramework=net10.0
var root="../articoli/2026.10 NavierStokes/src/NavierStokes.Web/Components/Pages";
(string Page,(string Binding,string Label)[] Fields)[] pages=[
 ("Quadro",[("Side","Lato del quadro"),("FanSpeed","Velocità dell'aria"),("FluidName","Fluido")]),
 ("Tubo",[("fluidName","Fluido"),("diameterMm","Diametro interno"),("lengthM","Lunghezza del tubo"),("flowLpm","Portata"),("roughnessMm","Materiale del tubo")]),
 ("Vento",[("shapeName","Forma"),("areaM2","Superficie esposta"),("windKmh","Vento")]),
 ("Fischio",[("Preset","Oggetto"),("diameterMm","Diametro"),("windKmh","Vento")])
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
