# Examples

Common patterns for using `Inlay.Template`. For composition, conditionals, class toggling, and list iteration, see [Advanced Examples](advanced-examples.md).

## Building a Page Layout

```csharp
using InlayHtmlTemplate;

public static class PageLayout
{
    public static InlayTemplate Render(string title, IHtmlContent bodyContent)
    {
        return Inlay.Template($"""
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
var body = Inlay.Template($"<h1>Welcome, {userName}!</h1>");
var page = PageLayout.Render("Home", body);
```

## User Profile Card

```csharp
var name = user.Name;        // "Alice <Admin>"
var bio = user.Bio;          // "I love C# & .NET"
var avatar = user.AvatarUrl; // "https://example.com/alice.jpg"
var website = user.Website;  // "https://alice.dev"

var html = Inlay.Template($"""
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

Since `Inlay.Template` returns `InlayTemplate`, the result of one template embeds directly in another — no wrapping needed:

```csharp
InlayTemplate RenderListItem(string text) =>
    Inlay.Template($"<li>{text}</li>");

InlayTemplate RenderList(IEnumerable<string> items)
{
    var listItems = Inlay.Each(items, item => $"<li>{item}</li>");
    return Inlay.Template($"<ul>{listItems}</ul>");
}

var html = RenderList(new[] { "First", "<script>xss</script>", "Third" });
// Each item is individually escaped, then composed safely
```

## Dynamic Table from Data

```csharp
InlayTemplate RenderTable(IEnumerable<User> users) =>
    Inlay.Template($"""
        <table>
            <thead>
                <tr><th>Name</th><th>Email</th><th>Role</th></tr>
            </thead>
            <tbody>
                {Inlay.Each(users, u => $"""
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

## Form with Boolean Attributes

Boolean HTML attributes like `disabled`, `checked`, `selected`, and `required` are handled automatically. Write `disabled={condition}` and the engine renders the attribute when truthy, omits it when falsy:

```csharp
InlayTemplate RenderForm(string action, string csrfToken, bool isSubmitting) =>
    Inlay.Template($"""
        <form method="post" action="{action}">
            <input type="hidden" name="__RequestVerificationToken" value="{csrfToken}" />
            <label for="name">Name:</label>
            <input type="text" id="name" name="name" required={true} />
            <label>
                <input type="checkbox" name="terms" checked={false} />
                I accept the terms
            </label>
            <button type="submit" disabled={isSubmitting}>Submit</button>
        </form>
        """);

// When isSubmitting is false:
//   <button type="submit">Submit</button>
// When isSubmitting is true:
//   <button type="submit" disabled>Submit</button>
```

The quoted form `disabled="{isSubmitting}"` also works, but the unquoted form is recommended — it makes it clearer that the value controls the attribute's presence, not its content.

This works with any of the 25 standard boolean attributes (`disabled`, `checked`, `selected`, `required`, `readonly`, `hidden`, `open`, `autofocus`, `multiple`, etc.). Non-boolean attributes like `name`, `class`, and `type` continue to render their values normally.

## Integration with ASP.NET Core Middleware

```csharp
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 404)
    {
        var path = context.Request.Path.ToString();
        var html = Inlay.Template($"""
            <h1>Page Not Found</h1>
            <p>The page <code>{path}</code> does not exist.</p>
            <a href="/">Go home</a>
            """);

        await html.ExecuteAsync(context);
    }
});
```
