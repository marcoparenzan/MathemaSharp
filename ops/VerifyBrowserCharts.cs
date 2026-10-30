#:property TargetFramework=net11.0
#:property JsonSerializerIsReflectionEnabledByDefault=true
#:property PublishAot=false
#:project ../src/tools/Ecosystem.Validation/Ecosystem.Validation/Ecosystem.Validation.csproj
#:package Microsoft.Playwright@1.56.0
using Microsoft.Playwright;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
var options=Ecosystem.Validation.EvidenceOptions.Parse(args);
using var playwright=await Playwright.CreateAsync();
await using var browser=await playwright.Chromium.LaunchAsync(new() { Channel="msedge", Headless=true });
await using var context=await browser.NewContextAsync(new() { ViewportSize=new() { Width=740,Height=300 }, DeviceScaleFactor=2 });
var page=await context.NewPageAsync();
await page.RouteAsync("**/*",r=>r.AbortAsync()); // inline SVG only: no network access required
var output=options.SaveImages?"ops/visual/browser":"ops/validation/browser-no-images";Directory.CreateDirectory(output);
var results=new JsonArray();var dashboards=new JsonArray();int total=0,failed=0;
using var manifest=JsonDocument.Parse(File.ReadAllText("ops/migrated-notebooks.json"));
foreach(var entry in manifest.RootElement.EnumerateArray())
{
 var path=entry.GetProperty("path").GetString()!;int index=0;
 var dashboard=new System.Text.StringBuilder();
 using var notebook=JsonDocument.Parse(File.ReadAllText(path));
 foreach(var cell in notebook.RootElement.GetProperty("cells").EnumerateArray())
 if(cell.TryGetProperty("outputs",out var outputs))foreach(var o in outputs.EnumerateArray())
 {
  if(!o.TryGetProperty("data",out var data)||!data.TryGetProperty("text/html",out var html))continue;
  var text=html.ValueKind==JsonValueKind.Array?string.Concat(html.EnumerateArray().Select(v=>v.GetString())):html.GetString();
  dashboard.Append(text);
  foreach(var svg in XElement.Parse(text!).DescendantsAndSelf().Where(e=>e.Name.LocalName=="svg"))
  {
   await page.SetContentAsync("<!doctype html><html><body style='margin:0;background:white'>"+svg+"</body></html>");
   var json=await page.EvaluateAsync<string>("""
    () => {
      const svg=document.querySelector('svg'), frame=svg.getBoundingClientRect();
      const texts=[...svg.querySelectorAll('text')].filter(t=>t.textContent.trim());
      const outside=texts.filter(t=>{const r=t.getBoundingClientRect();return r.left<frame.left-1 || r.right>frame.right+1 || r.top<frame.top-1 || r.bottom>frame.bottom+1;}).map(t=>t.textContent);
      const heading=texts[0], h=heading.getBoundingClientRect();
      const collisions=texts.slice(1).filter(t=>{const r=t.getBoundingClientRect();return r.left<h.right && r.right>h.left && r.top<h.bottom && r.bottom>h.top;}).map(t=>t.textContent);
      const hits=[...svg.querySelectorAll('circle')].filter(c=>c.querySelector('title'));
      let reachable=0;
      for(const c of hits){const p=svg.createSVGPoint();p.x=+c.getAttribute('cx');p.y=+c.getAttribute('cy');const q=p.matrixTransform(c.getScreenCTM());const e=document.elementFromPoint(q.x,q.y);if(e===c || e?.closest('circle')===c)reachable++;}
      return JSON.stringify({outside,collisions,tooltipTargets:hits.length,reachable});
    }
    """);
   var check=JsonNode.Parse(json)!;var name=Path.GetFileNameWithoutExtension(path)+"-"+(++index).ToString("D2");
   if(options.SaveImages)await page.Locator("svg").ScreenshotAsync(new() { Path=Path.Combine(output,name+".png") });
   bool ok=check["outside"]!.AsArray().Count==0 && check["collisions"]!.AsArray().Count==0
       && check["tooltipTargets"]!.GetValue<int>()>0 && check["reachable"]!.GetValue<int>()>0;
   if(!ok)failed++;
   results.Add((JsonNode)new JsonObject { ["Chart"]=name,["Browser"]=browser.Version,["Checks"]=check,["Success"]=ok });total++;
   if(!ok)Console.WriteLine(name+": "+json);
  }
 }
 if(index!=entry.GetProperty("expectedSvg").GetInt32())throw new Exception("Missing browser charts: "+path);
 if(entry.TryGetProperty("expectedKpi",out var ek))
 {
  await page.SetViewportSizeAsync(1280,720);
  await page.SetContentAsync("<!doctype html><html><body style='margin:16px;background:white'>"+dashboard+"</body></html>");
  var json=await page.EvaluateAsync<string>("""
   () => JSON.stringify({kpi:document.querySelectorAll('.mathema-kpi').length,
     quotes:document.querySelectorAll('.mathema-quote').length,
     emptyQuotes:[...document.querySelectorAll('.mathema-quote')].filter(e=>!e.textContent.trim()).length,
     horizontalOverflow:document.documentElement.scrollWidth>window.innerWidth,
     tiles:[...document.querySelectorAll('.mathema-kpi')].reduce((n,e)=>n+e.children.length,0)})
   """);
  var check=JsonNode.Parse(json)!;var id=entry.GetProperty("id").GetString()!;
  bool ok=check["kpi"]!.GetValue<int>()==ek.GetInt32() && check["quotes"]!.GetValue<int>()==entry.GetProperty("expectedQuotes").GetInt32()
   && check["emptyQuotes"]!.GetValue<int>()==0 && check["tiles"]!.GetValue<int>()>0 && !check["horizontalOverflow"]!.GetValue<bool>();
  if(!ok)failed++;
  if(options.SaveImages)await page.ScreenshotAsync(new() { Path=Path.Combine(output,id+"-dashboard.png"),FullPage=true });
  dashboards.Add((JsonNode)new JsonObject { ["Id"]=id,["Checks"]=check,["Success"]=ok });
  Console.WriteLine("Dashboard "+id+": "+json);
  await page.SetViewportSizeAsync(740,300);
 }
}
var report=new JsonObject { ["GeneratedUtc"]=DateTime.UtcNow.ToString("O"),["Browser"]="Edge Chromium headless",["Total"]=total,["Failed"]=failed,["SaveImages"]=options.SaveImages,["Notes"]="DOM bounds/title collision and tooltip hit-testing; at least one native title hit target reachable per SVG; overlapping targets may obscure individual samples. Native OS tooltip popup not verified. Dashboard checks use assembled HTML outputs at desktop width 1280 px, not the notebook UI/markdown. Not VS Code renderer verification.",["Charts"]=results,["Dashboards"]=dashboards };
File.WriteAllText(Path.Combine(output,"report.json"),report.ToJsonString(new JsonSerializerOptions { WriteIndented=true }));
Console.WriteLine($"Browser: {total} SVGs, {dashboards.Count} dashboards, {failed} failures.");
return failed==0?0:1;
