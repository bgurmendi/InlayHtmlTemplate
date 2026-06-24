using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.DaisyUI;

public static partial class Daisy
{
    public static IHtmlContent Badge(string text, BadgeVariant variant = BadgeVariant.Default) =>
        Inlay.Template($"""<span class="badge {BadgeClass(variant)}">{text}</span>""");

    public static IHtmlContent Badge(IHtmlContent content, BadgeVariant variant = BadgeVariant.Default) =>
        Inlay.Template($"""<span class="badge {BadgeClass(variant)}">{content}</span>""");

    static string BadgeClass(BadgeVariant variant) => variant switch
    {
        BadgeVariant.Primary => "badge-primary",
        BadgeVariant.Secondary => "badge-secondary",
        BadgeVariant.Accent => "badge-accent",
        BadgeVariant.Ghost => "badge-ghost",
        BadgeVariant.Outline => "badge-outline",
        BadgeVariant.Info => "badge-info",
        BadgeVariant.Success => "badge-success",
        BadgeVariant.Warning => "badge-warning",
        BadgeVariant.Error => "badge-error",
        BadgeVariant.Neutral => "badge-neutral",
        _ => ""
    };
}
