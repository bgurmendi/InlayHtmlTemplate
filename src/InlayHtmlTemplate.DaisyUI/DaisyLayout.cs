using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.DaisyUI;

public static class DaisyLayout
{
    public static InlayTemplate Render(
        string title,
        IHtmlContent body,
        string theme = "light",
        IHtmlContent? headExtra = null)
    {
        return Inlay.Template($"""
            <!DOCTYPE html>
            <html lang="en" data-theme="{theme}">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>{title}</title>
                <link href="https://cdn.jsdelivr.net/npm/daisyui@4/dist/full.min.css" rel="stylesheet" type="text/css" />
                <script src="https://cdn.tailwindcss.com"></script>
                {headExtra ?? HtmlString.Empty}
            </head>
            <body>
                {body}
            </body>
            </html>
            """);
    }
}
