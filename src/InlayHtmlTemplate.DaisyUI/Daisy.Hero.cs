using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.DaisyUI;

public static partial class Daisy
{
    public static IHtmlContent Hero(
        string title,
        string? subtitle = null,
        IHtmlContent? actions = null,
        string? backgroundImageUrl = null)
    {
        if (backgroundImageUrl is not null)
        {
            return Inlay.Template($"""
                <div class="hero min-h-96" style="background-image: url({backgroundImageUrl});">
                    <div class="hero-overlay bg-opacity-60"></div>
                    <div class="hero-content text-neutral-content text-center">
                        <div class="max-w-md">
                            <h1 class="mb-5 text-5xl font-bold">{title}</h1>
                            {Inlay.If(subtitle is not null, $"""<p class="mb-5">{subtitle}</p>""")}
                            {actions ?? HtmlString.Empty}
                        </div>
                    </div>
                </div>
                """);
        }

        return Inlay.Template($"""
            <div class="hero min-h-96 bg-base-200">
                <div class="hero-content text-center">
                    <div class="max-w-md">
                        <h1 class="text-5xl font-bold">{title}</h1>
                        {Inlay.If(subtitle is not null, $"""<p class="py-6">{subtitle}</p>""")}
                        {actions ?? HtmlString.Empty}
                    </div>
                </div>
            </div>
            """);
    }
}
