using System.Text;
using Microsoft.AspNetCore.Html;

namespace AspNetTemplates;

/// <summary>
/// Helper methods for composing HTML templates: conditionals, class toggling, and list iteration.
/// </summary>
public static class Html
{
    /// <summary>
    /// Wraps a pre-rendered HTML string as trusted content that won't be double-escaped.
    /// </summary>
    public static IHtmlContent Raw(string html) => new HtmlString(html);

    /// <summary>
    /// Renders content only when the condition is true. Returns empty otherwise.
    /// </summary>
    public static IHtmlContent If(bool condition, FormattableString content)
    {
        if (!condition) return HtmlString.Empty;
        return new HtmlString(SimpleHtmlTemplate.Render(content));
    }

    /// <summary>
    /// Renders content when the condition is true, or the fallback when false.
    /// </summary>
    public static IHtmlContent If(bool condition, FormattableString content, FormattableString fallback)
    {
        return new HtmlString(SimpleHtmlTemplate.Render(condition ? content : fallback));
    }

    /// <summary>
    /// Builds a CSS class string from conditionally active class names.
    /// </summary>
    public static string Css(params (string className, bool active)[] classes)
    {
        return string.Join(" ", classes.Where(c => c.active).Select(c => c.className));
    }

    /// <summary>
    /// Renders a template for each item in the collection.
    /// </summary>
    public static IHtmlContent Each<T>(IEnumerable<T> items, Func<T, FormattableString> template)
    {
        var sb = new StringBuilder();
        foreach (var item in items)
            sb.Append(SimpleHtmlTemplate.Render(template(item)));
        return new HtmlString(sb.ToString());
    }

    /// <summary>
    /// Renders a template for each item, or fallback content if the collection is empty.
    /// </summary>
    public static IHtmlContent Each<T>(IEnumerable<T> items, Func<T, FormattableString> template,
                                       FormattableString empty)
    {
        var list = items as IReadOnlyList<T> ?? items.ToList();
        if (list.Count == 0)
            return new HtmlString(SimpleHtmlTemplate.Render(empty));
        return Each(list, template);
    }

    /// <summary>
    /// Renders a template for each item with its zero-based index.
    /// </summary>
    public static IHtmlContent Each<T>(IEnumerable<T> items, Func<T, int, FormattableString> template)
    {
        var sb = new StringBuilder();
        var index = 0;
        foreach (var item in items)
        {
            sb.Append(SimpleHtmlTemplate.Render(template(item, index)));
            index++;
        }
        return new HtmlString(sb.ToString());
    }

    /// <summary>
    /// Renders a template for each item with its index, or fallback content if empty.
    /// </summary>
    public static IHtmlContent Each<T>(IEnumerable<T> items, Func<T, int, FormattableString> template,
                                       FormattableString empty)
    {
        var list = items as IReadOnlyList<T> ?? items.ToList();
        if (list.Count == 0)
            return new HtmlString(SimpleHtmlTemplate.Render(empty));
        return Each(list, template);
    }
}
