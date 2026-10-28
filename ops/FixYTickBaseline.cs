#:property TargetFramework=net11.0
using System.Xml.Linq;
var root=@"D:\Dev\Science";
var p=Path.Combine(root,"NDSharp","src","NDSharp.Plot","Axes.cs");var text=File.ReadAllText(p);
var old="top = Math.Min(top, (float)(py - sz.Height / 2)); bottom = Math.Max(bottom, (float)(py + sz.Height / 2));";
if(!text.Contains(old))throw new Exception("Expected tick extent anchor missing");
text=text.Replace(old,"// Matplotlib ytick default is center_baseline, not geometric center.\n                    top = Math.Min(top, (float)(py - sz.Ascent / 2)); bottom = Math.Max(bottom, (float)(py + sz.Ascent / 2 + sz.Descent));");
text=text.Replace("Renderer.DrawText(canvas, text, lx, py, TickLabelSize", "Renderer.DrawText(canvas, text, lx, py + sz.Descent / 2, TickLabelSize");
File.WriteAllText(p,text);
foreach(var item in new[]{("NDSharp","NDSharp.Plot","2.6.3-net11-preview.2"),("PySharp","PySharpLib.Matplotlib","2.5.1-net11-preview.2"),("PySharp","PySharp","2.5.1-net11-preview.3"),("MatSharp","MatSharpLib.Plot","0.1.1-net11-preview.2"),("MatSharp","MatSharp","0.1.1-net11-preview.2")})
{
p=Path.Combine(root,item.Item1,"src",item.Item2,item.Item2+".csproj");var d=XDocument.Load(p);d.Descendants("Version").Single().Value=item.Item3;d.Save(p);
}
var tests=Path.Combine(root,"NDSharp","src","NDSharp.Plot.Tests","TickBaselineTests.cs");
File.WriteAllText(tests,"""
using NDSharp.Plot;
using Xunit;
namespace NDSharp.Plot.Tests;
public class TickBaselineTests
{
    [Fact]
    public void TextMetricsHaveAscentAndDescentForBaselineAlignment()
    {
        var size=TextMetrics.Measure("0",10,100);
        Assert.True(size.Ascent>0);
        Assert.True(size.Descent>0);
        var upper=size.Ascent/2;
        var lower=size.Ascent/2+size.Descent;
        Assert.Equal(size.Height,upper+lower,10);
        Assert.True(lower>upper);
    }
}
""");
