using AspNetTemplates;
using Xunit;

namespace AspNetTemplates.Tests;

public class SimpleHtmlTemplateTests
{
    [Fact]
    public void Render_PlainText_NoEscaping()
    {
        var title = "Title";
        var result = SimpleHtmlTemplate.Render($"<h1>{title}</h1>");

        Assert.Equal("<h1>Title</h1>", result);
    }

    [Fact]
    public void Render_HtmlContent_Escapes()
    {
        var userInput = "<script>alert('xss')</script>";
        var result = SimpleHtmlTemplate.Render($"<div>{userInput}</div>");

        Assert.DoesNotContain("<script>", result);
    }

    [Fact]
    public void Render_AttributeContext_Escapes()
    {
        var className = "foo\" onclick=\"alert(1)";
        var result = SimpleHtmlTemplate.Render($"<div class=\"{className}\">test</div>");

        Assert.DoesNotContain("\" onclick=\"", result);
        Assert.Contains("&quot;", result);
    }

    [Fact]
    public void Render_UrlAttribute_BlocksJavascript()
    {
        var url = "javascript:alert(1)";
        var result = SimpleHtmlTemplate.Render($"<a href=\"{url}\">link</a>");

        Assert.DoesNotContain("javascript", result);
    }

    [Fact]
    public void Render_NullArgument_ProducesEmptyString()
    {
        string? value = null;
        var result = SimpleHtmlTemplate.Render($"<span>{value}</span>");

        Assert.Equal("<span></span>", result);
    }

    [Fact]
    public void Render_MultipleArguments()
    {
        var title = "Hello";
        var body = "World";
        var result = SimpleHtmlTemplate.Render($"<h1>{title}</h1><p>{body}</p>");

        Assert.Equal("<h1>Hello</h1><p>World</p>", result);
    }
}
