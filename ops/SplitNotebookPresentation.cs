#:property TargetFramework=net11.0
// One-time, idempotent extraction: presentation helpers are not part of the chart renderer.
var charts="notebooks/shared/Charts.csx";
var target="notebooks/shared/Presentation.csx";
var text=File.ReadAllText(charts);
const string marker="/// <summary>Tabella HTML semplice:";
int index=text.IndexOf(marker,StringComparison.Ordinal);
if(index<0)
{
 if(File.Exists(target)&&text.Contains("#load \"Presentation.csx\"")){Console.WriteLine("Already extracted.");return 0;}
 throw new Exception("Expected extraction marker missing; no files changed.");
}
if(text.IndexOf(marker,index+marker.Length,StringComparison.Ordinal)>=0)throw new Exception("Ambiguous extraction marker.");
var presentation="// HTML notebook presentation, independent of the SVG renderer. Extracted without semantic changes.\nusing System.Globalization;\nusing System.Net;\nusing System.Text;\n\n"+text[index..];
if(File.Exists(target)&&File.ReadAllText(target)!=presentation)throw new Exception("Existing presentation differs; refusing overwrite.");
File.WriteAllText(target,presentation);
File.WriteAllText(charts,"#load \"Presentation.csx\"\n"+text[..index]);
Console.WriteLine("Table/Kpi/Tile extracted; legacy Charts.csx loads the same helpers.");return 0;
