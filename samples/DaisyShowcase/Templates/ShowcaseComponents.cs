using DaisyShowcase.Models;
using InlayHtmlTemplate;
using InlayHtmlTemplate.Components;
using InlayHtmlTemplate.DaisyUI;
using Microsoft.AspNetCore.Html;

namespace DaisyShowcase.Templates;

public static class ShowcaseComponents
{
    public static InlayTemplate ContactForm(string? error = null)
    {
        var f = new ContactForm();

        var body = Inlay.Template($"""
            <h1 class="text-4xl font-bold mb-8">Contact</h1>

            {Inlay.If(error is not null, $"""
                {Daisy.Alert(error ?? "", AlertVariant.Error)}
                <div class="mb-4"></div>
                """)}

            <form method="post" action="/Showcase/ContactSubmit">
                <div class="card bg-base-100 shadow-xl">
                    <div class="card-body">
                        {new Grid(gap: 4)
                            .AddField(f, x => x.Name, spanMd: 6)
                            .AddField(f, x => x.Surname, spanMd: 6)
                            .NewRow()
                            .AddField(f, x => x.Email, spanMd: 8)
                            .AddField(f, x => x.Phone, spanMd: 4)
                            .NewRow()
                            .AddField(f, x => x.Address)
                            .NewRow()
                            .AddField(f, x => x.Zip, spanMd: 3)
                            .AddField(f, x => x.City, spanMd: 5)
                            .Add(Daisy.FormControl(
                                Daisy.Select([
                                    new SelectOption("es", "Spain"),
                                    new SelectOption("fr", "France"),
                                    new SelectOption("de", "Germany"),
                                    new SelectOption("it", "Italy"),
                                    new SelectOption("pt", "Portugal"),
                                ], name: "Country", placeholder: "Select country"),
                                label: "Country"), spanMd: 4)
                            .NewRow()
                            .AddField(f, x => x.Message)
                            .NewRow()
                            .Add(Inlay.Template($"""
                                <div class="flex gap-2 justify-end pt-2">
                                    {Daisy.Button("Clear", ButtonVariant.Ghost, type: "reset")}
                                    {Daisy.Button("Send", ButtonVariant.Primary, type: "submit")}
                                </div>
                                """))}
                    </div>
                </div>
            </form>
            """);

        return ShowcaseTemplates.WithSidebar("Contact", "Contact", body);
    }

    public static InlayTemplate ContactResult(ContactForm form)
    {
        var body = Inlay.Template($"""
            <h1 class="text-4xl font-bold mb-8">Message Received</h1>

            {Daisy.Alert("Your contact form was submitted successfully.", AlertVariant.Success)}

            <div class="mt-6">
                <div class="card bg-base-100 shadow-xl">
                    <div class="card-body">
                        <h2 class="card-title mb-4">Submitted Data</h2>
                        {new Grid(gap: 3)
                            .AddField(form, f => f.Name, spanMd: 6)
                            .AddField(form, f => f.Surname, spanMd: 6)
                            .NewRow()
                            .AddField(form, f => f.Email, spanMd: 8)
                            .AddField(form, f => f.Phone, spanMd: 4)
                            .NewRow()
                            .AddField(form, f => f.Address)
                            .NewRow()
                            .AddField(form, f => f.Zip, spanMd: 3)
                            .AddField(form, f => f.City, spanMd: 5)
                            .AddField(form, f => f.Country, spanMd: 4)
                            .NewRow()
                            .AddField(form, f => f.Message)}
                    </div>
                </div>
            </div>

            <div class="mt-6">
                {Daisy.Button("Back to form", ButtonVariant.Primary, href: "/Showcase/Contact")}
            </div>
            """);

        return ShowcaseTemplates.WithSidebar("Contact Result", "Contact", body);
    }
}
