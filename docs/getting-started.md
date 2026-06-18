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
var html = SimpleHtmlTemplate.Render($"<h1>Hello, {name}!</h1>");
```

The key insight is that `SimpleHtmlTemplate.Render` accepts a `FormattableString`, not a regular `string`. This means C# passes the template format and arguments separately, allowing the engine to inspect the HTML structure and escape each value appropriately.

## Using in ASP.NET Core Controllers

```csharp
using AspNetTemplates;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var userName = GetCurrentUser();
        var html = SimpleHtmlTemplate.Render($@"
            <div class=""welcome"">
                <h1>Welcome, {userName}!</h1>
            </div>
        ");
        return Content(html, "text/html");
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
    var html = SimpleHtmlTemplate.Render($"<h1>Hello, {name}!</h1>");
    return Results.Content(html, "text/html");
});

app.Run();
```

## Next Steps

- Read the [API Reference](api-reference.md) for details on all contexts and behaviors
- Check out the [Examples](examples.md) for common patterns
- Look at the `samples/WebApp` project for a full ASP.NET Core MVC integration
