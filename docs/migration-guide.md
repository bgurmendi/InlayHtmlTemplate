# Razor → Inlay Migration Guide

This guide helps developers familiar with Razor views translate their knowledge to InlayHtmlTemplate.

## Concept Mapping

| Razor Concept | Inlay Equivalent |
|---|---|
| `@model` directive | C# method parameter or `class` field |
| `@` expression | `{arg}` inside a `FormattableString` |
| `@:` / `<text>` | Plain string literals in `$""` |
| `@Html.Raw(...)` | `Inlay.Raw(...)` |
| `@if` / `@else` | `Inlay.If(condition, content, fallback?)` |
| `@foreach` | `Inlay.Each(items, item => $"...")` |
| `Layout` / `RenderBody()` | Function parameters / composition via `IHtmlContent` |
| `@section` | Named function parameters |
| `@Html.Partial` | Direct method call returning `IHtmlContent` |
| `@Html.DisplayFor` / `@Html.EditorFor` | `Grid.AddField(...)` (Components) |
| `_ViewImports` | `global using` directives |
| `Model.` | Direct variable access (no special keyword) |

## Step-by-Step Migration

### 1. Replace `@model` with a Method Parameter

```razor
@* Razor *@
@model Product
<h2>@Model.Name</h2>
<p>@Model.Price</p>
```

```csharp
// Inlay
string ProductTemplate(Product model) =>
    Inlay.Template($"<h2>{model.Name}</h2><p>{model.Price}</p>");
```

### 2. Replace Conditionals

```razor
@* Razor *@
@if (isLoggedIn)
{
    <span>Welcome, @userName!</span>
}
else
{
    <a href="/login">Log in</a>
}
```

```csharp
// Inlay
Inlay.If(isLoggedIn,
    $"<span>Welcome, {userName}!</span>",
    $"<a href=\"/login\">Log in</a>");
```

### 3. Replace Loops

```razor
@* Razor *@
<ul>
@foreach (var item in items)
{
    <li>@item.Name</li>
}
</ul>
```

```csharp
// Inlay
Inlay.Template($"<ul>{Inlay.Each(items, item => $"<li>{item.Name}</li>")}</ul>");
```

With empty fallback:

```csharp
Inlay.Template($"{Inlay.Each(items, item => $"<li>{item.Name}</li>",
    $"<li>No items found</li>")}");
```

### 4. Replace Layout with Function Composition

In Razor, `_Layout.cshtml` wraps child pages. In Inlay, you compose:

```csharp
// Layout function
IHtmlContent Layout(string title, IHtmlContent body)
{
    return Inlay.Template($"""
        <!DOCTYPE html>
        <html><head><title>{title}</title></head>
        <body>
            <header>My Site</header>
            <main>{body}</main>
            <footer>&copy; 2026</footer>
        </body></html>
        """);
}

// Page function — returns IHtmlContent for composability
IHtmlContent IndexPage(string name)
{
    var content = Inlay.Template($"<h1>Hello, {name}!</h1>");
    return Layout("Home", content);
}
```

### 5. Replace Sections with Named Parameters

Razor `@section` maps to named function parameters:

```csharp
IHtmlContent Page(string title, IHtmlContent body, IHtmlContent? sidebar = null)
{
    return Inlay.Template($"""
        <div class="layout">
            <div class="content">{body}</div>
            {sidebar ?? Inlay.Raw("")}
        </div>
        """);
}
```

### 6. Replace `@Html.Partial` with Direct Calls

Since templates are just C# methods, partials become direct function calls:

```csharp
// Before (Razor): @Html.Partial("_Header", Model)
// After (Inlay):
Header(model)
```

## ViewImports → Global Usings

```csharp
// _GlobalUsings.cs
global using InlayHtmlTemplate;
global using static InlayHtmlTemplate.Inlay;
```

## Changes to Project File

Remove any Razor-related packages and SDK references:

```xml
<!-- Remove from .csproj -->
<!--
<Project Sdk="Microsoft.NET.Sdk.Web">  →  <Project Sdk="Microsoft.NET.Sdk">
-->
```

Use `Microsoft.NET.Sdk` (not `Microsoft.NET.Sdk.Web`) unless you need web-specific features. Add a `FrameworkReference` only if required:

```xml
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```
