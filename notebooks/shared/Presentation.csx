// HTML notebook presentation, independent of the SVG renderer. Extracted without semantic changes.
using System.Globalization;
using System.Net;
using System.Text;

/// <summary>Tabella HTML semplice: utile per confrontare modelli e per la vista tabellare dei grafici.</summary>
public static class Table
{
    public static void Show(string[] headers, IEnumerable<object[]> rows)
    {
        var html = new StringBuilder("<table style=\"border-collapse:collapse;font:13px system-ui,sans-serif\"><tr>");
        foreach (var h in headers) html.Append($"<th style=\"text-align:left;padding:4px 10px;border-bottom:1px solid #c9c8c2\">{WebUtility.HtmlEncode(h)}</th>");
        html.Append("</tr>");
        foreach (var row in rows)
        {
            html.Append("<tr>");
            foreach (var cell in row)
            {
                var text = cell is double d ? d.ToString("0.####", CultureInfo.InvariantCulture) : Convert.ToString(cell, CultureInfo.InvariantCulture);
                var align = cell is double or int ? "right" : "left";
                html.Append($"<td style=\"text-align:{align};padding:4px 10px;border-bottom:1px solid #eeede9\">{WebUtility.HtmlEncode(text)}</td>");
            }
            html.Append("</tr>");
        }
        Display.Html(html.Append("</table>").ToString());
    }
}

/// <summary>Un indicatore: etichetta, valore grande, nota e stato. Lo stato si legge SEMPRE da simbolo e parola, mai dal solo colore.</summary>
public sealed record Tile(string Label, string Value, string Note = "", string Status = "");

/// <summary>Riga di tile KPI per le dashboard dei notebook (stat tile): valore in evidenza, contesto sotto, stato con simbolo e parola.</summary>
public static class Kpi
{
    static string StatusText(string status) => status switch
    {
        "ok" => "✓ nella norma",
        "warn" => "▲ attenzione",
        "alarm" => "✖ allarme",
        _ => "",
    };

    public static void Tiles(params Tile[] tiles)
    {
        var html = new StringBuilder("<div class=\"mathema-kpi\" style=\"display:grid;grid-template-columns:repeat(auto-fit,minmax(170px,1fr));gap:8px;font-family:system-ui,sans-serif\">");
        foreach (var t in tiles)
        {
            var status = StatusText(t.Status);
            html.Append("<div style=\"background:#fcfcfb;border:1px solid #e4e3df;border-radius:8px;padding:10px 12px\">")
                .Append($"<div style=\"font-size:12px;color:#52514e\">{WebUtility.HtmlEncode(t.Label)}</div>")
                .Append($"<div style=\"font-size:26px;font-weight:600;color:#0b0b0b;line-height:1.2\">{WebUtility.HtmlEncode(t.Value)}</div>")
                .Append($"<div style=\"font-size:12px;color:#52514e\">{WebUtility.HtmlEncode(t.Note)}</div>");
            if (status.Length > 0) html.Append($"<div style=\"font-size:12px;font-weight:600;color:#0b0b0b;margin-top:2px\">{status}</div>");
            html.Append("</div>");
        }
        Display.Html(html.Append("</div>").ToString());
    }

    /// <summary>Riquadro di testo (es. la risposta al cliente), con il testo già composto dai numeri calcolati.</summary>
    public static void Quote(string title, string text) =>
        Display.Html($"<div class=\"mathema-quote\" style=\"background:#fcfcfb;border-left:4px solid #2a78d6;padding:10px 14px;margin:8px 0;font-family:system-ui,sans-serif;color:#0b0b0b\"><div style=\"font-size:12px;color:#52514e\">{WebUtility.HtmlEncode(title)}</div><div style=\"font-size:14px\">{WebUtility.HtmlEncode(text)}</div></div>");
}
