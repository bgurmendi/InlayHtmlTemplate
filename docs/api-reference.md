# API Reference

## SimpleHtmlTemplate

### `SimpleHtmlTemplate.Render(FormattableString formattable)`

Renders an HTML template with context-aware escaping.

**Parameters:**
- `formattable` — A C# interpolated string (`$"..."`). Must be passed directly as an interpolated string, not as a pre-built `string`.

**Returns:** `string` — The rendered HTML with all interpolated values escaped according to their context.

**Important:** The method signature uses `FormattableString`, which means you must pass an interpolated string literal directly. Assigning the interpolated string to a `string` variable first will lose the template structure:

```csharp
// Correct - FormattableString is preserved
var html = SimpleHtmlTemplate.Render($"<p>{userInput}</p>");

// Wrong - this becomes a regular string, no escaping happens
string template = $"<p>{userInput}</p>";
// var html = SimpleHtmlTemplate.Render(template); // Won't compile
```

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
Objects implementing `Microsoft.AspNetCore.Html.IHtmlContent` are inserted without escaping. Use `HtmlString` to pass pre-sanitized HTML:

```csharp
var trusted = new HtmlString("<strong>safe</strong>");
SimpleHtmlTemplate.Render($"<div>{trusted}</div>");
// Output: <div><strong>safe</strong></div>
```
