using Microsoft.AspNetCore.Mvc;
using DaisyShowcase.Models;
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

    public IActionResult Contact() => ShowcaseComponents.ContactForm();

    [HttpPost]
    public IActionResult ContactSubmit(ContactForm form)
    {
        if (string.IsNullOrWhiteSpace(form.Name) || string.IsNullOrWhiteSpace(form.Email))
            return ShowcaseComponents.ContactForm(error: "Name and Email are required.");

        return ShowcaseComponents.ContactResult(form);
    }
}
