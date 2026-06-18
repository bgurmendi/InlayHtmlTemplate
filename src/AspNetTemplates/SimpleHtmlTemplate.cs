using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace AspNetTemplates;

/// <summary>
/// Renders HTML templates using C# string interpolation with automatic context-aware escaping.
/// </summary>
public class SimpleHtmlTemplate
{
    /// <summary>
    /// Renders an interpolated string as HTML with context-aware escaping.
    /// </summary>
    public static string Render(FormattableString formattable)
    {
        using var writer = new StringWriter();
        RenderTo(formattable, writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    internal static void RenderTo(FormattableString formattable, TextWriter writer, HtmlEncoder encoder)
    {
        var format = formattable.Format;
        var args = formattable.GetArguments();
        var contextAnalyzer = new HtmlContextAnalyzer();

        int argIndex = 0;
        for (int i = 0; i < format.Length; i++)
        {
            contextAnalyzer.ProcessChar(format[i]);

            if (i < format.Length - 1 && format[i] == '{' && format[i + 1] != '{')
            {
                int end = format.IndexOf('}', i);
                if (end == -1) break;

                var context = contextAnalyzer.CurrentContext;

                if (argIndex < args.Length)
                {
                    WriteWithContext(args[argIndex], writer, encoder, context);
                    argIndex++;
                }

                for (int j = i; j <= end && j < format.Length; j++)
                {
                    contextAnalyzer.ProcessChar(format[j]);
                }

                i = end;
            }
            else
            {
                writer.Write(format[i]);
            }
        }
    }

    internal static void WriteWithContext(object? arg, TextWriter writer,
                                          HtmlEncoder encoder, HtmlContext context)
    {
        if (arg == null) return;

        if (arg is IHtmlContent htmlContent)
        {
            htmlContent.WriteTo(writer, encoder);
            return;
        }

        var value = arg.ToString() ?? "";

        switch (context)
        {
            case HtmlContext.Content:
            case HtmlContext.Attribute:
                encoder.Encode(writer, value);
                break;
            case HtmlContext.UrlAttribute:
                if (value.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                    writer.Write('#');
                else
                    UrlEncoder.Default.Encode(writer, value);
                break;
            default:
                encoder.Encode(writer, value);
                break;
        }
    }
}

/// <summary>
/// Represents the HTML context where an interpolated value appears.
/// </summary>
public enum HtmlContext
{
    /// <summary>Inside element content.</summary>
    Content,
    /// <summary>Inside an attribute value.</summary>
    Attribute,
    /// <summary>Inside a URL attribute (href, src).</summary>
    UrlAttribute,
    /// <summary>Inside a script tag (reserved).</summary>
    Script
}

/// <summary>
/// Character-by-character HTML parser that tracks the current escaping context.
/// </summary>
public class HtmlContextAnalyzer
{
    private HtmlContext _currentContext = HtmlContext.Content;
    private bool _inTag = false;
    private bool _inAttribute = false;
    private string _currentAttribute = "";
    private char _quoteChar = '\0';

    /// <summary>The current HTML context based on characters processed so far.</summary>
    public HtmlContext CurrentContext => _currentContext;

    /// <summary>Feeds a character to the analyzer, updating the internal state.</summary>
    public void ProcessChar(char c)
    {
        if (!_inTag)
        {
            if (c == '<')
            {
                _inTag = true;
            }
        }
        else
        {
            if (_quoteChar == '\0')
            {
                if (c == '>')
                {
                    _inTag = false;
                    _inAttribute = false;
                    _currentAttribute = "";
                    _currentContext = HtmlContext.Content;
                }
                else if (c == '\'' || c == '"')
                {
                    _quoteChar = c;
                    _currentContext = GetAttributeContext(_currentAttribute);
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (!_inAttribute)
                        _currentAttribute = "";
                }
                else if (char.IsLetter(c) && !_inAttribute)
                {
                    _currentAttribute += c;
                }
                else if (c == '=')
                {
                    _inAttribute = true;
                }
            }
            else
            {
                if (c == _quoteChar)
                {
                    _quoteChar = '\0';
                    _inAttribute = false;
                    _currentAttribute = "";
                    _currentContext = HtmlContext.Content;
                }
            }
        }
    }

    private HtmlContext GetAttributeContext(string attribute)
    {
        attribute = attribute.ToLowerInvariant();

        if (attribute == "href" || attribute == "src")
            return HtmlContext.UrlAttribute;

        return HtmlContext.Attribute;
    }
}
