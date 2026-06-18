# Examples

Common patterns for using `Html.Template`. For composition, conditionals, class toggling, and list iteration, see [Advanced Examples](advanced-examples.md).

## Building a Page Layout

```csharp
using AspNetTemplates;

public static class PageLayout
{
    public static HtmlTemplate Render(string title, IHtmlContent bodyContent)
    {
        return Html.Template($"""
            <!DOCTYPE html>
            <html>
            <head>
                <title>{title}</title>
            </head>
            <body>
                {bodyContent}
            </body>
            </html>
            """);
    }
}

// Usage:
var body = Html.Template($"<h1>Welcome, {userName}!</h1>");
var page = PageLayout.Render("Home", body);
```

## User Profile Card

```csharp
var name = user.Name;        // "Alice <Admin>"
var bio = user.Bio;          // "I love C# & .NET"
var avatar = user.AvatarUrl; // "https://example.com/alice.jpg"
var website = user.Website;  // "https://alice.dev"

var html = Html.Template($"""
    <div class="profile-card">
        <img src="{avatar}" alt="{name}" />
        <h2>{name}</h2>
        <p>{bio}</p>
        <a href="{website}">Website</a>
    </div>
    """);
```

Each value is escaped according to its position:
- `{name}` in the `alt` attribute: attribute-escaped
- `{name}` in `<h2>`: content-escaped (`<Admin>` becomes `&lt;Admin&gt;`)
- `{avatar}` in `src`: URL-encoded
- `{bio}` in `<p>`: content-escaped (`&` becomes `&amp;`)
- `{website}` in `href`: URL-encoded

## Composing Templates

Since `Html.Template` returns `HtmlTemplate`, the result of one template embeds directly in another — no wrapping needed:

```csharp
HtmlTemplate RenderListItem(string text) =>
    Html.Template($"<li>{text}</li>");

HtmlTemplate RenderList(IEnumerable<string> items)
{
    var listItems = Html.Each(items, item => $"<li>{item}</li>");
    return Html.Template($"<ul>{listItems}</ul>");
}

var html = RenderList(new[] { "First", "<script>xss</script>", "Third" });
// Each item is individually escaped, then composed safely
```

## Dynamic Table from Data

```csharp
HtmlTemplate RenderTable(IEnumerable<User> users) =>
    Html.Template($"""
        <table>
            <thead>
                <tr><th>Name</th><th>Email</th><th>Role</th></tr>
            </thead>
            <tbody>
                {Html.Each(users, u => $"""
                    <tr>
                        <td>{u.Name}</td>
                        <td><a href="mailto:{u.Email}">{u.Email}</a></td>
                        <td>{u.Role}</td>
                    </tr>
                """)}
            </tbody>
        </table>
        """);
```

## Form with CSRF Protection

```csharp
HtmlTemplate RenderForm(string action, string csrfToken) =>
    Html.Template($"""
        <form method="post" action="{action}">
            <input type="hidden" name="__RequestVerificationToken" value="{csrfToken}" />
            <label for="name">Name:</label>
            <input type="text" id="name" name="name" />
            <button type="submit">Submit</button>
        </form>
        """);
```

## Integration with ASP.NET Core Middleware

```csharp
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 404)
    {
        var path = context.Request.Path.ToString();
        var html = Html.Template($"""
            <h1>Page Not Found</h1>
            <p>The page <code>{path}</code> does not exist.</p>
            <a href="/">Go home</a>
            """);

        await html.ExecuteAsync(context);
    }
});
```
