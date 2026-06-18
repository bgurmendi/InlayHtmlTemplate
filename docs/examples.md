# Examples

## Building a Page Layout

```csharp
using AspNetTemplates;

public static class PageLayout
{
    public static string Render(string title, string bodyContent)
    {
        var body = new HtmlString(bodyContent);
        return SimpleHtmlTemplate.Render($@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>{title}</title>
            </head>
            <body>
                {body}
            </body>
            </html>
        ");
    }
}
```

## User Profile Card

```csharp
var name = user.Name;        // "Alice <Admin>"
var bio = user.Bio;          // "I love C# & .NET"
var avatar = user.AvatarUrl; // "https://example.com/alice.jpg"
var website = user.Website;  // "https://alice.dev"

var html = SimpleHtmlTemplate.Render($@"
    <div class=""profile-card"">
        <img src=""{avatar}"" alt=""{name}"" />
        <h2>{name}</h2>
        <p>{bio}</p>
        <a href=""{website}"">Website</a>
    </div>
");
```

Each value is escaped according to its position:
- `{name}` in the `alt` attribute: attribute-escaped
- `{name}` in `<h2>`: content-escaped (`<Admin>` becomes `&lt;Admin&gt;`)
- `{avatar}` in `src`: URL-encoded
- `{bio}` in `<p>`: content-escaped (`&` becomes `&amp;`)
- `{website}` in `href`: URL-encoded

## Composing Templates with IHtmlContent

```csharp
using Microsoft.AspNetCore.Html;

string RenderListItem(string text) =>
    SimpleHtmlTemplate.Render($"<li>{text}</li>");

string RenderList(IEnumerable<string> items)
{
    var listItems = string.Join("\n", items.Select(RenderListItem));
    var rawList = new HtmlString(listItems);
    return SimpleHtmlTemplate.Render($"<ul>{rawList}</ul>");
}

var html = RenderList(new[] { "First", "<script>xss</script>", "Third" });
// Each item is individually escaped, then composed safely
```

## Dynamic Table from Data

```csharp
string RenderRow(string name, string email, string role) =>
    SimpleHtmlTemplate.Render($@"
        <tr>
            <td>{name}</td>
            <td><a href=""mailto:{email}"">{email}</a></td>
            <td>{role}</td>
        </tr>
    ");

string RenderTable(IEnumerable<User> users)
{
    var rows = string.Join("\n", users.Select(u => RenderRow(u.Name, u.Email, u.Role)));
    var rawRows = new HtmlString(rows);
    return SimpleHtmlTemplate.Render($@"
        <table>
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Email</th>
                    <th>Role</th>
                </tr>
            </thead>
            <tbody>{rawRows}</tbody>
        </table>
    ");
}
```

## Form with CSRF Protection

```csharp
string RenderForm(string action, string csrfToken)
{
    return SimpleHtmlTemplate.Render($@"
        <form method=""post"" action=""{action}"">
            <input type=""hidden"" name=""__RequestVerificationToken"" value=""{csrfToken}"" />
            <label for=""name"">Name:</label>
            <input type=""text"" id=""name"" name=""name"" />
            <button type=""submit"">Submit</button>
        </form>
    ");
}
```

## Conditional Content

```csharp
string RenderAlert(string? message, string type = "info")
{
    if (message == null) return "";

    return SimpleHtmlTemplate.Render($@"
        <div class=""alert alert-{type}"" role=""alert"">
            {message}
        </div>
    ");
}
```

## Integration with ASP.NET Core Middleware

```csharp
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 404)
    {
        var path = context.Request.Path.ToString();
        var html = SimpleHtmlTemplate.Render($@"
            <h1>Page Not Found</h1>
            <p>The page <code>{path}</code> does not exist.</p>
            <a href=""/"">Go home</a>
        ");

        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(html);
    }
});
```
