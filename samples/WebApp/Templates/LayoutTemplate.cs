using InlayHtmlTemplate;
using Microsoft.AspNetCore.Html;

namespace WebApp.Templates;

public static class LayoutTemplate
{
    public static InlayTemplate Render(string title, IHtmlContent body, string? activeNav = null)
    {
        return Inlay.Template($"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>{title} - InlayHtmlTemplate Demo</title>
                <link rel="stylesheet" href="/css/site.css" />
            </head>
            <body>
                <header>
                    <nav>
                        <a class="brand" href="/">InlayHtmlTemplate</a>
                        <ul>
                            {NavLink("Home", "/", activeNav)}
                            {NavLink("Privacy", "/Home/Privacy", activeNav)}
                        </ul>
                    </nav>
                </header>
                <main>
                    {body}
                </main>
                <footer>
                    <p>&copy; 2026 - InlayHtmlTemplate Demo</p>
                </footer>
            </body>
            </html>
            """);
    }

    static IHtmlContent NavLink(string label, string href, string? activeNav) =>
        Inlay.Template($"""
            <li>
                <a class="{Inlay.Css(("nav-link", true), ("active", label == activeNav))}" href="{href}">{label}</a>
            </li>
            """);
}
