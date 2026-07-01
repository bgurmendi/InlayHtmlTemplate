using System.Text.Encodings.Web;
using InlayHtmlTemplate.DaisyUI;
using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.Components;

public class Field : IHtmlContent
{
    private string? _label;
    private string? _value;
    private string _type = "text";
    private string? _placeholder;
    private string? _name;
    private string? _id;
    private string? _error;
    private InputVariant _variant = InputVariant.Default;

    public Field SetLabel(string label) { _label = label; return this; }
    public Field SetValue(string value) { _value = value; return this; }
    public Field SetType(string type) { _type = type; return this; }
    public Field SetPlaceholder(string placeholder) { _placeholder = placeholder; return this; }
    public Field SetName(string name) { _name = name; return this; }
    public Field SetId(string id) { _id = id; return this; }
    public Field SetError(string error) { _error = error; return this; }
    public Field SetVariant(InputVariant variant) { _variant = variant; return this; }

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        var input = Daisy.TextInput(
            name: _name,
            value: _value,
            type: _type,
            placeholder: _placeholder,
            variant: _variant);

        Daisy.FormControl(
            input,
            label: _label,
            helperText: _error).WriteTo(writer, encoder);
    }
}
