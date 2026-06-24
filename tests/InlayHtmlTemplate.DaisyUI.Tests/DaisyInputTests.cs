using System.Text.Encodings.Web;
using InlayHtmlTemplate;
using InlayHtmlTemplate.DaisyUI;
using Microsoft.AspNetCore.Html;
using Xunit;

namespace InlayHtmlTemplate.DaisyUI.Tests;

public class DaisyInputTests
{
    static string Render(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    [Fact]
    public void TextInput_renders_with_type_and_placeholder()
    {
        var html = Render(Daisy.TextInput(name: "email", type: "email", placeholder: "you@example.com"));
        Assert.Contains("type=\"email\"", html);
        Assert.Contains("name=\"email\"", html);
        Assert.Contains("placeholder=\"you@example.com\"", html);
        Assert.Contains("input input-bordered", html.Replace("w-full ", ""));
    }

    [Fact]
    public void TextInput_variant_and_size()
    {
        var html = Render(Daisy.TextInput(variant: InputVariant.Primary, size: InputSize.Sm));
        Assert.Contains("input-primary", html);
        Assert.Contains("input-sm", html);
    }

    [Fact]
    public void TextInput_ghost()
    {
        var html = Render(Daisy.TextInput(ghost: true, bordered: false));
        Assert.Contains("input-ghost", html);
        Assert.DoesNotContain("input-bordered", html);
    }

    [Fact]
    public void Textarea_renders_with_rows()
    {
        var html = Render(Daisy.Textarea(name: "bio", placeholder: "Tell us", rows: 4));
        Assert.Contains("<textarea", html);
        Assert.Contains("name=\"bio\"", html);
        Assert.Contains("rows=\"4\"", html);
        Assert.Contains("textarea textarea-bordered", html.Replace("w-full ", ""));
    }

    [Fact]
    public void Textarea_variant()
    {
        var html = Render(Daisy.Textarea(variant: InputVariant.Success));
        Assert.Contains("textarea-success", html);
    }

    [Fact]
    public void Select_renders_options_and_placeholder()
    {
        var options = new[]
        {
            new SelectOption("es", "Spain"),
            new SelectOption("fr", "France", Selected: true),
        };
        var html = Render(Daisy.Select(options, name: "country", placeholder: "Pick one"));
        Assert.Contains("<select", html);
        Assert.Contains("name=\"country\"", html);
        Assert.Contains("Pick one", html);
        Assert.Contains("value=\"es\"", html);
        Assert.Contains("Spain", html);
        Assert.Contains("France", html);
    }

    [Fact]
    public void Select_variant_and_size()
    {
        var html = Render(Daisy.Select([], variant: InputVariant.Accent, size: InputSize.Lg));
        Assert.Contains("select-accent", html);
        Assert.Contains("select-lg", html);
    }

    [Fact]
    public void FileInput_renders_with_accept()
    {
        var html = Render(Daisy.FileInput(name: "doc", accept: ".pdf,.docx"));
        Assert.Contains("type=\"file\"", html);
        Assert.Contains("name=\"doc\"", html);
        Assert.Contains("accept=\".pdf,.docx\"", html);
        Assert.Contains("file-input", html);
    }

    [Fact]
    public void FileInput_variant()
    {
        var html = Render(Daisy.FileInput(variant: InputVariant.Warning));
        Assert.Contains("file-input-warning", html);
    }

    [Fact]
    public void Checkbox_renders_with_label()
    {
        var html = Render(Daisy.Checkbox(name: "terms", label: "Accept terms", variant: InputVariant.Primary));
        Assert.Contains("type=\"checkbox\"", html);
        Assert.Contains("Accept terms", html);
        Assert.Contains("checkbox-primary", html);
        Assert.Contains("label-text", html);
    }

    [Fact]
    public void Checkbox_without_label()
    {
        var html = Render(Daisy.Checkbox(name: "ok"));
        Assert.Contains("type=\"checkbox\"", html);
        Assert.DoesNotContain("label-text", html);
    }

    [Fact]
    public void Toggle_renders_with_label()
    {
        var html = Render(Daisy.Toggle(name: "dark", label: "Dark mode", variant: InputVariant.Success));
        Assert.Contains("toggle", html);
        Assert.Contains("toggle-success", html);
        Assert.Contains("Dark mode", html);
    }

    [Fact]
    public void Radio_renders_with_value()
    {
        var html = Render(Daisy.Radio("plan", "pro", label: "Pro Plan", variant: InputVariant.Secondary));
        Assert.Contains("type=\"radio\"", html);
        Assert.Contains("name=\"plan\"", html);
        Assert.Contains("value=\"pro\"", html);
        Assert.Contains("Pro Plan", html);
        Assert.Contains("radio-secondary", html);
    }

    [Fact]
    public void RadioGroup_renders_all_options()
    {
        var options = new[]
        {
            new SelectOption("a", "Alpha", Selected: true),
            new SelectOption("b", "Beta"),
        };
        var html = Render(Daisy.RadioGroup("choice", options));
        Assert.Contains("Alpha", html);
        Assert.Contains("Beta", html);
        Assert.Contains("value=\"a\"", html);
        Assert.Contains("value=\"b\"", html);
    }

    [Fact]
    public void Range_renders_with_min_max()
    {
        var html = Render(Daisy.Range(name: "vol", min: 0, max: 100, value: 50, step: 10));
        Assert.Contains("type=\"range\"", html);
        Assert.Contains("min=\"0\"", html);
        Assert.Contains("max=\"100\"", html);
        Assert.Contains("value=\"50\"", html);
        Assert.Contains("step=\"10\"", html);
        Assert.Contains("range", html);
    }

    [Fact]
    public void Range_variant()
    {
        var html = Render(Daisy.Range(variant: InputVariant.Accent));
        Assert.Contains("range-accent", html);
    }

    [Fact]
    public void FormControl_renders_label_and_helper()
    {
        var input = Daisy.TextInput(name: "user");
        var html = Render(Daisy.FormControl(input, label: "Username", altLabel: "Required", helperText: "Pick a unique name"));
        Assert.Contains("form-control", html);
        Assert.Contains("Username", html);
        Assert.Contains("Required", html);
        Assert.Contains("Pick a unique name", html);
    }

    [Fact]
    public void FormControl_without_labels()
    {
        var input = Daisy.TextInput();
        var html = Render(Daisy.FormControl(input));
        Assert.Contains("form-control", html);
        Assert.DoesNotContain("label-text\"", html);
    }

    [Fact]
    public void TextInput_not_disabled_by_default()
    {
        var html = Render(Daisy.TextInput());
        Assert.DoesNotContain("disabled", html);
    }

    [Fact]
    public void TextInput_disabled_when_set()
    {
        var html = Render(Daisy.TextInput(disabled: true));
        Assert.Contains(" disabled", html);
    }

    [Fact]
    public void Checkbox_not_checked_by_default()
    {
        var html = Render(Daisy.Checkbox());
        Assert.DoesNotContain("checked", html);
    }

    [Fact]
    public void Checkbox_checked_when_set()
    {
        var html = Render(Daisy.Checkbox(isChecked: true));
        Assert.Contains(" checked", html);
    }

    [Fact]
    public void Toggle_not_checked_by_default()
    {
        var html = Render(Daisy.Toggle());
        Assert.DoesNotContain("checked", html);
    }

    [Fact]
    public void Select_option_not_selected_by_default()
    {
        var html = Render(Daisy.Select([new SelectOption("a", "A")]));
        Assert.DoesNotContain("selected", html);
    }

    [Fact]
    public void Select_option_selected_when_set()
    {
        var html = Render(Daisy.Select([new SelectOption("a", "A", Selected: true)]));
        Assert.Contains(" selected", html);
    }

    [Fact]
    public void Radio_not_checked_by_default()
    {
        var html = Render(Daisy.Radio("g", "v"));
        Assert.DoesNotContain("checked", html);
    }
}
