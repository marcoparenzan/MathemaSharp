// Opt-in facade: load PlotSharp and the host's Display API before this script.
// Local PlotSharp builds support NaN Y line gaps; stems stay finite. All-missing series/infinity are rejected.
// Previously packaged previews are finite-only. Legacy Charts.csx remains for custom ticks and other helpers.
public sealed record Line(string Name, double[] Y, int Start = 0, bool Dashed = false);
public static class Chart
{
    static PlotSharp.IndexedCharts.Line Convert(Line line) => new(line.Name, line.Y, line.Start, line.Dashed);
    public static void Lines(string title, params Line[] lines) => Display.Html(PlotSharp.IndexedCharts.Lines(title, lines.Select(Convert).ToArray()));
    public static void Panels(string title, params Line[] lines) => Display.Html(PlotSharp.IndexedCharts.Panels(title, lines.Select(Convert).ToArray()));
    public static void Stems(string title, double[] values, double band = double.NaN, int firstLag = 1) => Display.Html(PlotSharp.IndexedCharts.Stems(title, values, double.IsNaN(band) ? null : band, firstLag));
}
