using Microsoft.AspNetCore.Mvc;
using DaisyShowcase.Templates;

namespace DaisyShowcase.Controllers;

public class ShowcaseController : Controller
{
    public IActionResult Index() => ShowcaseTemplates.Index();

    public IActionResult Feedback() => ShowcaseTemplates.Feedback();

    public IActionResult Actions() => ShowcaseTemplates.Actions();

    public IActionResult DataDisplay() => ShowcaseTemplates.DataDisplay();

    public IActionResult Navigation() => ShowcaseTemplates.Navigation();

    public IActionResult FormInputs() => ShowcaseTemplates.FormInputs();

    public IActionResult FormControls() => ShowcaseTemplates.FormControls();

    public IActionResult Layouts() => ShowcaseTemplates.Layouts();

    public IActionResult GridLayout() => ShowcaseTemplates.GridLayout();
}
