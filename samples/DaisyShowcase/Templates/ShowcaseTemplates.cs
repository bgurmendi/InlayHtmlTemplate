using InlayHtmlTemplate;
using InlayHtmlTemplate.DaisyUI;
using Microsoft.AspNetCore.Html;

namespace DaisyShowcase.Templates;

public static class ShowcaseTemplates
{
    public static InlayTemplate Index()
    {
        var body = Inlay.Template($"""
            {Daisy.Hero("InlayHtmlTemplate + DaisyUI",
                "Type-safe, composable UI components rendered with C# string interpolation. No Razor, no .cshtml — just functions.",
                Inlay.Template($"""
                    <div class="flex gap-2">
                        {Daisy.Button("Browse Components", ButtonVariant.Primary, href: "/Showcase/Components")}
                        {Daisy.Button("View Layouts", ButtonVariant.Ghost, href: "/Showcase/Layouts")}
                    </div>
                    """))}

            <div class="p-8">
                <h2 class="text-3xl font-bold mb-6 text-center">Highlights</h2>
                <div class="grid grid-cols-1 md:grid-cols-3 gap-6 max-w-5xl mx-auto">
                    {Daisy.Card(
                        title: "Context-Aware XSS Protection",
                        body: Inlay.Template($"<p>Every interpolated value is escaped based on its HTML context — content, attributes, or URLs.</p>"))}
                    {Daisy.Card(
                        title: "DaisyUI Components",
                        body: Inlay.Template($"<p>Pre-built alerts, buttons, cards, modals, navbars, stats, tables, and more — all as C# functions.</p>"))}
                    {Daisy.Card(
                        title: "Zero-Copy Rendering",
                        body: Inlay.Template($"<p>Templates compose without intermediate strings. Nested components render directly to the response stream.</p>"))}
                </div>

                <div class="divider my-12"></div>

                <h2 class="text-3xl font-bold mb-6 text-center">Quick Stats</h2>
                <div class="flex justify-center">
                    {Daisy.Stats([
                        new StatItem("Components", "10", "Alerts, cards, modals..."),
                        new StatItem("Layouts", "2", "Base + sidebar"),
                        new StatItem("Dependencies", "1", "InlayHtmlTemplate"),
                    ])}
                </div>
            </div>
            """);

        return WithSidebar("Home", "Index", body);
    }

    public static InlayTemplate Components()
    {
        var body = Inlay.Template($"""
            <h1 class="text-4xl font-bold mb-8">Components</h1>

            {Section("Alerts", AlertsDemo())}
            {Section("Badges", BadgesDemo())}
            {Section("Buttons", ButtonsDemo())}
            {Section("Cards", CardsDemo())}
            {Section("Hero", HeroDemo())}
            {Section("Modal", ModalDemo())}
            {Section("Stats", StatsDemo())}
            {Section("Table", TableDemo())}
            {Section("Toast", ToastDemo())}
            {Section("Footer", FooterDemo())}
            {Section("Text Inputs", TextInputsDemo())}
            {Section("Textarea", TextareaDemo())}
            {Section("Select", SelectDemo())}
            {Section("Checkbox &amp; Toggle", CheckToggleDemo())}
            {Section("Radio", RadioDemo())}
            {Section("Range", RangeDemo())}
            {Section("File Input", FileInputDemo())}
            {Section("Form Control", FormControlDemo())}
            """);

        return WithSidebar("Components", "Components", body);
    }

    public static InlayTemplate Layouts()
    {
        var sidebarCode = Inlay.Raw(
            "<div class=\"mockup-code\"><pre><code>" +
            "DaisySidebarLayout.Render(\n" +
            "    title: \"My App\",\n" +
            "    sidebar: mySidebarContent,\n" +
            "    body: myPageContent,\n" +
            "    navbar: Daisy.Navbar(...),\n" +
            "    footer: Daisy.FooterSimple(\"© 2026\"),\n" +
            "    theme: \"nord\"\n" +
            ");" +
            "</code></pre></div>");

        var baseCode = Inlay.Raw(
            "<div class=\"mockup-code\"><pre><code>" +
            "DaisyLayout.Render(\n" +
            "    title: \"Page Title\",\n" +
            "    body: myContent,\n" +
            "    theme: \"light\"\n" +
            ");" +
            "</code></pre></div>");

        var body = Inlay.Template($"""
            <h1 class="text-4xl font-bold mb-8">Layouts</h1>

            {Section("Sidebar Layout", Inlay.Template($"""
                <p class="mb-4">This page itself uses <code>DaisySidebarLayout</code>. The sidebar collapses into a drawer on mobile and stays pinned on large screens.</p>
                {sidebarCode}
                """))}

            {Section("Base Layout", Inlay.Template($"""
                <p class="mb-4"><code>DaisyLayout</code> provides the HTML skeleton with DaisyUI CDN. Use it when you don't need a sidebar.</p>
                {baseCode}
                """))}

            {Section("Themes", Inlay.Template($"""
                <p class="mb-4">Pass any DaisyUI theme name via the <code>theme</code> parameter:</p>
                <div class="flex flex-wrap gap-2">
                    {Daisy.Badge("light", BadgeVariant.Outline)}
                    {Daisy.Badge("dark", BadgeVariant.Outline)}
                    {Daisy.Badge("cupcake", BadgeVariant.Outline)}
                    {Daisy.Badge("nord", BadgeVariant.Outline)}
                    {Daisy.Badge("dracula", BadgeVariant.Outline)}
                    {Daisy.Badge("forest", BadgeVariant.Outline)}
                    {Daisy.Badge("retro", BadgeVariant.Outline)}
                    {Daisy.Badge("cyberpunk", BadgeVariant.Outline)}
                </div>
                """))}
            """);

        return WithSidebar("Layouts", "Layouts", body);
    }

    static InlayTemplate WithSidebar(string title, string activePage, IHtmlContent body)
    {
        var sidebar = Inlay.Template($"""
            <div class="text-xl font-bold mb-4 p-2">DaisyUI Showcase</div>
            <ul class="menu">
                <li><a class="{Inlay.Css(("active", activePage == "Index"))}" href="/">Home</a></li>
                <li><a class="{Inlay.Css(("active", activePage == "Components"))}" href="/Showcase/Components">Components</a></li>
                <li><a class="{Inlay.Css(("active", activePage == "Layouts"))}" href="/Showcase/Layouts">Layouts</a></li>
            </ul>
            """);

        var navbar = Daisy.Navbar(
            start: Inlay.Template($"""
                <label for="app-drawer" class="btn btn-ghost drawer-button lg:hidden">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
                    </svg>
                </label>
                <span class="text-lg font-bold lg:hidden">Showcase</span>
                """),
            end: Inlay.Template($"""
                <a class="btn btn-ghost btn-sm" href="https://github.com">GitHub</a>
                """));

        var footer = Daisy.FooterSimple("© 2026 InlayHtmlTemplate — rendered with zero Razor files.");

        return DaisySidebarLayout.Render(
            $"InlayHtmlTemplate DaisyUI — {title}",
            sidebar,
            body,
            navbar: navbar,
            footer: footer,
            theme: "nord");
    }

    static IHtmlContent Section(string title, IHtmlContent content) =>
        Inlay.Template($"""
            <section id="{title.ToLowerInvariant().Replace(' ', '-')}" class="mb-12">
                <h2 class="text-2xl font-bold mb-4 border-b pb-2">{title}</h2>
                {content}
            </section>
            """);

    static IHtmlContent AlertsDemo() =>
        Inlay.Template($"""
            <div class="flex flex-col gap-3">
                {Daisy.Alert("This is an informational message.", AlertVariant.Info)}
                {Daisy.Alert("Operation completed successfully!", AlertVariant.Success)}
                {Daisy.Alert("Please review before proceeding.", AlertVariant.Warning)}
                {Daisy.Alert("Something went wrong.", AlertVariant.Error)}
            </div>
            """);

    static IHtmlContent BadgesDemo() =>
        Inlay.Template($"""
            <div class="flex flex-wrap gap-2">
                {Daisy.Badge("default")}
                {Daisy.Badge("primary", BadgeVariant.Primary)}
                {Daisy.Badge("secondary", BadgeVariant.Secondary)}
                {Daisy.Badge("accent", BadgeVariant.Accent)}
                {Daisy.Badge("ghost", BadgeVariant.Ghost)}
                {Daisy.Badge("outline", BadgeVariant.Outline)}
                {Daisy.Badge("info", BadgeVariant.Info)}
                {Daisy.Badge("success", BadgeVariant.Success)}
                {Daisy.Badge("warning", BadgeVariant.Warning)}
                {Daisy.Badge("error", BadgeVariant.Error)}
                {Daisy.Badge("neutral", BadgeVariant.Neutral)}
            </div>
            """);

    static IHtmlContent ButtonsDemo() =>
        Inlay.Template($"""
            <div class="mb-4">
                <h3 class="font-semibold mb-2">Variants</h3>
                <div class="flex flex-wrap gap-2">
                    {Daisy.Button("Default")}
                    {Daisy.Button("Primary", ButtonVariant.Primary)}
                    {Daisy.Button("Secondary", ButtonVariant.Secondary)}
                    {Daisy.Button("Accent", ButtonVariant.Accent)}
                    {Daisy.Button("Ghost", ButtonVariant.Ghost)}
                    {Daisy.Button("Link", ButtonVariant.Link)}
                </div>
            </div>
            <div class="mb-4">
                <h3 class="font-semibold mb-2">Sizes</h3>
                <div class="flex flex-wrap items-center gap-2">
                    {Daisy.Button("Large", ButtonVariant.Primary, ButtonSize.Lg)}
                    {Daisy.Button("Default", ButtonVariant.Primary)}
                    {Daisy.Button("Small", ButtonVariant.Primary, ButtonSize.Sm)}
                    {Daisy.Button("Tiny", ButtonVariant.Primary, ButtonSize.Xs)}
                </div>
            </div>
            <div class="mb-4">
                <h3 class="font-semibold mb-2">Outline</h3>
                <div class="flex flex-wrap gap-2">
                    {Daisy.Button("Primary", ButtonVariant.Primary, outline: true)}
                    {Daisy.Button("Secondary", ButtonVariant.Secondary, outline: true)}
                    {Daisy.Button("Accent", ButtonVariant.Accent, outline: true)}
                </div>
            </div>
            <div>
                <h3 class="font-semibold mb-2">Link Button</h3>
                {Daisy.Button("Go to Home", ButtonVariant.Primary, href: "/")}
            </div>
            """);

    static IHtmlContent CardsDemo() =>
        Inlay.Template($"""
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
                {Daisy.Card(
                    title: "Simple Card",
                    body: Inlay.Template($"<p>A basic card with just a title and body text.</p>"))}
                {Daisy.Card(
                    title: "Card with Image",
                    body: Inlay.Template($"<p>Cards can include a figure image.</p>"),
                    imageUrl: "https://picsum.photos/seed/inlay1/400/200",
                    actions: Daisy.Button("Action", ButtonVariant.Primary))}
                {Daisy.Card(
                    title: "Bordered & Compact",
                    body: Inlay.Template($"<p>A compact, bordered variant.</p>"),
                    compact: true,
                    bordered: true)}
            </div>
            """);

    static IHtmlContent HeroDemo() =>
        Inlay.Template($"""
            <div class="rounded-box overflow-hidden">
                {Daisy.Hero(
                    "Build with Inlay",
                    "Type-safe HTML templates for .NET",
                    Daisy.Button("Get Started", ButtonVariant.Primary))}
            </div>
            """);

    static IHtmlContent ModalDemo() =>
        Inlay.Template($"""
            <div class="flex gap-2">
                {Daisy.ModalTrigger("demo_modal", "Open Modal", ButtonVariant.Primary)}
                {Daisy.Modal("demo_modal",
                    title: "Hello from Inlay!",
                    body: Inlay.Template($"""
                        <p>This modal is rendered entirely with <code>Daisy.Modal()</code> — a C# function call, not a template file.</p>
                        """),
                    actions: Daisy.Button("Confirm", ButtonVariant.Primary))}
            </div>
            """);

    static IHtmlContent StatsDemo() =>
        Inlay.Template($"""
            <div class="flex flex-col gap-4">
                <h3 class="font-semibold">Horizontal</h3>
                {Daisy.Stats([
                    new StatItem("Downloads", "31K", "Jan 1st - Jun 1st"),
                    new StatItem("New Users", "4,200", "↗︎ 400 (22%)"),
                    new StatItem("New Registers", "1,200", "↘︎ 90 (14%)"),
                ])}
                <h3 class="font-semibold">Vertical</h3>
                {Daisy.Stats([
                    new StatItem("Downloads", "31K"),
                    new StatItem("Users", "4,200"),
                ], vertical: true)}
            </div>
            """);

    static IHtmlContent TableDemo()
    {
        var users = new[]
        {
            (Name: "Alice Johnson", Email: "alice@example.com", Role: "Admin"),
            (Name: "Bob Smith", Email: "bob@example.com", Role: "Editor"),
            (Name: "Carol White", Email: "carol@example.com", Role: "Viewer"),
            (Name: "Dave Brown", Email: "dave@example.com", Role: "Editor"),
        };

        return Daisy.Table(
            ["Name", "Email", "Role"],
            users,
            u => $"""
                <tr>
                    <td>{u.Name}</td>
                    <td>{u.Email}</td>
                    <td>{Daisy.Badge(u.Role, u.Role == "Admin" ? BadgeVariant.Primary : BadgeVariant.Ghost)}</td>
                </tr>
                """,
            zebra: true);
    }

    static IHtmlContent ToastDemo() =>
        Inlay.Template($"""
            <p class="mb-4">Toasts position alerts in a fixed corner. Below is a static preview (not fixed-positioned in this demo):</p>
            <div class="relative h-32 bg-base-200 rounded-box overflow-hidden">
                <div class="toast toast-bottom toast-end" style="position: absolute;">
                    {Daisy.Alert("File saved.", AlertVariant.Success)}
                </div>
            </div>
            """);

    static IHtmlContent FooterDemo() =>
        Inlay.Template($"""
            <div class="rounded-box overflow-hidden">
                {Daisy.Footer(
                    new FooterSection("Company", [
                        new FooterLink("About", "/about"),
                        new FooterLink("Contact", "/contact"),
                        new FooterLink("Blog", "/blog"),
                    ]),
                    new FooterSection("Product", [
                        new FooterLink("Features", "/features"),
                        new FooterLink("Pricing", "/pricing"),
                        new FooterLink("Docs", "/docs"),
                    ]),
                    new FooterSection("Legal", [
                        new FooterLink("Terms", "/terms"),
                        new FooterLink("Privacy", "/privacy"),
                        new FooterLink("License", "/license"),
                    ]))}
            </div>
            """);

    static IHtmlContent TextInputsDemo() =>
        Inlay.Template($"""
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                    <h3 class="font-semibold mb-2">Types</h3>
                    <div class="flex flex-col gap-3">
                        {Daisy.TextInput(placeholder: "Text input", type: "text")}
                        {Daisy.TextInput(placeholder: "you@example.com", type: "email")}
                        {Daisy.TextInput(placeholder: "Password", type: "password")}
                        {Daisy.TextInput(placeholder: "Search...", type: "search")}
                        {Daisy.TextInput(placeholder: "0", type: "number")}
                    </div>
                </div>
                <div>
                    <h3 class="font-semibold mb-2">Variants</h3>
                    <div class="flex flex-col gap-3">
                        {Daisy.TextInput(placeholder: "Primary", variant: InputVariant.Primary)}
                        {Daisy.TextInput(placeholder: "Secondary", variant: InputVariant.Secondary)}
                        {Daisy.TextInput(placeholder: "Accent", variant: InputVariant.Accent)}
                        {Daisy.TextInput(placeholder: "Success", variant: InputVariant.Success)}
                        {Daisy.TextInput(placeholder: "Warning", variant: InputVariant.Warning)}
                        {Daisy.TextInput(placeholder: "Error", variant: InputVariant.Error)}
                    </div>
                </div>
            </div>
            <div class="mt-4">
                <h3 class="font-semibold mb-2">Sizes</h3>
                <div class="flex flex-col gap-3 max-w-md">
                    {Daisy.TextInput(placeholder: "Large", size: InputSize.Lg)}
                    {Daisy.TextInput(placeholder: "Default")}
                    {Daisy.TextInput(placeholder: "Small", size: InputSize.Sm)}
                    {Daisy.TextInput(placeholder: "Extra small", size: InputSize.Xs)}
                </div>
            </div>
            <div class="mt-4">
                <h3 class="font-semibold mb-2">Ghost</h3>
                {Daisy.TextInput(placeholder: "Ghost input (no background)", ghost: true, bordered: false)}
            </div>
            """);

    static IHtmlContent TextareaDemo() =>
        Inlay.Template($"""
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                {Daisy.Textarea(placeholder: "Default textarea", rows: 3)}
                {Daisy.Textarea(placeholder: "Primary variant", rows: 3, variant: InputVariant.Primary)}
            </div>
            """);

    static IHtmlContent SelectDemo() =>
        Inlay.Template($"""
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                    <h3 class="font-semibold mb-2">With placeholder</h3>
                    {Daisy.Select([
                        new SelectOption("react", "React"),
                        new SelectOption("vue", "Vue"),
                        new SelectOption("angular", "Angular"),
                        new SelectOption("svelte", "Svelte"),
                    ], placeholder: "Pick a framework")}
                </div>
                <div>
                    <h3 class="font-semibold mb-2">Variants</h3>
                    <div class="flex flex-col gap-3">
                        {Daisy.Select([new SelectOption("a", "Primary select")], variant: InputVariant.Primary)}
                        {Daisy.Select([new SelectOption("b", "Accent select")], variant: InputVariant.Accent)}
                    </div>
                </div>
            </div>
            """);

    static IHtmlContent CheckToggleDemo() =>
        Inlay.Template($"""
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                    <h3 class="font-semibold mb-2">Checkboxes</h3>
                    <div class="flex flex-col">
                        {Daisy.Checkbox(label: "Default checkbox", isChecked: true)}
                        {Daisy.Checkbox(label: "Primary", variant: InputVariant.Primary)}
                        {Daisy.Checkbox(label: "Secondary", variant: InputVariant.Secondary, isChecked: true)}
                        {Daisy.Checkbox(label: "Accent", variant: InputVariant.Accent)}
                        {Daisy.Checkbox(label: "Success", variant: InputVariant.Success, isChecked: true)}
                    </div>
                </div>
                <div>
                    <h3 class="font-semibold mb-2">Toggles</h3>
                    <div class="flex flex-col">
                        {Daisy.Toggle(label: "Default toggle")}
                        {Daisy.Toggle(label: "Primary", variant: InputVariant.Primary, isChecked: true)}
                        {Daisy.Toggle(label: "Secondary", variant: InputVariant.Secondary)}
                        {Daisy.Toggle(label: "Accent", variant: InputVariant.Accent, isChecked: true)}
                        {Daisy.Toggle(label: "Success", variant: InputVariant.Success)}
                    </div>
                </div>
            </div>
            """);

    static IHtmlContent RadioDemo() =>
        Inlay.Template($"""
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                    <h3 class="font-semibold mb-2">Individual radios</h3>
                    <div class="flex flex-col">
                        {Daisy.Radio("size", "sm", label: "Small", variant: InputVariant.Primary)}
                        {Daisy.Radio("size", "md", label: "Medium", isChecked: true, variant: InputVariant.Primary)}
                        {Daisy.Radio("size", "lg", label: "Large", variant: InputVariant.Primary)}
                    </div>
                </div>
                <div>
                    <h3 class="font-semibold mb-2">RadioGroup</h3>
                    <div class="flex flex-col">
                        {Daisy.RadioGroup("plan", [
                            new SelectOption("free", "Free", Selected: true),
                            new SelectOption("pro", "Pro"),
                            new SelectOption("enterprise", "Enterprise"),
                        ], variant: InputVariant.Secondary)}
                    </div>
                </div>
            </div>
            """);

    static IHtmlContent RangeDemo() =>
        Inlay.Template($"""
            <div class="flex flex-col gap-4 max-w-md">
                {Daisy.Range(value: 40)}
                {Daisy.Range(value: 70, variant: InputVariant.Primary)}
                {Daisy.Range(value: 25, step: 25, variant: InputVariant.Accent)}
                <div>
                    <h3 class="font-semibold mb-2">Sizes</h3>
                    <div class="flex flex-col gap-3">
                        {Daisy.Range(value: 50, size: InputSize.Lg)}
                        {Daisy.Range(value: 50)}
                        {Daisy.Range(value: 50, size: InputSize.Sm)}
                        {Daisy.Range(value: 50, size: InputSize.Xs)}
                    </div>
                </div>
            </div>
            """);

    static IHtmlContent FileInputDemo() =>
        Inlay.Template($"""
            <div class="flex flex-col gap-4 max-w-md">
                {Daisy.FileInput()}
                {Daisy.FileInput(variant: InputVariant.Primary)}
                {Daisy.FileInput(variant: InputVariant.Accent, accept: "image/*")}
                {Daisy.FileInput(size: InputSize.Sm)}
            </div>
            """);

    static IHtmlContent FormControlDemo() =>
        Inlay.Template($"""
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="flex flex-col gap-4">
                    {Daisy.FormControl(
                        Daisy.TextInput(name: "username", placeholder: "johndoe"),
                        label: "Username",
                        altLabel: "Required",
                        helperText: "Choose a unique username")}
                    {Daisy.FormControl(
                        Daisy.TextInput(name: "email", type: "email", placeholder: "you@example.com", variant: InputVariant.Primary),
                        label: "Email")}
                    {Daisy.FormControl(
                        Daisy.TextInput(name: "pwd", type: "password", placeholder: "********"),
                        label: "Password",
                        helperText: "At least 8 characters")}
                </div>
                <div class="flex flex-col gap-4">
                    {Daisy.FormControl(
                        Daisy.Select([
                            new SelectOption("dev", "Developer"),
                            new SelectOption("design", "Designer"),
                            new SelectOption("pm", "Product Manager"),
                        ], name: "role", placeholder: "Select your role"),
                        label: "Role")}
                    {Daisy.FormControl(
                        Daisy.Textarea(name: "notes", placeholder: "Any additional notes...", rows: 3),
                        label: "Notes",
                        altLabel: "Optional")}
                    {Daisy.FormControl(
                        Daisy.FileInput(name: "avatar", accept: "image/*"),
                        label: "Profile picture")}
                </div>
            </div>
            """);
}
