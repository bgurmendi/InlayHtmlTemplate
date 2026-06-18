# AspNetTemplates

Context-aware HTML template engine for ASP.NET Core that uses C# string interpolation (`FormattableString`) to automatically escape values based on their HTML context.

## Why?

Traditional HTML escaping applies the same encoding everywhere. But HTML has different contexts (element content, attributes, URLs) that require different escaping strategies. `AspNetTemplates` analyzes the template structure and applies the correct encoding automatically, preventing XSS without manual effort.

## Performance

- **Cached analysis** — Each `$"..."` format string is parsed once and cached. Subsequent renders skip parsing entirely and only encode the runtime values.
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

## Building

```bash
dotnet build
dotnet test
```

## License

[MIT](LICENSE)
