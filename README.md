# AspNetTemplates

Context-aware HTML template engine for ASP.NET Core that uses C# string interpolation (`FormattableString`) to automatically escape values based on their HTML context.

## Why?

Traditional HTML escaping applies the same encoding everywhere. But HTML has different contexts (element content, attributes, URLs) that require different escaping strategies. `AspNetTemplates` analyzes the template structure and applies the correct encoding automatically, preventing XSS without manual effort.

## Installation

```bash
dotnet add package AspNetTemplates
```

## Quick Start

```csharp
using AspNetTemplates;

var userName = "<script>alert('xss')</script>";
var html = SimpleHtmlTemplate.Render($"<h1>Hello, {userName}!</h1>");
// Output: <h1>Hello, &lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;!</h1>
```

## Context-Aware Escaping

The engine detects three HTML contexts and escapes accordingly:

### Element Content
```csharp
var text = "<b>bold</b>";
SimpleHtmlTemplate.Render($"<p>{text}</p>");
// Output: <p>&lt;b&gt;bold&lt;/b&gt;</p>
```

### Attribute Values
```csharp
var className = "foo\" onclick=\"alert(1)";
SimpleHtmlTemplate.Render($"<div class=\"{className}\">test</div>");
// Attribute is safely escaped, onclick injection is neutralized
```

### URL Attributes (href, src)
```csharp
var url = "javascript:alert(1)";
SimpleHtmlTemplate.Render($"<a href=\"{url}\">click</a>");
// javascript: URLs are blocked and replaced with #
```

## Passing Raw HTML

Values implementing `IHtmlContent` are inserted without escaping:

```csharp
using Microsoft.AspNetCore.Html;

var rawHtml = new HtmlString("<strong>trusted</strong>");
SimpleHtmlTemplate.Render($"<div>{rawHtml}</div>");
// Output: <div><strong>trusted</strong></div>
```

## Building

```bash
dotnet build
dotnet test
```

## License

[MIT](LICENSE)
