using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.DaisyUI;

public static partial class Daisy
{
    public static IHtmlContent Table<T>(
        IEnumerable<string> headers,
        IEnumerable<T> rows,
        Func<T, FormattableString> rowTemplate,
        bool zebra = false,
        bool compact = false,
        bool pinRows = false)
    {
        var css = Inlay.Css(
            ("table", true),
            ("table-zebra", zebra),
            ("table-compact", compact),
            ("table-pin-rows", pinRows));

        return Inlay.Template($"""
            <div class="overflow-x-auto">
                <table class="{css}">
                    <thead>
                        <tr>
                            {Inlay.Each(headers, h => $"<th>{h}</th>")}
                        </tr>
                    </thead>
                    <tbody>
                        {Inlay.Each(rows, rowTemplate)}
                    </tbody>
                </table>
            </div>
            """);
    }
}
