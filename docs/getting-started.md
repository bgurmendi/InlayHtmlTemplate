# Getting Started

## Installation

Install the NuGet package:

```bash
dotnet add package AspNetTemplates
```

Or via the Package Manager Console:

```powershell
Install-Package AspNetTemplates
```

## Basic Usage

Add the namespace:

```csharp
using AspNetTemplates;
```

Render HTML using C# string interpolation:

```csharp
var name = "World";
var html = Html.Template($"<h1>Hello, {name}!</h1>");
```

`Html.Template` accepts a `FormattableString` (a C# interpolated string `$"..."`), escapes each value based on its HTML context, and returns `IHtmlContent`. This means C# passes the template format and arguments separately, allowing the engine to inspect the HTML structure and escape each value appropriately.

## Composing Templates

Since `Html.Template` returns `IHtmlContent`, and `IHtmlContent` values are inserted without double-escaping, templates compose naturally:

```csharp
var header = Html.Template($"<h1>{title}</h1>");
var nav = Html.Template($"""<nav><a href="/">Home</a></nav>""");
var page = Html.Template($"<div>{nav}{header}<p>{body}</p></div>");
```

No wrapping, no special calls — just embed one template result inside another.

## Using in ASP.NET Core Controllers

```csharp
using AspNetTemplates;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var userName = GetCurrentUser();
        var html = Html.Template($"""
            <div class="welcome">
                <h1>Welcome, {userName}!</h1>
            </div>
            """);
        return Content(html.ToString(), "text/html");
    }
}
```

## Using in Minimal APIs

```csharp
using AspNetTemplates;

var app = WebApplication.CreateBuilder(args).Build();

app.MapGet("/", (HttpContext ctx) =>
{
    var name = ctx.Request.Query["name"].ToString();
    var html = Html.Template($"<h1>Hello, {name}!</h1>");
    return Results.Content(html.ToString(), "text/html");
});

app.Run();
```

## Next Steps

- Read the [API Reference](api-reference.md) for details on all contexts and behaviors
- Check out the [Examples](examples.md) for common patterns
- See the [Advanced Examples](advanced-examples.md) for conditionals, lists, and composition
- Look at the `samples/WebApp` project for a full ASP.NET Core MVC integration
