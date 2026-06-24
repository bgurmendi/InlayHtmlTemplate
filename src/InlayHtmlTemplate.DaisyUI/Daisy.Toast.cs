using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.DaisyUI;

public static partial class Daisy
{
    public static IHtmlContent Toast(IHtmlContent content, ToastPosition position = ToastPosition.BottomEnd) =>
        Inlay.Template($"""
            <div class="toast {ToastPositionClass(position)}">
                {content}
            </div>
            """);

    static string ToastPositionClass(ToastPosition position) => position switch
    {
        ToastPosition.TopStart => "toast-top toast-start",
        ToastPosition.TopCenter => "toast-top toast-center",
        ToastPosition.TopEnd => "toast-top toast-end",
        ToastPosition.MiddleStart => "toast-middle toast-start",
        ToastPosition.MiddleCenter => "toast-middle toast-center",
        ToastPosition.MiddleEnd => "toast-middle toast-end",
        ToastPosition.BottomStart => "toast-bottom toast-start",
        ToastPosition.BottomCenter => "toast-bottom toast-center",
        ToastPosition.BottomEnd => "toast-bottom toast-end",
        _ => "toast-bottom toast-end"
    };
}
