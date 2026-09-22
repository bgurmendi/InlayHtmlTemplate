using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
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
        var sb = new StringBuilder();
        using (var writer = new StringWriter(sb))
        {
            WriteTo(writer, HtmlEncoder.Default);
        }
        await response.WriteAsync(sb.ToString(), Encoding.UTF8);
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var response = httpContext.Response;
        response.ContentType = "text/html; charset=utf-8";
        var sb = new StringBuilder();
        using (var writer = new StringWriter(sb))
        {
            WriteTo(writer, HtmlEncoder.Default);
        }
        await response.WriteAsync(sb.ToString(), Encoding.UTF8);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        using var writer = new StringWriter();
        WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
}
