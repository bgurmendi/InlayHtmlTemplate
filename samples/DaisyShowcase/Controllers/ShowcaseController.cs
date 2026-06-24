using Microsoft.AspNetCore.Mvc;
using DaisyShowcase.Templates;

namespace DaisyShowcase.Controllers;

public class ShowcaseController : Controller
{
    public IActionResult Index() => ShowcaseTemplates.Index();

    public IActionResult Components() => ShowcaseTemplates.Components();

    public IActionResult Layouts() => ShowcaseTemplates.Layouts();
}
