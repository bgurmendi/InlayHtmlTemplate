using InlayHtmlTemplate;
using Xunit;

namespace InlayHtmlTemplate.Tests;

public class HtmlContextAnalyzerTests
{
    private static HtmlContext AnalyzeFull(string html)
    {
        var analyzer = new HtmlContextAnalyzer();
        foreach (var c in html)
            analyzer.ProcessChar(c);
        return analyzer.CurrentContext;
    }

    // --- Initial state ---

    [Fact]
    public void InitialContext_IsContent()
    {
        var analyzer = new HtmlContextAnalyzer();
        Assert.Equal(HtmlContext.Content, analyzer.CurrentContext);
    }

    // --- Content context ---

    [Fact]
    public void PlainText_IsContent()
    {
        Assert.Equal(HtmlContext.Content, AnalyzeFull("hello world"));
    }

    [Fact]
    public void AfterClosingTag_IsContent()
    {
        Assert.Equal(HtmlContext.Content, AnalyzeFull("<div>"));
    }

    [Fact]
    public void BetweenTags_IsContent()
    {
        Assert.Equal(HtmlContext.Content, AnalyzeFull("<p>text</p>"));
    }

    // --- Tag detection ---

    [Fact]
    public void OpenAngleBracket_EntersTag()
    {
        var analyzer = new HtmlContextAnalyzer();
        foreach (var c in "<div class=\"")
            analyzer.ProcessChar(c);
        Assert.Equal(HtmlContext.Attribute, analyzer.CurrentContext);
    }

    [Fact]
    public void CloseAngleBracket_ExitsTag()
    {
        Assert.Equal(HtmlContext.Content, AnalyzeFull("<div>"));
    }

    // --- Attribute context ---

    [Fact]
    public void DoubleQuotedAttribute_IsAttribute()
    {
        Assert.Equal(HtmlContext.Attribute, AnalyzeFull("<div class=\""));
    }

    [Fact]
    public void SingleQuotedAttribute_IsAttribute()
    {
        Assert.Equal(HtmlContext.Attribute, AnalyzeFull("<div class='"));
    }

    [Fact]
    public void ClosingQuote_ReturnsToContent()
    {
        Assert.Equal(HtmlContext.Content, AnalyzeFull("<div class=\"value\" "));
    }

    [Fact]
    public void TitleAttribute_IsAttribute()
    {
        Assert.Equal(HtmlContext.Attribute, AnalyzeFull("<span title=\""));
    }

    [Fact]
    public void DataAttribute_IsAttribute()
    {
        Assert.Equal(HtmlContext.Attribute, AnalyzeFull("<div data=\""));
    }

    // --- URL attribute context ---

    [Fact]
    public void HrefAttribute_IsUrlAttribute()
    {
        Assert.Equal(HtmlContext.UrlAttribute, AnalyzeFull("<a href=\""));
    }

    [Fact]
    public void SrcAttribute_IsUrlAttribute()
    {
        Assert.Equal(HtmlContext.UrlAttribute, AnalyzeFull("<img src=\""));
    }

    [Fact]
    public void HrefSingleQuote_IsUrlAttribute()
    {
        Assert.Equal(HtmlContext.UrlAttribute, AnalyzeFull("<a href='"));
    }

    [Fact]
    public void AfterHrefCloses_BackToContent()
    {
        Assert.Equal(HtmlContext.Content, AnalyzeFull("<a href=\"url\" "));
    }

    // --- Whitespace resets attribute name ---

    [Fact]
    public void SpaceBetweenAttributes_ResetsName()
    {
        Assert.Equal(HtmlContext.UrlAttribute, AnalyzeFull("<a class=\"x\" href=\""));
    }

    [Fact]
    public void MultipleSpaces_StillResets()
    {
        Assert.Equal(HtmlContext.UrlAttribute, AnalyzeFull("<a   href=\""));
    }

    [Fact]
    public void TabBetweenAttributes_ResetsName()
    {
        Assert.Equal(HtmlContext.UrlAttribute, AnalyzeFull("<a\thref=\""));
    }

    // --- Tag name vs attribute name ---

    [Fact]
    public void TagName_DoesNotPollute_AttributeName()
    {
        Assert.Equal(HtmlContext.UrlAttribute, AnalyzeFull("<a href=\""));
    }

    [Fact]
    public void LongTagName_DoesNotPollute()
    {
        Assert.Equal(HtmlContext.Attribute, AnalyzeFull("<button class=\""));
    }

    // --- Multiple attributes in sequence ---

    [Fact]
    public void SecondAttribute_HasCorrectContext()
    {
        Assert.Equal(HtmlContext.UrlAttribute, AnalyzeFull("<a class=\"link\" href=\""));
    }

    [Fact]
    public void ThreeAttributes_LastIsCorrect()
    {
        Assert.Equal(HtmlContext.UrlAttribute, AnalyzeFull("<a id=\"x\" class=\"y\" href=\""));
    }

    // --- Edge cases ---

    [Fact]
    public void EmptyTag_ReturnsToContent()
    {
        Assert.Equal(HtmlContext.Content, AnalyzeFull("<>"));
    }

    [Fact]
    public void SelfClosingTag_ReturnsToContent()
    {
        Assert.Equal(HtmlContext.Content, AnalyzeFull("<br/>"));
    }

    [Fact]
    public void NestedTags_TrackCorrectly()
    {
        Assert.Equal(HtmlContext.Content, AnalyzeFull("<div><span></span></div>"));
    }

    [Fact]
    public void EqualsSign_SetsInAttribute()
    {
        var analyzer = new HtmlContextAnalyzer();
        foreach (var c in "<div x=\"")
            analyzer.ProcessChar(c);
        Assert.Equal(HtmlContext.Attribute, analyzer.CurrentContext);
    }

    [Fact]
    public void NoEqualsSign_LettersAccumulate()
    {
        Assert.Equal(HtmlContext.Attribute, AnalyzeFull("<div disabled class=\""));
    }

    [Fact]
    public void AfterCompleteTag_ContentResets()
    {
        var analyzer = new HtmlContextAnalyzer();
        foreach (var c in "<div class=\"x\">")
            analyzer.ProcessChar(c);

        Assert.Equal(HtmlContext.Content, analyzer.CurrentContext);
    }

    [Fact]
    public void QuoteInsideAttribute_DoesNotCloseWrongQuote()
    {
        var analyzer = new HtmlContextAnalyzer();
        foreach (var c in "<div title=\"it's")
            analyzer.ProcessChar(c);
        Assert.Equal(HtmlContext.Attribute, analyzer.CurrentContext);
    }

    [Fact]
    public void SingleQuoteAttribute_ClosedBySingleQuote()
    {
        var analyzer = new HtmlContextAnalyzer();
        foreach (var c in "<div title='value'")
            analyzer.ProcessChar(c);
        Assert.Equal(HtmlContext.Content, analyzer.CurrentContext);
    }
}
