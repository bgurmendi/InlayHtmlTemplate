using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.DaisyUI;

public static partial class Daisy
{
    public static IHtmlContent Footer(params FooterSection[] sections) =>
        Inlay.Template($"""
            <footer class="footer bg-neutral text-neutral-content p-10">
                {Inlay.Each(sections, s => $"""
                    <nav>
                        <h6 class="footer-title">{s.Title}</h6>
                        {Inlay.Each(s.Links, l => $"""<a class="link link-hover" href="{Inlay.Raw(l.Href)}">{l.Label}</a>""")}
                    </nav>
                    """)}
            </footer>
            """);

    public static IHtmlContent FooterSimple(string text) =>
        Inlay.Template($"""
            <footer class="footer footer-center bg-base-300 text-base-content p-4">
                <aside><p>{text}</p></aside>
            </footer>
            """);
}
