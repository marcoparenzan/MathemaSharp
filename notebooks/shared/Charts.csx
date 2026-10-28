#load "Presentation.csx"
// Grafici SVG statici per l'output HTML dei notebook (Show.Html). Nessuna dipendenza: stili inline, superficie chiara esplicita,
// palette categoriale validata (slot in ordine fisso), linee 2px, griglia hairline, tooltip nativi <title> su ogni punto.
using System.Globalization;
using System.Net;
using System.Text;

/// <summary>Una serie: Y[i] è disegnato all'ascissa Start + i. Dashed per le previsioni.</summary>
public sealed record Line(string Name, double[] Y, int Start = 0, bool Dashed = false);

public static class Chart
{
    static readonly string[] Palette = { "#2a78d6", "#eb6834", "#1baf7a", "#eda100", "#e87ba4", "#008300", "#6250d6", "#e34948" };
    const string Surface = "#fcfcfb", Grid = "#e4e3df", TextPrimary = "#0b0b0b", TextSecondary = "#52514e";
    const int Width = 720, Left = 56, Right = 16, Top = 40, Bottom = 32;

    static string F(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);
    static string Esc(string s) => WebUtility.HtmlEncode(s);

    /// <summary>Linee su un unico asse y. Legenda sopra il grafico quando le serie sono almeno due.</summary>
    public static void Lines(string title, params Line[] lines) => Display.Html(LinesSvg(title, 260, lines));

    /// <summary>Small multiples: un pannello per serie, ognuno con la propria scala y (es. componenti STL).</summary>
    public static void Panels(string title, params Line[] lines)
    {
        var html = new StringBuilder($"<div style=\"font:600 15px system-ui,sans-serif;color:{TextPrimary};margin:4px 0\">{Esc(title)}</div>");
        for (var i = 0; i < lines.Length; i++) html.Append(LinesSvg(lines[i].Name, 150, new[] { lines[i] with { } }, Palette[i % Palette.Length]));
        Display.Html(html.ToString());
    }

    /// <summary>Correlogramma: aste per lag 1..n con banda ±band (es. 1.96/√n) come riferimento di rumore bianco.</summary>
    public static void Stems(string title, double[] values, double band = double.NaN, int firstLag = 1)
    {
        const int height = 220;
        var limit = Math.Max(values.Max(v => Math.Abs(v)), double.IsNaN(band) ? 0 : band) * 1.1;
        limit = Math.Max(limit, 1e-12);
        double plotW = Width - Left - Right, plotH = height - Top - Bottom;
        double X(int i) => Left + (i + 0.5) * plotW / values.Length;
        double Y(double v) => Top + (limit - v) / (2 * limit) * plotH;
        var svg = Open(title, height);
        foreach (var tick in new[] { -limit / 1.1, 0, limit / 1.1 })
            svg.Append(GridLine(Y(tick), F(tick)));
        if (!double.IsNaN(band))
            foreach (var b in new[] { band, -band })
                svg.Append($"<line x1=\"{Left}\" x2=\"{Width - Right}\" y1=\"{F(Y(b))}\" y2=\"{F(Y(b))}\" stroke=\"{Palette[1]}\" stroke-width=\"1.5\" stroke-dasharray=\"5 4\"/>");
        for (var i = 0; i < values.Length; i++)
        {
            var x = F(X(i));
            svg.Append($"<g><title>lag {i + firstLag}: {values[i]:F3}</title>");
            svg.Append($"<line x1=\"{x}\" x2=\"{x}\" y1=\"{F(Y(0))}\" y2=\"{F(Y(values[i]))}\" stroke=\"{Palette[0]}\" stroke-width=\"2\" stroke-linecap=\"round\"/>");
            svg.Append($"<circle cx=\"{x}\" cy=\"{F(Y(values[i]))}\" r=\"4\" fill=\"{Palette[0]}\" stroke=\"{Surface}\" stroke-width=\"2\"/></g>");
            if (values.Length <= 30 || (i + firstLag) % 5 == 0)
                svg.Append($"<text x=\"{x}\" y=\"{height - 10}\" text-anchor=\"middle\" font-size=\"11\" fill=\"{TextSecondary}\">{i + firstLag}</text>");
        }
        if (!double.IsNaN(band))
            svg.Append($"<text x=\"{Width - Right}\" y=\"{F(Y(band) - 5)}\" text-anchor=\"end\" font-size=\"11\" fill=\"{TextSecondary}\">±{band:F3}</text>");
        Display.Html(svg.Append("</svg>").ToString());
    }

    static string LinesSvg(string title, int height, Line[] lines, string? singleColor = null)
    {
        var all = lines.SelectMany(l => l.Y.Where(double.IsFinite)).ToArray();
        double min = all.Min(), max = all.Max();
        if (max - min < 1e-12) { min -= 1; max += 1; }
        var pad = (max - min) * 0.05;
        min -= pad; max += pad;
        var xMax = Math.Max(1, lines.Max(l => l.Start + l.Y.Length - 1));
        var legend = lines.Length >= 2;
        var top = legend ? Top + 18 : Top;
        double plotW = Width - Left - Right, plotH = height - top - Bottom;
        double X(double i) => Left + i / xMax * plotW;
        double Y(double v) => top + (max - v) / (max - min) * plotH;

        var svg = Open(title, height);
        foreach (var tick in NiceTicks(min, max, 4)) svg.Append(GridLine(Y(tick), F(tick)));
        foreach (var tick in NiceTicks(0, xMax, 6).Where(t => t >= 0 && t <= xMax))
            svg.Append($"<text x=\"{F(X(tick))}\" y=\"{height - 10}\" text-anchor=\"middle\" font-size=\"11\" fill=\"{TextSecondary}\">{F(tick)}</text>");

        for (var s = 0; s < lines.Length; s++)
        {
            var line = lines[s];
            var color = singleColor ?? Palette[s % Palette.Length];
            var path = new StringBuilder();
            var pen = false;
            for (var i = 0; i < line.Y.Length; i++)
            {
                if (!double.IsFinite(line.Y[i])) { pen = false; continue; }
                path.Append(pen ? " L" : " M").Append(F(X(line.Start + i))).Append(',').Append(F(Y(line.Y[i])));
                pen = true;
            }
            var dash = line.Dashed ? " stroke-dasharray=\"6 4\"" : "";
            svg.Append($"<path d=\"{path}\" fill=\"none\" stroke=\"{color}\" stroke-width=\"2\" stroke-linejoin=\"round\" stroke-linecap=\"round\"{dash}/>");
            for (var i = 0; i < line.Y.Length; i++)
                if (double.IsFinite(line.Y[i]))
                    svg.Append($"<circle cx=\"{F(X(line.Start + i))}\" cy=\"{F(Y(line.Y[i]))}\" r=\"6\" fill=\"transparent\"><title>{Esc(line.Name)} · t={line.Start + i}: {line.Y[i]:F3}</title></circle>");
            if (legend)
            {
                var lx = Left + s * 150;
                svg.Append($"<line x1=\"{lx}\" x2=\"{lx + 18}\" y1=\"{Top}\" y2=\"{Top}\" stroke=\"{color}\" stroke-width=\"2\"{dash}/>");
                svg.Append($"<text x=\"{lx + 24}\" y=\"{Top + 4}\" font-size=\"12\" fill=\"{TextPrimary}\">{Esc(line.Name)}</text>");
            }
        }
        return svg.Append("</svg>").ToString();
    }

    static StringBuilder Open(string title, int height) => new StringBuilder(
        $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {Width} {height}\" width=\"100%\" style=\"max-width:{Width}px;background:{Surface};border-radius:8px;font-family:system-ui,sans-serif\" role=\"img\" aria-label=\"{Esc(title)}\">" +
        $"<text x=\"{Left}\" y=\"22\" font-size=\"14\" font-weight=\"600\" fill=\"{TextPrimary}\">{Esc(title)}</text>");

    static string GridLine(double y, string label) =>
        $"<line x1=\"{Left}\" x2=\"{Width - Right}\" y1=\"{F(y)}\" y2=\"{F(y)}\" stroke=\"{Grid}\" stroke-width=\"1\"/>" +
        $"<text x=\"{Left - 8}\" y=\"{F(y + 4)}\" text-anchor=\"end\" font-size=\"11\" fill=\"{TextSecondary}\">{label}</text>";

    static IEnumerable<double> NiceTicks(double min, double max, int count)
    {
        var raw = (max - min) / count;
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(raw)));
        var step = new[] { 1, 2, 2.5, 5, 10 }.Select(m => m * magnitude).First(s => s >= raw);
        for (var t = Math.Ceiling(min / step) * step; t <= max + 1e-9 * step; t += step) yield return Math.Abs(t) < 1e-12 * step ? 0 : t;
    }
}

