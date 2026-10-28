#:property TargetFramework=net11.0
#:property JsonSerializerIsReflectionEnabledByDefault=true
#:property PublishAot=false
#:project "../../articoli/2026.10 NavierStokes/src/NavierStokes.Core/NavierStokes.Core.csproj"
#:project ../src/tools/Ecosystem.Validation/Ecosystem.Validation/Ecosystem.Validation.csproj
#:package Microsoft.Playwright@1.56.0
using NavierStokes.Core;
using Microsoft.Playwright;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Xml.Linq;
using System.Text.RegularExpressions;
var options=Ecosystem.Validation.EvidenceOptions.Parse(args);
var output=options.SaveImages?"ops/visual/navier":"ops/validation/navier-adoption-no-images";Directory.CreateDirectory(output);
var cavity=new CavitySolver(17,100);cavity.Run(40);
var cylinder=new CylinderLbm(100,80,40,8);cylinder.Run(40);
(string Name,string Svg)[] reports=[("cavity-ghia",Reports.CavityGhia(cavity)),("cavity-energy",Reports.CavityEnergy(cavity)),("cylinder-probe",Reports.CylinderProbe(cylinder))];
using var playwright=await Playwright.CreateAsync();
await using var browser=await playwright.Chromium.LaunchAsync(new() { Channel="msedge",Headless=true });
var page=await browser.NewPageAsync(new() { ViewportSize=new() { Width=760,Height=420 },DeviceScaleFactor=2 });
await page.RouteAsync("**/*",r=>r.AbortAsync());
var checks=new JsonArray();int failed=0;
foreach(var report in reports)
{
 XElement.Parse(report.Svg);
 if(options.SaveImages)File.WriteAllText(Path.Combine(output,report.Name+".svg"),report.Svg);
 await page.SetContentAsync("<!doctype html><html><body style='margin:0'>"+report.Svg+"</body></html>");
 var json=await page.EvaluateAsync<string>("""
 () => { const svg=document.querySelector('svg'),frame=svg.getBoundingClientRect();
 const text=[...svg.querySelectorAll('text')].filter(t=>t.textContent.trim()),h=text[0].getBoundingClientRect();
 const outside=text.filter(t=>{const r=t.getBoundingClientRect();return r.left<frame.left-1||r.right>frame.right+1||r.top<frame.top-1||r.bottom>frame.bottom+1;}).map(t=>t.textContent);
 const collisions=text.slice(1).filter(t=>{const r=t.getBoundingClientRect();return r.left<h.right&&r.right>h.left&&r.top<h.bottom&&r.bottom>h.top;}).map(t=>t.textContent);
 return JSON.stringify({outside,collisions,pointTitles:svg.querySelectorAll('circle title').length}); }
 """);
 var check=JsonNode.Parse(json)!;bool ok=check["outside"]!.AsArray().Count==0&&check["collisions"]!.AsArray().Count==0&&check["pointTitles"]!.GetValue<int>()>0;
 if(!ok)failed++;
 checks.Add((JsonNode)new JsonObject { ["Report"]=report.Name,["Success"]=ok,["Checks"]=check });
 if(options.SaveImages)await page.Locator("svg").ScreenshotAsync(new() { Path=Path.Combine(output,report.Name+".png") });
 Console.WriteLine(report.Name+": "+json);
}
// Field report is intentionally retained, not claimed as migrated or visually approved.
var field=Reports.CavityVelocity(cavity);XElement.Parse(field);if(options.SaveImages)File.WriteAllText(Path.Combine(output,"field-retained.svg"),field);
var pressure=XElement.Parse(Reports.CavityPressure(cavity));
var vectorPath=pressure.Elements().Single(e=>e.Name.LocalName=="path"&&e.Attribute("stroke-opacity")?.Value=="0.75").Attribute("d")?.Value;
if(string.IsNullOrEmpty(vectorPath))throw new Exception("Production pressure vector geometry missing.");
var apps=new JsonArray();
foreach(var name in new[]{"MathemaSharp.Lab","MathemaSharp.Docs"})
{
 var files=Directory.EnumerateFiles("src/apps/"+name,"*",SearchOption.AllDirectories)
  .Where(p=>!p.Split(Path.DirectorySeparatorChar).Any(s=>s is "bin" or "obj"))
  .Where(p=>Path.GetExtension(p) is ".cs" or ".razor").ToArray();
 var hits=files.Where(p=>Regex.IsMatch(File.ReadAllText(p),@"Charts\.|<svg|LineChart")).ToArray();
 if(hits.Length>0)failed++;
 apps.Add((JsonNode)new JsonObject { ["App"]=name,["InspectedSourceFiles"]=files.Length,["LocalRendererMatches"]=hits.Length,
 ["Classification"]=name.EndsWith("Lab")?"Numerical console output; no local SVG renderer found":"Markdown/asset viewer; no local SVG renderer found" });
}
var result=new JsonObject { ["GeneratedUtc"]=DateTime.UtcNow.ToString("O"),["Browser"]=browser.Version,["Failed"]=failed,["SaveImages"]=options.SaveImages,
 ["CoreAssembly"]=typeof(CavitySolver).Assembly.Location,["PlotSharpAssembly"]=typeof(PlotSharp.Charts).Assembly.Location,
 ["Reports"]=checks,["Apps"]=apps,["PressureVectorsValidated"]=true,["Notes"]="Three production line reports on small smoke fixtures, not numerical accuracy or full Web UI proof. Navier FieldPlot is an API/options compatibility facade; full node-field SVG assembly/header/style and component kernels render in PlotSharp. Numeric source data and scenario/UI decisions remain in NavierStokes. Node rendering is separate from cell Heatmap. Not a complete raster backend migration. App inventory is a lexical source scan, not whole-asset migration proof." };
File.WriteAllText(Path.Combine(output,"report.json"),result.ToJsonString(new JsonSerializerOptions { WriteIndented=true }));
return failed==0?0:1;
