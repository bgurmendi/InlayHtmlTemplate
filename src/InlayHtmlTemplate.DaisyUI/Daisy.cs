using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.DaisyUI;

public static partial class Daisy
{
    static string VariantClass(string prefix, string variant) =>
        variant == "" ? "" : $"{prefix}-{variant}";

}
