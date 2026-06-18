# API Reference

## Html (static class)

The primary API for rendering and composing HTML templates. All methods that produce HTML return `IHtmlContent`, so their results compose into outer templates without double-escaping.

### `Html.Template(FormattableString formattable)`

Renders an interpolated string as HTML with context-aware escaping.

**Parameters:**
- `formattable` — A C# interpolated string (`$"..."` or `$"""..."""`). Must be passed directly as an interpolated string, not as a pre-built `string`.

**Returns:** `IHtmlContent` — The rendered HTML. Composes directly into outer templates.

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

## SimpleHtmlTemplate

Low-level rendering engine used internally by `Html.Template`. Returns `string` instead of `IHtmlContent`. Prefer `Html.Template` for application code.

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
Standard HTML encoding via `WebUtility.HtmlEncode`. Characters like `<`, `>`, `&`, `"` are converted to their HTML entity equivalents.

### Attribute Context
HTML encoding plus additional escaping for quote characters:
- `"` → `&quot;`
- `'` → `&#39;`

This prevents attribute breakout attacks where a value like `foo" onclick="alert(1)` could inject new attributes.

### URL Attribute Context
URL encoding via `WebUtility.UrlEncode`, with an additional check: values starting with `javascript:` (case-insensitive) are replaced with `#` to prevent script injection through URL attributes.

### IHtmlContent Bypass
Objects implementing `Microsoft.AspNetCore.Html.IHtmlContent` are inserted without escaping. This is how template composition works — `Html.Template` returns `IHtmlContent`, so its result passes through without double-escaping when used inside another template:

```csharp
var inner = Html.Template($"<span>{name}</span>");
var outer = Html.Template($"<div>{inner}</div>");
// inner is IHtmlContent → inserted as-is, no double-escaping
```
