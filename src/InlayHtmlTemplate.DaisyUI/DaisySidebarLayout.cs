using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.DaisyUI;

public static class DaisySidebarLayout
{
    public static InlayTemplate Render(
        string title,
        IHtmlContent sidebar,
        IHtmlContent body,
        IHtmlContent? navbar = null,
        IHtmlContent? footer = null,
        string theme = "light",
        string drawerId = "app-drawer",
        IHtmlContent? headExtra = null)
    {
        return DaisyLayout.Render(title,
            Inlay.Template($"""
                <div class="drawer lg:drawer-open">
                    <input id="{drawerId}" type="checkbox" class="drawer-toggle" />
                    <div class="drawer-content flex flex-col">
                        {Inlay.If(navbar is not null, $"""
                            <div class="sticky top-0 z-30">
                                {navbar ?? HtmlString.Empty}
                            </div>
                            """)}
                        <main class="flex-1 p-6">
                            {body}
                        </main>
                        {footer ?? HtmlString.Empty}
                    </div>
                    <div class="drawer-side">
                        <label for="{drawerId}" aria-label="close sidebar" class="drawer-overlay"></label>
                        <aside class="bg-base-200 min-h-full w-64 p-4">
                            {sidebar}
                        </aside>
                    </div>
                </div>
                """),
            theme,
            headExtra);
    }
}
