# AspNetTemplates

HTML templates for ASP.NET Core using plain C# string interpolation.

```csharp
HtmlTemplate RenderCard(User user) =>
    Html.Template($"""
        <div class="{Html.Css(("card", true), ("admin", user.IsAdmin))}">
            <img src="{user.AvatarUrl}" alt="{user.Name}" />
            <h2>{user.Name}</h2>
            {Html.If(user.IsAdmin, $"""<span class="badge">Admin</span>""")}
            <ul>
                {Html.Each(user.Roles, role => $"<li>{role}</li>",
                    $"<li>No roles assigned</li>")}
            </ul>
        </div>
        """);
```

Every `{value}` is automatically escaped based on its HTML context. The result is an `IActionResult` — return it directly from a controller.

## Why?

It looks like simple string interpolation, but under the hood the engine is designed for production use:

- **Context-aware XSS protection** — Values are escaped differently depending on whether they appear in [element content, attributes, or URLs](#context-aware-escaping). No manual encoding, no forgotten edge cases.

- **Simpler composition than Razor** — Templates are regular C# functions that return `HtmlTemplate`. Compose them by nesting — no partial views, no `@section`, no `_ViewImports`. See [Template Helpers](#template-helpers) and [Advanced Examples](docs/advanced-examples.md).

- **High performance, zero waste** — Format strings are [parsed once and cached](#performance) with zero-copy spans. Rendering is [deferred](#performance) and writes directly to the response stream — no intermediate strings, no double allocations from nested templates.

- **Native ASP.NET Core integration** — `HtmlTemplate` implements `IActionResult`, `IResult`, and `IHtmlContent`. Return templates directly from [MVC controllers and minimal APIs](docs/getting-started.md#using-in-aspnet-core-controllers) without `.ToString()` or `Content()` wrapping.

## Performance

- **Cached analysis** — Each `$"..."` format string is parsed once and cached. The analysis identifies literal segments and the HTML context of each argument slot. Subsequent renders skip parsing entirely and only encode the runtime values.
- **Zero-copy literals** — Literal segments are stored as `(offset, length)` spans into the original format string, not as copied strings. At render time, `writer.Write(format.AsSpan(offset, length))` reads directly from the interned string memory — zero allocations, sequential cache-friendly reads.
- **Deferred rendering** — `Html.Template` stores the template without rendering it. The HTML is written directly to the response stream on demand — no intermediate string allocations.
- **Zero-copy composition** — Nested templates render recursively onto the same `TextWriter`. A layout wrapping a page wrapping components produces zero intermediate strings.
- **Lock-free cache** — The template cache uses `ConcurrentDictionary` with reference equality. Format strings from interpolated literals are compiler constants, so lookup is an O(1) identity comparison with no contention on reads.

## Installation

```bash
dotnet add package AspNetTemplates
```

## Quick Start

```csharp
using AspNetTemplates;

var userName = "<script>alert('xss')</script>";
var html = Html.Template($"<h1>Hello, {userName}!</h1>");
// Output: <h1>Hello, &lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;!</h1>
```

`Html.Template` returns `HtmlTemplate`, which implements `IHtmlContent` (for composition), `IActionResult` (for MVC controllers), and `IResult` (for minimal APIs). Templates compose naturally — the result of one embeds directly in another without double-escaping:

```csharp
var header = Html.Template($"<h1>{title}</h1>");
var page = Html.Template($"<div>{header}<p>{body}</p></div>");
```

## Context-Aware Escaping

The engine detects three HTML contexts and escapes accordingly:

### Element Content
```csharp
var text = "<b>bold</b>";
Html.Template($"<p>{text}</p>");
// Output: <p>&lt;b&gt;bold&lt;/b&gt;</p>
```

### Attribute Values
```csharp
var className = """foo" onclick="alert(1)""";
Html.Template($"""<div class="{className}">test</div>""");
// Attribute is safely escaped, onclick injection is neutralized
```

### URL Attributes (href, src)
```csharp
var url = "javascript:alert(1)";
Html.Template($"""<a href="{url}">click</a>""");
// javascript: URLs are blocked and replaced with #
```

## Template Helpers

The `Html` class provides helpers for common template patterns:

```csharp
// Conditional rendering
Html.If(isAdmin, $"""<span class="badge">Admin</span>""")
Html.If(isLoggedIn, $"<span>{name}</span>", $"""<a href="/login">Log in</a>""")

// CSS class toggling
Html.Css(("btn", true), ("active", isActive), ("disabled", isDisabled))

// List iteration
Html.Each(items, item => $"<li>{item}</li>")

// List with fallback for empty collections
Html.Each(items,
    item => $"<li>{item}</li>",
    $"""<li class="empty">No items found.</li>""")
```

All helpers return `IHtmlContent` and compose naturally into outer templates. `HtmlTemplate` can be returned directly from controllers — no `.ToString()` or `Content()` wrapping needed. See [Advanced Examples](docs/advanced-examples.md) for composition patterns and complete examples.

## Passing Raw HTML

`Html.Raw` wraps a string as trusted `IHtmlContent` for content from external sources (markdown renderers, sanitizers, etc.):

```csharp
var sanitizedHtml = mySanitizer.Sanitize(userHtml);
Html.Template($"<div>{Html.Raw(sanitizedHtml)}</div>");
```

## Documentation

- [Getting Started](docs/getting-started.md) — Installation, basic usage, controllers, minimal APIs
- [Examples](docs/examples.md) — Layouts, profile cards, tables, forms, middleware
- [Advanced Examples](docs/advanced-examples.md) — Composition, conditionals, class toggling, list iteration, complete dashboard
- [API Reference](docs/api-reference.md) — All methods, escaping behavior, caching internals

## Building

```bash
dotnet build
dotnet test
```

## License

[MIT](LICENSE)
