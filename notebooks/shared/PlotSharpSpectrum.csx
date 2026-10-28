// Local source-build API, separate from the facade used by finite-only packaged previews.
public static class SpectrumChart
{
    public static void Show(string title, double[] values) => Display.Html(PlotSharp.IndexedCharts.Stems(title, values, band: null, firstLag: 0, xLabel: "k"));
}
