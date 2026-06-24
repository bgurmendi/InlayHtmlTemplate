using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.DaisyUI;

public static partial class Daisy
{
    public static IHtmlContent Modal(
        string id,
        string? title = null,
        IHtmlContent? body = null,
        IHtmlContent? actions = null)
    {
        return Inlay.Template($"""
            <dialog id="{id}" class="modal">
                <div class="modal-box">
                    {Inlay.If(title is not null, $"""<h3 class="text-lg font-bold">{title}</h3>""")}
                    <div class="py-4">{body ?? HtmlString.Empty}</div>
                    <div class="modal-action">
                        {actions ?? HtmlString.Empty}
                        <form method="dialog">
                            <button class="btn">Close</button>
                        </form>
                    </div>
                </div>
                <form method="dialog" class="modal-backdrop">
                    <button>close</button>
                </form>
            </dialog>
            """);
    }

    public static IHtmlContent ModalTrigger(string modalId, string label, ButtonVariant variant = ButtonVariant.Default) =>
        Inlay.Template($"""<button class="btn {ButtonVariantClass(variant)}" onclick="{modalId}.showModal()">{ label}</button>""");

    public static IHtmlContent ModalTrigger(string modalId, IHtmlContent content) =>
        Inlay.Template($"""<button class="btn" onclick="{modalId}.showModal()">{content}</button>""");
}
