# API Reference

## Html (static class)

The primary API for rendering and composing HTML templates.

### `Html.Template(FormattableString formattable)`

Creates a deferred HTML template with context-aware escaping. Rendering happens on demand when the result is written to a response or converted to string.

**Parameters:**
- `formattable` — A C# interpolated string (`$"..."` or `$"""..."""`). Must be passed directly as an interpolated string, not as a pre-built `string`.

**Returns:** `HtmlTemplate` — Implements `IHtmlContent` (for composition), `IActionResult` (for MVC controllers), and `IResult` (for minimal APIs). Can be returned directly from controller actions.

**Important:** The method signature uses `FormattableString`, which means you must pass an interpolated string literal directly. Assigning the interpolated string to a `string` variable first will lose the template structure:

```csharp
// Correct - FormattableString is preserved
var html = Html.Template($"<p>{userInput}</p>");

// Wrong - this becomes a regular string, no escaping happens
string template = $"<p>{userInput}</p>";
// Html.Template(template); // Won't compile
```

### `Html.Raw(string html)`

Wraps a pre-rendered HTML string as trusted `IHtmlContent`. Use for HTML coming from external sources (markdown renderers, sanitizers, etc.).

**Returns:** `IHtmlContent`

### `Html.If(bool condition, FormattableString content)`

Renders the template only when `condition` is `true`. Returns `HtmlString.Empty` otherwise.

**Returns:** `IHtmlContent`

### `Html.If(bool condition, FormattableString content, FormattableString fallback)`

Renders `content` when `true`, `fallback` when `false`.

**Returns:** `IHtmlContent`

### `Html.Css(params (string className, bool active)[] classes)`

Builds a space-separated CSS class string including only entries where `active` is `true`.

**Returns:** `string` — A plain string (not `IHtmlContent`), so the value goes through normal attribute escaping when used inside a template.

### `Html.Each<T>(IEnumerable<T> items, Func<T, FormattableString> template)`

Renders the template for each item in the collection and concatenates the results.

**Returns:** `IHtmlContent`

### `Html.Each<T>(IEnumerable<T> items, Func<T, FormattableString> template, FormattableString empty)`

Same as above, but renders the `empty` template when the collection has no elements. The collection is materialized once to check emptiness.

**Returns:** `IHtmlContent`

### `Html.Each<T>(IEnumerable<T> items, Func<T, int, FormattableString> template)`

Like `Each`, but the lambda also receives the zero-based index of each item.

**Returns:** `IHtmlContent`

### `Html.Each<T>(IEnumerable<T> items, Func<T, int, FormattableString> template, FormattableString empty)`

Index variant with empty-collection fallback.

**Returns:** `IHtmlContent`

## HtmlTemplate (class)

The concrete type returned by `Html.Template`. Stores a `FormattableString` and defers rendering until needed. Nested templates render recursively onto the same `TextWriter` — no intermediate string allocations.

**Implements:** `IHtmlContent`, `IActionResult`, `IResult`

### Key methods

- `WriteTo(TextWriter, HtmlEncoder)` — Renders the template to a writer. Nested `IHtmlContent` args are rendered recursively onto the same writer.
- `ExecuteResultAsync(ActionContext)` — Writes directly to the HTTP response stream with `Content-Type: text/html; charset=utf-8`.
- `ExecuteAsync(HttpContext)` — Same, for minimal APIs.
- `ToString()` — Renders to a string (for cases where you need the raw HTML).

### Caching

The first time a given `$"..."` literal is rendered, its format string is analyzed to determine the literal segments and the HTML context of each argument slot. The result is cached by format string reference in a lock-free `ConcurrentDictionary`. Since format strings from interpolated literals are compiler constants with stable references, the cache lookup is an O(1) identity comparison — no string hashing or content comparison.

After warmup, every render reuses the cached analysis and only performs encoding and writing of the runtime argument values.

## SimpleHtmlTemplate

Low-level rendering engine. Returns `string` instead of `HtmlTemplate`. Prefer `Html.Template` for application code.

### `SimpleHtmlTemplate.Render(FormattableString formattable)`

**Returns:** `string` — The rendered HTML as a plain string.

## HtmlContext (enum)

Represents the HTML context where an interpolated value appears.

| Value          | Description                                      | Escaping Strategy               |
|----------------|--------------------------------------------------|---------------------------------|
| `Content`      | Inside element content (`<p>{value}</p>`)        | `HtmlEncode`                    |
| `Attribute`    | Inside an attribute (`class="{value}"`)          | `HtmlEncode` + quote escaping   |
| `UrlAttribute` | Inside `href` or `src` (`href="{value}"`)        | `UrlEncode` + `javascript:` block |
| `Script`       | Inside a `<script>` tag (reserved for future use)| `HtmlEncode` (default)          |

## HtmlContextAnalyzer

A character-by-character HTML parser that tracks the current context.

### Properties

- `CurrentContext` (`HtmlContext`) — The current HTML context based on characters processed so far.

### Methods

- `ProcessChar(char c)` — Feeds a character to the analyzer, updating the internal state.

This class is public and can be used independently if you need to analyze HTML context for other purposes.

## Escaping Behavior Details

### Content Context
HTML encoding via `HtmlEncoder.Default`. Characters like `<`, `>`, `&`, `"`, `'` are converted to their HTML entity equivalents.

### Attribute Context
Same HTML encoding as content — `HtmlEncoder.Default` handles both quote characters natively:
- `"` → `&quot;`
- `'` → `&#x27;`

This prevents attribute breakout attacks where a value like `foo" onclick="alert(1)` could inject new attributes.

### URL Attribute Context
URL encoding via `UrlEncoder.Default`, with an additional check: values starting with `javascript:` (case-insensitive) are replaced with `#` to prevent script injection through URL attributes.

### IHtmlContent Bypass
Objects implementing `Microsoft.AspNetCore.Html.IHtmlContent` are inserted without escaping by calling `WriteTo` on the same `TextWriter`. This is how template composition works — `HtmlTemplate` implements `IHtmlContent`, so nested templates render recursively without intermediate strings:

```csharp
var inner = Html.Template($"<span>{name}</span>");
var outer = Html.Template($"<div>{inner}</div>");
// inner is IHtmlContent → inserted as-is, no double-escaping
```
