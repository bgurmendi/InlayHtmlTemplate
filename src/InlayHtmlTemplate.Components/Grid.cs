using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.Components;

public class Grid : IHtmlContent
{
    private readonly List<List<GridCell>> _rows = [];
    private readonly int _gap;

    public Grid(int gap = 4)
    {
        _gap = gap;
    }

    public Grid NewRow()
    {
        _rows.Add([]);
        return this;
    }

    public Grid Add(IHtmlContent content, int span = 12, int? spanSm = null, int? spanMd = null, int? spanLg = null)
    {
        if (span is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(span));
        if (spanSm is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(spanSm));
        if (spanMd is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(spanMd));
        if (spanLg is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(spanLg));

        if (_rows.Count == 0) NewRow();
        _rows[^1].Add(new GridCell(content, span, spanSm, spanMd, spanLg));
        return this;
    }

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        writer.Write("<style data-il-grid>");
        writer.Write(Styles);
        writer.Write("</style>");

        var gapValue = GapToRem(_gap);
        writer.Write("<div style=\"display:flex;flex-direction:column;gap:");
        writer.Write(gapValue);
        writer.Write("\">");

        foreach (var row in _rows)
        {
            if (row.Count == 0) continue;
            writer.Write("<div style=\"display:grid;grid-template-columns:repeat(12,1fr);gap:");
            writer.Write(gapValue);
            writer.Write("\">");
            foreach (var cell in row)
            {
                cell.WriteTo(writer, encoder);
            }
            writer.Write("</div>");
        }

        writer.Write("</div>");
    }

    internal static string GapToRem(int gap) => gap switch
    {
        0 => "0", 1 => "0.25rem", 2 => "0.5rem", 3 => "0.75rem",
        4 => "1rem", 5 => "1.25rem", 6 => "1.5rem", 8 => "2rem",
        10 => "2.5rem", 12 => "3rem", 16 => "4rem",
        _ => "1rem"
    };

    public static readonly string Styles = GenerateStyles();

    private static string GenerateStyles()
    {
        var sb = new StringBuilder();
        AppendSpanRules(sb, "il-span", null);
        AppendSpanRules(sb, "il-sm-span", Breakpoints.Sm);
        AppendSpanRules(sb, "il-md-span", Breakpoints.Md);
        AppendSpanRules(sb, "il-lg-span", Breakpoints.Lg);
        return sb.ToString();
    }

    private static void AppendSpanRules(StringBuilder sb, string prefix, int? minWidth)
    {
        if (minWidth.HasValue)
            sb.Append($"@media(min-width:{minWidth}px){{");

        for (var i = 1; i <= 12; i++)
            sb.Append($".{prefix}-{i}{{grid-column:span {i}}}");

        if (minWidth.HasValue)
            sb.Append('}');
    }

    private static class Breakpoints
    {
        public const int Sm = 640;
        public const int Md = 768;
        public const int Lg = 1024;
    }
}

internal class GridCell(IHtmlContent content, int span, int? spanSm, int? spanMd, int? spanLg) : IHtmlContent
{
    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        var css = new StringBuilder($"il-span-{span}");
        if (spanSm.HasValue) css.Append($" il-sm-span-{spanSm.Value}");
        if (spanMd.HasValue) css.Append($" il-md-span-{spanMd.Value}");
        if (spanLg.HasValue) css.Append($" il-lg-span-{spanLg.Value}");

        writer.Write("<div class=\"");
        writer.Write(css);
        writer.Write("\">");
        content.WriteTo(writer, encoder);
        writer.Write("</div>");
    }
}
