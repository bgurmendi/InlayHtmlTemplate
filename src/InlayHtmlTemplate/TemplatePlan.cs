using System.Collections.Concurrent;
using System.Text.Encodings.Web;

namespace InlayHtmlTemplate;

/// <summary>
/// Pre-analyzed template structure. Stores literal spans into the original format string
/// and the HTML context for each argument slot. Cached by format string reference.
/// </summary>
internal sealed class TemplatePlan
{
    private static readonly ConcurrentDictionary<string, TemplatePlan> Cache = new(ReferenceEqualityComparer.Instance);

    private readonly string _format;
    private readonly int[] _literalRanges;
    private readonly HtmlContext[] _argContexts;
    private readonly string?[] _argAttrNames;

    private TemplatePlan(string format, int[] literalRanges, HtmlContext[] argContexts, string?[] argAttrNames)
    {
        _format = format;
        _literalRanges = literalRanges;
        _argContexts = argContexts;
        _argAttrNames = argAttrNames;
    }

    internal static TemplatePlan GetOrAnalyze(string format)
        => Cache.GetOrAdd(format, static f => Analyze(f));

    private static TemplatePlan Analyze(string format)
    {
        var literalRanges = new List<int>();
        var contexts = new List<HtmlContext>();
        var attrNames = new List<string?>();
        var contextAnalyzer = new HtmlContextAnalyzer();
        int literalStart = 0;

        for (int i = 0; i < format.Length; i++)
        {
            contextAnalyzer.ProcessChar(format[i]);

            if (i < format.Length - 1 && format[i] == '{' && format[i + 1] != '{')
            {
                int end = format.IndexOf('}', i);
                if (end == -1) break;

                int litLength = i - literalStart;
                var context = contextAnalyzer.CurrentContext;
                string? attrName = null;

                bool isQuoted = false;
                if (context == HtmlContext.BooleanAttribute)
                {
                    attrName = contextAnalyzer.CurrentAttribute;
                    isQuoted = contextAnalyzer.IsQuotedValue;
                    litLength -= TrimBooleanAttribute(format, literalStart, litLength, attrName, isQuoted);
                }

                literalRanges.Add(literalStart);
                literalRanges.Add(litLength);
                contexts.Add(context);
                attrNames.Add(attrName);

                for (int j = i; j <= end && j < format.Length; j++)
                    contextAnalyzer.ProcessChar(format[j]);

                i = end;
                literalStart = end + 1;

                if (context == HtmlContext.BooleanAttribute)
                {
                    if (isQuoted && literalStart < format.Length)
                    {
                        contextAnalyzer.ProcessChar(format[literalStart]);
                        literalStart++;
                        i++;
                    }
                    else if (!isQuoted)
                    {
                        contextAnalyzer.ProcessChar(' ');
                    }
                }
            }
        }

        literalRanges.Add(literalStart);
        literalRanges.Add(format.Length - literalStart);

        return new TemplatePlan(format, literalRanges.ToArray(), contexts.ToArray(), attrNames.ToArray());
    }

    private static int TrimBooleanAttribute(string format, int litStart, int litLength, string attrName, bool isQuoted)
    {
        int pos = litStart + litLength - 1;
        int trimLen = 0;

        if (isQuoted)
        {
            pos--;
            trimLen++;
        }

        // Skip whitespace between = and quote
        while (pos >= litStart && char.IsWhiteSpace(format[pos])) { trimLen++; pos--; }

        // Skip =
        if (pos >= litStart && format[pos] == '=') { trimLen++; pos--; }

        // Skip whitespace between name and =
        while (pos >= litStart && char.IsWhiteSpace(format[pos])) { trimLen++; pos--; }

        // Skip attribute name
        trimLen += attrName.Length;
        pos -= attrName.Length;

        // Skip leading whitespace before attribute name
        while (pos >= litStart && char.IsWhiteSpace(format[pos])) { trimLen++; pos--; }

        return trimLen;
    }

    internal void RenderTo(object?[] args, TextWriter writer, HtmlEncoder encoder)
    {
        WriteLiteral(writer, 0);

        for (int i = 0; i < _argContexts.Length; i++)
        {
            if (i < args.Length)
            {
                if (_argContexts[i] == HtmlContext.BooleanAttribute && _argAttrNames[i] is { } attrName)
                {
                    if (IsTruthy(args[i]))
                    {
                        writer.Write(' ');
                        writer.Write(attrName);
                    }
                }
                else
                {
                    SimpleHtmlTemplate.WriteWithContext(args[i], writer, encoder, _argContexts[i]);
                }
            }
            WriteLiteral(writer, i + 1);
        }
    }

    private static bool IsTruthy(object? value) => value switch
    {
        bool b => b,
        null => false,
        string s => s.Length > 0 && !s.Equals("false", StringComparison.OrdinalIgnoreCase),
        _ => true
    };

    private void WriteLiteral(TextWriter writer, int index)
    {
        int offset = _literalRanges[index * 2];
        int length = _literalRanges[index * 2 + 1];
        if (length > 0)
            writer.Write(_format.AsSpan(offset, length));
    }
}
