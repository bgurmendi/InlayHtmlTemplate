using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.DaisyUI;

public static partial class Daisy
{
    public static IHtmlContent TextInput(
        string? name = null,
        string? value = null,
        string? placeholder = null,
        string type = "text",
        InputVariant variant = InputVariant.Default,
        InputSize size = InputSize.Default,
        bool bordered = true,
        bool ghost = false,
        bool disabled = false)
    {
        var css = Inlay.Css(
            ("input", true),
            ("w-full", true),
            (InputVariantClass("input", variant), variant != InputVariant.Default),
            (InputSizeClass("input", size), size != InputSize.Default),
            ("input-bordered", bordered),
            ("input-ghost", ghost));

        return Inlay.Template(
            $"""<input type="{type}" name="{name ?? ""}" value="{value ?? ""}" placeholder="{placeholder ?? ""}" class="{css}" disabled={disabled} />""");
    }

    public static IHtmlContent Textarea(
        string? name = null,
        string? value = null,
        string? placeholder = null,
        int? rows = null,
        InputVariant variant = InputVariant.Default,
        InputSize size = InputSize.Default,
        bool bordered = true,
        bool disabled = false)
    {
        var css = Inlay.Css(
            ("textarea", true),
            ("w-full", true),
            (InputVariantClass("textarea", variant), variant != InputVariant.Default),
            (InputSizeClass("textarea", size), size != InputSize.Default),
            ("textarea-bordered", bordered));

        var rowsAttr = rows.HasValue ? $""" rows="{rows.Value}" """ : "";

        return Inlay.Template(
            $"""<textarea name="{name ?? ""}" placeholder="{placeholder ?? ""}" class="{css}" disabled={disabled}{Inlay.Raw(rowsAttr)}>{value ?? ""}</textarea>""");
    }

    public static IHtmlContent Select(
        IEnumerable<SelectOption> options,
        string? name = null,
        string? placeholder = null,
        InputVariant variant = InputVariant.Default,
        InputSize size = InputSize.Default,
        bool bordered = true,
        bool disabled = false)
    {
        var css = Inlay.Css(
            ("select", true),
            ("w-full", true),
            (InputVariantClass("select", variant), variant != InputVariant.Default),
            (InputSizeClass("select", size), size != InputSize.Default),
            ("select-bordered", bordered));

        return Inlay.Template($"""
            <select name="{name ?? ""}" class="{css}" disabled={disabled}>
                {Inlay.If(placeholder is not null, $"""<option disabled selected>{placeholder}</option>""")}
                {Inlay.Each(options, o =>
                    $"""<option value="{o.Value}" selected={o.Selected}>{o.Label}</option>""")}
            </select>
            """);
    }

    public static IHtmlContent FileInput(
        string? name = null,
        string? accept = null,
        InputVariant variant = InputVariant.Default,
        InputSize size = InputSize.Default,
        bool bordered = true,
        bool disabled = false)
    {
        var css = Inlay.Css(
            ("file-input", true),
            ("w-full", true),
            (InputVariantClass("file-input", variant), variant != InputVariant.Default),
            (InputSizeClass("file-input", size), size != InputSize.Default),
            ("file-input-bordered", bordered));

        var acceptAttr = accept is not null ? $""" accept="{accept}" """ : "";

        return Inlay.Template(
            $"""<input type="file" name="{name ?? ""}" class="{css}" disabled={disabled}{Inlay.Raw(acceptAttr)} />""");
    }

    static string InputVariantClass(string prefix, InputVariant variant) => variant switch
    {
        InputVariant.Primary => $"{prefix}-primary",
        InputVariant.Secondary => $"{prefix}-secondary",
        InputVariant.Accent => $"{prefix}-accent",
        InputVariant.Info => $"{prefix}-info",
        InputVariant.Success => $"{prefix}-success",
        InputVariant.Warning => $"{prefix}-warning",
        InputVariant.Error => $"{prefix}-error",
        _ => ""
    };

    static string InputSizeClass(string prefix, InputSize size) => size switch
    {
        InputSize.Xs => $"{prefix}-xs",
        InputSize.Sm => $"{prefix}-sm",
        InputSize.Lg => $"{prefix}-lg",
        _ => ""
    };
}
