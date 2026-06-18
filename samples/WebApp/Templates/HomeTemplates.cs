using AspNetTemplates;
using Microsoft.AspNetCore.Html;
using WebApp.Models;

namespace WebApp.Templates;

public static class HomeTemplates
{
    public static HtmlTemplate Index(IEnumerable<Feature> features)
    {
        return Html.Template($"""
            <section class="hero">
                <h1>AspNetTemplates</h1>
                <p>Context-aware HTML templates using C# string interpolation.</p>
                <p class="subtitle">This page is rendered entirely with <code>Html.Template</code> — no Razor, no .cshtml files.</p>
            </section>

            <section>
                <h2>Features</h2>
                <div class="feature-grid">
                    {Html.Each(features,
                        f => $"""
                            <div class="{Html.Css(("feature", true), ("highlight", f.IsNew))}">
                                <h3>{f.Title}</h3>
                                <p>{f.Description}</p>
                                {Html.If(f.IsNew, $"""<span class="badge">New</span>""")}
                            </div>
                            """,
                        $"<p>No features to display.</p>")}
                </div>
            </section>

            <section>
                <h2>XSS Protection Demo</h2>
                <p>Try injecting HTML — it's automatically escaped based on context:</p>
                {EscapingDemo()}
            </section>
            """);
    }

    static HtmlTemplate EscapingDemo()
    {
        var userInput = """<img src="x" onerror="alert('xss')">""";
        var maliciousClass = """foo" onclick="alert(1)""";
        var maliciousUrl = "javascript:alert(1)";

        return Html.Template($"""
            <table class="demo-table">
                <thead>
                    <tr>
                        <th>Context</th>
                        <th>Input</th>
                        <th>Rendered (view source)</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>Content</td>
                        <td><code>{userInput}</code></td>
                        <td>{userInput}</td>
                    </tr>
                    <tr>
                        <td>Attribute</td>
                        <td><code>{maliciousClass}</code></td>
                        <td><span class="{maliciousClass}">safe</span></td>
                    </tr>
                    <tr>
                        <td>URL</td>
                        <td><code>{maliciousUrl}</code></td>
                        <td><a href="{maliciousUrl}">blocked link</a></td>
                    </tr>
                </tbody>
            </table>
            """);
    }

    public static HtmlTemplate Privacy() =>
        Html.Template($"""
            <h1>Privacy Policy</h1>
            <p>This is a demo application for the AspNetTemplates library.</p>
            <p>No data is collected or stored.</p>
            """);
}
