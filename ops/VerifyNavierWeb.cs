#:property TargetFramework=net10.0
#:property JsonSerializerIsReflectionEnabledByDefault=true
#:property PublishAot=false
#:project ../src/tools/Ecosystem.Validation/Ecosystem.Validation/Ecosystem.Validation.csproj
#:package Microsoft.Playwright@1.56.0
using Ecosystem.Validation;
using Microsoft.Playwright;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;

if(args.Any(a=>a is not ("--screenshot" or "--quadro" or "--fischio" or "--mobile")))throw new ArgumentException("Supported: --quadro OR --fischio OR --mobile, and optional --screenshot (one image).");
bool quadro=args.Contains("--quadro"),fischio=args.Contains("--fischio"),mobile=args.Contains("--mobile");
if((quadro&&fischio)||(mobile&&(quadro||fischio)))throw new ArgumentException("Choose one mode; mobile is calculator-only.");
var output=Path.GetFullPath(mobile?"ops/validation/navier-mobile-smoke":fischio?"ops/validation/navier-fischio-smoke":quadro?"ops/validation/navier-quadro-smoke":"ops/validation/navier-web-smoke");Directory.CreateDirectory(output);
var project=Path.GetFullPath("../articoli/2026.10 NavierStokes/src/NavierStokes.Web/NavierStokes.Web.csproj");
var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();int port=((IPEndPoint)listener.LocalEndpoint).Port;listener.Stop();
var url=$"http://127.0.0.1:{port}";
var psi=new ProcessStartInfo("dotnet") { WorkingDirectory=Path.GetDirectoryName(project)!,UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true };
foreach(var a in new[]{"run","--project",project,"-c","Release","--no-build","--no-launch-profile","--","--urls",url})psi.ArgumentList.Add(a);
psi.Environment["ASPNETCORE_ENVIRONMENT"]="Development";psi.Environment["DOTNET_ENVIRONMENT"]="Development";
psi.Environment.Remove("ASPNETCORE_HTTPS_PORT");psi.Environment.Remove("ASPNETCORE_HTTPS_PORTS");
using var process=Process.Start(psi)??throw new Exception("Could not start Web application.");
var stdout=process.StandardOutput.ReadToEndAsync();var stderr=process.StandardError.ReadToEndAsync();
var steps=new JsonArray();var layouts=new JsonArray();var errors=new List<string>();bool success=false;string? failure=null;
try
{
 using var client=new HttpClient { Timeout=TimeSpan.FromSeconds(1) };bool ready=false;var deadline=DateTime.UtcNow.AddSeconds(30);
 while(DateTime.UtcNow<deadline&&!process.HasExited)
 {
  try { using var response=await client.GetAsync(url);ready=response.IsSuccessStatusCode;if(ready)break; } catch(HttpRequestException){} catch(TaskCanceledException){}
  await Task.Delay(250);
 }
 if(!ready)throw new Exception("Kestrel did not become ready on loopback.");
 using var playwright=await Playwright.CreateAsync();
 await using var browser=await playwright.Chromium.LaunchAsync(new() { Channel="msedge",Headless=true });
 var page=await browser.NewPageAsync(new() { ViewportSize=new() { Width=mobile?360:1280,Height=mobile?800:900 } });page.SetDefaultTimeout(15000);
 page.PageError+=(_,message)=>errors.Add(message);
 await page.RouteAsync("**/*",r=>r.Request.Url.StartsWith(url+"/",StringComparison.Ordinal)?r.ContinueAsync():r.AbortAsync());

 async Task ChangeRange(string label,string value,Func<string,double> parse,Func<double,bool> expected)
 {
  var end=DateTime.UtcNow.AddSeconds(15);
  while(DateTime.UtcNow<end)
  {
   await page.GetByLabel(label,new() { Exact=true }).EvaluateAsync<bool>("(el,value)=>{el.value=value;el.dispatchEvent(new Event('input',{bubbles:true}));return true;}",value);
   await Task.Delay(300);
   if(expected(parse(await page.Locator(".answer .big").InnerTextAsync())))return;
  }
  throw new Exception("Interactive update timed out: "+label);
 }
 async Task CheckCharts(int expected)
 {
  if(await page.Locator(".chart svg").CountAsync()!=expected)throw new Exception("Unexpected SVG count on "+page.Url);
  if(await page.Locator(".chart svg g.point-overlays").CountAsync()!=expected)throw new Exception("Shared PlotSharp overlay missing.");
  if(await page.Locator("#blazor-error-ui").IsVisibleAsync())throw new Exception("Blazor error UI is visible.");
 }

 async Task CheckMobileLayout(string name)
 {
  if(!mobile)return;
  var scrollChecks=new JsonArray();
  var regions=page.Locator(".chart");
  for(int index=0;index<await regions.CountAsync();index++)
  {
   var region=regions.Nth(index);
   if(await region.GetAttributeAsync("tabindex")!="0")throw new Exception("Chart region is not keyboard focusable.");
   if(!await region.EvaluateAsync<bool>("el=>el.scrollWidth>el.clientWidth"))throw new Exception("Expected native-width scrollable chart on narrow viewport.");
   await region.FocusAsync();await region.PressAsync("ArrowRight");
   await page.WaitForFunctionAsync("index=>document.querySelectorAll('.chart')[index].scrollLeft>0",index,new() { Timeout=5000 });
   bool endReached=await region.EvaluateAsync<bool>("el=>{el.scrollLeft=el.scrollWidth;const svg=el.querySelector('svg').getBoundingClientRect(),box=el.getBoundingClientRect();return Math.abs(el.scrollLeft-(el.scrollWidth-el.clientWidth))<=1&&svg.right<=box.right+1&&svg.right>box.left;}");
   if(!endReached)throw new Exception("Could not reveal the right edge of a chart.");
   scrollChecks.Add((JsonNode)new JsonObject { ["ChartIndex"]=index,["KeyboardMoved"]=true,["RightEdgeReached"]=true });
  }
  var json=await page.EvaluateAsync<string>("""
   () => {
    const width=window.innerWidth;
    const nodes=[...document.querySelectorAll('main input,main select,main .panel,main .answer,main .chart')].filter(e=>e.getBoundingClientRect().width>0);
    const outside=nodes.filter(e=>{const r=e.getBoundingClientRect();return r.left < -1 || r.right > width+1;})
      .map(e=>({tag:e.tagName,cls:e.className,left:e.getBoundingClientRect().left,right:e.getBoundingClientRect().right}));
    const controlNodes=nodes.filter(e=>e.matches('input,select'));
    const controls=controlNodes.length;
    const controlHeights=controlNodes.map(e=>e.getBoundingClientRect().height);
    const rgba=s=>{
      const match=s.match(/^rgba?\(([^)]+)\)$/);if(!match)throw new Error('Unsupported CSS color: '+s);
      const parts=match[1].split(/[\s,]+/).filter(Boolean).map(Number);
      if(parts.some(v=>!Number.isFinite(v)))throw new Error('Invalid CSS color: '+s);
      return [parts[0],parts[1],parts[2],parts[3]??1];
    };
    const over=(layer,base)=>base.map((v,i)=>layer[i]*layer[3]+v*(1-layer[3]));
    const colorSamples=[...document.querySelectorAll('.hint,.eyebrow,.card .tag,.brand span,footer,.btn:not(:disabled)')]
      .filter(el=>el.getClientRects().length&&getComputedStyle(el).visibility==='visible'
        &&[...el.childNodes].some(n=>n.nodeType===Node.TEXT_NODE&&n.textContent.trim()))
      .map(el=>{
        const chain=[];for(let parent=el;parent;parent=parent.parentElement)chain.unshift(parent);
        let background=[255,255,255];
        for(const parent of chain){const style=getComputedStyle(parent);
          if(style.backgroundImage!=='none'||parseFloat(style.opacity)!==1)throw new Error('Unsupported background/opacity in contrast sample');
          background=over(rgba(style.backgroundColor),background);
        }
        const foreground=over(rgba(getComputedStyle(el).color),background);
        return {tag:el.tagName,cls:el.className,text:el.textContent.trim().slice(0,60),foreground:foreground.map(Math.round),background:background.map(Math.round)};
      });
    const texts=[...document.querySelectorAll('.chart svg text')].filter(t=>t.textContent.trim());
    const sizes=texts.map(t=>{const m=t.getScreenCTM();return m?parseFloat(getComputedStyle(t).fontSize)*Math.hypot(m.a,m.b):0;});
    return JSON.stringify({viewport:width,documentWidth:document.documentElement.scrollWidth,outside,controls,
      svgTextCount:texts.length,minSvgTextPixels:sizes.length?Math.min(...sizes):null,smallSvgTextCount:sizes.filter(v=>!Number.isFinite(v)||v<10).length,
      minControlHeight:controlHeights.length?Math.min(...controlHeights):null,shortControlCount:controlHeights.filter(v=>!Number.isFinite(v)||v<44).length,colorSamples});
   }
   """);
  var check=JsonNode.Parse(json)!;
  static UiRgb Color(JsonNode node)
  {
   var channels=node.AsArray();if(channels.Count!=3)throw new FormatException("Expected three CSS RGB channels.");
   return new(channels[0]!.GetValue<byte>(),channels[1]!.GetValue<byte>(),channels[2]!.GetValue<byte>());
  }
  var contrasts=new JsonArray();
  foreach(var sample in check["colorSamples"]!.AsArray())
  {
   var foreground=Color(sample!["foreground"]!);var background=Color(sample["background"]!);
   contrasts.Add((JsonNode)new JsonObject { ["Tag"]=sample["tag"]!.GetValue<string>(),["Class"]=sample["cls"]!.GetValue<string>(),
    ["Ratio"]=UiContrast.Ratio(foreground,background),["Meets4_5"]=UiContrast.MeetsNormalText(foreground,background) });
  }
  check["contrastChecks"]=contrasts;
  var evidence=new UiLayout(check["viewport"]!.GetValue<int>(),check["documentWidth"]!.GetValue<int>(),check["outside"]!.AsArray().Count);
  bool textOk=name=="home" || (check["minSvgTextPixels"] is { } minimum
    && UiLayout.MeetsSvgTextFloor(minimum.GetValue<double>(),check["svgTextCount"]!.GetValue<int>())
    && check["smallSvgTextCount"]!.GetValue<int>()==0);
  bool controlsOk=name=="home" || (check["minControlHeight"] is { } controlHeight
    && UiLayout.MeetsControlHeightFloor(controlHeight.GetValue<double>(),check["controls"]!.GetValue<int>())
    && check["shortControlCount"]!.GetValue<int>()==0);
  bool contrastOk=contrasts.Count>0&&contrasts.All(c=>c!["Meets4_5"]!.GetValue<bool>());
  bool ok=evidence.ViewportWidth==360&&evidence.FitsViewport&&textOk&&controlsOk&&contrastOk;
  layouts.Add((JsonNode)new JsonObject { ["Page"]=name,["Success"]=ok,["Checks"]=check,["ScrollChecks"]=scrollChecks });
  var minimumContrast=contrasts.Count>0?contrasts.Min(c=>c!["Ratio"]!.GetValue<double>()):0;
  Console.WriteLine($"Layout {name}: viewport {evidence.ViewportWidth}, document {evidence.DocumentWidth}, sampled contrast min {minimumContrast:0.00}:1, {contrasts.Count} text samples, pass={ok}.");
  if(!ok)throw new Exception("Narrow layout, measured size or sampled text contrast failed: "+name);
 }

 var home=await page.GotoAsync(url+"/");if(home?.Status!=200)throw new Exception("Home did not return HTTP 200.");
 if(await page.Locator("h1").CountAsync()!=1)throw new Exception("Missing home heading.");
 await CheckMobileLayout("home");
 string summary;
 if(quadro||fischio)
 {
  string route=fischio?"fischio":"quadro",gridLabel=fischio?"LBM grid":"Numerical grid",gridValue=fischio?"140":"31";
  string hydrateLabel=fischio?"Wind":"Cabinet side",hydrateValue=fischio?"45":"85",hydrateText=fischio?"45 km/h":"85 cm";
  string startLabel=fischio?"Watch the vortices":"Start simulation";
  string[] controls=fischio?["Object","Diameter","Wind","LBM grid"]:["Numerical grid","Cabinet side","Air speed","Fluid"];
  await page.Locator($"a.card[href='{route}']").ClickAsync();await page.WaitForURLAsync("**/"+route);
  // First confirm an actual bound update: DOM presence alone does not prove Blazor hydration.
  var hydrated=false;var hydrateDeadline=DateTime.UtcNow.AddSeconds(15);
  while(DateTime.UtcNow<hydrateDeadline)
  {
   await page.GetByLabel(hydrateLabel,new() { Exact=true }).EvaluateAsync<bool>("(el,value)=>{el.value=value;el.dispatchEvent(new Event('input',{bubbles:true}));return true;}",hydrateValue);
   await Task.Delay(300);
   hydrated=await page.GetByLabel(hydrateLabel,new() { Exact=true }).EvaluateAsync<bool>("(el,text)=>el.closest('.field').querySelector('output').textContent.trim()===text",hydrateText);
   if(hydrated)break;
  }
  if(!hydrated)throw new Exception(route+" did not become interactive.");
  await page.GetByLabel(gridLabel,new() { Exact=true }).SelectOptionAsync(gridValue);
  var cycles=new JsonArray();
  for(int cycle=1;cycle<=2;cycle++)
  {
   await page.GetByRole(AriaRole.Button,new() { Name=startLabel,Exact=true }).ClickAsync();
   await page.WaitForFunctionAsync("document.querySelector('.status')?.textContent.trim().startsWith('step ')",null,new() { Timeout=15000 });
   var progressText=await page.Locator(".status").InnerTextAsync();
   foreach(var label in controls)
    if(await page.GetByLabel(label,new() { Exact=true }).IsEnabledAsync())throw new Exception("Control not locked while running: "+label);
   if(await page.GetByLabel(gridLabel,new() { Exact=true }).InputValueAsync()!=gridValue)throw new Exception("Expected smallest simulation grid.");
   if(await page.Locator(".chart svg").CountAsync()<1)throw new Exception("Live field SVG missing.");
   await page.GetByRole(AriaRole.Button,new() { Name="Stop",Exact=true }).ClickAsync();
   await page.WaitForFunctionAsync("document.querySelector('.status')?.textContent.trim()==='Simulation stopped.'",null,new() { Timeout=15000 });
   if(!await page.GetByRole(AriaRole.Button,new() { Name=startLabel,Exact=true }).IsEnabledAsync())throw new Exception("Start did not become usable again.");
   foreach(var label in controls)
    if(!await page.GetByLabel(label,new() { Exact=true }).IsEnabledAsync())throw new Exception("Control not restored: "+label);
   if(await page.Locator("#blazor-error-ui").IsVisibleAsync())throw new Exception("Blazor error after cancellation.");
   cycles.Add((JsonNode)new JsonObject { ["Cycle"]=cycle,["ProgressObserved"]=progressText,["Stopped"]=true,["ControlsRestored"]=true });
  }
  steps.Add((JsonNode)new JsonObject { ["Page"]=route,["Grid"]=int.Parse(gridValue),["Cycles"]=cycles,["Success"]=true });
  summary=$"Live {route}: grid {gridValue}, two start/progress/stop cycles, controls restored. No convergence, shedding frequency or accuracy claim.";
 }
 else
 {
 await page.Locator("a.card[href='tubo']").ClickAsync();await page.WaitForURLAsync("**/tubo");
 var pressureBefore=UiNumbers.PressureBar(await page.Locator(".answer .big").InnerTextAsync());
 if(pressureBefore<=0)throw new Exception("Invalid initial pressure.");
 await ChangeRange("Pipe length","100",UiNumbers.PressureBar,p=>Math.Abs(p-2*pressureBefore)<=0.03);
 var pressureAfter=UiNumbers.PressureBar(await page.Locator(".answer .big").InnerTextAsync());await CheckCharts(2);
 await CheckMobileLayout("tubo");
 steps.Add((JsonNode)new JsonObject { ["Page"]="tubo",["BeforeBar"]=pressureBefore,["AfterBar"]=pressureAfter,["Check"]="Doubling length doubles rounded pressure within 0.03 bar",["Success"]=true });

 await page.GotoAsync(url+"/");await page.Locator("a.card[href='vento']").ClickAsync();await page.WaitForURLAsync("**/vento");
 var forceBefore=UiNumbers.ForceNewtons(await page.Locator(".answer .big").InnerTextAsync());
 if(forceBefore<=0)throw new Exception("Invalid initial force.");
 await ChangeRange("Wind","180",UiNumbers.ForceNewtons,f=>Math.Abs(f-4*forceBefore)<=3);
 var forceFast=UiNumbers.ForceNewtons(await page.Locator(".answer .big").InnerTextAsync());
 await ChangeRange("Exposed area","1",UiNumbers.ForceNewtons,f=>Math.Abs(2*f-forceFast)<=2);
 var forceHalfArea=UiNumbers.ForceNewtons(await page.Locator(".answer .big").InnerTextAsync());await CheckCharts(1);
 steps.Add((JsonNode)new JsonObject { ["Page"]="vento",["BeforeN"]=forceBefore,["DoubledSpeedN"]=forceFast,["HalfAreaN"]=forceHalfArea,["Check"]="Rounded force scales with speed squared and area",["Success"]=true });
 if(mobile)
 {
  var wind=page.GetByLabel("Wind",new() { Exact=true });
  int beforeWind=int.Parse(await wind.InputValueAsync());
  await wind.FocusAsync();await wind.PressAsync("ArrowRight");
  double keyboardForce=forceHalfArea;bool updated=false;var keyboardDeadline=DateTime.UtcNow.AddSeconds(15);
  while(DateTime.UtcNow<keyboardDeadline)
  {
   keyboardForce=UiNumbers.ForceNewtons(await page.Locator(".answer .big").InnerTextAsync());
   updated=int.Parse(await wind.InputValueAsync())==beforeWind+1 && keyboardForce>forceHalfArea;
   if(updated)break;
   await Task.Delay(200);
  }
  if(!updated)throw new Exception("Native keyboard input did not update the wind calculator.");
  bool focusVisible=await wind.EvaluateAsync<bool>("el=>el.matches(':focus-visible')&&parseFloat(getComputedStyle(el).outlineWidth)>=2&&getComputedStyle(el).outlineStyle!=='none'");
  if(!focusVisible)throw new Exception("Keyboard focus indicator missing on range input.");
  steps.Add((JsonNode)new JsonObject { ["Page"]="vento",["Keyboard"]="ArrowRight",["WindBeforeKmh"]=beforeWind,["WindAfterKmh"]=beforeWind+1,
    ["ForceBeforeN"]=forceHalfArea,["ForceAfterN"]=keyboardForce,["FocusVisible"]=focusVisible,["Success"]=true });
  Console.WriteLine($"Native keyboard: wind {beforeWind} -> {beforeWind+1} km/h, force {forceHalfArea} -> {keyboardForce} N, focus visible.");
 }
 await CheckMobileLayout("vento");
 summary=$"Live Web smoke: tubo {pressureBefore} -> {pressureAfter} bar; vento {forceBefore} -> {forceFast} -> {forceHalfArea} N.";
 }
 if(errors.Count>0)throw new Exception("Uncaught browser error: "+errors[0]);
 if(args.Contains("--screenshot"))await page.ScreenshotAsync(new() { Path=Path.Combine(output,fischio?"fischio.png":quadro?"quadro.png":"vento.png"),FullPage=true });
 success=true;Console.WriteLine(summary+" No images unless requested.");
}
catch(Exception ex){failure=ex.Message;Console.WriteLine("Web smoke failed: "+failure);}
finally
{
 if(!process.HasExited)process.Kill(entireProcessTree:true);
 await process.WaitForExitAsync();File.WriteAllText(Path.Combine(output,"server.log"),await stdout+await stderr);
 var report=new JsonObject { ["GeneratedUtc"]=DateTime.UtcNow.ToString("O"),["Success"]=success,["Failure"]=failure,["BaseUrl"]=url,["Steps"]=steps,["Layouts"]=layouts,
 ["ScreenshotsRequested"]=args.Contains("--screenshot"),["Mode"]=mobile?"mobile-calculators":fischio?"fischio-cancellation":quadro?"quadro-cancellation":"calculators",
 ["Scope"]=mobile?"Running Kestrel + Edge at viewport 360x800: home/tubo/vento horizontal fit, SVG text at least 10 effective CSS pixels, input/select heights at least 44 CSS pixels, keyboard chart scrolling/right-edge reach, native range keyboard update and focus indicator; sampled support/accent HTML text contrast at least 4.5:1 on composited solid backgrounds. Not all text/SVG contrast, gradients/opacity, physical phones, touch hit-testing, complete accessibility or deployment.":fischio?"Running Kestrel + Edge: smallest LBM grid 140x50, progress and two stop/restart cycles. Cancellation only, not periodicity/Strouhal, convergence, mobile or deployment.":quadro?"Running Kestrel + Edge: grid 31, progress observed and two stop/restart cycles. Cancellation only, not convergence/accuracy, fischio, mobile or deployment.":"Running Kestrel + Edge, home navigation and interactive tubo/vento at desktop size. Range input events dispatched through DOM; rounded display tolerances. Not quadro/fischio simulations, mobile, factual editorial validation or deployment." };
 File.WriteAllText(Path.Combine(output,"report.json"),report.ToJsonString(new JsonSerializerOptions { WriteIndented=true }));
 Console.WriteLine("Report: "+output);
}
return success?0:1;
