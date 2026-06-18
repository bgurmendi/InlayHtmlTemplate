using System.Text.Encodings.Web;
using InlayHtmlTemplate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace InlayHtmlTemplate.Tests;

public class HtmlTemplateTests
{
    [Fact]
    public void Template_ImplementsIActionResult()
    {
        var template = Inlay.Template($"<p>test</p>");

        Assert.IsAssignableFrom<IActionResult>(template);
    }

    [Fact]
    public void Template_ImplementsIResult()
    {
        var template = Inlay.Template($"<p>test</p>");

        Assert.IsAssignableFrom<IResult>(template);
    }

    [Fact]
    public void Template_WriteTo_ProducesCorrectOutput()
    {
        var name = "Alice";
        var template = Inlay.Template($"<h1>{name}</h1>");

        using var writer = new StringWriter();
        template.WriteTo(writer, HtmlEncoder.Default);

        Assert.Equal("<h1>Alice</h1>", writer.ToString());
    }

    [Fact]
    public void Template_ToString_MatchesWriteTo()
    {
        var name = "Bob";
        var template = Inlay.Template($"<p>{name}</p>");

        using var writer = new StringWriter();
        template.WriteTo(writer, HtmlEncoder.Default);

        Assert.Equal(writer.ToString(), template.ToString());
    }

    [Fact]
    public void Template_NestedTemplates_ComposeWithoutIntermediateStrings()
    {
        var inner = Inlay.Template($"<span>inner</span>");
        var outer = Inlay.Template($"<div>{inner}</div>");

        Assert.Equal("<div><span>inner</span></div>", outer.ToString());
    }

    [Fact]
    public void Template_DeeplyNested_ComposesCorrectly()
    {
        var a = Inlay.Template($"<em>deep</em>");
        var b = Inlay.Template($"<span>{a}</span>");
        var c = Inlay.Template($"<div>{b}</div>");

        Assert.Equal("<div><span><em>deep</em></span></div>", c.ToString());
    }

    [Fact]
    public async Task ExecuteResultAsync_SetsContentTypeAndWritesBody()
    {
        var template = Inlay.Template($"<h1>Hello</h1>");

        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        await template.ExecuteResultAsync(actionContext);

        Assert.Equal("text/html; charset=utf-8", httpContext.Response.ContentType);

        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body);
        var body = await reader.ReadToEndAsync();
        Assert.Equal("<h1>Hello</h1>", body);
    }

    [Fact]
    public async Task ExecuteAsync_SetsContentTypeAndWritesBody()
    {
        var name = "World";
        var template = Inlay.Template($"<p>{name}</p>");

        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        await template.ExecuteAsync(httpContext);

        Assert.Equal("text/html; charset=utf-8", httpContext.Response.ContentType);

        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body);
        var body = await reader.ReadToEndAsync();
        Assert.Equal("<p>World</p>", body);
    }

    [Fact]
    public void Each_DeferredRendering_DoesNotEagerlyIterate()
    {
        var iterationCount = 0;
        IEnumerable<string> LazyItems()
        {
            iterationCount++;
            yield return "a";
            iterationCount++;
            yield return "b";
        }

        var result = Inlay.Each(LazyItems(), item => $"<li>{item}</li>");

        Assert.Equal(0, iterationCount);

        _ = result.ToString();

        Assert.Equal(2, iterationCount);
    }

    [Fact]
    public void SimpleHtmlTemplate_Render_StillWorks()
    {
        var name = "Compat";
        var result = SimpleHtmlTemplate.Render($"<p>{name}</p>");

        Assert.Equal("<p>Compat</p>", result);
    }
}
