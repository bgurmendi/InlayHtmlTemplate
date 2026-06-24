using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.DaisyUI;

public static partial class Daisy
{
    public static IHtmlContent Stats(IEnumerable<StatItem> items, bool vertical = false)
    {
        var css = Inlay.Css(("stats", true), ("shadow", true), ("stats-vertical", vertical));
        return Inlay.Template($"""
            <div class="{css}">
                {Inlay.Each(items, item => $"""
                    <div class="stat">
                        <div class="stat-title">{item.Title}</div>
                        <div class="stat-value">{item.Value}</div>
                        {Inlay.If(item.Description is not null, $"""<div class="stat-desc">{item.Description}</div>""")}
                    </div>
                    """)}
            </div>
            """);
    }

    public static IHtmlContent Stat(string title, string value, string? description = null) =>
        Inlay.Template($"""
            <div class="stat">
                <div class="stat-title">{title}</div>
                <div class="stat-value">{value}</div>
                {Inlay.If(description is not null, $"""<div class="stat-desc">{description}</div>""")}
            </div>
            """);
}
