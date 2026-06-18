using InlayHtmlTemplate;
using Microsoft.AspNetCore.Html;

namespace WebApp.Templates;

public static class ErrorTemplate
{
    public static InlayTemplate Render(string? requestId)
    {
        var showId = !string.IsNullOrEmpty(requestId);

        return Inlay.Template($"""
            <h1 class="error-title">Error</h1>
            <p>An error occurred while processing your request.</p>
            {Inlay.If(showId, $"""
                <p><strong>Request ID:</strong> <code>{requestId}</code></p>
                """)}
            """);
    }
}
