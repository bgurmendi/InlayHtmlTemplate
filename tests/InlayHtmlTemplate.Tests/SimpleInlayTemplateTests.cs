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

    // --- Context analyzer coverage ---

    [Fact]
    public void Render_NoArguments_LiteralOnly()
    {
        var result = Inlay.Template($"<p>static</p>").ToString();

        Assert.Equal("<p>static</p>", result);
    }

    [Fact]
    public void Render_AdjacentArguments()
    {
        var a = "X";
        var b = "Y";
        var result = Inlay.Template($"{a}{b}").ToString();

        Assert.Equal("XY", result);
    }

    [Fact]
    public void Render_TagOpenClose_ResetsContext()
    {
        var inside = "<b>bold</b>";
        var after = "<i>italic</i>";
        var result = Inlay.Template($"<div>{inside}</div>{after}").ToString();

        Assert.DoesNotContain("<b>", result);
        Assert.DoesNotContain("<i>", result);
    }

    [Fact]
    public void Render_SrcAttribute_UrlEncoded()
    {
        var url = "https://example.com/img?a=1&b=2";
        var result = Inlay.Template($"""<img src="{url}" />""").ToString();

        Assert.DoesNotContain("&b=", result);
        Assert.Contains("src=", result);
    }

    [Fact]
    public void Render_RegularAttribute_NotUrlEncoded()
    {
        var title = "Hello & World";
        var result = Inlay.Template($"""<div title="{title}">text</div>""").ToString();

        Assert.Contains("&amp;", result);
        Assert.DoesNotContain("Hello & World", result);
    }

    [Fact]
    public void Render_MultipleAttributes()
    {
        var cls = "main";
        var id = "test";
        var result = Inlay.Template($"""<div class="{cls}" id="{id}">x</div>""").ToString();

        Assert.Contains("class=", result);
        Assert.Contains("id=", result);
        Assert.Contains("main", result);
        Assert.Contains("test", result);
    }

    [Fact]
    public void Render_UrlAttribute_SafeUrl_Encoded()
    {
        var url = "/search?q=hello world";
        var result = Inlay.Template($"""<a href="{url}">link</a>""").ToString();

        Assert.Contains("%20", result);
        Assert.Contains("%2F", result);
    }

    [Fact]
    public void Render_SingleQuoteAttribute()
    {
        var value = "it's tricky";
        var result = Inlay.Template($"<div title=\"{value}\">x</div>").ToString();

        Assert.DoesNotContain("'", result);
    }

    [Fact]
    public void Render_WhitespaceInTag_ResetsAttributeName()
    {
        var cls = "test";
        var result = Inlay.Template($"""<div  class="{cls}">x</div>""").ToString();

        Assert.Contains("test", result);
    }

    [Fact]
    public void Render_ContentAfterTag_IsContentContext()
    {
        var text = "<script>xss</script>";
        var result = Inlay.Template($"<p>before</p>{text}<p>after</p>").ToString();

        Assert.DoesNotContain("<script>", result);
        Assert.Contains("before", result);
        Assert.Contains("after", result);
    }
}
