using System.Text.Encodings.Web;
using InlayHtmlTemplate;
using InlayHtmlTemplate.DaisyUI;
using Microsoft.AspNetCore.Html;
using Xunit;

namespace InlayHtmlTemplate.DaisyUI.Tests;

public class DaisyLayoutTests
{
    static string Render(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    [Fact]
    public void DaisyLayout_renders_full_html_with_theme()
    {
        var html = Render(DaisyLayout.Render("Test Page", Inlay.Template($"<p>Hello</p>"), theme: "dracula"));
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("data-theme=\"dracula\"", html);
        Assert.Contains("<title>Test Page</title>", html);
        Assert.Contains("<p>Hello</p>", html);
        Assert.Contains("daisyui", html);
        Assert.Contains("tailwindcss", html);
    }

    [Fact]
    public void DaisyLayout_includes_head_extra()
    {
        var extra = Inlay.Template($"""<link rel="icon" href="/favicon.ico" />""");
        var html = Render(DaisyLayout.Render("T", Inlay.Template($"<p>x</p>"), headExtra: extra));
        Assert.Contains("favicon.ico", html);
    }

    [Fact]
    public void SidebarLayout_renders_drawer_structure()
    {
        var sidebar = Inlay.Template($"<nav>sidebar</nav>");
        var body = Inlay.Template($"<p>main content</p>");
        var html = Render(DaisySidebarLayout.Render("Test", sidebar, body));
        Assert.Contains("drawer lg:drawer-open", html);
        Assert.Contains("drawer-toggle", html);
        Assert.Contains("drawer-content", html);
        Assert.Contains("drawer-side", html);
        Assert.Contains("<nav>sidebar</nav>", html);
        Assert.Contains("<p>main content</p>", html);
    }

    [Fact]
    public void SidebarLayout_includes_navbar_and_footer()
    {
        var sidebar = Inlay.Template($"<nav>s</nav>");
        var body = Inlay.Template($"<p>b</p>");
        var navbar = Daisy.Navbar(start: Inlay.Template($"<span>Brand</span>"));
        var footer = Daisy.FooterSimple("foot");

        var html = Render(DaisySidebarLayout.Render("T", sidebar, body, navbar: navbar, footer: footer));
        Assert.Contains("Brand", html);
        Assert.Contains("foot", html);
    }

    [Fact]
    public void SidebarLayout_custom_drawer_id()
    {
        var html = Render(DaisySidebarLayout.Render(
            "T",
            Inlay.Template($"<nav>s</nav>"),
            Inlay.Template($"<p>b</p>"),
            drawerId: "my-drawer"));
        Assert.Contains("id=\"my-drawer\"", html);
        Assert.Contains("for=\"my-drawer\"", html);
    }
}
