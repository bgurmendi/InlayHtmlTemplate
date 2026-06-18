using AspNetTemplates;
using Microsoft.AspNetCore.Html;

namespace WebApp.Templates;

public static class ErrorTemplate
{
    public static IHtmlContent Render(string? requestId)
    {
        var showId = !string.IsNullOrEmpty(requestId);

        return Html.Template($"""
            <h1 class="error-title">Error</h1>
            <p>An error occurred while processing your request.</p>
            {Html.If(showId, $"""
                <p><strong>Request ID:</strong> <code>{requestId}</code></p>
                """)}
            """);
    }
}
