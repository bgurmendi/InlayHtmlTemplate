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
                "Inlay.Template returns IHtmlContent, so templates nest naturally without double-escaping.",
                false),
            new Feature("Conditional Rendering",
                "Inlay.If renders fragments only when a condition is true, with optional else branch.",
                true),
            new Feature("CSS Class Toggling",
                "Inlay.Css builds class strings from conditional tuples — no more ternary concatenation.",
                true),
            new Feature("List Iteration",
                "Inlay.Each renders a template per item, with an optional fallback for empty collections.",
                true),
        };

        var body = HomeTemplates.Index(features);
        return LayoutTemplate.Render("Home", body, "Home");
    }

    public IActionResult Privacy()
    {
        var body = HomeTemplates.Privacy();
        return LayoutTemplate.Render("Privacy", body, "Privacy");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var body = ErrorTemplate.Render(requestId);
        return LayoutTemplate.Render("Error", body);
    }
}
