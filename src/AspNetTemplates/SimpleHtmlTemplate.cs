using System.Net;
using System.Text;
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
        var result = new StringBuilder();
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
                    var arg = args[argIndex];
                    AppendWithContext(arg, result, context);
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
                result.Append(format[i]);
            }
        }

        return result.ToString();
    }

    private static void AppendWithContext(object? arg, StringBuilder builder,
                                         HtmlContext context)
    {
        if (arg == null)
        {
            return;
        }

        if (arg is IHtmlContent htmlContent)
        {
            builder.Append(htmlContent);
            return;
        }

        var value = arg?.ToString() ?? "";

        switch (context)
        {
            case HtmlContext.Content:
                builder.Append(WebUtility.HtmlEncode(value));
                break;
            case HtmlContext.Attribute:
                var encoded = WebUtility.HtmlEncode(value)
                    .Replace("\"", "&quot;")
                    .Replace("'", "&#39;");
                builder.Append(encoded);
                break;
            case HtmlContext.UrlAttribute:
                if (value.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                    value = "#";
                builder.Append(WebUtility.UrlEncode(value));
                break;
            default:
                builder.Append(WebUtility.HtmlEncode(value));
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
