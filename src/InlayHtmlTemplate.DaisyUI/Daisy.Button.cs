using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.DaisyUI;

public static partial class Daisy
{
    public static IHtmlContent Button(
        string label,
        ButtonVariant variant = ButtonVariant.Default,
        ButtonSize size = ButtonSize.Default,
        bool outline = false,
        bool disabled = false,
        string type = "button",
        string? href = null)
    {
        var css = Inlay.Css(
            ("btn", true),
            (ButtonVariantClass(variant), variant != ButtonVariant.Default),
            (ButtonSizeClass(size), size != ButtonSize.Default),
            ("btn-outline", outline),
            ("btn-disabled", disabled));

        if (href is not null)
            return Inlay.Template($"""<a class="{css}" href="{Inlay.Raw(href)}">{label}</a>""");

        return Inlay.Template($"""<button class="{css}" type="{type}" disabled={disabled}>{label}</button>""");
    }

    public static IHtmlContent Button(
        IHtmlContent content,
        ButtonVariant variant = ButtonVariant.Default,
        ButtonSize size = ButtonSize.Default,
        bool outline = false,
        bool disabled = false,
        string type = "button")
    {
        var css = Inlay.Css(
            ("btn", true),
            (ButtonVariantClass(variant), variant != ButtonVariant.Default),
            (ButtonSizeClass(size), size != ButtonSize.Default),
            ("btn-outline", outline),
            ("btn-disabled", disabled));

        return Inlay.Template($"""<button class="{css}" type="{type}" disabled={disabled}>{content}</button>""");
    }

    static string ButtonVariantClass(ButtonVariant variant) => variant switch
    {
        ButtonVariant.Primary => "btn-primary",
        ButtonVariant.Secondary => "btn-secondary",
        ButtonVariant.Accent => "btn-accent",
        ButtonVariant.Ghost => "btn-ghost",
        ButtonVariant.Link => "btn-link",
        ButtonVariant.Info => "btn-info",
        ButtonVariant.Success => "btn-success",
        ButtonVariant.Warning => "btn-warning",
        ButtonVariant.Error => "btn-error",
        _ => ""
    };

    static string ButtonSizeClass(ButtonSize size) => size switch
    {
        ButtonSize.Xs => "btn-xs",
        ButtonSize.Sm => "btn-sm",
        ButtonSize.Lg => "btn-lg",
        _ => ""
    };
}
