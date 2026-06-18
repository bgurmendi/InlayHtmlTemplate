using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InlayHtmlTemplate;

/// <summary>
/// A deferred HTML template that renders on demand. Can be returned directly from controllers.
/// </summary>
public sealed class InlayTemplate : IHtmlContent, IActionResult, IResult
{
    private readonly FormattableString _formattable;

    /// <inheritdoc cref="InlayTemplate"/>
    public InlayTemplate(FormattableString formattable)
        => _formattable = formattable;

    /// <summary>
    /// Writes the rendered HTML to the given writer. Nested IHtmlContent args
    /// are rendered recursively onto the same writer — no intermediate strings.
    /// </summary>
    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        => SimpleHtmlTemplate.RenderTo(_formattable, writer, encoder);

    /// <inheritdoc/>
    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.ContentType = "text/html; charset=utf-8";
        await using var writer = new StreamWriter(response.Body, leaveOpen: true);
        WriteTo(writer, HtmlEncoder.Default);
        await writer.FlushAsync();
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType = "text/html; charset=utf-8";
        await using var writer = new StreamWriter(httpContext.Response.Body, leaveOpen: true);
        WriteTo(writer, HtmlEncoder.Default);
        await writer.FlushAsync();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        using var writer = new StringWriter();
        WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
}
