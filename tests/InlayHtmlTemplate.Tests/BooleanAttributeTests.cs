using InlayHtmlTemplate;
using Xunit;

namespace InlayHtmlTemplate.Tests;

public class BooleanAttributeTests
{
    [Fact]
    public void Disabled_true_renders_attribute()
    {
        var html = SimpleHtmlTemplate.Render($"""<input disabled="{true}" />""");
        Assert.Contains(" disabled", html);
        Assert.DoesNotContain("disabled=\"", html);
    }

    [Fact]
    public void Disabled_false_omits_attribute()
    {
        var html = SimpleHtmlTemplate.Render($"""<input disabled="{false}" />""");
        Assert.DoesNotContain("disabled", html);
    }

    [Fact]
    public void Checked_true_renders_attribute()
    {
        var html = SimpleHtmlTemplate.Render($"""<input type="checkbox" checked="{true}" />""");
        Assert.Contains(" checked", html);
    }

    [Fact]
    public void Checked_false_omits_attribute()
    {
        var html = SimpleHtmlTemplate.Render($"""<input type="checkbox" checked="{false}" />""");
        Assert.DoesNotContain("checked", html);
    }

    [Fact]
    public void Selected_true_renders_attribute()
    {
        var html = SimpleHtmlTemplate.Render($"""<option selected="{true}">A</option>""");
        Assert.Contains(" selected", html);
    }

    [Fact]
    public void Selected_false_omits_attribute()
    {
        var html = SimpleHtmlTemplate.Render($"""<option selected="{false}">A</option>""");
        Assert.DoesNotContain("selected", html);
    }

    [Fact]
    public void Required_true_renders_attribute()
    {
        var html = SimpleHtmlTemplate.Render($"""<input required="{true}" />""");
        Assert.Contains(" required", html);
    }

    [Fact]
    public void Readonly_true_renders_attribute()
    {
        var html = SimpleHtmlTemplate.Render($"""<input readonly="{true}" />""");
        Assert.Contains(" readonly", html);
    }

    [Fact]
    public void Multiple_boolean_attrs_render_independently()
    {
        var html = SimpleHtmlTemplate.Render($"""<input disabled="{true}" required="{false}" />""");
        Assert.Contains(" disabled", html);
        Assert.DoesNotContain("required", html);
    }

    [Fact]
    public void Boolean_attr_preserves_surrounding_attributes()
    {
        var html = SimpleHtmlTemplate.Render($"""<input type="text" disabled="{true}" class="foo" />""");
        Assert.Contains("type=\"text\"", html);
        Assert.Contains(" disabled", html);
        Assert.Contains("class=\"foo\"", html);
    }

    [Fact]
    public void Boolean_attr_false_preserves_surrounding_attributes()
    {
        var html = SimpleHtmlTemplate.Render($"""<input type="text" disabled="{false}" class="foo" />""");
        Assert.Contains("type=\"text\"", html);
        Assert.DoesNotContain("disabled", html);
        Assert.Contains("class=\"foo\"", html);
    }

    [Fact]
    public void Non_boolean_attr_renders_value_normally()
    {
        var name = "test";
        var html = SimpleHtmlTemplate.Render($"""<input name="{name}" />""");
        Assert.Contains("name=\"test\"", html);
    }

    [Fact]
    public void Boolean_attr_with_string_true_renders()
    {
        object val = "true";
        var html = SimpleHtmlTemplate.Render($"""<input disabled="{val}" />""");
        Assert.Contains(" disabled", html);
    }

    [Fact]
    public void Boolean_attr_with_empty_string_omits()
    {
        object val = "";
        var html = SimpleHtmlTemplate.Render($"""<input disabled="{val}" />""");
        Assert.DoesNotContain("disabled", html);
    }

    [Fact]
    public void Boolean_attr_with_null_omits()
    {
        object? val = null;
        var html = SimpleHtmlTemplate.Render($"""<input disabled="{val}" />""");
        Assert.DoesNotContain("disabled", html);
    }

    [Fact]
    public void Hidden_true_renders_attribute()
    {
        var html = SimpleHtmlTemplate.Render($"""<div hidden="{true}">content</div>""");
        Assert.Contains(" hidden", html);
        Assert.Contains("content", html);
    }

    [Fact]
    public void Open_true_renders_attribute()
    {
        var html = SimpleHtmlTemplate.Render($"""<details open="{true}">info</details>""");
        Assert.Contains(" open", html);
    }

    // --- Unquoted form: disabled={value} ---

    [Fact]
    public void Unquoted_disabled_true_renders_attribute()
    {
        var html = SimpleHtmlTemplate.Render($"""<input disabled={true} />""");
        Assert.Contains(" disabled", html);
        Assert.DoesNotContain("disabled=", html);
    }

    [Fact]
    public void Unquoted_disabled_false_omits_attribute()
    {
        var html = SimpleHtmlTemplate.Render($"""<input disabled={false} />""");
        Assert.DoesNotContain("disabled", html);
    }

    [Fact]
    public void Unquoted_checked_true_renders_attribute()
    {
        var html = SimpleHtmlTemplate.Render($"""<input type="checkbox" checked={true} />""");
        Assert.Contains(" checked", html);
    }

    [Fact]
    public void Unquoted_checked_false_omits_attribute()
    {
        var html = SimpleHtmlTemplate.Render($"""<input type="checkbox" checked={false} />""");
        Assert.DoesNotContain("checked", html);
    }

    [Fact]
    public void Unquoted_selected_true_renders_attribute()
    {
        var html = SimpleHtmlTemplate.Render($"""<option selected={true}>A</option>""");
        Assert.Contains(" selected", html);
    }

    [Fact]
    public void Unquoted_multiple_boolean_attrs()
    {
        var html = SimpleHtmlTemplate.Render($"""<input disabled={true} required={false} checked={true} />""");
        Assert.Contains(" disabled", html);
        Assert.DoesNotContain("required", html);
        Assert.Contains(" checked", html);
    }

    [Fact]
    public void Unquoted_preserves_surrounding_attributes()
    {
        var html = SimpleHtmlTemplate.Render($"""<input type="text" disabled={true} class="foo" />""");
        Assert.Contains("type=\"text\"", html);
        Assert.Contains(" disabled", html);
        Assert.Contains("class=\"foo\"", html);
    }

    [Fact]
    public void Unquoted_false_preserves_surrounding_attributes()
    {
        var html = SimpleHtmlTemplate.Render($"""<input type="text" disabled={false} class="foo" />""");
        Assert.Contains("type=\"text\"", html);
        Assert.DoesNotContain("disabled", html);
        Assert.Contains("class=\"foo\"", html);
    }

    // --- Analyzer tests ---

    [Fact]
    public void Analyzer_detects_boolean_context_quoted()
    {
        var analyzer = new HtmlContextAnalyzer();
        foreach (var c in "<input disabled=\"")
            analyzer.ProcessChar(c);
        Assert.Equal(HtmlContext.BooleanAttribute, analyzer.CurrentContext);
        Assert.Equal("disabled", analyzer.CurrentAttribute);
        Assert.True(analyzer.IsQuotedValue);
    }

    [Fact]
    public void Analyzer_detects_boolean_context_unquoted()
    {
        var analyzer = new HtmlContextAnalyzer();
        foreach (var c in "<input disabled=")
            analyzer.ProcessChar(c);
        Assert.Equal(HtmlContext.BooleanAttribute, analyzer.CurrentContext);
        Assert.Equal("disabled", analyzer.CurrentAttribute);
        Assert.False(analyzer.IsQuotedValue);
    }

    [Fact]
    public void Analyzer_non_boolean_stays_attribute_context()
    {
        var analyzer = new HtmlContextAnalyzer();
        foreach (var c in "<input name=\"")
            analyzer.ProcessChar(c);
        Assert.Equal(HtmlContext.Attribute, analyzer.CurrentContext);
    }
}
