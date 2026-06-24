using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.DaisyUI;

public static partial class Daisy
{
    public static IHtmlContent FormControl(
        IHtmlContent input,
        string? label = null,
        string? altLabel = null,
        string? helperText = null)
    {
        return Inlay.Template($"""
            <div class="form-control w-full">
                {Inlay.If(label is not null || altLabel is not null, $"""
                    <label class="label">
                        {Inlay.If(label is not null, $"""<span class="label-text">{label}</span>""")}
                        {Inlay.If(altLabel is not null, $"""<span class="label-text-alt">{altLabel}</span>""")}
                    </label>
                    """)}
                {input}
                {Inlay.If(helperText is not null, $"""
                    <label class="label">
                        <span class="label-text-alt">{helperText}</span>
                    </label>
                    """)}
            </div>
            """);
    }
}
