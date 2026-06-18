using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Templates;

namespace WebApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var features = new[]
        {
            new Feature("Context-Aware Escaping",
                "Automatically escapes values based on HTML context: content, attributes, and URLs.",
                false),
            new Feature("Template Composition",
                "Html.Template returns IHtmlContent, so templates nest naturally without double-escaping.",
                false),
            new Feature("Conditional Rendering",
                "Html.If renders fragments only when a condition is true, with optional else branch.",
                true),
            new Feature("CSS Class Toggling",
                "Html.Css builds class strings from conditional tuples — no more ternary concatenation.",
                true),
            new Feature("List Iteration",
                "Html.Each renders a template per item, with an optional fallback for empty collections.",
                true),
        };

        var body = HomeTemplates.Index(features);
        var html = LayoutTemplate.Render("Home", body, "Home");
        return Content(html.ToString()!, "text/html");
    }

    public IActionResult Privacy()
    {
        var body = HomeTemplates.Privacy();
        var html = LayoutTemplate.Render("Privacy", body, "Privacy");
        return Content(html.ToString()!, "text/html");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var body = ErrorTemplate.Render(requestId);
        var html = LayoutTemplate.Render("Error", body);
        return Content(html.ToString()!, "text/html");
    }
}
