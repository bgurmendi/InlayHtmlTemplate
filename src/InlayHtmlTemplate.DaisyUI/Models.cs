namespace InlayHtmlTemplate.DaisyUI;

public record StatItem(string Title, string Value, string? Description = null);

public record FooterSection(string Title, IEnumerable<FooterLink> Links);

public record FooterLink(string Label, string Href);

public record SelectOption(string Value, string Label, bool Selected = false);
