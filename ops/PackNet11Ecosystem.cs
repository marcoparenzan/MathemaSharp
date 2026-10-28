#:property TargetFramework=net11.0
#:property JsonSerializerIsReflectionEnabledByDefault=true
using System.Xml.Linq;
using System.Diagnostics;
using System.Text.Json;
var root=@"D:\Dev\Science";var feed=@"D:\Dev\NuGetLocalFeed";var logs=@"D:\Dev\BuildArtifacts\net11-pack";
Directory.CreateDirectory(feed);Directory.CreateDirectory(logs);
var projects=new List<(string Repo,string Path,string Id,string Version)>();
foreach(var repo in new[]{"NDSharp","MathemaSharp","PySharp","MatSharp","PlotSharp","JupyterSharp"})
foreach(var path in Directory.EnumerateFiles(Path.Combine(root,repo,"src"),"*.csproj",SearchOption.AllDirectories).Where(p=>!p.Split('\\').Any(s=>new[]{"imports","obj","bin"}.Contains(s))))
{
 var d=XDocument.Load(path);var id=d.Descendants("PackageId").FirstOrDefault()?.Value;
 if(repo=="MathemaSharp" && path.Contains("\\libs\\"))id=Path.GetFileNameWithoutExtension(path);
 if(repo=="PlotSharp" && Path.GetFileName(path)=="PlotSharp.csproj")id="PlotSharp";
 if(id is null || d.Descendants("IsPackable").Any(e=>e.Value=="false"))continue;
 var old=d.Descendants("Version").FirstOrDefault()?.Value??(repo=="MathemaSharp"?"0.2.0":"0.1.0");
 var v=Version.Parse(old.Split('-')[0]);
 var version=$"{v.Major}.{v.Minor}.{Math.Max(v.Build,0)+1}-net11-preview.1";
 if(old.Contains("net11-preview"))version=old;
 var pg=d.Root!.Elements("PropertyGroup").FirstOrDefault()??new XElement("PropertyGroup");if(pg.Parent is null)d.Root.Add(pg);
 var element=d.Descendants("Version").FirstOrDefault();if(element is null)pg.Add(new XElement("Version",version));else element.Value=version;
 d.Save(path);projects.Add((repo,path,id,version));
}
var map=projects.ToDictionary(p=>p.Id,p=>p.Version);
foreach(var repo in new[]{"NDSharp","MathemaSharp","PySharp","MatSharp","PlotSharp","JupyterSharp"})
foreach(var path in Directory.EnumerateFiles(Path.Combine(root,repo),"*",SearchOption.AllDirectories).Where(p=>!p.Split('\\').Any(s=>new[]{"imports","obj","bin",".git",".ralfai"}.Contains(s))).Where(p=>p.EndsWith(".csproj")||p.EndsWith(".props")))
{
 var d=XDocument.Load(path);bool changed=false;
 foreach(var e in d.Descendants().Where(e=>e.Name.LocalName is "PackageReference" or "PackageVersion"))
 if(e.Attribute("Include")?.Value is string id && map.TryGetValue(id,out var v) && e.Attribute("Version") is { } attr){attr.Value=v;changed=true;}
 if(changed)d.Save(path);
}
var results=new List<object>();
foreach(var p in projects)
{
 // Never overwrite a feed package: versioned packages are immutable.
 var target=Path.Combine(feed,$"{p.Id}.{p.Version}.nupkg");
 if(File.Exists(target)){Console.WriteLine("EXISTS "+target);results.Add(new {p.Id,p.Version,Status="ExistingNotRepacked"});continue;}
 var start=new ProcessStartInfo("dotnet"){WorkingDirectory=Path.Combine(root,p.Repo),UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true};
 foreach(var arg in new[]{"pack",p.Path,"-c","Release","--output",feed,"--nologo","-v","quiet"})start.ArgumentList.Add(arg);
 using var child=Process.Start(start)!;var stdout=child.StandardOutput.ReadToEndAsync();var stderr=child.StandardError.ReadToEndAsync();child.WaitForExit();var log=await stdout+await stderr;
 File.WriteAllText(Path.Combine(logs,p.Id+".log"),log);Console.WriteLine($"PACK {p.Id} {p.Version}: {child.ExitCode}");
 if(child.ExitCode!=0){Console.WriteLine(log);File.WriteAllText(Path.Combine(logs,"report.json"),JsonSerializer.Serialize(results));return 1;}
 results.Add(new {p.Id,p.Version,Status="Packed"});
}
File.WriteAllText(Path.Combine(logs,"report.json"),JsonSerializer.Serialize(results,new JsonSerializerOptions{WriteIndented=true}));
return 0;
