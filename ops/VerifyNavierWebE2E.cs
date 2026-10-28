#:property TargetFramework=net11.0
#:property JsonSerializerIsReflectionEnabledByDefault=true
#:property PublishAot=false
#:project "../../articoli/2026.10 NavierStokes/src/NavierStokes.Core/NavierStokes.Core.csproj"
#:package Microsoft.Playwright@1.56.0
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Playwright;
using NavierStokes.Core;

var output="ops/visual/navier-web";Directory.CreateDirectory(output);
var project=Path.GetFullPath("../articoli/2026.10 NavierStokes/src/NavierStokes.Web/NavierStokes.Web.csproj");
var reservation=new TcpListener(IPAddress.Loopback,0);reservation.Start();int port=((IPEndPoint)reservation.LocalEndpoint).Port;reservation.Stop();
var origin=$"http://127.0.0.1:{port}";
var psi=new ProcessStartInfo("dotnet"){WorkingDirectory=Path.GetDirectoryName(project),UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true};
foreach(var arg in new[]{"run","--project",project,"-c","Release","--no-build","--no-launch-profile","--","--urls",origin})psi.ArgumentList.Add(arg);
psi.Environment["ASPNETCORE_ENVIRONMENT"]="Development";psi.Environment["DOTNET_ENVIRONMENT"]="Development";
using var server=Process.Start(psi)!;
var stdout=server.StandardOutput.ReadToEndAsync();var stderr=server.StandardError.ReadToEndAsync();
var checks=new JsonArray();var errors=new List<string>();int failed=0;string? failure=null;
IPlaywright? playwright=null;IBrowser? browser=null;IPage? page=null;
void Record(string name,JsonNode data) { checks.Add((JsonNode)new JsonObject { ["Name"]=name,["Success"]=true,["Data"]=data });Console.WriteLine("PASS "+name); }
async Task Answer(double value,string format="0.00")
{
 var accepted=new[]{value.ToString(format,CultureInfo.InvariantCulture),value.ToString(format,CultureInfo.GetCultureInfo("it-IT"))};
 await page!.WaitForFunctionAsync("values => values.some(v => document.querySelector('.answer .big')?.textContent.includes(v))",accepted,new() { Timeout=30000 });
}
async Task Visit(string route)
{
 await page!.GotoAsync(origin+route,new() { WaitUntil=WaitUntilState.DOMContentLoaded });
 await page.Locator("h1").WaitForAsync();
 // Wait for the initial circuit handshake; every subsequent interaction is verified by an observed UI change.
 await page.WaitForTimeoutAsync(1000);
}
async Task Layout(string name)
{
 var json=await page!.EvaluateAsync<string>("""
 () => JSON.stringify({width:innerWidth,horizontalOverflow:document.documentElement.scrollWidth>innerWidth,
   svg:document.querySelectorAll('.chart svg').length,errorUi:getComputedStyle(document.querySelector('#blazor-error-ui')??document.body).display==='block'&&!!document.querySelector('#blazor-error-ui')})
 """);
 var data=JsonNode.Parse(json)!;
 if(data["horizontalOverflow"]!.GetValue<bool>()||data["errorUi"]!.GetValue<bool>())throw new Exception("Layout/circuit failure: "+name+" "+json);
 Record(name,data);
 await page.ScreenshotAsync(new() { Path=Path.Combine(output,name+".png"),FullPage=true });
}
try
{
 using var client=new HttpClient { Timeout=TimeSpan.FromSeconds(2) };
 bool ready=false;var deadline=DateTime.UtcNow.AddSeconds(45);
 while(DateTime.UtcNow<deadline&&!server.HasExited)
 {
  try { using var response=await client.GetAsync(origin);if(response.IsSuccessStatusCode){ready=true;break;} }catch(HttpRequestException){}catch(TaskCanceledException){}
  await Task.Delay(200);
 }
 if(!ready)throw new Exception("Local Kestrel server did not become ready.");
 playwright=await Playwright.CreateAsync();browser=await playwright.Chromium.LaunchAsync(new() { Channel="msedge",Headless=true });
 page=await browser.NewPageAsync(new() { ViewportSize=new() { Width=1280,Height=800 },DeviceScaleFactor=2 });
 page.PageError+=(_,error)=>errors.Add(error);
 await page.RouteAsync("**/*",route=>route.Request.Url.StartsWith(origin+"/",StringComparison.Ordinal)?route.ContinueAsync():route.AbortAsync());
 await Visit("/");await Layout("home-desktop");
 await Visit("/tubo");
 await Answer(Pipe.Solve(Fluid.Water,0.025,50,40,0.0015).PressureDropPa/1e5);
 await page.GetByLabel("Fluido",new() { Exact=true }).SelectOptionAsync(Fluid.Oil.Name);
 await Answer(Pipe.Solve(Fluid.Oil,0.025,50,40,0.0015).PressureDropPa/1e5);
 await page.Locator(".chart svg").First.WaitForAsync();
 Record("pipe-fluid-binding",new JsonObject { ["Fluid"]=Fluid.Oil.Name,["Svg"]=await page.Locator(".chart svg").CountAsync() });
 await page.GetByLabel("Fluido",new() { Exact=true }).SelectOptionAsync(Fluid.Water.Name);
 await page.GetByLabel("Materiale del tubo",new() { Exact=true }).SelectOptionAsync("0.15");
 await Answer(Pipe.Solve(Fluid.Water,0.025,50,40,0.15).PressureDropPa/1e5);
 Record("pipe-roughness-binding",new JsonObject { ["RoughnessMm"]=0.15 });
 await Layout("pipe-desktop");await page.SetViewportSizeAsync(390,844);await Layout("pipe-mobile");await page.SetViewportSizeAsync(1280,800);
 await Visit("/vento");
 await page.GetByLabel("Vento",new() { Exact=true }).PressAsync("End");
 await Answer(Wind.Force(200/3.6,2,Shape.FlatPlate),"N0");
 if(await page.GetByLabel("Vento",new() { Exact=true }).InputValueAsync()!="200")throw new Exception("Wind slider did not change.");
 Record("wind-keyboard-binding",new JsonObject { ["WindKmh"]=200,["Svg"]=await page.Locator(".chart svg").CountAsync() });
 await Layout("wind-desktop");await page.SetViewportSizeAsync(390,844);await Layout("wind-mobile");await page.SetViewportSizeAsync(1280,800);
 await Visit("/quadro");
 await page.GetByRole(AriaRole.Button,new() { Name="Avvia simulazione",Exact=true }).ClickAsync();
 await page.WaitForFunctionAsync("() => document.querySelector('.status')?.textContent.includes('passo')",null,new() { Timeout=60000 });
 await page.GetByRole(AriaRole.Button,new() { Name="Ferma",Exact=true }).ClickAsync();
 await page.WaitForFunctionAsync("() => document.querySelector('.status')?.textContent.includes('ferm')",null,new() { Timeout=60000 });
 Record("cavity-start-stop",new JsonObject { ["Status"]=await page.Locator(".status").InnerTextAsync() });
 await page.GetByLabel("Griglia numerica",new() { Exact=true }).SelectOptionAsync("31");
 await page.GetByRole(AriaRole.Button,new() { Name="Avvia simulazione",Exact=true }).ClickAsync();
 await page.WaitForFunctionAsync("() => document.querySelector('.status')?.textContent.startsWith('Fatto:')",null,new() { Timeout=300000 });
 if(await page.Locator(".chart svg").CountAsync()!=3)throw new Exception("Completed cavity did not render three charts.");
 Record("cavity-completed",new JsonObject { ["Status"]=await page.Locator(".status").InnerTextAsync(),["Svg"]=3 });
 await Layout("cavity-desktop");await page.SetViewportSizeAsync(390,844);await Layout("cavity-mobile");await page.SetViewportSizeAsync(1280,800);
 await Visit("/fischio");
 await page.GetByLabel("Oggetto",new() { Exact=true }).SelectOptionAsync("1500");
 await page.WaitForFunctionAsync("() => document.querySelector('.answer .big')?.textContent.includes('Oscilla')");
 await page.GetByLabel("Griglia LBM",new() { Exact=true }).SelectOptionAsync("140");
 await page.GetByRole(AriaRole.Button,new() { Name="Guarda i vortici",Exact=true }).ClickAsync();
 await page.WaitForFunctionAsync("() => document.querySelector('.status')?.textContent==='Fatto.'",null,new() { Timeout=300000 });
 if(await page.Locator(".chart svg").CountAsync()!=2)throw new Exception("Completed cylinder did not render two charts.");
 Record("cylinder-completed",new JsonObject { ["Svg"]=2,["GridWidth"]=140 });
 await Layout("cylinder-desktop");await page.SetViewportSizeAsync(390,844);await Layout("cylinder-mobile");
 if(errors.Count>0)throw new Exception("Browser page errors: "+string.Join(" | ",errors));
}
catch(Exception exception)
{
 failed=1;failure=exception.ToString();Console.WriteLine(failure);
 if(page is not null)try { await page.ScreenshotAsync(new() { Path=Path.Combine(output,"failure.png"),FullPage=true }); }catch(Exception shot){Console.WriteLine("Failure screenshot: "+shot.Message);}
}
finally
{
 if(browser is not null)await browser.DisposeAsync();playwright?.Dispose();
 if(!server.HasExited)server.Kill(entireProcessTree:true);
 await server.WaitForExitAsync();
 File.WriteAllText(Path.Combine(output,"server.log"),await stdout+await stderr);
 var result=new JsonObject { ["GeneratedUtc"]=DateTime.UtcNow.ToString("O"),["Origin"]=origin,["Failed"]=failed,["Failure"]=failure,["Checks"]=checks,
  ["Notes"]="Real local Kestrel and Blazor Server interactions in Edge with external font/network requests blocked; desktop/mobile overflow checks. Not all browsers, VS Code or native tooltip popup validation." };
 File.WriteAllText(Path.Combine(output,"report.json"),result.ToJsonString(new JsonSerializerOptions { WriteIndented=true }));
}
return failed==0?0:1;
