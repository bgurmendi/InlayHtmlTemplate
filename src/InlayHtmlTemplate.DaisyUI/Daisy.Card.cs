using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.DaisyUI;

public static partial class Daisy
{
    public static IHtmlContent Card(
        string? title = null,
        IHtmlContent? body = null,
        string? imageUrl = null,
        string? imageAlt = null,
        IHtmlContent? actions = null,
        bool compact = false,
        bool bordered = false,
        bool glass = false)
    {
        var css = Inlay.Css(
            ("card", true),
            ("bg-base-100", !glass),
            ("glass", glass),
            ("shadow-xl", true),
            ("card-compact", compact),
            ("card-bordered", bordered));

        return Inlay.Template($"""
            <div class="{css}">
                {Inlay.If(imageUrl is not null, $"""
                    <figure><img src="{imageUrl ?? ""}" alt="{imageAlt ?? title ?? ""}" /></figure>
                    """)}
                <div class="card-body">
                    {Inlay.If(title is not null, $"""<h2 class="card-title">{title}</h2>""")}
                    {body ?? HtmlString.Empty}
                    {Inlay.If(actions is not null, $"""<div class="card-actions justify-end">{actions ?? HtmlString.Empty}</div>""")}
                </div>
            </div>
            """);
    }
}
