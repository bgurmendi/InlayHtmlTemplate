# Getting Started

## Installation

Install the NuGet package:

```bash
dotnet add package InlayHtmlTemplate
```

Or via the Package Manager Console:

```powershell
Install-Package InlayHtmlTemplate
```

## Basic Usage

Add the namespace:

```csharp
using InlayHtmlTemplate;
```

Render HTML using C# string interpolation:

```csharp
var name = "World";
var html = Inlay.Template($"<h1>Hello, {name}!</h1>");
```

`Inlay.Template` accepts a `FormattableString` (a C# interpolated string `$"..."`), escapes each value based on its HTML context, and returns `InlayTemplate`. This means C# passes the template format and arguments separately, allowing the engine to inspect the HTML structure and escape each value appropriately. `InlayTemplate` implements `IHtmlContent` (for composition), `IActionResult` (for MVC controllers), and `IResult` (for minimal APIs).

## Composing Templates

Since `Inlay.Template` returns `InlayTemplate` (which implements `IHtmlContent`), and `IHtmlContent` values are inserted without double-escaping, templates compose naturally:

```csharp
var header = Inlay.Template($"<h1>{title}</h1>");
var nav = Inlay.Template($"""<nav><a href="/">Home</a></nav>""");
var page = Inlay.Template($"<div>{nav}{header}<p>{body}</p></div>");
```

No wrapping, no special calls — just embed one template result inside another.

## Using in ASP.NET Core Controllers

`InlayTemplate` implements `IActionResult`, so you can return it directly:

```csharp
using InlayHtmlTemplate;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var userName = GetCurrentUser();
        return Inlay.Template($"""
            <div class="welcome">
                <h1>Welcome, {userName}!</h1>
            </div>
            """);
    }
}
```

## Using in Minimal APIs

`InlayTemplate` also implements `IResult`, so it works with minimal APIs too:

```csharp
using InlayHtmlTemplate;

var app = WebApplication.CreateBuilder(args).Build();

app.MapGet("/", (HttpContext ctx) =>
{
    var name = ctx.Request.Query["name"].ToString();
    return Inlay.Template($"<h1>Hello, {name}!</h1>");
});

app.Run();
```

## Next Steps

- Read the [API Reference](api-reference.md) for details on all contexts and behaviors
- Check out the [Examples](examples.md) for common patterns
- See the [Advanced Examples](advanced-examples.md) for conditionals, lists, and composition
- Look at the `samples/WebApp` project for a full ASP.NET Core MVC integration
