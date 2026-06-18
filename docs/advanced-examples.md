# Advanced Examples

## How template composition works

`SimpleHtmlTemplate.Render` accepts a `FormattableString` (a C# interpolated string like `$"..."`), escapes each interpolated value according to its HTML context, and returns a plain `string` with the result.

The key question is: **what happens when you want to embed one rendered template inside another?**

```csharp
var item = "Bread & butter";
var li = SimpleHtmlTemplate.Render($"<li>{item}</li>");
// li = "<li>Bread &amp; butter</li>"    ← already safe HTML
```

If you now put `li` directly into another template, it's treated as a regular `string` and gets escaped *again*:

```csharp
// WRONG: double-escaped
var result = SimpleHtmlTemplate.Render($"<ul>{li}</ul>");
// result = "<ul>&lt;li&gt;Bread &amp;amp; butter&lt;/li&gt;</ul>"    ← broken!
```

The solution is `IHtmlContent`. Any value that implements this interface is inserted **as-is**, without escaping. The simplest way is `Html.Raw`:

```csharp
// CORRECT: Html.Raw marks the string as trusted HTML
var result = SimpleHtmlTemplate.Render($"<ul>{Html.Raw(li)}</ul>");
// result = "<ul><li>Bread &amp; butter</li></ul>"    ← correct
```

**This is the only rule you need to compose templates:**
- `string` values are always escaped (safe by default)
- `IHtmlContent` values are always trusted (you opt in explicitly)

Every helper in the `Html` class returns `IHtmlContent`, so their results compose naturally into outer templates without double-escaping.

### Composition patterns

The most common pattern is a function that renders a fragment and returns its result for use in a parent template:

```csharp
// Each function renders its own template and returns the result
IHtmlContent RenderBadge(string role) =>
    Html.Raw(SimpleHtmlTemplate.Render($"<span class=\"badge\">{role}</span>"));

IHtmlContent RenderCard(string name, string role) =>
    Html.Raw(SimpleHtmlTemplate.Render(
        $"<div class=\"card\"><h3>{name}</h3>{RenderBadge(role)}</div>"));

// The outer template receives pre-rendered IHtmlContent — no double-escaping
var html = SimpleHtmlTemplate.Render($"<section>{RenderCard("Alice", "Admin")}</section>");
```

You can also build up fragments in a loop and join them:

```csharp
var rows = users.Select(u =>
    SimpleHtmlTemplate.Render($"<tr><td>{u.Name}</td><td>{u.Email}</td></tr>"));

var body = Html.Raw(string.Join("\n", rows));

var table = SimpleHtmlTemplate.Render($"<table>{body}</table>");
```

The `Html.Each` helper encapsulates exactly this pattern — more on that below.


## Conditional fragments with `Html.If`

`Html.If` renders a template only when the condition is `true`. It returns `IHtmlContent`, so the result composes into any outer template.

```csharp
var isAdmin = true;
var userName = "Alice";

var html = SimpleHtmlTemplate.Render($@"
    <div class=""user-header"">
        <span>{userName}</span>
        {Html.If(isAdmin, $"<span class=\"badge badge-admin\">Admin</span>")}
    </div>
");
```

When `isAdmin` is `false`, the badge is simply absent — no empty tags, no wrapper divs.

### If / else with fallback

The two-argument overload provides an else branch:

```csharp
var isLoggedIn = false;
var userName = "Guest";

var nav = Html.If(isLoggedIn,
    $"<span>Welcome, {userName}</span>",
    $"<a href=\"/login\">Log in</a>");

var html = SimpleHtmlTemplate.Render($"<nav>{nav}</nav>");
// <nav><a href="/login">Log in</a></nav>
```

### Nesting conditionals

Since `Html.If` returns `IHtmlContent`, you can use it inside another `Html.If`:

```csharp
var isLoggedIn = true;
var isAdmin = true;
var name = "Alice";

var header = Html.If(isLoggedIn,
    $@"<div class=""toolbar"">
        <span>{name}</span>
        {Html.If(isAdmin, $"<button>Admin Panel</button>")}
    </div>",
    $"<a href=\"/login\">Log in</a>");
```


## Class toggling with `Html.Css`

`Html.Css` builds a class string from a list of `(className, active)` tuples. Only the classes where `active` is `true` are included.

```csharp
var isActive = true;
var isDisabled = false;
var hasError = true;

var html = SimpleHtmlTemplate.Render(
    $"<button class=\"{Html.Css(
        ("btn", true),
        ("btn-primary", isActive),
        ("btn-disabled", isDisabled),
        ("btn-error", hasError)
    )}\">Submit</button>");

// <button class="btn btn-primary btn-error">Submit</button>
```

`Html.Css` returns a plain `string` (not `IHtmlContent`), so the value is still attribute-escaped by the template engine. Since CSS class names don't contain special characters, this is transparent — but it means you can't accidentally inject attribute breakouts through class names.

### Common patterns

Toggling a single class:

```csharp
var tab = "settings";
var currentTab = "settings";

var html = SimpleHtmlTemplate.Render(
    $"<li class=\"{Html.Css(("nav-link", true), ("active", tab == currentTab))}\">{tab}</li>");
```

State-driven styling:

```csharp
var status = OrderStatus.Shipped;

var html = SimpleHtmlTemplate.Render(
    $@"<span class=""{Html.Css(
        ("badge", true),
        ("badge-warning", status == OrderStatus.Pending),
        ("badge-info", status == OrderStatus.Shipped),
        ("badge-success", status == OrderStatus.Delivered)
    )}"">{status}</span>");
```


## Iterating lists with `Html.Each`

`Html.Each` renders a template for every item in a collection. It returns `IHtmlContent`, so the result embeds in an outer template without double-escaping.

```csharp
var fruits = new[] { "Apple", "Banana", "Cherry" };

var html = SimpleHtmlTemplate.Render(
    $"<ul>{Html.Each(fruits, fruit => $"<li>{fruit}</li>")}</ul>");

// <ul><li>Apple</li><li>Banana</li><li>Cherry</li></ul>
```

Escaping works per-item — if a value contains HTML, it's escaped individually:

```csharp
var items = new[] { "Safe text", "<img src=x onerror=alert(1)>" };

var html = SimpleHtmlTemplate.Render(
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

var html = SimpleHtmlTemplate.Render($@"
    <table>
        <thead><tr><th>Product</th><th>Price</th></tr></thead>
        <tbody>
            {Html.Each(products, p => $@"
                <tr>
                    <td><a href=""{p.Url}"">{p.Name}</a></td>
                    <td>${p.Price}</td>
                </tr>")}
        </tbody>
    </table>");
```

### Using the index

The overload with `(item, index)` is useful for zebra striping or numbered lists:

```csharp
var steps = new[] { "Mix ingredients", "Preheat oven", "Bake 25 min" };

var html = SimpleHtmlTemplate.Render(
    $@"<ol>{Html.Each(steps, (step, i) =>
        $"<li class=\"{Html.Css(("step", true), ("even", i % 2 == 0))}\">{step}</li>")}</ol>");
```


## Empty list fallback with `Html.Each`

The three-argument overload adds a fallback template that renders when the collection is empty:

```csharp
var notifications = Array.Empty<string>();

var html = SimpleHtmlTemplate.Render($@"
    <div class=""notification-panel"">
        <h3>Notifications</h3>
        <ul>
            {Html.Each(notifications,
                n    => $"<li>{n}</li>",
                $"<li class=\"empty\">You're all caught up!</li>")}
        </ul>
    </div>");

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

var html = SimpleHtmlTemplate.Render($@"
    <div class=""task-list"">
        {Html.Each(tasks,
            t => $@"<div class=""{Html.Css(("task", true), ("done", t.Done))}"">
                    {Html.If(t.Done,
                        $"<s>{t.Title}</s>",
                        $"<span>{t.Title}</span>")}
                </div>",
            $"<p class=\"empty\">No tasks yet. Enjoy your free time!</p>")}
    </div>");
```


## Complete example: user dashboard

This example combines all the helpers in a realistic scenario:

```csharp
record User(string Name, string Role, string AvatarUrl);
record Activity(string Description, DateTime When, bool IsRead);

IHtmlContent RenderDashboard(User user, IEnumerable<Activity> activities)
{
    var isAdmin = user.Role == "Admin";

    var html = SimpleHtmlTemplate.Render($@"
        <div class=""dashboard"">
            <header class=""{Html.Css(("header", true), ("header-admin", isAdmin))}"">
                <img src=""{user.AvatarUrl}"" alt=""{user.Name}"" />
                <h1>{user.Name}</h1>
                {Html.If(isAdmin, $"<span class=\"role-badge\">Admin</span>")}
            </header>

            <section class=""activity"">
                <h2>Recent Activity</h2>
                <ul>
                    {Html.Each(activities,
                        a => $@"<li class=""{Html.Css(("activity-item", true), ("unread", !a.IsRead))}"">
                                <span>{a.Description}</span>
                                <time>{a.When:yyyy-MM-dd}</time>
                            </li>",
                        $"<li class=\"empty\">No recent activity.</li>")}
                </ul>
            </section>

            {Html.If(isAdmin, $@"
                <section class=""admin-panel"">
                    <h2>Admin Tools</h2>
                    <button>Manage Users</button>
                    <button>View Logs</button>
                </section>")}
        </div>");

    return Html.Raw(html);
}
```
