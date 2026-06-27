using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Xunit;

namespace InlayHtmlTemplate.Components.Tests;

public class GridTests
{
    static string Render(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    [Fact]
    public void Grid_renders_style_tag()
    {
        var html = Render(new Grid().Add(new HtmlString("x")));
        Assert.Contains("<style data-il-grid>", html);
    }

    [Fact]
    public void Grid_renders_row_with_grid_columns()
    {
        var html = Render(new Grid().Add(new HtmlString("x")));
        Assert.Contains("display:grid", html);
        Assert.Contains("grid-template-columns:repeat(12,1fr)", html);
    }

    [Fact]
    public void Grid_renders_outer_flex_container()
    {
        var html = Render(new Grid().Add(new HtmlString("x")));
        Assert.Contains("display:flex", html);
        Assert.Contains("flex-direction:column", html);
    }

    [Fact]
    public void Grid_renders_gap()
    {
        var html = Render(new Grid(gap: 6).Add(new HtmlString("x")));
        Assert.Contains("gap:1.5rem", html);
    }

    [Fact]
    public void Grid_default_gap_is_1rem()
    {
        var html = Render(new Grid().Add(new HtmlString("x")));
        Assert.Contains("gap:1rem", html);
    }

    [Fact]
    public void Cell_renders_base_span_class()
    {
        var html = Render(new Grid().Add(new HtmlString("content"), span: 6));
        Assert.Contains("class=\"il-span-6\"", html);
        Assert.Contains("content", html);
    }

    [Fact]
    public void Cell_renders_responsive_span_classes()
    {
        var html = Render(new Grid().Add(new HtmlString("x"), span: 12, spanSm: 6, spanMd: 4, spanLg: 3));
        Assert.Contains("il-span-12", html);
        Assert.Contains("il-sm-span-6", html);
        Assert.Contains("il-md-span-4", html);
        Assert.Contains("il-lg-span-3", html);
    }

    [Fact]
    public void Cell_renders_only_specified_responsive_classes()
    {
        var html = Render(new Grid().Add(new HtmlString("x"), span: 12, spanMd: 6));
        Assert.Contains("class=\"il-span-12 il-md-span-6\"", html);
    }

    [Fact]
    public void NewRow_creates_separate_row_divs()
    {
        var html = Render(
            new Grid()
                .Add(new HtmlString("row1"))
                .NewRow()
                .Add(new HtmlString("row2")));

        var firstRowEnd = html.IndexOf("</div><div style=\"display:grid");
        Assert.True(firstRowEnd > 0, "Should have two separate row divs");
        Assert.Contains("row1", html);
        Assert.Contains("row2", html);
    }

    [Fact]
    public void Add_without_NewRow_auto_creates_first_row()
    {
        var html = Render(new Grid().Add(new HtmlString("auto")));
        Assert.Contains("auto", html);
        Assert.Contains("display:grid", html);
    }

    [Fact]
    public void Empty_rows_are_skipped()
    {
        var grid = new Grid()
            .NewRow()
            .NewRow()
            .Add(new HtmlString("content"));

        var html = Render(grid);
        var gridDivCount = CountOccurrences(html, "display:grid");
        Assert.Equal(1, gridDivCount);
    }

    [Fact]
    public void Span_out_of_range_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Grid().Add(new HtmlString("x"), span: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Grid().Add(new HtmlString("x"), span: 13));
    }

    [Fact]
    public void Responsive_span_out_of_range_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Grid().Add(new HtmlString("x"), spanMd: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Grid().Add(new HtmlString("x"), spanLg: 13));
    }

    [Fact]
    public void Styles_contain_all_breakpoints()
    {
        var styles = Grid.Styles;
        Assert.Contains("@media(min-width:640px)", styles);
        Assert.Contains("@media(min-width:768px)", styles);
        Assert.Contains("@media(min-width:1024px)", styles);
    }

    [Fact]
    public void Styles_contain_all_span_values()
    {
        var styles = Grid.Styles;
        for (var i = 1; i <= 12; i++)
        {
            Assert.Contains($".il-span-{i}{{grid-column:span {i}}}", styles);
            Assert.Contains($".il-md-span-{i}{{grid-column:span {i}}}", styles);
        }
    }

    [Fact]
    public void Cell_content_is_rendered_inside_div()
    {
        var content = Inlay.Template($"<input class=\"input\" />");
        var html = Render(new Grid().Add(content, span: 6));
        Assert.Matches("class=\"il-span-6\">.*<input class=\"input\" />.*</div>", html);
    }

    private static int CountOccurrences(string text, string pattern)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }
}
