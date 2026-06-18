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
        var result = Html.Template($"<div>{trusted}</div>").ToString();

        Assert.Equal("<div><strong>bold</strong></div>", result);
    }

    // --- Html.If ---

    [Fact]
    public void If_True_RendersContent()
    {
        var name = "Admin";
        var badge = Html.If(true, $"<span class=\"badge\">{name}</span>");
        var result = Html.Template($"<div>{badge}</div>").ToString();

        Assert.Contains("<span", result);
        Assert.Contains("Admin", result);
    }

    [Fact]
    public void If_False_RendersEmpty()
    {
        var badge = Html.If(false, $"<span>Admin</span>");
        var result = Html.Template($"<div>{badge}</div>").ToString();

        Assert.Equal("<div></div>", result);
    }

    [Fact]
    public void If_WithFallback_RendersFallbackWhenFalse()
    {
        var content = Html.If(false,
            $"<span>Logged in</span>",
            $"<a href=\"/login\">Log in</a>");
        var result = Html.Template($"<nav>{content}</nav>").ToString();

        Assert.Contains("Log in", result);
        Assert.DoesNotContain("Logged in", result);
    }

    [Fact]
    public void If_EscapesInterpolatedValues()
    {
        var userInput = "<script>xss</script>";
        var content = Html.If(true, $"<p>{userInput}</p>");
        var result = Html.Template($"<div>{content}</div>").ToString();

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
        var result = Html.Template($"<div class=\"{classes}\">content</div>").ToString();

        Assert.Contains("tab active", result);
        Assert.DoesNotContain("disabled", result);
    }

    // --- Html.Each ---

    [Fact]
    public void Each_RendersAllItems()
    {
        var items = new[] { "One", "Two", "Three" };
        var list = Html.Each(items, item => $"<li>{item}</li>");
        var result = Html.Template($"<ul>{list}</ul>").ToString();

        Assert.Contains("<li>One</li>", result);
        Assert.Contains("<li>Two</li>", result);
        Assert.Contains("<li>Three</li>", result);
    }

    [Fact]
    public void Each_EscapesEachItem()
    {
        var items = new[] { "Safe", "<script>xss</script>" };
        var list = Html.Each(items, item => $"<li>{item}</li>");
        var result = Html.Template($"<ul>{list}</ul>").ToString();

        Assert.Contains("<li>Safe</li>", result);
        Assert.DoesNotContain("<script>", result);
    }

    [Fact]
    public void Each_EmptyList_RendersNothing()
    {
        var items = Array.Empty<string>();
        var list = Html.Each(items, item => $"<li>{item}</li>");
        var result = Html.Template($"<ul>{list}</ul>").ToString();

        Assert.Equal("<ul></ul>", result);
    }

    [Fact]
    public void Each_WithFallback_ShowsFallbackWhenEmpty()
    {
        var items = Array.Empty<string>();
        var list = Html.Each(items,
            item => $"<li>{item}</li>",
            $"<li class=\"empty\">No items found.</li>");
        var result = Html.Template($"<ul>{list}</ul>").ToString();

        Assert.Contains("No items found.", result);
    }

    [Fact]
    public void Each_WithFallback_IgnoresFallbackWhenNotEmpty()
    {
        var items = new[] { "One" };
        var list = Html.Each(items,
            item => $"<li>{item}</li>",
            $"<li>No items</li>");
        var result = Html.Template($"<ul>{list}</ul>").ToString();

        Assert.Contains("<li>One</li>", result);
        Assert.DoesNotContain("No items", result);
    }

    [Fact]
    public void Each_WithIndex()
    {
        var items = new[] { "A", "B" };
        var list = Html.Each(items, (item, i) => $"<li data-index=\"{i}\">{item}</li>");
        var result = Html.Template($"<ul>{list}</ul>").ToString();

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

        var result = Html.Template($"<div>{badge}<ul>{list}</ul></div>").ToString();

        Assert.Contains("<span", result);
        Assert.Contains("Alice", result);
        Assert.Contains("Bob", result);
    }

    [Fact]
    public void Composition_TemplateInsideTemplate()
    {
        var name = "Alice";
        var header = Html.Template($"<h1>{name}</h1>");
        var page = Html.Template($"<div>{header}</div>").ToString();

        Assert.Equal("<div><h1>Alice</h1></div>", page);
    }
}
