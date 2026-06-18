using AspNetTemplates;
using Xunit;

namespace AspNetTemplates.Tests;

public class HtmlHelpersTests
{
    // --- Html.Raw ---

    [Fact]
    public void Raw_BypassesEscaping()
    {
        var trusted = Html.Raw("<strong>bold</strong>");
        var result = SimpleHtmlTemplate.Render($"<div>{trusted}</div>");

        Assert.Equal("<div><strong>bold</strong></div>", result);
    }

    // --- Html.If ---

    [Fact]
    public void If_True_RendersContent()
    {
        var name = "Admin";
        var badge = Html.If(true, $"<span class=\"badge\">{name}</span>");
        var result = SimpleHtmlTemplate.Render($"<div>{badge}</div>");

        Assert.Contains("<span", result);
        Assert.Contains("Admin", result);
    }

    [Fact]
    public void If_False_RendersEmpty()
    {
        var badge = Html.If(false, $"<span>Admin</span>");
        var result = SimpleHtmlTemplate.Render($"<div>{badge}</div>");

        Assert.Equal("<div></div>", result);
    }

    [Fact]
    public void If_WithFallback_RendersFallbackWhenFalse()
    {
        var content = Html.If(false,
            $"<span>Logged in</span>",
            $"<a href=\"/login\">Log in</a>");
        var result = SimpleHtmlTemplate.Render($"<nav>{content}</nav>");

        Assert.Contains("Log in", result);
        Assert.DoesNotContain("Logged in", result);
    }

    [Fact]
    public void If_EscapesInterpolatedValues()
    {
        var userInput = "<script>xss</script>";
        var content = Html.If(true, $"<p>{userInput}</p>");
        var result = SimpleHtmlTemplate.Render($"<div>{content}</div>");

        Assert.DoesNotContain("<script>", result);
    }

    // --- Html.Css ---

    [Fact]
    public void Css_ActiveClassesOnly()
    {
        var classes = Html.Css(
            ("btn", true),
            ("btn-primary", true),
            ("btn-lg", false));

        Assert.Equal("btn btn-primary", classes);
    }

    [Fact]
    public void Css_NoneActive_ReturnsEmpty()
    {
        var classes = Html.Css(
            ("hidden", false),
            ("disabled", false));

        Assert.Equal("", classes);
    }

    [Fact]
    public void Css_WorksInsideTemplate()
    {
        var isActive = true;
        var isDisabled = false;
        var classes = Html.Css(("tab", true), ("active", isActive), ("disabled", isDisabled));
        var result = SimpleHtmlTemplate.Render($"<div class=\"{classes}\">content</div>");

        Assert.Contains("tab active", result);
        Assert.DoesNotContain("disabled", result);
    }

    // --- Html.Each ---

    [Fact]
    public void Each_RendersAllItems()
    {
        var items = new[] { "One", "Two", "Three" };
        var list = Html.Each(items, item => $"<li>{item}</li>");
        var result = SimpleHtmlTemplate.Render($"<ul>{list}</ul>");

        Assert.Contains("<li>One</li>", result);
        Assert.Contains("<li>Two</li>", result);
        Assert.Contains("<li>Three</li>", result);
    }

    [Fact]
    public void Each_EscapesEachItem()
    {
        var items = new[] { "Safe", "<script>xss</script>" };
        var list = Html.Each(items, item => $"<li>{item}</li>");
        var result = SimpleHtmlTemplate.Render($"<ul>{list}</ul>");

        Assert.Contains("<li>Safe</li>", result);
        Assert.DoesNotContain("<script>", result);
    }

    [Fact]
    public void Each_EmptyList_RendersNothing()
    {
        var items = Array.Empty<string>();
        var list = Html.Each(items, item => $"<li>{item}</li>");
        var result = SimpleHtmlTemplate.Render($"<ul>{list}</ul>");

        Assert.Equal("<ul></ul>", result);
    }

    [Fact]
    public void Each_WithFallback_ShowsFallbackWhenEmpty()
    {
        var items = Array.Empty<string>();
        var list = Html.Each(items,
            item => $"<li>{item}</li>",
            $"<li class=\"empty\">No items found.</li>");
        var result = SimpleHtmlTemplate.Render($"<ul>{list}</ul>");

        Assert.Contains("No items found.", result);
    }

    [Fact]
    public void Each_WithFallback_IgnoresFallbackWhenNotEmpty()
    {
        var items = new[] { "One" };
        var list = Html.Each(items,
            item => $"<li>{item}</li>",
            $"<li>No items</li>");
        var result = SimpleHtmlTemplate.Render($"<ul>{list}</ul>");

        Assert.Contains("<li>One</li>", result);
        Assert.DoesNotContain("No items", result);
    }

    [Fact]
    public void Each_WithIndex()
    {
        var items = new[] { "A", "B" };
        var list = Html.Each(items, (item, i) => $"<li data-index=\"{i}\">{item}</li>");
        var result = SimpleHtmlTemplate.Render($"<ul>{list}</ul>");

        Assert.Contains("data-index=\"0\"", result);
        Assert.Contains("data-index=\"1\"", result);
    }

    // --- Composition ---

    [Fact]
    public void Composition_NestedHelpers()
    {
        var isAdmin = true;
        var users = new[] { "Alice", "Bob" };

        var badge = Html.If(isAdmin, $"<span class=\"badge\">Admin</span>");
        var list = Html.Each(users, u => $"<li>{u}</li>");

        var result = SimpleHtmlTemplate.Render($"<div>{badge}<ul>{list}</ul></div>");

        Assert.Contains("<span", result);
        Assert.Contains("Alice", result);
        Assert.Contains("Bob", result);
    }
}
