# API Reference

## Inlay (static class)

The primary API for rendering and composing HTML templates.

### `Inlay.Template(FormattableString formattable)`

Creates a deferred HTML template with context-aware escaping. Rendering happens on demand when the result is written to a response or converted to string.

**Parameters:**
- `formattable` — A C# interpolated string (`$"..."` or `$"""..."""`). Must be passed directly as an interpolated string, not as a pre-built `string`.

**Returns:** `InlayTemplate` — Implements `IHtmlContent` (for composition), `IActionResult` (for MVC controllers), and `IResult` (for minimal APIs). Can be returned directly from controller actions.

**Important:** The method signature uses `FormattableString`, which means you must pass an interpolated string literal directly. Assigning the interpolated string to a `string` variable first will lose the template structure:

```csharp
// Correct - FormattableString is preserved
var html = Inlay.Template($"<p>{userInput}</p>");

// Wrong - this becomes a regular string, no escaping happens
string template = $"<p>{userInput}</p>";
// Inlay.Template(template); // Won't compile
```

### `Inlay.Raw(string html)`

Wraps a pre-rendered HTML string as trusted `IHtmlContent`. Use for HTML coming from external sources (markdown renderers, sanitizers, etc.).

**Returns:** `IHtmlContent`

### `Inlay.If(bool condition, FormattableString content)`

Renders the template only when `condition` is `true`. Returns `HtmlString.Empty` otherwise.

**Returns:** `IHtmlContent`

### `Inlay.If(bool condition, FormattableString content, FormattableString fallback)`

Renders `content` when `true`, `fallback` when `false`.

**Returns:** `IHtmlContent`

### `Inlay.Css(params (string className, bool active)[] classes)`

Builds a space-separated CSS class string including only entries where `active` is `true`.

**Returns:** `string` — A plain string (not `IHtmlContent`), so the value goes through normal attribute escaping when used inside a template.

### `Inlay.Each<T>(IEnumerable<T> items, Func<T, FormattableString> template)`

Renders the template for each item in the collection and concatenates the results.

**Returns:** `IHtmlContent`

### `Inlay.Each<T>(IEnumerable<T> items, Func<T, FormattableString> template, FormattableString empty)`

Same as above, but renders the `empty` template when the collection has no elements. The collection is materialized once to check emptiness.

**Returns:** `IHtmlContent`

### `Inlay.Each<T>(IEnumerable<T> items, Func<T, int, FormattableString> template)`

Like `Each`, but the lambda also receives the zero-based index of each item.

**Returns:** `IHtmlContent`

### `Inlay.Each<T>(IEnumerable<T> items, Func<T, int, FormattableString> template, FormattableString empty)`

Index variant with empty-collection fallback.

**Returns:** `IHtmlContent`

## InlayTemplate (class)

The concrete type returned by `Inlay.Template`. Stores a `FormattableString` and defers rendering until needed. Nested templates render recursively onto the same `TextWriter` — no intermediate string allocations.

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

Low-level rendering engine. Returns `string` instead of `InlayTemplate`. Prefer `Inlay.Template` for application code.

### `SimpleHtmlTemplate.Render(FormattableString formattable)`

**Returns:** `string` — The rendered HTML as a plain string.

## HtmlContext (enum)

Represents the HTML context where an interpolated value appears.

| Value              | Description                                          | Behavior                          |
|--------------------|------------------------------------------------------|-----------------------------------|
| `Content`          | Inside element content (`<p>{value}</p>`)            | `HtmlEncode`                      |
| `Attribute`        | Inside an attribute (`class="{value}"`)              | `HtmlEncode` + quote escaping     |
| `UrlAttribute`     | Inside `href` or `src` (`href="{value}"`)            | `UrlEncode` + `javascript:` block |
| `BooleanAttribute` | Inside a boolean attribute (`disabled="{value}"`)    | Attribute rendered or omitted     |
| `Script`           | Inside a `<script>` tag (reserved for future use)    | `HtmlEncode` (default)            |

## HtmlContextAnalyzer

A character-by-character HTML parser that tracks the current context.

### Properties

- `CurrentContext` (`HtmlContext`) — The current HTML context based on characters processed so far.
- `CurrentAttribute` (`string`) — The name of the attribute currently being parsed (empty when outside an attribute).

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

### Boolean Attribute Context

When an interpolated value appears inside a recognized boolean attribute (`disabled={value}` or `disabled="{value}"`), the engine renders the attribute based on the value's truthiness instead of encoding the value as text:

- `bool true` → renders ` disabled` (attribute present, no value)
- `bool false` → renders nothing (attribute omitted entirely)
- Non-empty string (except `"false"`) → renders the attribute
- Empty string, `null`, or string `"false"` → omits the attribute

The engine removes the attribute name (and surrounding quotes if present) from the literal segments during analysis, so no stale `=""` fragments are left behind.

Both forms are supported — the unquoted form is recommended for clarity since the value is never rendered as text:

```csharp
// Recommended: unquoted — clearer that the value controls presence, not content
Inlay.Template($"""<input type="text" disabled={isDisabled} />""");

// Also valid: quoted — works identically
Inlay.Template($"""<input type="text" disabled="{isDisabled}" />""");
```

**Recognized boolean attributes (25):**

`allowfullscreen`, `async`, `autofocus`, `autoplay`, `checked`, `controls`, `default`, `defer`, `disabled`, `formnovalidate`, `hidden`, `inert`, `ismap`, `itemscope`, `loop`, `multiple`, `muted`, `nomodule`, `novalidate`, `open`, `playsinline`, `readonly`, `required`, `reversed`, `selected`

```csharp
// Multiple boolean attributes work independently
Inlay.Template($"""<input type="checkbox" checked={isChecked} disabled={isDisabled} />""");

// Non-boolean attributes like name, class, type are unaffected
Inlay.Template($"""<input type="text" name="{name}" disabled={isDisabled} class="{css}" />""");
```

### IHtmlContent Bypass
Objects implementing `Microsoft.AspNetCore.Html.IHtmlContent` are inserted without escaping by calling `WriteTo` on the same `TextWriter`. This is how template composition works — `InlayTemplate` implements `IHtmlContent`, so nested templates render recursively without intermediate strings:

```csharp
var inner = Inlay.Template($"<span>{name}</span>");
var outer = Inlay.Template($"<div>{inner}</div>");
// inner is IHtmlContent → inserted as-is, no double-escaping
```
