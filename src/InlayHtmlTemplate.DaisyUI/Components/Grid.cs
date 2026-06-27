
namespace InlayHtmlTemplate.DaisyUI.Components;


public class Grid : IHtmlContent
{
    private readonly List<GridRow> _items = [];


    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        foreach (var item in _items)
        {
            item.WriteTo(writer, encoder);
        }
    }


    public Grid NewRow()
    {
        _items.Add(new GridRow());
        return this;
    }

    public Grid AddCell(GridCell content)
    {
        _items.Last().AddColumn(new GridColumn(content));
        return this;
    }

    public Grid AddCell(GridCell content, int span)
    {
        _items.Last().AddColumn(new GridColumn(content, span));
        return this;
    }
}


public class GridRow : IHtmlContent
{
    private readonly List<GridColumn> _items = [];

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        var sb = new StringBuilder();
        foreach (var item in _items)
        {
            sb.Append(item.Render());
        }
        return sb.ToString();
    }

    public GridRow AddColumn(GridColumn column)
    {
        _items.Add(column);
        return this;
    }
}


public class GridCell : IHtmlContent
{
    private IHtmlContent _content;
    // span for lg
    private int _spanLg = 1;
    // span for md
    private int _spanMd = 1;
    // span for sm
    private int _spanSm = 1;


    public GridCell(IHtmlContent content)
    {
        _cell = content;
    }

    public GridCell(IHtmlContent content, int span)
    {
        _cell = content;
        _span = span;
    }

    public GridCell SetSpan(int span)
    {
        _spanLg = span;
        _spanMd = span;
        _spanSm = span;
        return this;
    }

    public GridCell SetSpanMd(int span)
    {
        _spanMd = span;
        return this;
    }

    public GridCell SetSpanSm(int span)
    {
        _spanSm = span;
        return this;
    }

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        var template = Inlay.Template(
            $"""
            <div class="grid-cell" style="grid-column: span {_span}">
                {_content}
            </ div >
            """);
        template.Render(writer, encoder);
    }
}
