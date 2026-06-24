using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.DaisyUI;

public static partial class Daisy
{
    public static IHtmlContent Checkbox(
        string? name = null,
        string? label = null,
        bool isChecked = false,
        InputVariant variant = InputVariant.Default,
        InputSize size = InputSize.Default,
        bool disabled = false)
    {
        var css = Inlay.Css(
            ("checkbox", true),
            (InputVariantClass("checkbox", variant), variant != InputVariant.Default),
            (InputSizeClass("checkbox", size), size != InputSize.Default));

        if (label is not null)
            return Inlay.Template($"""
                <label class="label cursor-pointer gap-2">
                    <span class="label-text">{label}</span>
                    <input type="checkbox" name="{name ?? ""}" class="{css}" checked={isChecked} disabled={disabled} />
                </label>
                """);

        return Inlay.Template(
            $"""<input type="checkbox" name="{name ?? ""}" class="{css}" checked={isChecked} disabled={disabled} />""");
    }

    public static IHtmlContent Toggle(
        string? name = null,
        string? label = null,
        bool isChecked = false,
        InputVariant variant = InputVariant.Default,
        InputSize size = InputSize.Default,
        bool disabled = false)
    {
        var css = Inlay.Css(
            ("toggle", true),
            (InputVariantClass("toggle", variant), variant != InputVariant.Default),
            (InputSizeClass("toggle", size), size != InputSize.Default));

        if (label is not null)
            return Inlay.Template($"""
                <label class="label cursor-pointer gap-2">
                    <span class="label-text">{label}</span>
                    <input type="checkbox" name="{name ?? ""}" class="{css}" checked={isChecked} disabled={disabled} />
                </label>
                """);

        return Inlay.Template(
            $"""<input type="checkbox" name="{name ?? ""}" class="{css}" checked={isChecked} disabled={disabled} />""");
    }

    public static IHtmlContent Radio(
        string name,
        string value,
        string? label = null,
        bool isChecked = false,
        InputVariant variant = InputVariant.Default,
        InputSize size = InputSize.Default,
        bool disabled = false)
    {
        var css = Inlay.Css(
            ("radio", true),
            (InputVariantClass("radio", variant), variant != InputVariant.Default),
            (InputSizeClass("radio", size), size != InputSize.Default));

        if (label is not null)
            return Inlay.Template($"""
                <label class="label cursor-pointer gap-2">
                    <span class="label-text">{label}</span>
                    <input type="radio" name="{name}" value="{value}" class="{css}" checked={isChecked} disabled={disabled} />
                </label>
                """);

        return Inlay.Template(
            $"""<input type="radio" name="{name}" value="{value}" class="{css}" checked={isChecked} disabled={disabled} />""");
    }

    public static IHtmlContent RadioGroup(
        string name,
        IEnumerable<SelectOption> options,
        InputVariant variant = InputVariant.Default,
        InputSize size = InputSize.Default)
    {
        var radioCss = Inlay.Css(
            ("radio", true),
            (InputVariantClass("radio", variant), variant != InputVariant.Default),
            (InputSizeClass("radio", size), size != InputSize.Default));

        return Inlay.Each(options, o =>
            $"""
            <label class="label cursor-pointer gap-2">
                <span class="label-text">{o.Label}</span>
                <input type="radio" name="{name}" value="{o.Value}" class="{radioCss}" checked={o.Selected} />
            </label>
            """);
    }

    public static IHtmlContent Range(
        string? name = null,
        int min = 0,
        int max = 100,
        int? value = null,
        int? step = null,
        InputVariant variant = InputVariant.Default,
        InputSize size = InputSize.Default,
        bool disabled = false)
    {
        var css = Inlay.Css(
            ("range", true),
            (InputVariantClass("range", variant), variant != InputVariant.Default),
            (InputSizeClass("range", size), size != InputSize.Default));

        var stepAttr = step.HasValue ? $""" step="{step.Value}" """ : "";
        var valueAttr = value.HasValue ? $""" value="{value.Value}" """ : "";

        return Inlay.Template(
            $"""<input type="range" name="{name ?? ""}" min="{min}" max="{max}" class="{css}" disabled={disabled}{Inlay.Raw(stepAttr)}{Inlay.Raw(valueAttr)} />""");
    }
}
