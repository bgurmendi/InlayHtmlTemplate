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

    private TemplatePlan(string format, int[] literalRanges, HtmlContext[] argContexts)
    {
        _format = format;
        _literalRanges = literalRanges;
        _argContexts = argContexts;
    }

    internal static TemplatePlan GetOrAnalyze(string format)
        => Cache.GetOrAdd(format, static f => Analyze(f));

    private static TemplatePlan Analyze(string format)
    {
        var literalRanges = new List<int>();
        var contexts = new List<HtmlContext>();
        var contextAnalyzer = new HtmlContextAnalyzer();
        int literalStart = 0;

        for (int i = 0; i < format.Length; i++)
        {
            contextAnalyzer.ProcessChar(format[i]);

            if (i < format.Length - 1 && format[i] == '{' && format[i + 1] != '{')
            {
                int end = format.IndexOf('}', i);
                if (end == -1) break;

                literalRanges.Add(literalStart);
                literalRanges.Add(i - literalStart);

                contexts.Add(contextAnalyzer.CurrentContext);

                for (int j = i; j <= end && j < format.Length; j++)
                    contextAnalyzer.ProcessChar(format[j]);

                i = end;
                literalStart = end + 1;
            }
        }

        literalRanges.Add(literalStart);
        literalRanges.Add(format.Length - literalStart);

        return new TemplatePlan(format, literalRanges.ToArray(), contexts.ToArray());
    }

    internal void RenderTo(object?[] args, TextWriter writer, HtmlEncoder encoder)
    {
        WriteLiteral(writer, 0);

        for (int i = 0; i < _argContexts.Length; i++)
        {
            if (i < args.Length)
                SimpleHtmlTemplate.WriteWithContext(args[i], writer, encoder, _argContexts[i]);
            WriteLiteral(writer, i + 1);
        }
    }

    private void WriteLiteral(TextWriter writer, int index)
    {
        int offset = _literalRanges[index * 2];
        int length = _literalRanges[index * 2 + 1];
        if (length > 0)
            writer.Write(_format.AsSpan(offset, length));
    }
}
