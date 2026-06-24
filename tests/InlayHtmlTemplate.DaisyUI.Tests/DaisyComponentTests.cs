using System.Text.Encodings.Web;
using InlayHtmlTemplate;
using InlayHtmlTemplate.DaisyUI;
using Microsoft.AspNetCore.Html;
using Xunit;

namespace InlayHtmlTemplate.DaisyUI.Tests;

public class DaisyComponentTests
{
    static string Render(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    [Fact]
    public void Alert_renders_with_variant_class()
    {
        var html = Render(Daisy.Alert("test message", AlertVariant.Success));
        Assert.Contains("alert-success", html);
        Assert.Contains("test message", html);
    }

    [Fact]
    public void Badge_renders_with_variant_class()
    {
        var html = Render(Daisy.Badge("v1.0", BadgeVariant.Primary));
        Assert.Contains("badge badge-primary", html);
        Assert.Contains("v1.0", html);
    }

    [Fact]
    public void Button_renders_with_variant_and_size()
    {
        var html = Render(Daisy.Button("Click", ButtonVariant.Primary, ButtonSize.Lg));
        Assert.Contains("btn btn-primary btn-lg", html);
        Assert.Contains("Click", html);
    }

    [Fact]
    public void Button_outline_adds_class()
    {
        var html = Render(Daisy.Button("OK", outline: true));
        Assert.Contains("btn-outline", html);
    }

    [Fact]
    public void Button_href_renders_anchor_with_unencoded_url()
    {
        var html = Render(Daisy.Button("Go", href: "/some/path"));
        Assert.Contains("<a ", html);
        Assert.Contains("href=\"/some/path\"", html);
        Assert.DoesNotContain("%2F", html);
    }

    [Fact]
    public void Card_renders_title_and_body()
    {
        var body = Inlay.Template($"<p>content</p>");
        var html = Render(Daisy.Card(title: "My Card", body: body));
        Assert.Contains("card-title", html);
        Assert.Contains("My Card", html);
        Assert.Contains("<p>content</p>", html);
    }

    [Fact]
    public void Card_image_renders_figure()
    {
        var html = Render(Daisy.Card(imageUrl: "https://example.com/img.png"));
        Assert.Contains("<figure>", html);
        Assert.Contains("img.png", html);
    }

    [Fact]
    public void Hero_renders_title_and_subtitle()
    {
        var html = Render(Daisy.Hero("Title", "Subtitle"));
        Assert.Contains("hero", html);
        Assert.Contains("Title", html);
        Assert.Contains("Subtitle", html);
    }

    [Fact]
    public void Modal_renders_dialog_with_id()
    {
        var html = Render(Daisy.Modal("my_modal", title: "Hello"));
        Assert.Contains("<dialog id=\"my_modal\"", html);
        Assert.Contains("Hello", html);
    }

    [Fact]
    public void ModalTrigger_references_modal_id()
    {
        var html = Render(Daisy.ModalTrigger("my_modal", "Open"));
        Assert.Contains("my_modal.showModal()", html);
        Assert.Contains("Open", html);
    }

    [Fact]
    public void Navbar_renders_sections()
    {
        var html = Render(Daisy.Navbar(
            start: Inlay.Template($"<span>Start</span>"),
            end: Inlay.Template($"<span>End</span>")));
        Assert.Contains("navbar-start", html);
        Assert.Contains("navbar-end", html);
        Assert.DoesNotContain("navbar-center", html);
    }

    [Fact]
    public void NavLink_href_not_url_encoded()
    {
        var html = Render(Daisy.NavLink("Home", "/path/to/page"));
        Assert.Contains("href=\"/path/to/page\"", html);
        Assert.DoesNotContain("%2F", html);
    }

    [Fact]
    public void NavBrand_href_not_url_encoded()
    {
        var html = Render(Daisy.NavBrand("Brand", "/app/home"));
        Assert.Contains("href=\"/app/home\"", html);
        Assert.DoesNotContain("%2F", html);
    }

    [Fact]
    public void Stats_renders_items()
    {
        var items = new[] { new StatItem("Users", "42", "active") };
        var html = Render(Daisy.Stats(items));
        Assert.Contains("stat-title", html);
        Assert.Contains("Users", html);
        Assert.Contains("42", html);
        Assert.Contains("active", html);
    }

    [Fact]
    public void Stats_vertical_adds_class()
    {
        var html = Render(Daisy.Stats([new StatItem("X", "1")], vertical: true));
        Assert.Contains("stats-vertical", html);
    }

    [Fact]
    public void Table_renders_headers_and_rows()
    {
        var data = new[] { ("Alice", 30), ("Bob", 25) };
        var html = Render(Daisy.Table(
            ["Name", "Age"],
            data,
            d => $"<tr><td>{d.Item1}</td><td>{d.Item2}</td></tr>"));
        Assert.Contains("<th>Name</th>", html);
        Assert.Contains("<th>Age</th>", html);
        Assert.Contains("<td>Alice</td>", html);
        Assert.Contains("<td>Bob</td>", html);
    }

    [Fact]
    public void Table_zebra_adds_class()
    {
        var html = Render(Daisy.Table(["H"], Array.Empty<int>(), i => $"<tr><td>{i}</td></tr>", zebra: true));
        Assert.Contains("table-zebra", html);
    }

    [Fact]
    public void Toast_renders_with_position()
    {
        var html = Render(Daisy.Toast(Inlay.Template($"<div>msg</div>"), ToastPosition.TopEnd));
        Assert.Contains("toast-top", html);
        Assert.Contains("toast-end", html);
    }

    [Fact]
    public void Footer_renders_sections_with_unencoded_hrefs()
    {
        var html = Render(Daisy.Footer(
            new FooterSection("Docs", [new FooterLink("Guide", "/docs/guide")])));
        Assert.Contains("Docs", html);
        Assert.Contains("Guide", html);
        Assert.Contains("href=\"/docs/guide\"", html);
        Assert.DoesNotContain("%2F", html);
    }

    [Fact]
    public void FooterSimple_renders_text()
    {
        var html = Render(Daisy.FooterSimple("© 2026"));
        Assert.Contains("2026", html);
        Assert.Contains("footer-center", html);
    }
}
