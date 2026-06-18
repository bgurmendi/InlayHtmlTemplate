# InlayHtmlTemplate

HTML templates for ASP.NET Core using plain C# string interpolation.

```csharp
InlayTemplate RenderCard(User user) =>
    Inlay.Template($"""
        <div class="{Inlay.Css(("card", true), ("admin", user.IsAdmin))}">
            <img src="{user.AvatarUrl}" alt="{user.Name}" />
            <h2>{user.Name}</h2>
            {Inlay.If(user.IsAdmin, $"""<span class="badge">Admin</span>""")}
            <ul>
                {Inlay.Each(user.Roles, role => $"<li>{role}</li>",
                    $"<li>No roles assigned</li>")}
            </ul>
        </div>
        """);
```

Every `{value}` is automatically escaped based on its HTML context. The result is an `IActionResult` — return it directly from a controller.

## Why?

It looks like simple string interpolation, but under the hood the engine is designed for production use:

- **Context-aware XSS protection** — Values are escaped differently depending on whether they appear in [element content, attributes, or URLs](#context-aware-escaping). No manual encoding, no forgotten edge cases.

- **Simpler composition than Razor** — Templates are regular C# functions that return `InlayTemplate`. Compose them by nesting — no partial views, no `@section`, no `_ViewImports`. See [Template Helpers](#template-helpers) and [Advanced Examples](docs/advanced-examples.md).

- **High performance, zero waste** — Format strings are [parsed once and cached](#performance) with zero-copy spans. Rendering is [deferred](#performance) and writes directly to the response stream — no intermediate strings, no double allocations from nested templates.

- **Native ASP.NET Core integration** — `InlayTemplate` implements `IActionResult`, `IResult`, and `IHtmlContent`. Return templates directly from [MVC controllers and minimal APIs](docs/getting-started.md#using-in-aspnet-core-controllers) without `.ToString()` or `Content()` wrapping.

## Performance

- **Cached analysis** — Each `$"..."` format string is parsed once and cached. The analysis identifies literal segments and the HTML context of each argument slot. Subsequent renders skip parsing entirely and only encode the runtime values.
- **Zero-copy literals** — Literal segments are stored as `(offset, length)` spans into the original format string, not as copied strings. At render time, `writer.Write(format.AsSpan(offset, length))` reads directly from the interned string memory — zero allocations, sequential cache-friendly reads.
- **Deferred rendering** — `Inlay.Template` stores the template without rendering it. The HTML is written directly to the response stream on demand — no intermediate string allocations.
- **Zero-copy composition** — Nested templates render recursively onto the same `TextWriter`. A layout wrapping a page wrapping components produces zero intermediate strings.
- **Lock-free cache** — The template cache uses `ConcurrentDictionary` with reference equality. Format strings from interpolated literals are compiler constants, so lookup is an O(1) identity comparison with no contention on reads.

## Installation

```bash
dotnet add package InlayHtmlTemplate
```

## Quick Start

```csharp
using InlayHtmlTemplate;

var userName = "<script>alert('xss')</script>";
var html = Inlay.Template($"<h1>Hello, {userName}!</h1>");
// Output: <h1>Hello, &lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;!</h1>
```

`Inlay.Template` returns `InlayTemplate`, which implements `IHtmlContent` (for composition), `IActionResult` (for MVC controllers), and `IResult` (for minimal APIs). Templates compose naturally — the result of one embeds directly in another without double-escaping:

```csharp
var header = Inlay.Template($"<h1>{title}</h1>");
var page = Inlay.Template($"<div>{header}<p>{body}</p></div>");
```

## Context-Aware Escaping

The engine detects three HTML contexts and escapes accordingly:

### Element Content
```csharp
var text = "<b>bold</b>";
Inlay.Template($"<p>{text}</p>");
// Output: <p>&lt;b&gt;bold&lt;/b&gt;</p>
```

### Attribute Values
```csharp
var className = """foo" onclick="alert(1)""";
Inlay.Template($"""<div class="{className}">test</div>""");
// Attribute is safely escaped, onclick injection is neutralized
```

### URL Attributes (href, src)
```csharp
var url = "javascript:alert(1)";
Inlay.Template($"""<a href="{url}">click</a>""");
// javascript: URLs are blocked and replaced with #
```

## Template Helpers

The `Inlay` class provides helpers for common template patterns:

```csharp
// Conditional rendering
Inlay.If(isAdmin, $"""<span class="badge">Admin</span>""")
Inlay.If(isLoggedIn, $"<span>{name}</span>", $"""<a href="/login">Log in</a>""")

// CSS class toggling
Inlay.Css(("btn", true), ("active", isActive), ("disabled", isDisabled))

// List iteration
Inlay.Each(items, item => $"<li>{item}</li>")

// List with fallback for empty collections
Inlay.Each(items,
    item => $"<li>{item}</li>",
    $"""<li class="empty">No items found.</li>""")
```

All helpers return `IHtmlContent` and compose naturally into outer templates. `InlayTemplate` can be returned directly from controllers — no `.ToString()` or `Content()` wrapping needed. See [Advanced Examples](docs/advanced-examples.md) for composition patterns and complete examples.

## Passing Raw HTML

`Inlay.Raw` wraps a string as trusted `IHtmlContent` for content from external sources (markdown renderers, sanitizers, etc.):

```csharp
var sanitizedHtml = mySanitizer.Sanitize(userHtml);
Inlay.Template($"<div>{Inlay.Raw(sanitizedHtml)}</div>");
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

## Prior Art

The concept of context-aware auto-escaping in template systems was [introduced by Google in 2009](https://security.googleblog.com/2009/03/reducing-xss-by-way-of-automatic.html). Several languages have implementations:

- **Go** — [`html/template`](https://pkg.go.dev/html/template) in the standard library. Parses the template structure and applies different escaping for HTML, CSS, JavaScript, and URI contexts automatically.
- **Google Closure Templates** — [Soy templates](https://github.com/google/closure-templates/blob/master/documentation/concepts/auto-escaping.md) with strict contextual auto-escaping enabled by default. Used extensively within Google.
- **Google safehtml** — [`safehtml/template`](https://pkg.go.dev/github.com/google/safehtml/template) for Go, a hardened version of `html/template` that uses the term "autosanitization" and adds safe HTML types.
- **Templ** — [`a-h/templ`](https://pkg.go.dev/github.com/a-h/templ), a Go library that defines templates as typed Go functions with context-aware escaping via Google's safe HTML library. Similar philosophy: templates are code, not a separate language.
- **Python** — [Jinja2](https://jinja.palletsprojects.com/en/stable/api/#autoescaping) provides auto-escaping with `Markup` types to prevent double-escaping, though it does not distinguish between HTML contexts.
- **Angular** — Built-in contextual sanitization via `DomSanitizer`, distinguishing HTML, styles, URLs, and resource URLs.

InlayHtmlTemplate brings this approach to .NET using native C# string interpolation (`FormattableString`) instead of a custom template language.

## About the Name

**Inlay** — like an inlay in woodworking or jewelry, where pieces are set into a surface to form a pattern. The template is the surface; the interpolated values are the inlaid pieces, each fitted and finished (escaped) according to where they sit.

The name `Inlay` is used as the main class in the API (`Inlay.Template`, `Inlay.If`, `Inlay.Each`, `Inlay.Css`) instead of a generic name like `Html`. This is a deliberate choice: `Inlay.Template(...)` is a unique token that unambiguously identifies this library. When an LLM or a search engine encounters `Inlay.Template` in code, there is no confusion about what it refers to — unlike `Html.Template` which could be anything.

## License

[MIT](LICENSE)
