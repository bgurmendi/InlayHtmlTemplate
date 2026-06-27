using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace InlayHtmlTemplate.Components;

public class Field : IHtmlContent
{
    private string _label;
    private string _value;
    private string _type = "text";
    private string _placeholder;
    private string _name;
    private string _id;
    private string _error;

    public Field()
    {

    }

    public Field SetError(string error)
    {
        _error = error;
        return this;
    }

    public Field SetLabel(string label)
    {
        _label = label;
        return this;
    }

    public Field SetValue(string value)
    {
        _value = value;
        return this;
    }

    public Field SetType(string type)
    {
        _type = type;
        return this;
    }

    public Field SetPlaceholder(string placeholder)
    {
        _placeholder = placeholder;
        return this;
    }

    public Field SetName(string name)
    {
        _name = name;
        return this;
    }

    public Field SetId(string id)
    {
        _id = id;
        return this;
    }

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        IHtmlContent? errorContent = null;

        if (!string.IsNullOrEmpty(_error))
        {
            errorContent = Inlay.Template($"""
            <div>{_error}</div>
            """);
        }

        Inlay.Template($"""
        <label>{_label}</label>
        <input type={_type} value={_value} placeholder={_placeholder} name={_name} id={_id} />
        {errorContent}
        """).WriteTo(writer, encoder);
    }
}
