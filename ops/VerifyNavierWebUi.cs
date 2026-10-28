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
var output=Path.GetFullPath("ops/visual/navier-web");Directory.CreateDirectory(output);
var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();int port=((IPEndPoint)listener.LocalEndpoint).Port;listener.Stop();
var url=$"http://127.0.0.1:{port}";
var project=Path.GetFullPath("../articoli/2026.10 NavierStokes/src/NavierStokes.Web/NavierStokes.Web.csproj");
var psi=new ProcessStartInfo("dotnet") { WorkingDirectory=Path.GetDirectoryName(project)!,UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true };
foreach(var a in new[]{"run","--project",project,"-c","Release","--no-build","--no-launch-profile","--","--urls",url})psi.ArgumentList.Add(a);
psi.Environment["ASPNETCORE_ENVIRONMENT"]="Development";psi.Environment.Remove("ASPNETCORE_HTTPS_PORT");psi.Environment.Remove("HTTPS_PORT");
using var server=Process.Start(psi)??throw new Exception("Unable to start owned local Web process.");
var stdout=server.StandardOutput.ReadToEndAsync();var stderr=server.StandardError.ReadToEndAsync();
var cases=new JsonArray();var errors=new List<string>();string? failure=null;string browserVersion="";bool stopped=false;
try
{
 using var http=new HttpClient { Timeout=TimeSpan.FromSeconds(2) };
 var deadline=DateTime.UtcNow.AddSeconds(60);bool ready=false;
 while(DateTime.UtcNow<deadline&&!server.HasExited)
 {
  try { using var response=await http.GetAsync(url);if(response.IsSuccessStatusCode){ready=true;break;} } catch(HttpRequestException){} catch(TaskCanceledException){}
  await Task.Delay(250);
 }
 if(!ready)throw new Exception("Owned localhost Web process did not become ready.");
 using var playwright=await Playwright.CreateAsync();
 await using var browser=await playwright.Chromium.LaunchAsync(new() { Channel="msedge",Headless=true });browserVersion=browser.Version;
 var page=await browser.NewPageAsync(new() { ViewportSize=new() { Width=1440,Height=1000 },DeviceScaleFactor=1 });
 page.SetDefaultTimeout(30000);page.PageError+=(_,e)=>errors.Add(e);
 async Task Navigate(string path)
 {
  var response=await page.GotoAsync(url+path,new() { WaitUntil=WaitUntilState.NetworkIdle });
  if(response?.Status!=200)throw new Exception("Route failed: "+path);
 }
 async Task SetRange(string label,string value)
 {
  await page.GetByLabel(label,new() { Exact=true }).EvaluateAsync("(el,value)=>{el.value=value;el.dispatchEvent(new Event('input',{bubbles:true}));}",value);
 }
 async Task Capture(string name,JsonObject checks)
 {
  if(errors.Count>0)throw new Exception("Browser page error: "+errors[0]);
  bool bar=await page.EvaluateAsync<bool>("()=>{const e=document.querySelector('#blazor-error-ui');return !!e&&getComputedStyle(e).display!=='none';}");
  if(bar)throw new Exception("Blazor error UI visible: "+name);
  await page.ScreenshotAsync(new() { Path=Path.Combine(output,name+".png"),FullPage=true });
  cases.Add((JsonNode)new JsonObject { ["Case"]=name,["Success"]=true,["Checks"]=checks });Console.WriteLine(name+": OK");
 }
 await Navigate("/");
 foreach(var path in new[]{"quadro","tubo","fischio","vento"})if(await page.Locator($"a.card[href='{path}']").CountAsync()!=1)throw new Exception("Missing home navigation: "+path);
 await Capture("home",new() { ["ScenarioLinks"]=4 });

 await Navigate("/tubo");
 string pipeBefore=await page.Locator(".answer .big").InnerTextAsync();
 await SetRange("Portata","20");
 var pipeExpected=Pipe.Solve(Fluid.Water,0.025,50,20,0.0015).PressureDropPa/1e5;
 var expected=pipeExpected.ToString("0.00",CultureInfo.InvariantCulture);
 await page.WaitForFunctionAsync("expected=>document.querySelector('.answer .big').textContent.replaceAll(',','.').includes(expected)",expected);
 string pipeAfter=await page.Locator(".answer .big").InnerTextAsync();
 if(pipeBefore==pipeAfter||await page.Locator(".chart svg").CountAsync()!=2)throw new Exception("Pipe binding or chart update failed.");
 await Capture("pipe",new() { ["Before"]=pipeBefore,["After"]=pipeAfter,["Charts"]=2,["ExpectedPressureBar"]=expected });

 await Navigate("/vento");string windBefore=await page.Locator(".answer .big").InnerTextAsync();
 await SetRange("Vento","45");
 var expectedForce=Wind.Force(45/3.6,2,Shape.FlatPlate).ToString("0",CultureInfo.InvariantCulture);
 await page.WaitForFunctionAsync("expected=>document.querySelector('.answer .big').textContent.trim().startsWith(expected+' N')",expectedForce);
 string windAfter=await page.Locator(".answer .big").InnerTextAsync();
 if(windBefore==windAfter||await page.Locator(".chart svg").CountAsync()!=1)throw new Exception("Wind binding or chart update failed.");
 await Capture("wind",new() { ["Before"]=windBefore,["After"]=windAfter,["Charts"]=1,["ExpectedForceN"]=expectedForce });

 await Navigate("/quadro");await page.GetByLabel("Griglia numerica").SelectOptionAsync("31");await page.GetByLabel("Fluido").SelectOptionAsync("Olio");
 await SetRange("Lato del quadro","20");await SetRange("Velocità dell'aria","0.1");
 await page.GetByRole(AriaRole.Button,new() { Name="Avvia simulazione",Exact=true }).ClickAsync();
 await page.WaitForFunctionAsync("()=>document.querySelector('.status')?.textContent.startsWith('Fatto:')",null,new() { Timeout=180000 });
 if(await page.Locator(".chart svg").CountAsync()!=3||await page.Locator(".chart svg image").CountAsync()!=2||await page.Locator(".chart svg g.point-overlays").CountAsync()<1)throw new Exception("Cavity reports missing after UI simulation.");
 if(!await page.GetByLabel("Griglia numerica").IsEnabledAsync())throw new Exception("Cavity controls not restored.");
 await Capture("cavity-complete",new() { ["Grid"]=31,["Charts"]=3,["RetainedFieldImages"]=2,["Status"]=await page.Locator(".status").InnerTextAsync() });
 await page.GetByRole(AriaRole.Button,new() { Name="Avvia simulazione",Exact=true }).ClickAsync();
 await page.GetByRole(AriaRole.Button,new() { Name="Ferma",Exact=true }).ClickAsync();
 await page.WaitForFunctionAsync("()=>document.querySelector('.status')?.textContent.includes('Simulazione fermata.')");
 if(!await page.GetByLabel("Griglia numerica").IsEnabledAsync())throw new Exception("Cancel did not restore cavity controls.");
 await Capture("cavity-cancel",new() { ["Stopped"]=true,["ControlsEnabled"]=true });

 await Navigate("/fischio");await page.GetByLabel("Griglia LBM").SelectOptionAsync("140");
 await page.GetByRole(AriaRole.Button,new() { Name="Guarda i vortici",Exact=true }).ClickAsync();
 await page.WaitForFunctionAsync("()=>document.querySelector('.status')?.textContent.trim()==='Fatto.'",null,new() { Timeout=180000 });
 if(await page.Locator(".chart svg").CountAsync()!=2||await page.Locator(".chart svg image").CountAsync()!=1||await page.Locator(".chart svg g.point-overlays").CountAsync()<1)throw new Exception("Cylinder reports missing after UI simulation.");
 if(!await page.GetByLabel("Griglia LBM").IsEnabledAsync())throw new Exception("Cylinder controls not restored.");
 await Capture("cylinder-complete",new() { ["GridWidth"]=140,["Charts"]=2,["RetainedFieldImages"]=1,["Status"]=await page.Locator(".status").InnerTextAsync() });
}
catch(Exception e){failure=e.Message;Console.WriteLine("UI verification failed: "+failure);}
finally
{
 if(!server.HasExited)server.Kill(entireProcessTree:true);
 await server.WaitForExitAsync();stopped=server.HasExited;
 File.WriteAllText(Path.Combine(output,"server.log"),await stdout+await stderr);
}
var errorArray=new JsonArray();foreach(var e in errors)errorArray.Add((JsonNode)JsonValue.Create(e)!);
var report=new JsonObject { ["GeneratedUtc"]=DateTime.UtcNow.ToString("O"),["Success"]=failure is null&&errors.Count==0&&stopped,["Failure"]=failure,["LocalUrl"]=url,["OwnedServerPid"]=server.Id,["OwnedServerStopped"]=stopped,["Browser"]=browserVersion,["PageErrors"]=errorArray,["Cases"]=cases,
 ["Notes"]="Real HTTP/Blazor UI on localhost, numeric bindings and coarse simulations/cancellation. Not proof of mobile UI, OS tooltip popup, all simulation parameter combinations or numerical accuracy of coarse grids; editorial claims are not validated." };
File.WriteAllText(Path.Combine(output,"report.json"),report.ToJsonString(new JsonSerializerOptions { WriteIndented=true }));
return report["Success"]!.GetValue<bool>()?0:1;
