using InlayHtmlTemplate;
using Xunit;

namespace InlayHtmlTemplate.Tests;

public class SimpleHtmlTemplateTests
{
    [Fact]
    public void Render_PlainText_NoEscaping()
    {
        var title = "Title";
        var result = Inlay.Template($"<h1>{title}</h1>").ToString();

        Assert.Equal("<h1>Title</h1>", result);
    }

    [Fact]
    public void Render_HtmlContent_Escapes()
    {
        var userInput = "<script>alert('xss')</script>";
        var result = Inlay.Template($"<div>{userInput}</div>").ToString();

        Assert.DoesNotContain("<script>", result);
    }

    [Fact]
    public void Render_AttributeContext_Escapes()
    {
        var className = """foo" onclick="alert(1)""";
        var result = Inlay.Template($"""<div class="{className}">test</div>""").ToString();

        Assert.DoesNotContain("\" onclick=\"", result);
        Assert.Contains("&quot;", result);
    }

    [Fact]
    public void Render_UrlAttribute_BlocksJavascript()
    {
        var url = "javascript:alert(1)";
        var result = Inlay.Template($"""<a href="{url}">link</a>""").ToString();

        Assert.DoesNotContain("javascript", result);
    }

    [Fact]
    public void Render_NullArgument_ProducesEmptyString()
    {
        string? value = null;
        var result = Inlay.Template($"<span>{value}</span>").ToString();

        Assert.Equal("<span></span>", result);
    }

    [Fact]
    public void Render_MultipleArguments()
    {
        var title = "Hello";
        var body = "World";
        var result = Inlay.Template($"<h1>{title}</h1><p>{body}</p>").ToString();

        Assert.Equal("<h1>Hello</h1><p>World</p>", result);
    }
}
