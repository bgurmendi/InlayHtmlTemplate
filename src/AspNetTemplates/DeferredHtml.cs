using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace AspNetTemplates;

internal sealed class DeferredHtml : IHtmlContent
{
    private readonly Action<TextWriter, HtmlEncoder> _writeAction;

    public DeferredHtml(Action<TextWriter, HtmlEncoder> writeAction)
        => _writeAction = writeAction;

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        => _writeAction(writer, encoder);

    public override string ToString()
    {
        using var writer = new StringWriter();
        WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
}
