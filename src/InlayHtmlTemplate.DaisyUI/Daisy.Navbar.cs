using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.DaisyUI;

public static partial class Daisy
{
    public static IHtmlContent Navbar(
        IHtmlContent? start = null,
        IHtmlContent? center = null,
        IHtmlContent? end = null) =>
        Inlay.Template($"""
            <div class="navbar bg-base-100">
                {Inlay.If(start is not null, $"""<div class="navbar-start">{start ?? HtmlString.Empty}</div>""")}
                {Inlay.If(center is not null, $"""<div class="navbar-center">{center ?? HtmlString.Empty}</div>""")}
                {Inlay.If(end is not null, $"""<div class="navbar-end">{end ?? HtmlString.Empty}</div>""")}
            </div>
            """);

    public static IHtmlContent NavLink(string label, string href, bool isActive = false) =>
        Inlay.Template($"""<a class="btn btn-ghost {Inlay.Css(("btn-active", isActive))}" href="{Inlay.Raw(href)}">{label}</a>""");

    public static IHtmlContent NavBrand(string label, string href = "/") =>
        Inlay.Template($"""<a class="btn btn-ghost text-xl" href="{Inlay.Raw(href)}">{label}</a>""");
}
