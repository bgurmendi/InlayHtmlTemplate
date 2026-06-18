# Advanced Examples

## How template composition works

`Html.Template` accepts a `FormattableString` (a C# interpolated string `$"..."`), escapes each interpolated value according to its HTML context, and returns `IHtmlContent`.

The key to composition: **`IHtmlContent` values inside a template are inserted as-is, without escaping.** Since `Html.Template` returns `IHtmlContent`, templates compose naturally:

```csharp
var item = "Bread & butter";
var li = Html.Template($"<li>{item}</li>");
// li contains: <li>Bread &amp; butter</li>    ← already safe HTML, as IHtmlContent

var result = Html.Template($"<ul>{li}</ul>");
// result contains: <ul><li>Bread &amp; butter</li></ul>    ← no double-escaping
```

No wrapping, no `Html.Raw`, no `HtmlString` — just embed one template result inside another. This works because `Html.Template` returns `IHtmlContent`, and the engine knows not to re-escape it.

By contrast, a plain `string` is always escaped (safe by default):

```csharp
var userInput = "<b>bold</b>";
Html.Template($"<p>{userInput}</p>");
// <p>&lt;b&gt;bold&lt;/p&gt;    ← string is escaped
```

**That's the only rule:**
- `string` → always escaped
- `IHtmlContent` → always trusted

### Building reusable components

The natural pattern is functions that return `IHtmlContent`:

```csharp
IHtmlContent RenderBadge(string role) =>
    Html.Template($"""<span class="badge">{role}</span>""");

IHtmlContent RenderCard(string name, string role) =>
    Html.Template($"""<div class="card"><h3>{name}</h3>{RenderBadge(role)}</div>""");

// Nesting is automatic — RenderBadge returns IHtmlContent, so it composes
var html = Html.Template($"<section>{RenderCard("Alice", "Admin")}</section>");
```

### When you still need Html.Raw

`Html.Raw` is for HTML strings that come from **outside** the template system — a markdown renderer, a sanitizer, a database field with trusted HTML:

```csharp
var markdownHtml = markdownRenderer.ToHtml(userMarkdown);
var page = Html.Template($"<article>{Html.Raw(markdownHtml)}</article>");
```

For template-to-template composition, `Html.Template` is enough.


## Conditional fragments with `Html.If`

`Html.If` renders a template only when the condition is `true`. It returns `IHtmlContent`, composing into any outer template.

```csharp
var isAdmin = true;
var userName = "Alice";

var html = Html.Template($"""
    <div class="user-header">
        <span>{userName}</span>
        {Html.If(isAdmin, $"""<span class="badge badge-admin">Admin</span>""")}
    </div>
    """);
```

When `isAdmin` is `false`, the badge is simply absent — no empty tags, no wrapper divs.

### If / else with fallback

The two-argument overload provides an else branch:

```csharp
var isLoggedIn = false;
var userName = "Guest";

var nav = Html.If(isLoggedIn,
    $"<span>Welcome, {userName}</span>",
    $"""<a href="/login">Log in</a>""");

var html = Html.Template($"<nav>{nav}</nav>");
// <nav><a href="/login">Log in</a></nav>
```

### Nesting conditionals

Since `Html.If` returns `IHtmlContent`, you can nest them:

```csharp
var isLoggedIn = true;
var isAdmin = true;
var name = "Alice";

var header = Html.If(isLoggedIn,
    $"""
    <div class="toolbar">
        <span>{name}</span>
        {Html.If(isAdmin, $"<button>Admin Panel</button>")}
    </div>
    """,
    $"""<a href="/login">Log in</a>""");
```


## Class toggling with `Html.Css`

`Html.Css` builds a class string from a list of `(className, active)` tuples. Only classes where `active` is `true` are included.

```csharp
var isActive = true;
var isDisabled = false;
var hasError = true;

var html = Html.Template(
    $"""<button class="{Html.Css(
        ("btn", true),
        ("btn-primary", isActive),
        ("btn-disabled", isDisabled),
        ("btn-error", hasError)
    )}">Submit</button>""");

// <button class="btn btn-primary btn-error">Submit</button>
```

`Html.Css` returns a plain `string` (not `IHtmlContent`), so the value goes through normal attribute escaping. Since CSS class names don't contain special characters, this is transparent.

### Common patterns

Toggling a single class:

```csharp
var tab = "settings";
var currentTab = "settings";

var html = Html.Template(
    $"""<li class="{Html.Css(("nav-link", true), ("active", tab == currentTab))}">{tab}</li>""");
```

State-driven styling:

```csharp
var status = OrderStatus.Shipped;

var html = Html.Template(
    $"""<span class="{Html.Css(
        ("badge", true),
        ("badge-warning", status == OrderStatus.Pending),
        ("badge-info", status == OrderStatus.Shipped),
        ("badge-success", status == OrderStatus.Delivered)
    )}">{status}</span>""");
```


## Iterating lists with `Html.Each`

`Html.Each` renders a template for every item in a collection. It returns `IHtmlContent`, composing into any outer template.

```csharp
var fruits = new[] { "Apple", "Banana", "Cherry" };

var html = Html.Template(
    $"<ul>{Html.Each(fruits, fruit => $"<li>{fruit}</li>")}</ul>");

// <ul><li>Apple</li><li>Banana</li><li>Cherry</li></ul>
```

Escaping works per-item — if a value contains HTML, it's escaped individually:

```csharp
var items = new[] { "Safe text", "<img src=x onerror=alert(1)>" };

var html = Html.Template(
    $"<ul>{Html.Each(items, item => $"<li>{item}</li>")}</ul>");

// <ul><li>Safe text</li><li>&lt;img src=x onerror=alert(1)&gt;</li></ul>
```

### Objects with multiple fields

The lambda receives the full object, so you can use multiple properties:

```csharp
record Product(string Name, decimal Price, string Url);

var products = new[]
{
    new Product("Widget", 9.99m, "/products/widget"),
    new Product("Gadget", 24.50m, "/products/gadget"),
};

var html = Html.Template($"""
    <table>
        <thead><tr><th>Product</th><th>Price</th></tr></thead>
        <tbody>
            {Html.Each(products, p => $"""
                <tr>
                    <td><a href="{p.Url}">{p.Name}</a></td>
                    <td>${p.Price}</td>
                </tr>
            """)}
        </tbody>
    </table>
    """);
```

### Using the index

The overload with `(item, index)` is useful for zebra striping or numbered lists:

```csharp
var steps = new[] { "Mix ingredients", "Preheat oven", "Bake 25 min" };

var html = Html.Template(
    $"<ol>{Html.Each(steps, (step, i) =>
        $"""<li class="{Html.Css(("step", true), ("even", i % 2 == 0))}">{step}</li>""")}</ol>");
```


## Empty list fallback with `Html.Each`

The three-argument overload renders a fallback template when the collection is empty:

```csharp
var notifications = Array.Empty<string>();

var html = Html.Template($"""
    <div class="notification-panel">
        <h3>Notifications</h3>
        <ul>
            {Html.Each(notifications,
                n => $"<li>{n}</li>",
                $"""<li class="empty">You're all caught up!</li>""")}
        </ul>
    </div>
    """);

// The <ul> contains only: <li class="empty">You're all caught up!</li>
```

When items exist, the fallback is ignored completely:

```csharp
var notifications = new[] { "New message from Bob", "Server restarted" };

// Same template — now renders the two <li> items, fallback is never evaluated
```

### Combining fallback with other helpers

```csharp
record Task(string Title, bool Done);

var tasks = GetUserTasks(); // might be empty

var html = Html.Template($"""
    <div class="task-list">
        {Html.Each(tasks,
            t => $"""
                <div class="{Html.Css(("task", true), ("done", t.Done))}">
                    {Html.If(t.Done,
                        $"<s>{t.Title}</s>",
                        $"<span>{t.Title}</span>")}
                </div>
                """,
            $"""<p class="empty">No tasks yet. Enjoy your free time!</p>""")}
    </div>
    """);
```


## Complete example: user dashboard

This example combines all the helpers in a realistic scenario:

```csharp
record User(string Name, string Role, string AvatarUrl);
record Activity(string Description, DateTime When, bool IsRead);

IHtmlContent RenderDashboard(User user, IEnumerable<Activity> activities)
{
    var isAdmin = user.Role == "Admin";

    return Html.Template($"""
        <div class="dashboard">
            <header class="{Html.Css(("header", true), ("header-admin", isAdmin))}">
                <img src="{user.AvatarUrl}" alt="{user.Name}" />
                <h1>{user.Name}</h1>
                {Html.If(isAdmin, $"""<span class="role-badge">Admin</span>""")}
            </header>

            <section class="activity">
                <h2>Recent Activity</h2>
                <ul>
                    {Html.Each(activities,
                        a => $"""
                            <li class="{Html.Css(("activity-item", true), ("unread", !a.IsRead))}">
                                <span>{a.Description}</span>
                                <time>{a.When:yyyy-MM-dd}</time>
                            </li>
                            """,
                        $"""<li class="empty">No recent activity.</li>""")}
                </ul>
            </section>

            {Html.If(isAdmin, $"""
                <section class="admin-panel">
                    <h2>Admin Tools</h2>
                    <button>Manage Users</button>
                    <button>View Logs</button>
                </section>
                """)}
        </div>
        """);
}
```
