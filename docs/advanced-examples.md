# Advanced Examples

Template composition, conditionals, class toggling, and list iteration. For basic usage patterns (layouts, forms, tables), see [Examples](examples.md).

## How template composition works

`Inlay.Template` accepts a `FormattableString` (a C# interpolated string `$"..."`), escapes each interpolated value according to its HTML context, and returns `InlayTemplate`.

The key to composition: **`IHtmlContent` values inside a template are inserted as-is, without escaping.** Since `Inlay.Template` returns `InlayTemplate` (which implements `IHtmlContent`), templates compose naturally:

```csharp
var item = "Bread & butter";
var li = Inlay.Template($"<li>{item}</li>");
// li contains: <li>Bread &amp; butter</li>    ← already safe HTML, as IHtmlContent

var result = Inlay.Template($"<ul>{li}</ul>");
// result contains: <ul><li>Bread &amp; butter</li></ul>    ← no double-escaping
```

No wrapping, no `Inlay.Raw`, no `HtmlString` — just embed one template result inside another. This works because `InlayTemplate` implements `IHtmlContent`, and the engine knows not to re-escape it.

By contrast, a plain `string` is always escaped (safe by default):

```csharp
var userInput = "<b>bold</b>";
Inlay.Template($"<p>{userInput}</p>");
// <p>&lt;b&gt;bold&lt;/p&gt;    ← string is escaped
```

**That's the only rule:**
- `string` → always escaped
- `IHtmlContent` → always trusted

### Building reusable components

The natural pattern is functions that return `InlayTemplate`:

```csharp
InlayTemplate RenderBadge(string role) =>
    Inlay.Template($"""<span class="badge">{role}</span>""");

InlayTemplate RenderCard(string name, string role) =>
    Inlay.Template($"""<div class="card"><h3>{name}</h3>{RenderBadge(role)}</div>""");

// Nesting is automatic — RenderBadge returns InlayTemplate (IHtmlContent), so it composes
var html = Inlay.Template($"<section>{RenderCard("Alice", "Admin")}</section>");
```

### When you still need Inlay.Raw

`Inlay.Raw` is for HTML strings that come from **outside** the template system — a markdown renderer, a sanitizer, a database field with trusted HTML:

```csharp
var markdownHtml = markdownRenderer.ToHtml(userMarkdown);
var page = Inlay.Template($"<article>{Inlay.Raw(markdownHtml)}</article>");
```

For template-to-template composition, `Inlay.Template` is enough.


## Conditional fragments with `Inlay.If`

`Inlay.If` renders a template only when the condition is `true`. It returns `IHtmlContent`, composing into any outer template.

```csharp
var isAdmin = true;
var userName = "Alice";

var html = Inlay.Template($"""
    <div class="user-header">
        <span>{userName}</span>
        {Inlay.If(isAdmin, $"""<span class="badge badge-admin">Admin</span>""")}
    </div>
    """);
```

When `isAdmin` is `false`, the badge is simply absent — no empty tags, no wrapper divs.

### If / else with fallback

The two-argument overload provides an else branch:

```csharp
var isLoggedIn = false;
var userName = "Guest";

var nav = Inlay.If(isLoggedIn,
    $"<span>Welcome, {userName}</span>",
    $"""<a href="/login">Log in</a>""");

var html = Inlay.Template($"<nav>{nav}</nav>");
// <nav><a href="/login">Log in</a></nav>
```

### Nesting conditionals

Since `Inlay.If` returns `IHtmlContent`, you can nest them:

```csharp
var isLoggedIn = true;
var isAdmin = true;
var name = "Alice";

var header = Inlay.If(isLoggedIn,
    $"""
    <div class="toolbar">
        <span>{name}</span>
        {Inlay.If(isAdmin, $"<button>Admin Panel</button>")}
    </div>
    """,
    $"""<a href="/login">Log in</a>""");
```


## Boolean attributes

HTML boolean attributes (`disabled`, `checked`, `selected`, `required`, `readonly`, `hidden`, `open`, etc.) work naturally in templates — the engine detects them and renders the attribute only when the value is truthy:

```csharp
InlayTemplate RenderToggle(string name, string label, bool isChecked, bool isDisabled = false) =>
    Inlay.Template($"""
        <label>
            <input type="checkbox" name="{name}" checked={isChecked} disabled={isDisabled} />
            {label}
        </label>
        """);

var html = RenderToggle("notifications", "Email notifications", isChecked: true);
// <label><input type="checkbox" name="notifications" checked /> Email notifications</label>

var html2 = RenderToggle("sms", "SMS alerts", isChecked: false, isDisabled: true);
// <label><input type="checkbox" name="sms" disabled /> SMS alerts</label>
```

Both `disabled={value}` and `disabled="{value}"` are supported. The unquoted form is recommended for clarity — it makes it obvious that the value controls the attribute's presence, not its content.

This is especially useful in reusable components — you can forward boolean parameters directly into the template without wrapping them in `Inlay.If`:

```csharp
InlayTemplate RenderSelect(string name, IEnumerable<(string Value, string Label, bool Selected)> options) =>
    Inlay.Template($"""
        <select name="{name}">
            {Inlay.Each(options, o =>
                $"""<option value="{o.Value}" selected={o.Selected}>{o.Label}</option>""")}
        </select>
        """);
```

### Truthiness rules

| Value | Renders attribute? |
|---|---|
| `bool true` | Yes |
| `bool false` | No |
| Non-empty string (except `"false"`) | Yes |
| Empty string `""` | No |
| `null` | No |
| Any other non-null object | Yes |


## Class toggling with `Inlay.Css`

`Inlay.Css` builds a class string from a list of `(className, active)` tuples. Only classes where `active` is `true` are included.

```csharp
var isActive = true;
var isDisabled = false;
var hasError = true;

var html = Inlay.Template(
    $"""<button class="{Inlay.Css(
        ("btn", true),
        ("btn-primary", isActive),
        ("btn-disabled", isDisabled),
        ("btn-error", hasError)
    )}">Submit</button>""");

// <button class="btn btn-primary btn-error">Submit</button>
```

`Inlay.Css` returns a plain `string` (not `IHtmlContent`), so the value goes through normal attribute escaping. Since CSS class names don't contain special characters, this is transparent.

### Common patterns

Toggling a single class:

```csharp
var tab = "settings";
var currentTab = "settings";

var html = Inlay.Template(
    $"""<li class="{Inlay.Css(("nav-link", true), ("active", tab == currentTab))}">{tab}</li>""");
```

State-driven styling:

```csharp
var status = OrderStatus.Shipped;

var html = Inlay.Template(
    $"""<span class="{Inlay.Css(
        ("badge", true),
        ("badge-warning", status == OrderStatus.Pending),
        ("badge-info", status == OrderStatus.Shipped),
        ("badge-success", status == OrderStatus.Delivered)
    )}">{status}</span>""");
```


## Iterating lists with `Inlay.Each`

`Inlay.Each` renders a template for every item in a collection. It returns `IHtmlContent`, composing into any outer template.

```csharp
var fruits = new[] { "Apple", "Banana", "Cherry" };

var html = Inlay.Template(
    $"<ul>{Inlay.Each(fruits, fruit => $"<li>{fruit}</li>")}</ul>");

// <ul><li>Apple</li><li>Banana</li><li>Cherry</li></ul>
```

Escaping works per-item — if a value contains HTML, it's escaped individually:

```csharp
var items = new[] { "Safe text", "<img src=x onerror=alert(1)>" };

var html = Inlay.Template(
    $"<ul>{Inlay.Each(items, item => $"<li>{item}</li>")}</ul>");

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

var html = Inlay.Template($"""
    <table>
        <thead><tr><th>Product</th><th>Price</th></tr></thead>
        <tbody>
            {Inlay.Each(products, p => $"""
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

var html = Inlay.Template(
    $"<ol>{Inlay.Each(steps, (step, i) =>
        $"""<li class="{Inlay.Css(("step", true), ("even", i % 2 == 0))}">{step}</li>""")}</ol>");
```


## Empty list fallback with `Inlay.Each`

The three-argument overload renders a fallback template when the collection is empty:

```csharp
var notifications = Array.Empty<string>();

var html = Inlay.Template($"""
    <div class="notification-panel">
        <h3>Notifications</h3>
        <ul>
            {Inlay.Each(notifications,
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

var html = Inlay.Template($"""
    <div class="task-list">
        {Inlay.Each(tasks,
            t => $"""
                <div class="{Inlay.Css(("task", true), ("done", t.Done))}">
                    {Inlay.If(t.Done,
                        $"<s>{t.Title}</s>",
                        $"<span>{t.Title}</span>")}
                </div>
                """,
            $"""<p class="empty">No tasks yet. Enjoy your free time!</p>""")}
    </div>
    """);
```


## Passing content blocks to a template

A template function can receive an `IHtmlContent` parameter and embed it with `{body}`. The caller builds that body with its own `Inlay.Template` call — which can be as complex as needed. This is how layout/content composition works in practice.

### Layout with a full page body

A layout function wraps its content with the HTML shell:

```csharp
InlayTemplate Layout(string title, IHtmlContent body) =>
    Inlay.Template($"""
        <!DOCTYPE html>
        <html>
        <head><title>{title}</title></head>
        <body>
            <nav>
                <a href="/">Home</a>
                <a href="/products">Products</a>
            </nav>
            <main>{body}</main>
            <footer><p>&copy; 2026 My Store</p></footer>
        </body>
        </html>
        """);
```

The page function builds a complex body and passes it to the layout. The body itself uses helpers, loops, conditionals — everything composes because it all returns `IHtmlContent`:

```csharp
record Product(string Name, decimal Price, string ImageUrl, bool InStock);

InlayTemplate ProductPage(IEnumerable<Product> products, string? search)
{
    var body = Inlay.Template($"""
        <h1>Products</h1>

        {Inlay.If(search != null, $"""
            <p class="search-info">Showing results for: <strong>{search}</strong></p>
            """)}

        <div class="product-grid">
            {Inlay.Each(products, p => $"""
                <div class="{Inlay.Css(("product", true), ("out-of-stock", !p.InStock))}">
                    <img src="{p.ImageUrl}" alt="{p.Name}" />
                    <h3>{p.Name}</h3>
                    <span class="price">${p.Price}</span>
                    {Inlay.If(p.InStock,
                        $"""<button>Add to cart</button>""",
                        $"""<span class="sold-out">Sold out</span>""")}
                </div>
                """,
                $"""<p class="empty">No products found.</p>""")}
        </div>
        """);

    return Layout("Products", body);
}
```

The controller returns it directly — `InlayTemplate` is an `IActionResult`:

```csharp
public IActionResult Index(string? search)
{
    var products = _catalog.Search(search);
    return ProductPage(products, search);
}
```

### Multiple slots

When a wrapper needs more than one block, use several `IHtmlContent` parameters:

```csharp
InlayTemplate Card(string title, IHtmlContent body, IHtmlContent? actions = null) =>
    Inlay.Template($"""
        <div class="card">
            <div class="card-header"><h3>{title}</h3></div>
            <div class="card-body">{body}</div>
            {Inlay.If(actions != null, $"""
                <div class="card-footer">{actions}</div>
                """)}
        </div>
        """);
```

Each slot is built independently and passed in:

```csharp
var stats = Inlay.Template($"""
    <dl>
        <dt>Orders</dt><dd>{orderCount}</dd>
        <dt>Revenue</dt><dd>${revenue:F2}</dd>
    </dl>
    """);

var buttons = Inlay.Template($"""
    <a href="/orders">View all</a>
    <a href="/orders/export">Export CSV</a>
    """);

var html = Card("This Month", stats, buttons);
```


## Complete example: user dashboard

This example combines all the helpers in a realistic scenario:

```csharp
record User(string Name, string Role, string AvatarUrl);
record Activity(string Description, DateTime When, bool IsRead);

InlayTemplate RenderDashboard(User user, IEnumerable<Activity> activities)
{
    var isAdmin = user.Role == "Admin";

    return Inlay.Template($"""
        <div class="dashboard">
            <header class="{Inlay.Css(("header", true), ("header-admin", isAdmin))}">
                <img src="{user.AvatarUrl}" alt="{user.Name}" />
                <h1>{user.Name}</h1>
                {Inlay.If(isAdmin, $"""<span class="role-badge">Admin</span>""")}
            </header>

            <section class="activity">
                <h2>Recent Activity</h2>
                <ul>
                    {Inlay.Each(activities,
                        a => $"""
                            <li class="{Inlay.Css(("activity-item", true), ("unread", !a.IsRead))}">
                                <span>{a.Description}</span>
                                <time>{a.When:yyyy-MM-dd}</time>
                            </li>
                            """,
                        $"""<li class="empty">No recent activity.</li>""")}
                </ul>
            </section>

            {Inlay.If(isAdmin, $"""
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
