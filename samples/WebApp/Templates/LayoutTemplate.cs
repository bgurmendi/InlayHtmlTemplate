using AspNetTemplates;
using Microsoft.AspNetCore.Html;

namespace WebApp.Templates;

public static class LayoutTemplate
{
    public static IHtmlContent Render(string title, IHtmlContent body, string? activeNav = null)
    {
        return Html.Template($"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>{title} - AspNetTemplates Demo</title>
                <link rel="stylesheet" href="/css/site.css" />
            </head>
            <body>
                <header>
                    <nav>
                        <a class="brand" href="/">AspNetTemplates</a>
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
                    <p>&copy; 2026 - AspNetTemplates Demo</p>
                </footer>
            </body>
            </html>
            """);
    }

    static IHtmlContent NavLink(string label, string href, string? activeNav) =>
        Html.Template($"""
            <li>
                <a class="{Html.Css(("nav-link", true), ("active", label == activeNav))}" href="{href}">{label}</a>
            </li>
            """);
}
