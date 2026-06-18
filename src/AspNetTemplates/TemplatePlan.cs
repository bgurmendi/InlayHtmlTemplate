using System.Text;
using System.Text.Encodings.Web;

namespace AspNetTemplates;

/// <summary>
/// Pre-analyzed template structure. Stores literal segments and the HTML context
/// for each argument slot, so rendering only needs to encode and write values.
/// </summary>
internal sealed class TemplatePlan
{
    private readonly string[] _literals;
    private readonly HtmlContext[] _argContexts;

    private TemplatePlan(string[] literals, HtmlContext[] argContexts)
    {
        _literals = literals;
        _argContexts = argContexts;
    }

    internal static TemplatePlan Analyze(string format)
    {
        var literals = new List<string>();
        var contexts = new List<HtmlContext>();
        var contextAnalyzer = new HtmlContextAnalyzer();
        var currentLiteral = new StringBuilder();

        for (int i = 0; i < format.Length; i++)
        {
            contextAnalyzer.ProcessChar(format[i]);

            if (i < format.Length - 1 && format[i] == '{' && format[i + 1] != '{')
            {
                int end = format.IndexOf('}', i);
                if (end == -1) break;

                literals.Add(currentLiteral.ToString());
                currentLiteral.Clear();

                contexts.Add(contextAnalyzer.CurrentContext);

                for (int j = i; j <= end && j < format.Length; j++)
                    contextAnalyzer.ProcessChar(format[j]);

                i = end;
            }
            else
            {
                currentLiteral.Append(format[i]);
            }
        }

        literals.Add(currentLiteral.ToString());

        return new TemplatePlan(literals.ToArray(), contexts.ToArray());
    }

    internal void RenderTo(object?[] args, TextWriter writer, HtmlEncoder encoder)
    {
        writer.Write(_literals[0]);

        for (int i = 0; i < _argContexts.Length; i++)
        {
            if (i < args.Length)
                SimpleHtmlTemplate.WriteWithContext(args[i], writer, encoder, _argContexts[i]);
            writer.Write(_literals[i + 1]);
        }
    }
}
