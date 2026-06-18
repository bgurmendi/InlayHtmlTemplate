using InlayHtmlTemplate;
using Xunit;

namespace InlayHtmlTemplate.Tests;

public class HtmlHelpersTests
{
    // --- Inlay.Raw ---

    [Fact]
    public void Raw_BypassesEscaping()
    {
        var trusted = Inlay.Raw("<strong>bold</strong>");
        var result = Inlay.Template($"<div>{trusted}</div>").ToString();

        Assert.Equal("<div><strong>bold</strong></div>", result);
    }

    // --- Inlay.If ---

    [Fact]
    public void If_True_RendersContent()
    {
        var name = "Admin";
        var badge = Inlay.If(true, $"""<span class="badge">{name}</span>""");
        var result = Inlay.Template($"<div>{badge}</div>").ToString();

        Assert.Contains("<span", result);
        Assert.Contains("Admin", result);
    }

    [Fact]
    public void If_False_RendersEmpty()
    {
        var badge = Inlay.If(false, $"<span>Admin</span>");
        var result = Inlay.Template($"<div>{badge}</div>").ToString();

        Assert.Equal("<div></div>", result);
    }

    [Fact]
    public void If_WithFallback_RendersFallbackWhenFalse()
    {
        var content = Inlay.If(false,
            $"<span>Logged in</span>",
            $"""<a href="/login">Log in</a>""");
        var result = Inlay.Template($"<nav>{content}</nav>").ToString();

        Assert.Contains("Log in", result);
        Assert.DoesNotContain("Logged in", result);
    }

    [Fact]
    public void If_EscapesInterpolatedValues()
    {
        var userInput = "<script>xss</script>";
        var content = Inlay.If(true, $"<p>{userInput}</p>");
        var result = Inlay.Template($"<div>{content}</div>").ToString();

        Assert.DoesNotContain("<script>", result);
    }

    // --- Inlay.Css ---

    [Fact]
    public void Css_ActiveClassesOnly()
    {
        var classes = Inlay.Css(
            ("btn", true),
            ("btn-primary", true),
            ("btn-lg", false));

        Assert.Equal("btn btn-primary", classes);
    }

    [Fact]
    public void Css_NoneActive_ReturnsEmpty()
    {
        var classes = Inlay.Css(
            ("hidden", false),
            ("disabled", false));

        Assert.Equal("", classes);
    }

    [Fact]
    public void Css_WorksInsideTemplate()
    {
        var isActive = true;
        var isDisabled = false;
        var classes = Inlay.Css(("tab", true), ("active", isActive), ("disabled", isDisabled));
        var result = Inlay.Template($"""<div class="{classes}">content</div>""").ToString();

        Assert.Contains("tab active", result);
        Assert.DoesNotContain("disabled", result);
    }

    // --- Inlay.Each ---

    [Fact]
    public void Each_RendersAllItems()
    {
        var items = new[] { "One", "Two", "Three" };
        var list = Inlay.Each(items, item => $"<li>{item}</li>");
        var result = Inlay.Template($"<ul>{list}</ul>").ToString();

        Assert.Contains("<li>One</li>", result);
        Assert.Contains("<li>Two</li>", result);
        Assert.Contains("<li>Three</li>", result);
    }

    [Fact]
    public void Each_EscapesEachItem()
    {
        var items = new[] { "Safe", "<script>xss</script>" };
        var list = Inlay.Each(items, item => $"<li>{item}</li>");
        var result = Inlay.Template($"<ul>{list}</ul>").ToString();

        Assert.Contains("<li>Safe</li>", result);
        Assert.DoesNotContain("<script>", result);
    }

    [Fact]
    public void Each_EmptyList_RendersNothing()
    {
        var items = Array.Empty<string>();
        var list = Inlay.Each(items, item => $"<li>{item}</li>");
        var result = Inlay.Template($"<ul>{list}</ul>").ToString();

        Assert.Equal("<ul></ul>", result);
    }

    [Fact]
    public void Each_WithFallback_ShowsFallbackWhenEmpty()
    {
        var items = Array.Empty<string>();
        var list = Inlay.Each(items,
            item => $"<li>{item}</li>",
            $"""<li class="empty">No items found.</li>""");
        var result = Inlay.Template($"<ul>{list}</ul>").ToString();

        Assert.Contains("No items found.", result);
    }

    [Fact]
    public void Each_WithFallback_IgnoresFallbackWhenNotEmpty()
    {
        var items = new[] { "One" };
        var list = Inlay.Each(items,
            item => $"<li>{item}</li>",
            $"<li>No items</li>");
        var result = Inlay.Template($"<ul>{list}</ul>").ToString();

        Assert.Contains("<li>One</li>", result);
        Assert.DoesNotContain("No items", result);
    }

    [Fact]
    public void Each_WithIndex()
    {
        var items = new[] { "A", "B" };
        var list = Inlay.Each(items, (item, i) => $"""<li data-index="{i}">{item}</li>""");
        var result = Inlay.Template($"<ul>{list}</ul>").ToString();

        Assert.Contains("""data-index="0""", result);
        Assert.Contains("""data-index="1""", result);
    }

    // --- Composition ---

    [Fact]
    public void Composition_NestedHelpers()
    {
        var isAdmin = true;
        var users = new[] { "Alice", "Bob" };

        var badge = Inlay.If(isAdmin, $"""<span class="badge">Admin</span>""");
        var list = Inlay.Each(users, u => $"<li>{u}</li>");

        var result = Inlay.Template($"<div>{badge}<ul>{list}</ul></div>").ToString();

        Assert.Contains("<span", result);
        Assert.Contains("Alice", result);
        Assert.Contains("Bob", result);
    }

    [Fact]
    public void Composition_TemplateInsideTemplate()
    {
        var name = "Alice";
        var header = Inlay.Template($"<h1>{name}</h1>");
        var page = Inlay.Template($"<div>{header}</div>").ToString();

        Assert.Equal("<div><h1>Alice</h1></div>", page);
    }
}
