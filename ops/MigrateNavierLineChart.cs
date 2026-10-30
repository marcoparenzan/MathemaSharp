#:property TargetFramework=net11.0
// Idempotent local-source migration. Does not transfer field/raster algorithms or publish packages.
var path="../articoli/2026.10 NavierStokes/src/NavierStokes.Core/Charts.cs";
var text=File.ReadAllText(path);
const string begin="    public static string LineChart(IReadOnlyList<Series> series, LineChartOptions o)";
const string end="    // ───────────────────────── utilities";
int start=text.IndexOf(begin,StringComparison.Ordinal),finish=text.IndexOf(end,start,StringComparison.Ordinal);
if(start<0||finish<=start)throw new Exception("Missing migration boundaries; no files changed.");
if(text[start..finish].Contains("PlotSharp.Charts.LineChart")){Console.WriteLine("Already migrated.");return 0;}
var replacement="""
    /// <summary>Compatibility facade to shared PlotSharp; local field/PNG renderers remain below/above.</summary>
    public static string LineChart(IReadOnlyList<Series> series, LineChartOptions o)
    {
        ArgumentNullException.ThrowIfNull(series);
        ArgumentNullException.ThrowIfNull(o);
        var data = series.Select(s =>
        {
            ArgumentNullException.ThrowIfNull(s);
            return new PlotSharp.Charts.Series(s.Name, s.X, s.Y, s.Color, s.Markers, s.Line);
        }).ToArray();
        return PlotSharp.Charts.LineChart(data, new()
        {
            Title = o.Title, Subtitle = o.Subtitle, XLabel = o.XLabel, YLabel = o.YLabel,
            Width = o.Width, Height = o.Height, XMin = o.XMin, XMax = o.XMax,
            YMin = o.YMin, YMax = o.YMax, Highlight = o.Highlight,
            PreserveBoundaryMarkers = true, SeparateLegendRow = true, PointTooltips = true,
        });
    }

""";
File.WriteAllText(path,text[..start]+replacement+Environment.NewLine+text[finish..]);
Console.WriteLine("Navier line/scatter delegated; field/PNG code retained.");return 0;
