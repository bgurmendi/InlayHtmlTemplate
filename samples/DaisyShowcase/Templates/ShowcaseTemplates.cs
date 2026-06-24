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
                        {Daisy.Button("Browse Components", ButtonVariant.Primary, href: "/Showcase/Feedback")}
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
                        new StatItem("Components", "20", "Alerts, cards, modals..."),
                        new StatItem("Layouts", "2", "Base + sidebar"),
                        new StatItem("Dependencies", "1", "InlayHtmlTemplate"),
                    ])}
                </div>
            </div>
            """);

        return WithSidebar("Home", "Index", body);
    }

    // ── Feedback ──────────────────────────────────────────────

    public static InlayTemplate Feedback()
    {
        var body = Inlay.Template($"""
            <h1 class="text-4xl font-bold mb-8">Feedback</h1>

            {Demo("Alert",
                "Daisy.Alert(\"Operation completed!\", AlertVariant.Success)",
                Inlay.Template($"""
                    <div class="flex flex-col gap-3">
                        {Daisy.Alert("This is an informational message.", AlertVariant.Info)}
                        {Daisy.Alert("Operation completed successfully!", AlertVariant.Success)}
                        {Daisy.Alert("Please review before proceeding.", AlertVariant.Warning)}
                        {Daisy.Alert("Something went wrong.", AlertVariant.Error)}
                    </div>
                    """))}

            {Demo("Badge",
                "Daisy.Badge(\"v2.0\", BadgeVariant.Primary)",
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
                    """))}

            {Demo("Toast",
                "Daisy.Toast(content, ToastPosition.BottomEnd)",
                Inlay.Template($"""
                    <div class="relative h-32 bg-base-200 rounded-box overflow-hidden">
                        <div class="toast toast-bottom toast-end" style="position: absolute;">
                            {Daisy.Alert("File saved.", AlertVariant.Success)}
                        </div>
                    </div>
                    """))}
            """);

        return WithSidebar("Feedback", "Feedback", body);
    }

    // ── Actions ───────────────────────────────────────────────

    public static InlayTemplate Actions()
    {
        var body = Inlay.Template($"""
            <h1 class="text-4xl font-bold mb-8">Actions</h1>

            {Demo("Button — Variants",
                "Daisy.Button(\"Save\", ButtonVariant.Primary)",
                Inlay.Template($"""
                    <div class="flex flex-wrap gap-2">
                        {Daisy.Button("Default")}
                        {Daisy.Button("Primary", ButtonVariant.Primary)}
                        {Daisy.Button("Secondary", ButtonVariant.Secondary)}
                        {Daisy.Button("Accent", ButtonVariant.Accent)}
                        {Daisy.Button("Ghost", ButtonVariant.Ghost)}
                        {Daisy.Button("Link", ButtonVariant.Link)}
                    </div>
                    """))}

            {Demo("Button — Sizes",
                "Daisy.Button(\"Small\", ButtonVariant.Primary, ButtonSize.Sm)",
                Inlay.Template($"""
                    <div class="flex flex-wrap items-center gap-2">
                        {Daisy.Button("Large", ButtonVariant.Primary, ButtonSize.Lg)}
                        {Daisy.Button("Default", ButtonVariant.Primary)}
                        {Daisy.Button("Small", ButtonVariant.Primary, ButtonSize.Sm)}
                        {Daisy.Button("Tiny", ButtonVariant.Primary, ButtonSize.Xs)}
                    </div>
                    """))}

            {Demo("Button — Outline",
                "Daisy.Button(\"Save\", ButtonVariant.Primary, outline: true)",
                Inlay.Template($"""
                    <div class="flex flex-wrap gap-2">
                        {Daisy.Button("Primary", ButtonVariant.Primary, outline: true)}
                        {Daisy.Button("Secondary", ButtonVariant.Secondary, outline: true)}
                        {Daisy.Button("Accent", ButtonVariant.Accent, outline: true)}
                    </div>
                    """))}

            {Demo("Button — Link",
                "Daisy.Button(\"Go to Home\", ButtonVariant.Primary, href: \"/\")",
                Daisy.Button("Go to Home", ButtonVariant.Primary, href: "/"))}

            {Demo("Modal",
                "Daisy.Modal(\"id\", title: \"Title\", body: content)\nDaisy.ModalTrigger(\"id\", \"Open\")",
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
                    """))}
            """);

        return WithSidebar("Actions", "Actions", body);
    }

    // ── Data Display ──────────────────────────────────────────

    public static InlayTemplate DataDisplay()
    {
        var users = new[]
        {
            (Name: "Alice Johnson", Email: "alice@example.com", Role: "Admin"),
            (Name: "Bob Smith", Email: "bob@example.com", Role: "Editor"),
            (Name: "Carol White", Email: "carol@example.com", Role: "Viewer"),
            (Name: "Dave Brown", Email: "dave@example.com", Role: "Editor"),
        };

        var body = Inlay.Template($"""
            <h1 class="text-4xl font-bold mb-8">Data Display</h1>

            {Demo("Card",
                "Daisy.Card(title: \"Title\", body: content, imageUrl: \"...\")",
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
                    """))}

            {Demo("Stats",
                "Daisy.Stats([new StatItem(\"Users\", \"4,200\", \"+22%\")])",
                Inlay.Template($"""
                    {Daisy.Stats([
                        new StatItem("Downloads", "31K", "Jan 1st - Jun 1st"),
                        new StatItem("New Users", "4,200", "400 (22%)"),
                        new StatItem("New Registers", "1,200", "90 (14%)"),
                    ])}
                    <div class="mt-4">
                        {Daisy.Stats([
                            new StatItem("Downloads", "31K"),
                            new StatItem("Users", "4,200"),
                        ], vertical: true)}
                    </div>
                    """))}

            {Demo("Table",
                "Daisy.Table([\"Name\", \"Email\"], users, u => $\"<tr><td>{'{'}u.Name{'}'}</td></tr>\", zebra: true)",
                Daisy.Table(
                    ["Name", "Email", "Role"],
                    users,
                    u => $"""
                        <tr>
                            <td>{u.Name}</td>
                            <td>{u.Email}</td>
                            <td>{Daisy.Badge(u.Role, u.Role == "Admin" ? BadgeVariant.Primary : BadgeVariant.Ghost)}</td>
                        </tr>
                        """,
                    zebra: true))}
            """);

        return WithSidebar("Data Display", "DataDisplay", body);
    }

    // ── Navigation ────────────────────────────────────────────

    public static InlayTemplate Navigation()
    {
        var body = Inlay.Template($"""
            <h1 class="text-4xl font-bold mb-8">Navigation</h1>

            {Demo("Hero",
                "Daisy.Hero(\"Title\", \"Subtitle\", actions)",
                Inlay.Template($"""
                    <div class="rounded-box overflow-hidden">
                        {Daisy.Hero(
                            "Build with Inlay",
                            "Type-safe HTML templates for .NET",
                            Daisy.Button("Get Started", ButtonVariant.Primary))}
                    </div>
                    """))}

            {Demo("Navbar",
                "Daisy.Navbar(start: brand, end: links)",
                Inlay.Template($"""
                    <div class="rounded-box overflow-hidden">
                        {Daisy.Navbar(
                            start: Daisy.NavBrand("MyApp"),
                            end: Inlay.Template($"""
                                {Daisy.NavLink("Home", "/", isActive: true)}
                                {Daisy.NavLink("About", "/about")}
                                """))}
                    </div>
                    """))}

            {Demo("Footer",
                "Daisy.Footer(new FooterSection(\"Company\", [...]))",
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
                    """))}

            {Demo("Footer Simple",
                "Daisy.FooterSimple(\"© 2026 My Company\")",
                Inlay.Template($"""
                    <div class="rounded-box overflow-hidden">
                        {Daisy.FooterSimple("© 2026 My Company — All rights reserved.")}
                    </div>
                    """))}
            """);

        return WithSidebar("Navigation", "Navigation", body);
    }

    // ── Form Inputs ───────────────────────────────────────────

    public static InlayTemplate FormInputs()
    {
        var body = Inlay.Template($"""
            <h1 class="text-4xl font-bold mb-8">Form Inputs</h1>

            {Demo("TextInput — Types",
                "Daisy.TextInput(type: \"email\", placeholder: \"you@example.com\")",
                Inlay.Template($"""
                    <div class="flex flex-col gap-3 max-w-md">
                        {Daisy.TextInput(placeholder: "Text input", type: "text")}
                        {Daisy.TextInput(placeholder: "you@example.com", type: "email")}
                        {Daisy.TextInput(placeholder: "Password", type: "password")}
                        {Daisy.TextInput(placeholder: "Search...", type: "search")}
                        {Daisy.TextInput(placeholder: "0", type: "number")}
                    </div>
                    """))}

            {Demo("TextInput — Variants & Sizes",
                "Daisy.TextInput(variant: InputVariant.Primary, size: InputSize.Sm)",
                Inlay.Template($"""
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div class="flex flex-col gap-3">
                            {Daisy.TextInput(placeholder: "Primary", variant: InputVariant.Primary)}
                            {Daisy.TextInput(placeholder: "Success", variant: InputVariant.Success)}
                            {Daisy.TextInput(placeholder: "Error", variant: InputVariant.Error)}
                        </div>
                        <div class="flex flex-col gap-3">
                            {Daisy.TextInput(placeholder: "Large", size: InputSize.Lg)}
                            {Daisy.TextInput(placeholder: "Default")}
                            {Daisy.TextInput(placeholder: "Small", size: InputSize.Sm)}
                            {Daisy.TextInput(placeholder: "Extra small", size: InputSize.Xs)}
                        </div>
                    </div>
                    """))}

            {Demo("Textarea",
                "Daisy.Textarea(placeholder: \"Your message\", rows: 3, variant: InputVariant.Primary)",
                Inlay.Template($"""
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {Daisy.Textarea(placeholder: "Default textarea", rows: 3)}
                        {Daisy.Textarea(placeholder: "Primary variant", rows: 3, variant: InputVariant.Primary)}
                    </div>
                    """))}

            {Demo("Select",
                "Daisy.Select([new SelectOption(\"es\", \"Spain\")], placeholder: \"Pick one\")",
                Inlay.Template($"""
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {Daisy.Select([
                            new SelectOption("react", "React"),
                            new SelectOption("vue", "Vue"),
                            new SelectOption("angular", "Angular"),
                            new SelectOption("svelte", "Svelte"),
                        ], placeholder: "Pick a framework")}
                        {Daisy.Select([new SelectOption("a", "Accent select")], variant: InputVariant.Accent)}
                    </div>
                    """))}

            {Demo("FileInput",
                "Daisy.FileInput(accept: \"image/*\", variant: InputVariant.Primary)",
                Inlay.Template($"""
                    <div class="flex flex-col gap-4 max-w-md">
                        {Daisy.FileInput()}
                        {Daisy.FileInput(variant: InputVariant.Primary)}
                        {Daisy.FileInput(variant: InputVariant.Accent, accept: "image/*")}
                    </div>
                    """))}

            {Demo("FormControl",
                "Daisy.FormControl(Daisy.TextInput(...), label: \"Email\", helperText: \"We won't share it\")",
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
                    """))}
            """);

        return WithSidebar("Form Inputs", "FormInputs", body);
    }

    // ── Form Controls ─────────────────────────────────────────

    public static InlayTemplate FormControls()
    {
        var body = Inlay.Template($"""
            <h1 class="text-4xl font-bold mb-8">Form Controls</h1>

            {Demo("Checkbox",
                "Daisy.Checkbox(label: \"Accept terms\", variant: InputVariant.Primary, isChecked: true)",
                Inlay.Template($"""
                    <div class="flex flex-col max-w-sm">
                        {Daisy.Checkbox(label: "Default checkbox", isChecked: true)}
                        {Daisy.Checkbox(label: "Primary", variant: InputVariant.Primary)}
                        {Daisy.Checkbox(label: "Secondary", variant: InputVariant.Secondary, isChecked: true)}
                        {Daisy.Checkbox(label: "Accent", variant: InputVariant.Accent)}
                        {Daisy.Checkbox(label: "Success", variant: InputVariant.Success, isChecked: true)}
                    </div>
                    """))}

            {Demo("Toggle",
                "Daisy.Toggle(label: \"Dark mode\", variant: InputVariant.Primary, isChecked: true)",
                Inlay.Template($"""
                    <div class="flex flex-col max-w-sm">
                        {Daisy.Toggle(label: "Default toggle")}
                        {Daisy.Toggle(label: "Primary", variant: InputVariant.Primary, isChecked: true)}
                        {Daisy.Toggle(label: "Secondary", variant: InputVariant.Secondary)}
                        {Daisy.Toggle(label: "Accent", variant: InputVariant.Accent, isChecked: true)}
                        {Daisy.Toggle(label: "Success", variant: InputVariant.Success)}
                    </div>
                    """))}

            {Demo("Radio",
                "Daisy.Radio(\"size\", \"md\", label: \"Medium\", isChecked: true)",
                Inlay.Template($"""
                    <div class="flex flex-col max-w-sm">
                        {Daisy.Radio("size", "sm", label: "Small", variant: InputVariant.Primary)}
                        {Daisy.Radio("size", "md", label: "Medium", isChecked: true, variant: InputVariant.Primary)}
                        {Daisy.Radio("size", "lg", label: "Large", variant: InputVariant.Primary)}
                    </div>
                    """))}

            {Demo("RadioGroup",
                "Daisy.RadioGroup(\"plan\", [new SelectOption(\"free\", \"Free\", Selected: true), ...])",
                Inlay.Template($"""
                    <div class="flex flex-col max-w-sm">
                        {Daisy.RadioGroup("plan", [
                            new SelectOption("free", "Free", Selected: true),
                            new SelectOption("pro", "Pro"),
                            new SelectOption("enterprise", "Enterprise"),
                        ], variant: InputVariant.Secondary)}
                    </div>
                    """))}

            {Demo("Range",
                "Daisy.Range(value: 70, variant: InputVariant.Primary, step: 25)",
                Inlay.Template($"""
                    <div class="flex flex-col gap-4 max-w-md">
                        {Daisy.Range(value: 40)}
                        {Daisy.Range(value: 70, variant: InputVariant.Primary)}
                        {Daisy.Range(value: 25, step: 25, variant: InputVariant.Accent)}
                    </div>
                    """))}
            """);

        return WithSidebar("Form Controls", "FormControls", body);
    }

    // ── Layouts ────────────────────────────────────────────────

    public static InlayTemplate Layouts()
    {
        var body = Inlay.Template($"""
            <h1 class="text-4xl font-bold mb-8">Layouts</h1>

            {Demo("Sidebar Layout",
                "DaisySidebarLayout.Render(\"Title\", sidebar, body, navbar: ..., theme: \"nord\")",
                Inlay.Template($"""
                    <p>This page itself uses <code>DaisySidebarLayout</code>. The sidebar collapses into a drawer on mobile and stays pinned on large screens.</p>
                    """))}

            {Demo("Base Layout",
                "DaisyLayout.Render(\"Title\", body, theme: \"light\")",
                Inlay.Template($"""
                    <p><code>DaisyLayout</code> provides the HTML skeleton with DaisyUI CDN. Use it when you don't need a sidebar.</p>
                    """))}

            {Demo("Themes",
                "DaisyLayout.Render(\"Title\", body, theme: \"dracula\")",
                Inlay.Template($"""
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

    // ── Shared helpers ────────────────────────────────────────

    static InlayTemplate WithSidebar(string title, string activePage, IHtmlContent body)
    {
        var sidebar = Inlay.Template($"""
            <div class="text-xl font-bold mb-4 p-2">DaisyUI Showcase</div>
            <ul class="menu">
                <li><a class="{Inlay.Css(("active", activePage == "Index"))}" href="/">Home</a></li>
                <li class="menu-title">Components</li>
                <li><a class="{Inlay.Css(("active", activePage == "Feedback"))}" href="/Showcase/Feedback">Feedback</a></li>
                <li><a class="{Inlay.Css(("active", activePage == "Actions"))}" href="/Showcase/Actions">Actions</a></li>
                <li><a class="{Inlay.Css(("active", activePage == "DataDisplay"))}" href="/Showcase/DataDisplay">Data Display</a></li>
                <li><a class="{Inlay.Css(("active", activePage == "Navigation"))}" href="/Showcase/Navigation">Navigation</a></li>
                <li class="menu-title">Forms</li>
                <li><a class="{Inlay.Css(("active", activePage == "FormInputs"))}" href="/Showcase/FormInputs">Inputs</a></li>
                <li><a class="{Inlay.Css(("active", activePage == "FormControls"))}" href="/Showcase/FormControls">Controls</a></li>
                <li class="menu-title">Page</li>
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

    static IHtmlContent Demo(string title, string code, IHtmlContent rendered) =>
        Inlay.Template($"""
            <section class="mb-12">
                <h2 class="text-2xl font-bold mb-4 border-b pb-2">{title}</h2>
                <div class="mb-4">{rendered}</div>
                <pre class="bg-base-200 p-4 rounded-lg overflow-x-auto text-sm"><code>{code}</code></pre>
            </section>
            """);
}
